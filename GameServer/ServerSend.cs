using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace GameServer
{
    class ServerSend
    {
        #region Packet Sending
        // sends packets to specified client
        private static void SendTCPData(int _toClient, Packet _packet)
        {
            _packet.WriteLength();
            Server.clients[_toClient].tcp.SendData(_packet);
        }

        // sends packet to all clients
        private static void SendTCPDataToAll(Packet _packet)
        {
            _packet.WriteLength();
            for (int i = 1; i <= Server.MaxPlayers; i++)
            {
                Server.clients[i].tcp.SendData(_packet);
            }
        }

        // sends packet to all clients excluding one client
        private static void SendTCPDataToAll(Packet _packet, int _exceptClient)
        {
            _packet.WriteLength();
            for (int i = 1; i <= Server.MaxPlayers; i++)
            {
                if (i != _exceptClient)
                {
                    Server.clients[i].tcp.SendData(_packet);
                }
            }
        }
        #endregion




        #region Connection
        // welcome client to the server
        public static void Welcome(int _toClient, string _msg)
        {
            Console.WriteLine("SEND - 'Welcome' to " + _toClient);
            using (Packet _packet = new Packet((int)ServerPackets.welcome))
            {
                _packet.Write(_msg);
                _packet.Write(_toClient);

                SendTCPData(_toClient, _packet);
            }
        }

        // returns result of client checking for unique usernames
        public static void CheckUserResponse(int _toClient, bool _response, string _msg)
        {
            Console.WriteLine("SEND - 'CheckUserResponse' to " + _toClient);
            using (Packet _packet = new Packet((int)ServerPackets.checkUserResponse))
            {
                _packet.Write(_response);
                _packet.Write(_msg);

                SendTCPData(_toClient, _packet);
            }
        }

        // returns result of client requesting to sign up
        public static void SignUpResponse(int _toClient, bool _response, string _msg)
        {
            Console.WriteLine("SEND - 'SignUpResponse' to " + _toClient);
            using (Packet _packet = new Packet((int)ServerPackets.signupResponse))
            {
                _packet.Write(_response);
                _packet.Write(_msg);

                SendTCPData(_toClient, _packet);
            }
        }

        // returns result of client requesting to login
        public static void LoginResponse(int _toClient, bool _response, string _msg)
        {
            Console.WriteLine("SEND - 'LoginResponse' to " + _toClient);
            using (Packet _packet = new Packet((int)ServerPackets.loginResponse))
            {
                _packet.Write(_response);
                _packet.Write(_msg);

                SendTCPData(_toClient, _packet);
            }
        }
        #endregion




        #region Lobbies
        // sends success or fail response to client who requested to create a new lobby
        public static void CreateLobbyResponse(int _toClient, bool _response, string _msg, string _lobbyid)
        {
            Console.WriteLine("SEND - 'CreateLobbyResponse' to " + Server.clients[_toClient].username);
            using (Packet _packet = new Packet((int)ServerPackets.createLobbyResponse))
            {
                _packet.Write(_response);
                _packet.Write(_msg);
                _packet.Write(_lobbyid);

                SendTCPData(_toClient, _packet);
            }
        }

        public static void BrowseLobbyResponse(int _toClient, bool _response, string _msg, List<PokerGame> _availableLobbies)
        {
            Console.WriteLine("SEND - 'BrowseLobbyResponse' to " + Server.clients[_toClient].username);
            using (Packet _packet = new Packet((int)ServerPackets.browseLobbyResponse))
            {
                _packet.Write(_response);
                _packet.Write(_msg);
                _packet.Write(_availableLobbies.Count);
                foreach(PokerGame lobby in _availableLobbies)
                {
                    string lobbyStatus = "Waiting for more players";
                    int currentPlayers = lobby.numPlayersInLobby();
                    int maxPlayers = lobby.maxPlayers;
                    if (lobby.isRunning) { 
                         lobbyStatus = "Started";
                    }
                    _packet.Write(lobby.roomName);
                    _packet.Write(lobby.roomCode);
                    _packet.Write(lobbyStatus);
                    _packet.Write(currentPlayers);
                    _packet.Write(maxPlayers);
                }

                SendTCPData(_toClient, _packet);
            }
        }

        // returns lobby details from lobby id. If lobby not found, does not return extra data
        public static void ShowLobbyResponse(int _toClient, bool _response, string _msg, PokerGame _lobby)
        {
            Console.WriteLine("SEND - 'ShowLobbyResponse' to " + Server.clients[_toClient].username);
            using (Packet _packet = new Packet((int)ServerPackets.showLobbyResponse))
            {
                string lobbyType = "public";
                string lobbyStatus = "Waiting for more players";

                _packet.Write(_response);
                _packet.Write(_msg);
                if(_lobby != null)
                {
                    if (_lobby.isRunning)
                    {
                        lobbyStatus = "Started";
                    }
                    if (_lobby.privateGame)
                    {
                        lobbyType = "private";
                    }
                    _packet.Write(_lobby.roomName);
                    _packet.Write(_lobby.roomCode);
                    _packet.Write(lobbyStatus);
                    _packet.Write(_lobby.numPlayersInLobby());
                    _packet.Write(_lobby.maxPlayers);
                    _packet.Write(_lobby.startingTokens);
                    _packet.Write(_lobby.turnLimit);
                    _packet.Write(lobbyType);
                }
                

                SendTCPData(_toClient, _packet);
            }
        }

        // sends success or fail response to client who requested to join lobby
        public static void JoinLobbyResponse(int _toClient, bool _response, string _msg, string _lobbyid)
        {
            Console.WriteLine("SEND - 'JoinLobbyResponse' to " + Server.clients[_toClient].username);
            using (Packet _packet = new Packet((int)ServerPackets.joinLobbyResponse))
            {
                _packet.Write(_response);
                _packet.Write(_msg);
                _packet.Write(_lobbyid);

                SendTCPData(_toClient, _packet);
            }
        }

        // sends new player connection data to clients in lobby
        public static void JoinLobbyResult(int _toClient, Player _player)
        {
            Console.WriteLine("SEND - 'JoinLobbyResult' to " + Server.clients[_toClient].username);
            using (Packet _packet = new Packet((int)ServerPackets.joinLobbyResult))
            {
                _packet.Write(_player.id);
                _packet.Write(Server.GetUsernameByID(_player.id));
                _packet.Write(_player.tokens);

                SendTCPData(_toClient, _packet);
            }
        }

        // notifies all clients in the lobby when a player disconnects
        public static void LeaveLobbyResult(int _toClient, Player _player)
        {
            Console.WriteLine("SEND - 'LeaveLobbyResult' to " + Server.clients[_toClient].username);
            using (Packet _packet = new Packet((int)ServerPackets.leaveLobbyResult))
            {
                _packet.Write(_player.id);
                _packet.Write(Server.GetUsernameByID(_player.id));

                SendTCPData(_toClient, _packet);
            }
        }
        #endregion




        #region Game Loop

        // sends success or fail response to client who requested to start a new poker game
        public static void StartGameResponse(int _toClient, bool _response, string _msg)
        {
            Console.WriteLine("SEND - 'StartGameResponse' to " + Server.clients[_toClient].username);
            using (Packet _packet = new Packet((int)ServerPackets.startGameResponse))
            {
                _packet.Write(_response);
                _packet.Write(_msg);

                SendTCPData(_toClient, _packet);
            }
        }

        // game starts sends data to clients
        public static void StartGameResult(int _toClient, int _tokens, Player _dealer, Player _smallBlind, Player _bigBlind)
        {
            Console.WriteLine("SEND - 'StartGameResult' to " + Server.clients[_toClient].username);
            using (Packet _packet = new Packet((int)ServerPackets.startGameResult))
            {
                _packet.Write(_tokens);
                _packet.Write(_dealer.id);
                _packet.Write(_smallBlind.id);
                _packet.Write(_bigBlind.id);

                SendTCPData(_toClient, _packet);
            }
        }

        // shares the turn of the small blind
        public static void SmallBlindTurn(int _toClient, Player _player, int _smallBlindAmount)
        {
            Console.WriteLine("SEND - 'SmallBlindTurn' to " + Server.clients[_toClient].username);
            using (Packet _packet = new Packet((int)ServerPackets.smallBlindTurn))
            {
                _packet.Write(_player.id);
                _packet.Write(Server.GetUsernameByID(_player.id));
                _packet.Write(_smallBlindAmount);

                SendTCPData(_toClient, _packet);
            }
        }

        // shares the turn of the big blind
        public static void BigBlindTurn(int _toClient, Player _player, int _bigBlindAmount)
        {
            Console.WriteLine("SEND - 'BigBlindTurn' to " + Server.clients[_toClient].username);
            using (Packet _packet = new Packet((int)ServerPackets.bigBlindTurn))
            {
                _packet.Write(_player.id);
                _packet.Write(Server.GetUsernameByID(_player.id));
                _packet.Write(_bigBlindAmount);

                SendTCPData(_toClient, _packet);
            }
        }

        // deals hand to player
        public static void DealCards(int _toClient, Card _card1, Card _card2)
        {
            Console.WriteLine("SEND - 'DealCards' to " + Server.clients[_toClient].username);
            using (Packet _packet = new Packet((int)ServerPackets.dealCards))
            {
                _packet.Write(_card1.GetValue());
                _packet.Write(_card1.NamedSuite());
                _packet.Write(_card2.GetValue());
                _packet.Write(_card2.NamedSuite());

                SendTCPData(_toClient, _packet);
            }
        }


        // notifies clients who turn is next
        public static void NewTurn(int _toClient, Player _player)
        {
            Console.WriteLine("SEND - 'NewTurn' to " + Server.clients[_toClient].username);
            using (Packet _packet = new Packet((int)ServerPackets.newTurn))
            {
                _packet.Write(_player.id);
                _packet.Write(Server.GetUsernameByID(_player.id));

                SendTCPData(_toClient, _packet);
            }
        }

        // sends success or fail response to client who requested to raise
        public static void RaiseResponse(int _toClient, bool _response, string _msg)
        {
            Console.WriteLine("SEND - 'RaiseResponse' to " + Server.clients[_toClient].username);
            using (Packet _packet = new Packet((int)ServerPackets.raiseResponse))
            {
                _packet.Write(_response);
                _packet.Write(_msg);

                SendTCPData(_toClient, _packet);
            }
        }

        // sends to all clients the result of player raising
        public static void RaiseResult(int _toClient, Player _player, int _bet)
        {
            Console.WriteLine("SEND - 'RaiseResult' to " + Server.clients[_toClient].username);
            using (Packet _packet = new Packet((int)ServerPackets.raiseResult))
            {
                _packet.Write(_player.id);
                _packet.Write(_player.tokens);
                _packet.Write(_player.bet);

                SendTCPData(_toClient, _packet);
            }
        }

        // sends success or fail response to client who requested to cal;l
        public static void CallResponse(int _toClient, bool _response, string _msg)
        {
            Console.WriteLine("SEND - 'CallResponse' to " + Server.clients[_toClient].username);
            using (Packet _packet = new Packet((int)ServerPackets.callResponse))
            {
                _packet.Write(_response);
                _packet.Write(_msg);

                SendTCPData(_toClient, _packet);
            }
        }

        // sends to all clients the result of player calling
        public static void CallResult(int _toClient, Player _player)
        {
            Console.WriteLine("SEND - 'CallResult' to " + Server.clients[_toClient].username);
            using (Packet _packet = new Packet((int)ServerPackets.callResult))
            {
                _packet.Write(_player.id);
                _packet.Write(_player.tokens);
                _packet.Write(_player.bet);

                SendTCPData(_toClient, _packet);
            }
        }

        // sends success or fail response to client who requested to fold
        public static void FoldResponse(int _toClient, bool _response, string _msg)
        {
            Console.WriteLine("SEND - 'FoldResponse' to " + Server.clients[_toClient].username);
            using (Packet _packet = new Packet((int)ServerPackets.foldResponse))
            {
                _packet.Write(_response);
                _packet.Write(_msg);

                SendTCPData(_toClient, _packet);
            }
        }

        // sends to all clients the result of player folding
        public static void FoldResult(int _toClient, Player _player)
        {
            Console.WriteLine("SEND - 'FoldResult' to " + Server.clients[_toClient].username);
            using (Packet _packet = new Packet((int)ServerPackets.foldResult))
            {
                _packet.Write(_player.id);

                SendTCPData(_toClient, _packet);
            }
        }

        // sends success or fail response to client who requested to check
        public static void CheckResponse(int _toClient, bool _response, string _msg)
        {
            Console.WriteLine("SEND - 'CheckResponse' to " + Server.clients[_toClient].username);
            using (Packet _packet = new Packet((int)ServerPackets.checkResponse))
            {
                _packet.Write(_response);
                _packet.Write(_msg);

                SendTCPData(_toClient, _packet);
            }
        }

        // sends to all clients the result of player checking
        public static void CheckResult(int _toClient, Player _player)
        {
            Console.WriteLine("SEND - 'CheckResult' to " + Server.clients[_toClient].username);
            using (Packet _packet = new Packet((int)ServerPackets.checkResult))
            {
                _packet.Write(_player.id);
                _packet.Write(_player.tokens);
                _packet.Write(_player.bet);

                SendTCPData(_toClient, _packet);
            }
        }
        #endregion




        #region Game Results
        // reveals the flop to all clients in the lobby
        public static void RevealFlop(int _toClient, Card _card1, Card _card2, Card _card3, int _pot)
        {
            Console.WriteLine("SEND - 'RevealFlop' to " + Server.clients[_toClient].username);
            using (Packet _packet = new Packet((int)ServerPackets.revealFlop))
            {
                _packet.Write(_pot);
                _packet.Write(_card1.GetValue());
                _packet.Write(_card1.NamedSuite());
                _packet.Write(_card2.GetValue());
                _packet.Write(_card2.NamedSuite());
                _packet.Write(_card3.GetValue());
                _packet.Write(_card3.NamedSuite());

                SendTCPData(_toClient, _packet);
            }
        }

        // reveals the turn to all clients in the lobby
        public static void RevealTurn(int _toClient, Card _card, int _pot)
        {
            Console.WriteLine("SEND - 'RevealTurn' to " + Server.clients[_toClient].username);
            using (Packet _packet = new Packet((int)ServerPackets.revealTurn))
            {
                _packet.Write(_pot);
                _packet.Write(_card.GetValue());
                _packet.Write(_card.NamedSuite());

                SendTCPData(_toClient, _packet);
            }
        }

        // reveals the river to all clients in the lobby
        public static void RevealRiver(int _toClient, Card _card, int _pot)
        {
            Console.WriteLine("SEND - 'RevealRiver' to " + Server.clients[_toClient].username);
            using (Packet _packet = new Packet((int)ServerPackets.revealRiver))
            {
                _packet.Write(_pot);
                _packet.Write(_card.GetValue());
                _packet.Write(_card.NamedSuite());

                SendTCPData(_toClient, _packet);
            }
        }

        // sends results of game to clients
        public static void GameResult(int _toClient, List<PlayerResultClass> _playerResultList)
        {
            Console.WriteLine("SEND - 'GameResult' to " + Server.clients[_toClient].username);
            using (Packet _packet = new Packet((int)ServerPackets.gameResult))
            {
                _packet.Write(_playerResultList.Count);
                for(int i = 0; i < _playerResultList.Count; i++)
                {
                    _packet.Write(_playerResultList[i].username);
                    _packet.Write(_playerResultList[i].card1.GetValue());
                    _packet.Write(_playerResultList[i].card1.NamedSuite());
                    _packet.Write(_playerResultList[i].card2.GetValue());
                    _packet.Write(_playerResultList[i].card2.NamedSuite());
                    _packet.Write(_playerResultList[i].win);
                    _packet.Write(_playerResultList[i].handname);
                    _packet.Write(_playerResultList[i].tokens);
                    _packet.Write(_playerResultList[i].winnings);
                }

                SendTCPData(_toClient, _packet);
            }
        }
        #endregion



        #region Friends
        // returns list of friends for a specific user
        public static void ViewFriendsResponse(int _toClient, bool _response, string _msg, List<string> _friends)
        {
            Console.WriteLine("SEND - 'ViewFriendsResponse' to " + Server.clients[_toClient].username);
            using (Packet _packet = new Packet((int)ServerPackets.viewFriendsResponse))
            {
                _packet.Write(_response);
                _packet.Write(_msg);
                _packet.Write(_friends.Count);
                foreach(string friend in _friends)
                {
                    _packet.Write(friend);
                }

                SendTCPData(_toClient, _packet);
            }
        }

        // returns response if removal from friend list was successful or not
        public static void RemoveFriendResponse(int _toClient, bool _response, string _msg)
        {
            Console.WriteLine("SEND - 'RemoveFriendResponse' to " + Server.clients[_toClient].username);
            using (Packet _packet = new Packet((int)ServerPackets.removeFriendResponse))
            {
                _packet.Write(_response);
                _packet.Write(_msg);

                SendTCPData(_toClient, _packet);
            }
        }

        // returns response if friend request was successful or not
        public static void SendFriendInviteResponse(int _toClient, bool _response, string _msg)
        {
            Console.WriteLine("SEND - 'SendFriendInviteResponse' to " + Server.clients[_toClient].username);
            using (Packet _packet = new Packet((int)ServerPackets.sendFriendInviteResponse))
            {
                _packet.Write(_response);
                _packet.Write(_msg);

                SendTCPData(_toClient, _packet);
            }
        }

        // notifies client of new friend request
        public static void SendFriendInviteResult(int _toClient, string _senderUsername)
        {
            Console.WriteLine("SEND - 'SendFriendInviteResult' to " + Server.clients[_toClient].username);
            using (Packet _packet = new Packet((int)ServerPackets.sendFriendInviteResult))
            {
                _packet.Write(_senderUsername);

                SendTCPData(_toClient, _packet);
            }
        }

        // returns all outgoing friend invites the client has sent
        public static void ViewOutgoingInvitesResponse(int _toClient, bool _response, string _msg, List<string> _invites)
        {
            Console.WriteLine("SEND - 'ViewOutgoingInvitesResponse' to " + Server.clients[_toClient].username);
            using (Packet _packet = new Packet((int)ServerPackets.viewOutgoingInvitesResponse))
            {
                _packet.Write(_response);
                _packet.Write(_msg);
                _packet.Write(_invites.Count);
                foreach (string invite in _invites)
                {
                    _packet.Write(invite);
                }

                SendTCPData(_toClient, _packet);
            }
        }

        // returns response if friend request was cancelled or not
        public static void DeleteOutgoingResponse(int _toClient, bool _response, string _msg)
        {
            Console.WriteLine("SEND - 'DeleteOutgoingResponse' to " + Server.clients[_toClient].username);
            using (Packet _packet = new Packet((int)ServerPackets.deleteOutgoingResponse))
            {
                _packet.Write(_response);
                _packet.Write(_msg);

                SendTCPData(_toClient, _packet);
            }
        }

        // returns all the pending friend invites the client has recieved
        public static void ViewPendingInvitesResponse(int _toClient, bool _response, string _msg, List<string> _invites)
        {
            Console.WriteLine("SEND - 'ViewPendingInvitesResponse' to " + Server.clients[_toClient].username);
            using (Packet _packet = new Packet((int)ServerPackets.viewPendingInvitesResponse))
            {
                _packet.Write(_response);
                _packet.Write(_msg);
                _packet.Write(_invites.Count);
                foreach (string invite in _invites)
                {
                    _packet.Write(invite);
                }

                SendTCPData(_toClient, _packet);
            }
        }

        // sends the result when client requests to accept friend invite
        public static void AcceptPendingInviteResponse(int _toClient, bool _response, string _msg)
        {
            Console.WriteLine("SEND - 'AcceptPendingInviteResponse' to " + Server.clients[_toClient].username);
            using (Packet _packet = new Packet((int)ServerPackets.acceptPendingInviteResponse))
            {
                _packet.Write(_response);
                _packet.Write(_msg);

                SendTCPData(_toClient, _packet);
            }
        }

        // sends the result when client requests to decline friend invite
        public static void DeclinePendingInviteResponse(int _toClient, bool _response, string _msg)
        {
            Console.WriteLine("SEND - 'DeclinePendingInviteResponse' to " + Server.clients[_toClient].username);
            using (Packet _packet = new Packet((int)ServerPackets.declinePendingInviteResponse))
            {
                _packet.Write(_response);
                _packet.Write(_msg);

                SendTCPData(_toClient, _packet);
            }
        }

        #endregion


        #region Messaging
        // for sending a message to another client
        public static void MessageResponse(int _toClient, bool _response, string _msg)
        {
            Console.WriteLine("SEND - 'MessageResponse' to " + Server.clients[_toClient].username);
            using (Packet _packet = new Packet((int)ServerPackets.messageResponse))
            {
                _packet.Write(_response);
                _packet.Write(_msg);

                SendTCPData(_toClient, _packet);
            }
        }

        // for reciveing a message from another client
        public static void MessageResult(int _toClient, string _msg, string _senderUsername)
        {
            Console.WriteLine("SEND - 'MessageResult' to " + Server.clients[_toClient].username);
            using (Packet _packet = new Packet((int)ServerPackets.messageResult))
            {
                _packet.Write(_senderUsername);
                _packet.Write(_msg);
                
                SendTCPData(_toClient, _packet);
            }
        }
        #endregion


        #region GameHistory
        // for retrieving game history of user.
        public static void HistoryResponse(int _toClient, bool _response, string _msg, System.Data.DataRowCollection _userHistroy)
        {
            Console.WriteLine("SEND - 'HistoryResponse' to " + Server.clients[_toClient].username);
            using (Packet _packet = new Packet((int)ServerPackets.historyResponse))
            {
                //Write standard elements to packet.
                _packet.Write(_response);
                _packet.Write(_msg);

                if(_userHistroy != null)
                {
                    _packet.Write(_userHistroy.Count);

                    //Loop through contents of list and write to packet.
                    for (int i = 0; i < _userHistroy.Count; i++)
                    {
                        _packet.Write(int.Parse(_userHistroy[i]["bets"].ToString()));
                        _packet.Write(_userHistroy[i]["table_name"].ToString());
                        _packet.Write(_userHistroy[i]["date"].ToString());
                    }
                } 
                else
                {
                    // no history to return
                    _packet.Write(0);
                }
                

                SendTCPData(_toClient, _packet);
            }
        }
        #endregion
    }
}