using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Numerics;

namespace GameServer
{
    class Client
    {
        public static int dataBufferSize = 4096;

        public int id;
        public string lobbyid;
        public string username;
        public Player player;
        public TCP tcp;

        public Client(int _clientId, string _username)
        {
            id = _clientId;
            username = _username;
            tcp = new TCP(id);
            lobbyid = null;
        }

        // TCP socket class for enabling TCP socket connections from client ot server
        public class TCP
        {
            public TcpClient socket;

            private readonly int id;
            private NetworkStream stream;
            private Packet receivedData;
            private byte[] receiveBuffer;

            public TCP(int _id)
            {
                id = _id;
            }

            public void Connect(TcpClient _socket)
            {
                socket = _socket;
                socket.ReceiveBufferSize = dataBufferSize;
                socket.SendBufferSize = dataBufferSize;

                stream = socket.GetStream();

                receivedData = new Packet();
                receiveBuffer = new byte[dataBufferSize];

                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);

                ServerSend.Welcome(id, "Welcome to the server user " + id + "!");
            }

            public void SendData(Packet _packet)
            {
                try
                {
                    if (socket != null)
                    {
                        stream.BeginWrite(_packet.ToArray(), 0, _packet.Length(), null, null);
                    }
                }
                catch (Exception _ex)
                {
                    Console.WriteLine("ERROR - Cannot send data to player " + id + " via TCP: " + _ex);
                }
            }

            private void ReceiveCallback(IAsyncResult _result)
            {
                try
                {
                    int _byteLength = stream.EndRead(_result);
                    if (_byteLength <= 0)
                    {
                        Server.clients[id].Disconnect();
                        return;
                    }

                    byte[] _data = new byte[_byteLength];
                    Array.Copy(receiveBuffer, _data, _byteLength);

                    receivedData.Reset(HandleData(_data));
                    stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
                }
                catch (Exception _ex)
                {
                    Console.WriteLine("ERROR - Cannot receive TCP data: " + _ex);
                    Server.clients[id].Disconnect();
                }
            }

            private bool HandleData(byte[] _data)
            {
                int _packetLength = 0;

                receivedData.SetBytes(_data);

                if (receivedData.UnreadLength() >= 4)
                {
                    _packetLength = receivedData.ReadInt();
                    if (_packetLength <= 0)
                    {
                        return true;
                    }
                }

                while (_packetLength > 0 && _packetLength <= receivedData.UnreadLength())
                {
                    byte[] _packetBytes = receivedData.ReadBytes(_packetLength);
                    ThreadManager.ExecuteOnMainThread(() =>
                    {
                        using (Packet _packet = new Packet(_packetBytes))
                        {
                            int _packetId = _packet.ReadInt();
                            Server.packetHandlers[_packetId](id, _packet);
                        }
                    });

                    _packetLength = 0;
                    if (receivedData.UnreadLength() >= 4)
                    {
                        _packetLength = receivedData.ReadInt();
                        if (_packetLength <= 0)
                        {
                            return true;
                        }
                    }
                }

                if (_packetLength <= 1)
                {
                    return true;
                }

                return false;
            }

            // set values to null to close stream
            public void Disconnect()
            {
                socket.Close();
                stream = null;
                receivedData = null;
                receiveBuffer = null;
                socket = null;
            }
        }

        // send player data into lobby
        public void SendIntoGame(string _lobbyid)
        {
            PokerGame lobby = Server.GetLobbyByID(_lobbyid);
            if (lobby != null)
            {
                lobbyid = _lobbyid;
                player = new Player(id, lobby.startingTokens);

                Server.lobbies[_lobbyid].AddPlayer(player);
                Server.db.AddPlayerToLobby(Server.clients[id].username, _lobbyid);
            }
            else
            {
                ServerSend.JoinLobbyResponse(id, false, "Lobby not found", "");
            }
        }


        public void LeaveGame()
        {
            Console.WriteLine("NOTICE - " + tcp.socket.Client.RemoteEndPoint + " has left the lobby " + lobbyid);
            player = null;
            lobbyid = null;
        }

        // remove client from player list and notify other clients of disconnection
        private void Disconnect()
        {
            Console.WriteLine("NOTICE - " + tcp.socket.Client.RemoteEndPoint + " has disconnected");
            if(lobbyid != null)
            {
                if(Server.GetLobbyByID(lobbyid) != null)
                {
                    Server.lobbies[lobbyid].RemovePlayer(player);

                    // if no players remain, delete lobby
                    if(Server.lobbies[lobbyid].numPlayersInLobby() == 0)
                    {
                        Console.WriteLine("NOTICE - Deleting lobby " + lobbyid);
                        Server.lobbies.Remove(lobbyid);
                        Server.db.RemoveLobby(lobbyid);
                    }
                }
            }

            Server.db.RemovePlayerFromLobby(Server.clients[id].username);

            player = null;
            lobbyid = null;

            tcp.Disconnect();
        }

    }
}
