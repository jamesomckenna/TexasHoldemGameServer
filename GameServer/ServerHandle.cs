using System;
using System.Collections.Generic;
using System.Text;

namespace GameServer
{
    class ServerHandle
    {

        #region Connection

        // returns if a username has been taken or not
        public static void CheckUserRequest(int _fromClient, Packet _packet)
        {
            try
            {
                Console.WriteLine("HANDLE - 'CheckUserRequest' from " + _fromClient);
                string _username = _packet.ReadString();

                // handle user data
                if (_username.Length < 3 || _username.Length > 20)
                {
                    ServerSend.CheckUserResponse(_fromClient, false, "Invalid username. Amount of characters must be between 3 and 20.");
                    return;
                }

                SQLResponse result = Server.db.UsernameExist(_username);
                if (!result.success)
                {
                    ServerSend.CheckUserResponse(_fromClient, true, "Unique username");
                }
                else
                {
                    ServerSend.CheckUserResponse(_fromClient, false, result.message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR - " + ex.Message + ": " + ex.StackTrace);
            }
        }


        // creates a new user in the database
        public static void SignUpRequest(int _fromClient, Packet _packet)
        {
            try
            {
                Console.WriteLine("HANDLE - 'SignUpRequest' from " + _fromClient);
                int _clientIdCheck = _packet.ReadInt();
                string _username = _packet.ReadString();

                // matches user from id send to client in the Welcome packet
                if (_fromClient != _clientIdCheck)
                {
                    // if this is called, somethingh went VERY WRONG
                    Console.WriteLine("ERROR - Player '" + _username + "' (ID: " + _fromClient + ") has assumed the wrong client ID (" + _clientIdCheck + ")!");
                    ServerSend.SignUpResponse(_fromClient, false, "Error occured. ID check failed. Check server Logs");
                    return;
                }

                // handle user data
                if (_username.Length < 3 || _username.Length > 20)
                {
                    ServerSend.SignUpResponse(_fromClient, false, "Invalid username. Amount of characters must be between 3 and 20.");
                    return;
                }

                // checks if username exists in database
                SQLResponse result = Server.db.AddUser(_username);
                if (!result.success)
                {
                    ServerSend.SignUpResponse(_fromClient, result.success, result.message);
                    return;
                }

                Console.WriteLine("NOTICE - " + Server.clients[_fromClient].tcp.socket.Client.RemoteEndPoint + " connected successfully and is now player " + _fromClient + ".");
                Server.clients[_fromClient].username = _username;
                ServerSend.SignUpResponse(_fromClient, result.success, "Successfully signed up to the server.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR - " + ex.Message + ": " + ex.StackTrace);
            }
        }


        // creates a new user in the database
        public static void LoginRequest(int _fromClient, Packet _packet)
        {
            try
            {
                Console.WriteLine("HANDLE - 'LoginRequest' from " + _fromClient);
                int _clientIdCheck = _packet.ReadInt();
                string _username = _packet.ReadString();

                // matches user from id send to client in the Welcome packet
                if (_fromClient != _clientIdCheck)
                {
                    // if this is called, somethingh went VERY WRONG
                    Console.WriteLine("ERROR - Player '" + _username + "' (ID: " + _fromClient + ") has assumed the wrong client ID (" + _clientIdCheck + ")!");
                    ServerSend.LoginResponse(_fromClient, false, "Error occured. ID check failed. Check server Logs");
                    return;
                }

                // checks if username exists in database
                SQLResponse result = Server.db.UsernameExist(_username);
                if (!result.success)
                {
                    ServerSend.LoginResponse(_fromClient, result.success, result.message);
                    return;
                }

                Console.WriteLine("NOTICE - " + Server.clients[_fromClient].tcp.socket.Client.RemoteEndPoint + " connected successfully and is now player " + _fromClient + ".");
                Server.clients[_fromClient].username = _username;
                ServerSend.LoginResponse(_fromClient, result.success, "Success");
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR - " + ex.Message + ": " + ex.StackTrace);
            }
        }

        #endregion




        #region Lobbies

        // recieved when a player wishes to create a lobby 
        public static void CreateLobbyRequest(int _fromClient, Packet _packet)
        {
            try
            {
                Console.WriteLine("HANDLE - 'CreateLobbyRequest' from " + Server.clients[_fromClient].username);
                string _lobbyname = _packet.ReadString();
                string _lobbyid = _packet.ReadString();
                int _startingTokens = _packet.ReadInt();
                int _maxPlayers = _packet.ReadInt();
                int _minPlayers = _packet.ReadInt();
                int _turnLimit = _packet.ReadInt();
                bool _privateGame = _packet.ReadBool();

                Server.CreateLobby(_fromClient, _lobbyname, _lobbyid, _startingTokens, _maxPlayers, _minPlayers, _turnLimit, _privateGame);
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR - " + ex.Message + ": " + ex.StackTrace);
            }
        }

        // retrieves a list of all available public lobbies
        public static void BrowseLobbyRequest(int _fromClient, Packet _packet)
        {
            try
            {
                Console.WriteLine("HANDLE - 'BrowseLobbyRequest' from " + Server.clients[_fromClient].username);

                Server.prepareListOfPublicLobbies(_fromClient);
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR - " + ex.Message + ": " + ex.StackTrace);
            }
        }

        // retrieves a list of all available public lobbies
        public static void ShowLobbyRequest(int _fromClient, Packet _packet)
        {
            try
            {
                Console.WriteLine("HANDLE - 'ShowLobbyRequest' from " + Server.clients[_fromClient].username);
                string _lobbyid = _packet.ReadString();

                PokerGame lobby = Server.GetLobbyByID(_lobbyid);
                if (lobby != null)
                {
                    ServerSend.ShowLobbyResponse(_fromClient, true, "Success", lobby);
                }
                else
                {
                    ServerSend.ShowLobbyResponse(_fromClient, false, "Lobby not found", lobby);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR - " + ex.Message + ": " + ex.StackTrace);
            }
        }

        // recieved when a player requests to join the gamer lobby
        public static void JoinLobbyRequest(int _fromClient, Packet _packet)
        {
            try
            {
                Console.WriteLine("HANDLE - 'JoinLobbyRequest' from " + Server.clients[_fromClient].username);
                string _lobbyid = _packet.ReadString();

                Server.clients[_fromClient].SendIntoGame(_lobbyid);
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR - " + ex.Message + ": " + ex.StackTrace);
            }
        }

        #endregion




        #region Game Turn

        // recieved when a wants to start the game
        public static void StartGameRequest(int _fromClient, Packet _packet)
        {
            try
            {
                Console.WriteLine("HANDLE - 'StartGameRequest' from " + Server.clients[_fromClient].username);
                string _lobbyid = _packet.ReadString();

                // check if lobby exists
                PokerGame lobby = Server.GetLobbyByID(_lobbyid);
                if (lobby != null)
                {
                    lobby.StartGame(_fromClient);
                }
                else
                {
                    ServerSend.StartGameResponse(_fromClient, false, "Lobby not found");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR - " + ex.Message + ": " + ex.StackTrace);
            }
        }

        // recieved when a player requests to make a raise
        public static void RaiseRequest(int _fromClient, Packet _packet)
        {
            try
            {
                Console.WriteLine("HANDLE - 'RaiseRequest' from " + Server.clients[_fromClient].username);
                string _lobbyid = _packet.ReadString();
                int _bet = _packet.ReadInt();

                // check if lobby exists
                PokerGame lobby = Server.GetLobbyByID(_lobbyid);
                if (lobby != null)
                {
                    lobby.Raise(_fromClient, _bet);
                }
                else
                {
                    ServerSend.RaiseResponse(_fromClient, false, "Lobby not found");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR - " + ex.Message + ": " + ex.StackTrace);
            }
        }


        // receieved when a player requests to call
        public static void CallRequest(int _fromClient, Packet _packet)
        {
            try
            {
                Console.WriteLine("HANDLE - 'CallRequest' from " + Server.clients[_fromClient].username);
                string _lobbyid = _packet.ReadString();

                // check if lobby exists
                PokerGame lobby = Server.GetLobbyByID(_lobbyid);
                if (lobby != null)
                {
                    lobby.Call(_fromClient);
                }
                else
                {
                    ServerSend.CallResponse(_fromClient, false, "Lobby not found");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR - " + ex.Message + ": " + ex.StackTrace);
            }
        }


        // recieved when a player requests to fold 
        public static void FoldRequest(int _fromClient, Packet _packet)
        {
            try
            {
                Console.WriteLine("HANDLE - 'FoldRequest' from " + Server.clients[_fromClient].username);
                string _lobbyid = _packet.ReadString();

                // check if lobby exists
                PokerGame lobby = Server.GetLobbyByID(_lobbyid);
                if (lobby != null)
                {
                    lobby.Fold(_fromClient);
                }
                else
                {
                    ServerSend.FoldResponse(_fromClient, false, "Lobby not found");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR - " + ex.Message + ": " + ex.StackTrace);
            }
        }


        // receieved when a players requests to check
        public static void CheckRequest(int _fromClient, Packet _packet)
        {
            try
            {
                Console.WriteLine("HANDLE - 'CheckRequest' from " + Server.clients[_fromClient].username);
                string _lobbyid = _packet.ReadString();

                // check if lobby exists
                PokerGame lobby = Server.GetLobbyByID(_lobbyid);
                if (lobby != null)
                {
                    lobby.Check(_fromClient);
                }
                else
                {
                    ServerSend.CheckResponse(_fromClient, false, "Lobby not found");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR - " + ex.Message + ": " + ex.StackTrace);
            }
        }



        // receieved when a players requests to check
        public static void LeaveLobby(int _fromClient, Packet _packet)
        {
            try
            {
                Console.WriteLine("HANDLE - 'CheckRequest' from " + Server.clients[_fromClient].username);
                string _lobbyid = _packet.ReadString();

                // check if lobby exists
                PokerGame lobby = Server.GetLobbyByID(_lobbyid);
                if (lobby != null)
                {
                    lobby.RemovePlayer(Server.clients[_fromClient].player);

                    // if no players remain, delete lobby
                    if (Server.lobbies[_lobbyid].numPlayersInLobby() == 0)
                    {
                        Console.WriteLine("NOTICE - Deleting lobby " + _lobbyid);
                        Server.lobbies.Remove(_lobbyid);
                        Server.db.RemoveLobby(_lobbyid);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR - " + ex.Message + ": " + ex.StackTrace);
            }
        }
        #endregion




        #region Friends
        // exceutded when client requests to view their friends list
        public static void ViewFriendsRequest(int _fromClient, Packet _packet)
        {
            try
            {
                Console.WriteLine("HANDLE - 'ViewFriendsRequest' from " + Server.clients[_fromClient].username);
                Server.GetFriendsList(_fromClient);
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR - " + ex.Message + ": " + ex.StackTrace);
            }
        }

        // exceutded when client requests to remove a friend from their friends list
        public static void RemoveFriendsRequest(int _fromClient, Packet _packet)
        {
            try
            {
                Console.WriteLine("HANDLE - 'RemoveFriendsRequest' from " + Server.clients[_fromClient].username);
                string _friendUsername = _packet.ReadString();

                string username = Server.clients[_fromClient].username;

                SQLResponse response = Server.db.DeleteFriend(username, _friendUsername);
                ServerSend.RemoveFriendResponse(_fromClient, response.success, response.message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR - " + ex.Message + ": " + ex.StackTrace);
            }
        }

        // exceutded when client requests to send a friend request to an existing user
        public static void SendFriendInviteRequest(int _fromClient, Packet _packet)
        {
            try
            {
                Console.WriteLine("HANDLE - 'SendFriendInviteRequest' from " + Server.clients[_fromClient].username);
                string _recipientUsername = _packet.ReadString();

                string username = Server.clients[_fromClient].username;

                SQLResponse response = Server.db.SendInvite(username, _recipientUsername);
                ServerSend.SendFriendInviteResponse(_fromClient, response.success, response.message);
                if (response.success)
                {
                    // search active clients for username
                    int recipientid = 0;
                    for (int i = 1; i < Server.MaxPlayers; i++)
                    {
                        if (Server.clients[i] != null)
                        {
                            if (Server.clients[i].username == _recipientUsername)
                            {
                                recipientid = i;
                                ServerSend.SendFriendInviteResult(i, username);
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR - " + ex.Message + ": " + ex.StackTrace);
            }
        }

        // exceutded when client requests to view their sent friend invites
        public static void ViewOutgoingInvitesRequest(int _fromClient, Packet _packet)
        {
            try
            {
                Console.WriteLine("HANDLE - 'ViewOutgoingInvitesRequest' from " + Server.clients[_fromClient].username);
                Server.GetOutgoingInvitesList(_fromClient);
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR - " + ex.Message + ": " + ex.StackTrace);
            }
        }

        // exceutded when client requests to delete an outgoing friend request
        public static void DeleteOutgoingRequest(int _fromClient, Packet _packet)
        {
            try
            {
                Console.WriteLine("HANDLE - 'DeleteOutgoingRequest' from " + Server.clients[_fromClient].username);
                string _friendUsername = _packet.ReadString();

                string username = Server.clients[_fromClient].username;

                SQLResponse response = Server.db.DeleteFriend(username, _friendUsername);
                ServerSend.DeleteOutgoingResponse(_fromClient, response.success, response.message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR - " + ex.Message + ": " + ex.StackTrace);
            }
        }

        // exceutded when client requests to view their pending friend invites
        public static void ViewPendingInvitesRequest(int _fromClient, Packet _packet)
        {
            try
            {
                Console.WriteLine("HANDLE - 'ViewPendingInvitesRequest' from " + Server.clients[_fromClient].username);
                Server.GetPendingInvitesList(_fromClient);
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR - " + ex.Message + ": " + ex.StackTrace);
            }
        }


        // exceutded when client requests to accept a friend request
        public static void AcceptPendingInviteRequest(int _fromClient, Packet _packet)
        {
            try
            {
                Console.WriteLine("HANDLE - 'AcceptPendingInviteRequest' from " + Server.clients[_fromClient].username);
                string _friendUsername = _packet.ReadString();

                string username = Server.clients[_fromClient].username;

                SQLResponse response = Server.db.AcceptInvite(username, _friendUsername);
                ServerSend.AcceptPendingInviteResponse(_fromClient, response.success, response.message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR - " + ex.Message + ": " + ex.StackTrace);
            }
        }

        // exceutded when client requests to decline a friend request
        public static void DeclinePendingInviteRequest(int _fromClient, Packet _packet)
        {
            try
            {
                Console.WriteLine("HANDLE - 'DeclinePendingInviteRequest' from " + Server.clients[_fromClient].username);
                string _friendUsername = _packet.ReadString();

                string username = Server.clients[_fromClient].username;

                SQLResponse response = Server.db.DeleteFriend(username, _friendUsername);
                ServerSend.DeclinePendingInviteResponse(_fromClient, response.success, response.message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR - " + ex.Message + ": " + ex.StackTrace);
            }
        }
        #endregion




        #region Messaging
        // exceutded when client requests to send a text message to their friends list
        public static void MessageRequest(int _fromClient, Packet _packet)
        {
            try
            {
                Console.WriteLine("HANDLE - 'MessageRequest' from " + Server.clients[_fromClient].username);
                string _recipient = _packet.ReadString();
                string _msg = _packet.ReadString();

                Server.SendMessage(_fromClient, _recipient, _msg);
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR - " + ex.Message + ": " + ex.StackTrace);
            }
        }
        #endregion




        #region GameHistory
        // exceutded when client requests to view their list of previous games played
        public static void HistoryRequest(int _fromClient, Packet _packet)
        {
            try
            {
                Console.WriteLine("HANDLE - 'HistoryRequest' from " + Server.clients[_fromClient].username);

                string username = Server.clients[_fromClient].username;

                SQLResponse response = Server.db.ViewGameHistory(username);

                if (response.success)
                {
                    ServerSend.HistoryResponse(_fromClient, response.success, response.message, response.data.Tables[0].Rows);
                }
                else
                {
                    ServerSend.HistoryResponse(_fromClient, response.success, response.message, null);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR - " + ex.Message + ": " + ex.StackTrace);
            }
        }
        #endregion
    }
}
