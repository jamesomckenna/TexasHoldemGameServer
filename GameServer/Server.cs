using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Globalization;

namespace GameServer
{
    class Server
    {
        // Game Server Data init
        public static int MaxPlayers { get; private set; }
        public static int MaxLobbies { get; private set; }
        public static int Port { get; private set; }

        public static Dictionary<int, Client> clients = new Dictionary<int, Client>();
        public static Dictionary<string, PokerGame> lobbies = new Dictionary<string, PokerGame>();

        public delegate void PacketHandler(int _fromClient, Packet _packet);
        public static Dictionary<int, PacketHandler> packetHandlers;

        private static TcpListener tcpListener;
        public static SQL db;

        public static CultureInfo cultureInfo = new CultureInfo("en-AU");


        public static void Start(int _port, int _maxPlayers, int _maxLobbies, string _dbserver, string _dbuser, string _dbpass, string _dbname)
        {
            Port = _port;
            MaxPlayers = _maxPlayers;
            string dbuser = _dbuser;
            string dbpass = _dbpass;
            bool dbvalid = false;
            SQL dbtemp = null;

            Console.WriteLine("NOTICE - Starting server...");
            InitializeServerData();


            // loop to connect to database
            do {
                Console.WriteLine("Enter SQL server username:");
                dbuser = Console.ReadLine();
                Console.WriteLine("Enter SQL server password:");
                dbpass = Console.ReadLine();

                // connectiong to database
                Console.WriteLine("NOTICE - Initialising Database...");
                dbtemp = new SQL(_dbserver, dbuser, dbpass, _dbname);
                SQLResponse response = dbtemp.TruncateLobbies();
                if (response.success) {
                    dbvalid = true;
                    db = dbtemp;
                } else {
                    Console.WriteLine("ERROR - Invalid database credentials");
                }
            } while (!dbvalid);

            // start TCP socket connection
            tcpListener = new TcpListener(IPAddress.Any, Port);
            tcpListener.Start();
            tcpListener.BeginAcceptTcpClient(TCPConnectCallback, null);

            Console.WriteLine("NOTICE - Server started on port "+Port+".");
        }

        // on connection recieve, add player to player list
        private static void TCPConnectCallback(IAsyncResult _result)
        {
            TcpClient _client = tcpListener.EndAcceptTcpClient(_result);
            tcpListener.BeginAcceptTcpClient(TCPConnectCallback, null);
            Console.WriteLine("NOTICE - Incoming connection from "+_client.Client.RemoteEndPoint+"...");

            for (int i = 1; i <= MaxPlayers; i++)
            {
                if (clients[i].tcp.socket == null)
                {
                    clients[i].tcp.Connect(_client);
                    return;
                }
            }

            Console.WriteLine("NOTICE - " + _client.Client.RemoteEndPoint + " failed to connect: Server full!");
        }

        private static void InitializeServerData()
        {
            // initialise slots for players
            for (int i = 1; i <= MaxPlayers; i++)
            {
                clients.Add(i, new Client(i, i.ToString()));
            }

            // initialise packet handling
            packetHandlers = new Dictionary<int, PacketHandler>()
            {
                { (int)ClientPackets.createLobbyRequest, ServerHandle.CreateLobbyRequest },
                { (int)ClientPackets.joinLobbyRequest, ServerHandle.JoinLobbyRequest },
                { (int)ClientPackets.startGameRequest, ServerHandle.StartGameRequest },
                { (int)ClientPackets.raiseRequest, ServerHandle.RaiseRequest },
                { (int)ClientPackets.checkRequest, ServerHandle.CheckRequest },
                { (int)ClientPackets.callRequest, ServerHandle.CallRequest },
                { (int)ClientPackets.foldRequest, ServerHandle.FoldRequest },
                { (int)ClientPackets.checkUserRequest, ServerHandle.CheckUserRequest },
                { (int)ClientPackets.signupRequest, ServerHandle.SignUpRequest },
                { (int)ClientPackets.loginRequest, ServerHandle.LoginRequest },
                { (int)ClientPackets.browseLobbyRequest, ServerHandle.BrowseLobbyRequest },
                { (int)ClientPackets.showLobbyRequest, ServerHandle.ShowLobbyRequest },
                { (int)ClientPackets.viewFriendsRequest, ServerHandle.ViewFriendsRequest },
                { (int)ClientPackets.removeFriendRequest, ServerHandle.RemoveFriendsRequest },
                { (int)ClientPackets.sendFriendInviteRequest, ServerHandle.SendFriendInviteRequest },
                { (int)ClientPackets.viewOutgoingInvitesRequest, ServerHandle.ViewOutgoingInvitesRequest },
                { (int)ClientPackets.deleteOutgoingRequest, ServerHandle.DeleteOutgoingRequest },
                { (int)ClientPackets.viewPendingInvitesRequest, ServerHandle.ViewPendingInvitesRequest },
                { (int)ClientPackets.acceptPendingInviteRequest, ServerHandle.AcceptPendingInviteRequest },
                { (int)ClientPackets.declinePendingInviteRequest, ServerHandle.DeclinePendingInviteRequest },
                { (int)ClientPackets.messageRequest, ServerHandle.MessageRequest },
                { (int)ClientPackets.historyRequest, ServerHandle.HistoryRequest },
                { (int)ClientPackets.leaveLobby, ServerHandle.LeaveLobby },
            };
            Console.WriteLine("NOTICE - Initialized packets.");
        }


        // searches and returns a lobby from it's ID, if no lobby was found, returns null
        public static PokerGame GetLobbyByID(string _lobbyid)
        {
            if (lobbies.ContainsKey(_lobbyid))
            {
                PokerGame lobby = lobbies[_lobbyid];
                if (lobby != null)
                {
                    return lobby;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }


        // searches and returns client username from it's ID, if client was not found was found, returns the ID
        public static string GetUsernameByID(int _id)
        {
            if (clients[_id] != null)
            {
                return clients[_id].username;
            }
            else
            {
                return _id.ToString();
            }
        }

        

        public static void CreateLobby(int _clientid, string _lobbyName, string _lobbyid, int _startingTokens, int _maxPlayers, int _minPlayers, int _turnLimit, bool _privateGame)
        {
            // check if lobby count has not exceeded
            if (lobbies.Count < MaxLobbies)
            {
                // error handling user input
                if (lobbies.ContainsKey(_lobbyid))
                {
                    if(lobbies[_lobbyid] != null)
                    {
                        ServerSend.CreateLobbyResponse(_clientid, false, "Invalid lobby code. Room code is already in use.", "");
                        return;
                    }
                }

                // error handling user input
                if (_lobbyName.Length < 3 || _lobbyName.Length > 20)
                {
                    ServerSend.CreateLobbyResponse(_clientid, false, "Invalid name. Amount of characters must be between 3 and 20.", "");
                    return;
                }

                if (_lobbyid.Length < 3 || _lobbyid.Length > 20)
                {
                    ServerSend.CreateLobbyResponse(_clientid, false, "Invalid lobby code. Amount of characters must be between 3 and 20.", "");
                    return;
                }

                if (_startingTokens < 10)
                {
                    ServerSend.CreateLobbyResponse(_clientid, false, "Starting token amount is too small. Value must be greater than 10.", "");
                    return;
                }

                if (_startingTokens > 500)
                {
                    ServerSend.CreateLobbyResponse(_clientid, false, "Starting token amount is too large. Value must be smaller than 500.", "");
                    return;
                }

                if (_maxPlayers < _minPlayers)
                {
                    ServerSend.CreateLobbyResponse(_clientid, false, "The lobby size is too large. A lobby can only hold a maximum of 5 players.", "");
                    return;
                }

                if (_maxPlayers > 5)
                {
                    ServerSend.CreateLobbyResponse(_clientid, false, "The maximum player size is too large. A lobby can only hold a maximum of 5 players.", "");
                    return;
                }

                if (_maxPlayers < 2)
                {
                    ServerSend.CreateLobbyResponse(_clientid, false, "The maximum player size is too small. A lobby must have a minimum of 2 players.", "");
                    return;
                }

                if (_minPlayers > 5)
                {
                    ServerSend.CreateLobbyResponse(_clientid, false, "The minimum player size is too large. A lobby can only hold a maximum of 5 players.", "");
                    return;
                }

                if (_minPlayers < 2)
                {
                    ServerSend.CreateLobbyResponse(_clientid, false, "The minimum player size is too small. A lobby must have a minimum of 2 players.", "");
                    return;
                }

                Console.WriteLine("NOTICE - Creating game lobby. ID: " + _lobbyid);
                PokerGame lobby = new PokerGame(_lobbyName, _lobbyid, _startingTokens, _maxPlayers, _minPlayers, _turnLimit, _privateGame);                // create and add lobby to list of running lobbies
                lobbies.Add(_lobbyid, lobby);
                ServerSend.CreateLobbyResponse(_clientid, true, "Successfully created room", _lobbyid);                            // return successful response to client
                db.AddLobby(lobby);                                                                                                // add table to database
                clients[_clientid].SendIntoGame(_lobbyid);                                                                         // send client into lobby
            }
            else
            {
                ServerSend.CreateLobbyResponse(_clientid, false, "Server has reached lobby limit. Please try again another time.", null);
            }
        }


        public static void prepareListOfPublicLobbies(int _fromClient)
        {
            List<PokerGame> availableLobbies = new List<PokerGame>();
            foreach (PokerGame lobby in lobbies.Values)
            {
                if(lobby.privateGame == false && lobby.numPlayersInLobby() < lobby.maxPlayers)
                {
                    availableLobbies.Add(lobby);
                }
            }

            if (availableLobbies.Count > 0)
            {
                ServerSend.BrowseLobbyResponse(_fromClient, true, "Success", availableLobbies);
            }
            else
            {
                ServerSend.BrowseLobbyResponse(_fromClient, false, "No lobbies available.", availableLobbies);
            }
        }





        // get list of friends with client ID from the SQL database
        public static void GetFriendsList(int _fromClient)
        {
            string username = clients[_fromClient].username;
            List<string> userFriends = new List<string>();
            SQLResponse response = db.ViewFriends(username);

            if (response.success)
            {
                for (int i = 0; i < response.data.Tables[0].Rows.Count; i++)
                {
                    string senderName = response.data.Tables[0].Rows[i]["sender"].ToString();
                    string recipientName = response.data.Tables[0].Rows[i]["recipient"].ToString();

                    if (username == recipientName)
                    {
                        userFriends.Add(senderName);
                    }
                    else
                    {
                        userFriends.Add(recipientName);
                    }
                }
            }

            // send results
            ServerSend.ViewFriendsResponse(_fromClient, true, response.message, userFriends);
        }

        // get list of pending invites with client ID from the SQL database
        public static void GetPendingInvitesList(int _fromClient)
        {
            string username = clients[_fromClient].username;
            List<string> pendingInvites = new List<string>();
            SQLResponse response = db.ViewPendingInvites(username);

            if (response.success)
            {
                for (int i = 0; i < response.data.Tables[0].Rows.Count; i++)
                {
                    string senderName = response.data.Tables[0].Rows[i]["sender"].ToString();
                    pendingInvites.Add(senderName);
                }
            }

            // send results
            ServerSend.ViewPendingInvitesResponse(_fromClient, response.success, response.message, pendingInvites);
        }

        // get list of outgoing invites with client ID from the SQL database
        public static void GetOutgoingInvitesList(int _fromClient)
        {
            string username = clients[_fromClient].username;
            List<string> pendingInvites = new List<string>();
            SQLResponse response = db.ViewOutgoingInvites(username);

            if (response.success)
            {
                for (int i = 0; i < response.data.Tables[0].Rows.Count; i++)
                {
                    string recipientName = response.data.Tables[0].Rows[i]["recipient"].ToString();
                    pendingInvites.Add(recipientName);
                }
            }

            // send results
            ServerSend.ViewOutgoingInvitesResponse(_fromClient, response.success, response.message, pendingInvites);
        }


        // send a message from one friend to another
        public static void SendMessage(int _senderid, string _recipient, string _msg)
        {
            // check if recipient is sender
            string sendername = clients[_senderid].username;
            if(sendername == _recipient)
            {
                ServerSend.MessageResponse(_senderid, false, "You cannot send a message to yourself.");
            } 
            else
            {
                // check if recipient is friend of sender
                SQLResponse result = db.FindFriend(sendername, _recipient);
                if (!result.success)
                {
                    ServerSend.MessageResponse(_senderid, false, result.message);
                }
                else
                {
                    // search active clients for username
                    int recipientid = 0;
                    for (int i = 1; i < MaxPlayers; i++)
                    {
                        if (clients[i] != null)
                        {
                            if (clients[i].username == _recipient)
                            {
                                recipientid = i;
                                break;
                            }
                        }
                    }


                    if (recipientid != 0)
                    {
                        // if recipient found, send message
                        ServerSend.MessageResponse(_senderid, true, "Message sent.");
                        ServerSend.MessageResult(recipientid, sendername, _msg);
                    }
                    else
                    {
                        ServerSend.MessageResponse(_senderid, false, "Recipient is not online.");
                    }
                }
            }
        }


        // update lobby information from database
        // executed every minute on Program.cs
        public static void updateServerLobbies()
        {
            ThreadManager.ExecuteOnMainThread(() =>
            {
                if(db != null)
                {
                    SQLResponse response = db.GetAllLobbiesData();
                    if (response.success)
                    {
                        for (int i = 0; i < response.data.Tables[0].Rows.Count; i++)
                        {
                            var row = response.data.Tables[0].Rows[i];
                            string roomCode = row["roomCode"].ToString();
                            string roomName = row["tableName"].ToString();
                            bool privateGame = (bool)row["private"];

                            if (lobbies.ContainsKey(roomCode))
                            {
                                if(lobbies[roomCode].roomName != roomName || lobbies[roomCode].privateGame != privateGame)
                                {
                                    Console.WriteLine("NOTICE - Updating lobby " + roomCode);
                                    lobbies[roomCode].roomName = roomName;
                                    lobbies[roomCode].privateGame = privateGame;
                                }
                            }
                        }
                    }
                }
            });
        }


        // update lobby information from database
        // executed every second on Program.cs
        public static void updateLobbyTimers()
        {
            ThreadManager.ExecuteOnMainThread(() =>
            {
                foreach (PokerGame lobby in lobbies.Values)
                {
                    if (lobby.isRunning)
                    {
                        lobby.incrementTimer();
                    }
                }
            });
        }
    }
}
