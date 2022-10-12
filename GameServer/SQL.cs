using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using MySql.Data.MySqlClient;

namespace GameServer
{
    class SQL
    {
        private MySqlConnection conn;
        private string connstring;

        public SQL(string _dbserver, string _dbuser, string _dbpass, string _dbname)
        {
            connstring = string.Format("server=" + _dbserver + ";database=" + _dbname + ";username=" + _dbuser + ";password=" + _dbpass + ";SSL Mode=None");
            conn = new MySqlConnection(connstring);
        }

        // checks for unique usernames in the database
        public SQLResponse UsernameExist(string username)
        {
            try
            {
                // escape strings to avoid SQL injection
                username = MySqlHelper.EscapeString(username);
                
                conn.Open();
                string query = "SELECT `Username` FROM `users` WHERE `Username` = '" + username + "';";
                MySqlDataAdapter adp = new MySqlDataAdapter(new MySqlCommand(query, conn));
                DataSet result = new DataSet();
                adp.Fill(result);
                conn.Close();

                if (result.Tables[0].Rows.Count > 0)
                {
                    return new SQLResponse(true, "Username already taken.", result);
                }
                else
                {
                    return new SQLResponse(false, "Username not found", result);
                }
            }
            catch (MySqlException ex)
            {
                conn.Close();
                Console.WriteLine("ERROR - " + ex.Message + ": " + ex.StackTrace);
                return new SQLResponse(false, "An error occured at the server.", null);
            }
        }

        // adds a new user to the database
        public SQLResponse AddUser(string username)
        {
            try
            {
                // validate if username is in use
                SQLResponse resultUsername = UsernameExist(username);
                if (resultUsername.success)
                {
                    return new SQLResponse(false, "Username already taken.", null);
                }

                // escape strings to avoid SQL injection
                username = MySqlHelper.EscapeString(username);
            
                conn.Open();
                string query = "INSERT INTO `users` (`Username`) VALUE ('" + username + "');";
                MySqlDataAdapter adp = new MySqlDataAdapter(new MySqlCommand(query, conn));
                DataSet result = new DataSet();
                adp.Fill(result);
                conn.Close();

                return new SQLResponse(true, "Success", result);
            }
            catch (MySqlException ex)
            {
                conn.Close();
                Console.WriteLine("ERROR - " + ex.Message + ": " + ex.StackTrace);
                return new SQLResponse(false, "An error occured at the server.", null);
            }
        }

        // adds lobbies to the database
        public SQLResponse AddLobby(PokerGame lobby)
        {
            try
            {
                int maxPlayers = lobby.maxPlayers;
                int minPlayers = lobby.minPlayers;
                int turnLimit = lobby.turnLimit;
                int privateGame = 0;
                // escape strings to avoid SQL injection
                string roomCode = MySqlHelper.EscapeString(lobby.roomCode);
                string roomName = MySqlHelper.EscapeString(lobby.roomName);
                if (lobby.privateGame) { privateGame = 1; }

                conn.Open();
                string query = "INSERT INTO `tables` (`tableName`, `minPlayers`, `maxPlayers`, `turnLimit`, `private`, `roomCode`) VALUE ('" + roomName + "', '" + minPlayers + "', '" + maxPlayers + "', '" + turnLimit + "', '" + privateGame + "', '" + roomCode + "');";
                MySqlDataAdapter adp = new MySqlDataAdapter(new MySqlCommand(query, conn));
                DataSet result = new DataSet();
                adp.Fill(result);
                conn.Close();

                return new SQLResponse(true, "Success", result);

            }
            catch (MySqlException ex)
            {
                conn.Close();
                Console.WriteLine("ERROR - " + ex.Message + ": " + ex.StackTrace);
                return new SQLResponse(false, "An error occured at the server.", null);
            }
        }


        // queries list of friends
        public SQLResponse ViewFriends(string username)
        {
            try
            {
                // escape strings to avoid SQL injection
                username = MySqlHelper.EscapeString(username);

                conn.Open();
                string query = "SELECT `sender`, `recipient` FROM `friends` WHERE `status` = 'accepted' AND (`sender` = '" + username + "' OR `recipient` = '" + username + "');";
                MySqlDataAdapter adp = new MySqlDataAdapter(new MySqlCommand(query, conn));
                DataSet result = new DataSet();
                adp.Fill(result);
                conn.Close();

                if (result.Tables[0].Rows.Count == 0)
                {
                    return new SQLResponse(false, "You do not have any friends. :(", null);
                }
                return new SQLResponse(true, "Success", result);
            }
            catch (MySqlException ex)
            {
                conn.Close();
                Console.WriteLine("ERROR - " + ex.Message + ": " + ex.StackTrace);
                return new SQLResponse(false, "An error occured at the server.", null);
            }
        }


        // queries list of friends
        public SQLResponse DeleteFriend(string username, string friendname)
        {
            try
            {
                // validate friend exists
                SQLResponse result = UsernameExist(friendname);
                if (!result.success)
                {
                    return new SQLResponse(false, "User '" + friendname + "' does not exist", null);
                }

                // escape strings to avoid SQL injection
                username = MySqlHelper.EscapeString(username);
                friendname = MySqlHelper.EscapeString(friendname);

                // check if friend exists
                conn.Open();
                string querySelect = "SELECT `status` FROM `friends` WHERE (`sender` = '" + friendname + "' AND `recipient` = '" + username + "') OR (`sender` = '" + username + "' AND `recipient` = '" + friendname + "');";
                MySqlDataAdapter adpSelect = new MySqlDataAdapter(new MySqlCommand(querySelect, conn));
                DataSet resultSelect = new DataSet();
                adpSelect.Fill(resultSelect);
                conn.Close();

                if (resultSelect.Tables[0].Rows.Count == 0)
                {
                    return new SQLResponse(false, "You are not friends with that user.", null);
                }

                // delete friend
                conn.Open();
                string queryDelete = "DELETE FROM `friends` WHERE (`sender` = '" + friendname + "' AND `recipient` = '" + username + "') OR (`sender` = '" + username + "' AND `recipient` = '" + friendname + "')";
                MySqlDataAdapter adpDelete = new MySqlDataAdapter(new MySqlCommand(queryDelete, conn));
                DataSet resultDelete = new DataSet();
                adpDelete.Fill(resultDelete);
                conn.Close();

                return new SQLResponse(true, "Successfully removed user '" + friendname + "' from friends list.", null);
            }
            catch (MySqlException ex)
            {
                conn.Close();
                Console.WriteLine("ERROR - " + ex.Message + ": " + ex.StackTrace);
                return new SQLResponse(false, "An error occured at the server.", null);
            }
        }


        // queries list of pending friend invites
        public SQLResponse ViewPendingInvites(string username)
        {
            try
            {
                // escape strings to avoid SQL injection
                username = MySqlHelper.EscapeString(username);

                conn.Open();
                string query = "SELECT `sender` FROM `friends` WHERE `status` = 'pending' AND `recipient` = '" + username + "';";
                MySqlDataAdapter adp = new MySqlDataAdapter(new MySqlCommand(query, conn));
                DataSet result = new DataSet();
                adp.Fill(result);
                conn.Close();

                if (result.Tables[0].Rows.Count == 0)
                {
                    return new SQLResponse(false, "You do not have any pending friend requests.", null);
                }

                return new SQLResponse(true, "Success", result);
            }
            catch (MySqlException ex)
            {
                conn.Close();
                Console.WriteLine("ERROR - " + ex.Message + ": " + ex.StackTrace);
                return new SQLResponse(false, "An error occured at the server.", null);
            }
        }



        // queries list of outgoing friend invites
        public SQLResponse ViewOutgoingInvites(string username)
        {
            try
            {
                // escape strings to avoid SQL injection
                username = MySqlHelper.EscapeString(username);

                conn.Open();
                string query = "SELECT `recipient` FROM `friends` WHERE `status` = 'pending' AND `sender` = '" + username + "';";
                MySqlDataAdapter adp = new MySqlDataAdapter(new MySqlCommand(query, conn));
                DataSet result = new DataSet();
                adp.Fill(result);
                conn.Close();

                if (result.Tables[0].Rows.Count == 0)
                {
                    return new SQLResponse(false, "You do not have any outgoing friend request.", null);
                }

                return new SQLResponse(true, "Success", result);
            }
            catch (MySqlException ex)
            {
                // mysql error
                conn.Close();
                Console.WriteLine("ERROR - " + ex.Message + ": " + ex.StackTrace);
                return new SQLResponse(false, "An error occured at the server.", null);
            }
        }


        // sends an invite to another friend
        public SQLResponse SendInvite(string username, string friendname)
        {
            try
            {
                // validate friend exists
                if (username == friendname)
                {
                    return new SQLResponse(false, "You cannot friend yourself", null);
                }

                SQLResponse result = UsernameExist(friendname);
                if (!result.success)
                {
                    return new SQLResponse(false, "User '" + friendname + "' does not exist", null);
                }

                // escape strings to avoid SQL injection
                username = MySqlHelper.EscapeString(username);
                friendname = MySqlHelper.EscapeString(friendname);

                // check if invite exists
                conn.Open();
                string querySelect = "SELECT `status` FROM `friends` WHERE (`sender` = '" + friendname + "' AND `recipient` = '" + username + "') OR (`sender` = '" + username + "' AND `recipient` = '" + friendname + "');";
                MySqlDataAdapter adpSelect = new MySqlDataAdapter(new MySqlCommand(querySelect, conn));
                DataSet resultSelect = new DataSet();
                adpSelect.Fill(resultSelect);
                conn.Close();

                if (resultSelect.Tables[0].Rows.Count > 0)
                {
                    string status = resultSelect.Tables[0].Rows[0]["status"].ToString();
                    if (status == "accepted")
                    {
                        return new SQLResponse(false, "You are already friends with this user.", null);
                    }
                    else
                    {
                        return new SQLResponse(false, "There is already a pending request with this user.", null);
                    }
                }

                // send the invite
                conn.Open();
                string queryInsert = "INSERT INTO `friends` (`sender`, `recipient`, `status`) VALUE('" + username + "', '" + friendname + "', 'pending');";
                MySqlDataAdapter adpInsert = new MySqlDataAdapter(new MySqlCommand(queryInsert, conn));
                DataSet resultInsert = new DataSet();
                adpInsert.Fill(resultInsert);
                conn.Close();

                return new SQLResponse(true, "Invite has been sent.", null);
            }
            catch (MySqlException ex)
            {
                // mysql error 
                conn.Close();
                Console.WriteLine("ERROR - " + ex.Message + ": " + ex.StackTrace);
                return new SQLResponse(false, "An error occured at the server.", null);
            }
        }


        // queries list of pending friend invites
        public SQLResponse AcceptInvite(string username, string friendname)
        {
            try
            {
                // validate friend exists
                if (username == friendname)
                {
                    return new SQLResponse(false, "You cannot friend yourself", null);
                }

                SQLResponse result = UsernameExist(friendname);
                if (!result.success)
                {
                    return new SQLResponse(false, "User '" + friendname + "' does not exist", null);
                }

                username = MySqlHelper.EscapeString(username);
                friendname = MySqlHelper.EscapeString(friendname);

                // check if invite exists
                conn.Open();
                string querySelect = "SELECT `status` FROM `friends` WHERE `status` = 'pending' AND `recipient` = '" + username + "' AND `sender` = '" + friendname + "';";
                MySqlDataAdapter adpSelect = new MySqlDataAdapter(new MySqlCommand(querySelect, conn));
                DataSet resultSelect = new DataSet();
                adpSelect.Fill(resultSelect);
                conn.Close();
                
                if (resultSelect.Tables[0].Rows.Count == 0)
                {
                    return new SQLResponse(false, "Invite not found.", null);
                }

                // accept the invite
                conn.Open();
                string queryUpdate = "UPDATE `friends` SET `status` = 'accepted' WHERE `recipient` = '" + username + "' AND `sender` = '" + friendname + "';";
                MySqlDataAdapter adpUpdate = new MySqlDataAdapter(new MySqlCommand(queryUpdate, conn));
                DataSet resultUpdate = new DataSet();
                adpUpdate.Fill(resultUpdate);
                conn.Close();

                return new SQLResponse(true, friendname + " has been added as a friend.", null);
            }
            catch (MySqlException ex)
            {
                // mysql error
                conn.Close();
                Console.WriteLine("ERROR - " + ex.Message + ": " + ex.StackTrace);
                return new SQLResponse(false, "An error occured at the server.", null);
            }            
        }


        // queries list of outgoing friend invites
        public SQLResponse FindFriend(string username, string friendname)
        {
            try
            {
                // validate friend exists
                if (username == friendname)
                {
                    return new SQLResponse(false, "You cannot find yourself", null);
                }

                SQLResponse resultUsername = UsernameExist(friendname);
                if (!resultUsername.success)
                {
                    return new SQLResponse(false, "Friend not found.", null);
                }

                username = MySqlHelper.EscapeString(username);
                friendname = MySqlHelper.EscapeString(friendname);

                // check if invite exists
                conn.Open();
                string query = "SELECT `status` FROM `friends` WHERE `status` = 'accepted' AND (`sender` = '" + friendname + "' AND `recipient` = '" + username + "') OR (`sender` = '" + username + "' AND `recipient` = '" + friendname + "');";
                MySqlDataAdapter adp = new MySqlDataAdapter(new MySqlCommand(query, conn));
                DataSet result = new DataSet();
                adp.Fill(result);
                conn.Close();


                if (result.Tables[0].Rows.Count == 0)
                {
                    return new SQLResponse(false, "Friend not found.", null);
                }
                return new SQLResponse(true, "Success", result);
            }
            catch (MySqlException ex)
            {
                // mysql error
                conn.Close();
                Console.WriteLine("ERROR - " + ex.Message + ": " + ex.StackTrace);
                return new SQLResponse(false, "An error occured at the server.", null);
            }
        }


        // Retrieves Game History/Statistics
        public SQLResponse ViewGameHistory(string username)
        {
            try
            {
                // Escape Characters to Stop SQLI
                username = MySqlHelper.EscapeString(username);

                conn.Open();

                //Retrieve Stats 
                string query = "SELECT * FROM `history` WHERE `username` = '" + username + " ' ORDER BY `date` DESC;";
                MySqlDataAdapter adp = new MySqlDataAdapter(new MySqlCommand(query, conn));
                DataSet result = new DataSet();
                adp.Fill(result);
                conn.Close();

                if (result.Tables[0].Rows.Count == 0)
                {
                    return new SQLResponse(false, "There was no game history found.", null);
                }

                return new SQLResponse(true, "Success", result);
            }
            catch (MySqlException ex)
            {
                // mysql error
                conn.Close();
                Console.WriteLine("ERROR - " + ex.Message + ": " + ex.StackTrace);
                return new SQLResponse(false, "An error occured at the server.", null);
            }
        }

        // Retrieves Game History/Statistics
        public SQLResponse AddGameRecord(string username, int tokens, string tablename)
        {
            try
            {
                // Escape Characters to Stop SQLI
                username = MySqlHelper.EscapeString(username);
                tablename = MySqlHelper.EscapeString(tablename);

                conn.Open();

                //Retrieve Stats 
                string query = "INSERT INTO `history` (`username`, `bets`, `table_name`) VALUE ('" + username + "', '" + tokens + "', '" + tablename + "');";
                MySqlDataAdapter adp = new MySqlDataAdapter(new MySqlCommand(query, conn));
                DataSet result = new DataSet();
                adp.Fill(result);
                conn.Close();

                return new SQLResponse(true, "Success added record into database", result);
            }
            catch (MySqlException ex)
            {
                // mysql error
                conn.Close();
                Console.WriteLine("ERROR - " + ex.Message + ": " + ex.StackTrace);
                return new SQLResponse(false, "An error occured at the server.", null);
            }
        }



        // Clears list of live tables. Used when server starts up
        public SQLResponse TruncateLobbies()
        {
            try
            {
                conn.Open();

                //Retrieve Stats 
                string query = "UPDATE `users` AS `u` SET `u`.`table_number` = NULL; SET FOREIGN_KEY_CHECKS = 0; TRUNCATE TABLE `tables`; SET FOREIGN_KEY_CHECKS = 1; ";
                MySqlDataAdapter adp = new MySqlDataAdapter(new MySqlCommand(query, conn));
                DataSet result = new DataSet();
                adp.Fill(result);
                conn.Close();

                Console.WriteLine("NOTICE - Cleared all tables from the database");
                return new SQLResponse(true, "Truncated all tables in the database", result);
            }
            catch (MySqlException ex)
            {
                // mysql error
                conn.Close();
                //Console.WriteLine("ERROR - " + ex.Message + ": " + ex.StackTrace);
                return new SQLResponse(false, "An error occured at the server.", null);
            }
        }


        // removes a table from the 
        public SQLResponse RemoveLobby(string roomCode)
        {
            try
            {
                // Escape Characters to Stop SQLI
                roomCode = MySqlHelper.EscapeString(roomCode);

                conn.Open();

                //Retrieve Stats 
                string query = "UPDATE `users` AS `u`, `tables` AS `t` SET `u`.`table_number` = NULL WHERE `u`.`table_number` = `t`.`tableNumber` AND `t`.`roomCode` = '" + roomCode + "'; DELETE FROM `tables` WHERE `roomCode` = '" + roomCode+ "';";
                MySqlDataAdapter adp = new MySqlDataAdapter(new MySqlCommand(query, conn));
                DataSet result = new DataSet();
                adp.Fill(result);
                conn.Close();

                return new SQLResponse(true, "Removed table from the database", result);
            }
            catch (MySqlException ex)
            {
                // mysql error
                conn.Close();
                Console.WriteLine("ERROR - " + ex.Message + ": " + ex.StackTrace);
                return new SQLResponse(false, "An error occured at the server.", null);
            }
        }



        // removes a table from the 
        public SQLResponse GetAllLobbiesData()
        {
            try
            {
                conn.Open();

                //Retrieve Stats 
                string query = "SELECT `roomCode`, `tableName`, `private` FROM `tables`;";
                MySqlDataAdapter adp = new MySqlDataAdapter(new MySqlCommand(query, conn));
                DataSet result = new DataSet();
                adp.Fill(result);
                conn.Close();

                return new SQLResponse(true, "Retrieved Data from Tables", result);
            }
            catch (MySqlException ex)
            {
                // mysql error
                conn.Close();
                Console.WriteLine("ERROR - " + ex.Message + ": " + ex.StackTrace);
                return new SQLResponse(false, "An error occured at the server.", null);
            }
        }


        // assign player to lobby
        public SQLResponse AddPlayerToLobby(string username, string roomCode)
        {
            try
            {
                // Escape Characters to Stop SQLI
                username = MySqlHelper.EscapeString(username);
                roomCode = MySqlHelper.EscapeString(roomCode);

                conn.Open();

                // get lobby ID from database
                string querySelect = "SELECT `tableNumber` FROM `tables` WHERE `roomCode` = '" + roomCode + "'";
                MySqlDataAdapter adpSelect = new MySqlDataAdapter(new MySqlCommand(querySelect, conn));
                DataSet resultSelect = new DataSet();
                adpSelect.Fill(resultSelect);
                conn.Close();

                if(resultSelect.Tables[0].Rows.Count == 0)
                {
                    Console.WriteLine("ERROR - Could not add user " + username + " to lobby " + roomCode + ". Lobby not found.");
                    return new SQLResponse(false, "Table not found", null);
                }

                string lobbyid = resultSelect.Tables[0].Rows[0]["tableNumber"].ToString();

                conn.Open();

                // assign player a table
                string query = "UPDATE `users` SET `table_number` = '" + lobbyid + "' WHERE `username` = '" + username + "';";
                MySqlDataAdapter adp = new MySqlDataAdapter(new MySqlCommand(query, conn));
                DataSet result = new DataSet();
                adp.Fill(result);
                conn.Close();

                return new SQLResponse(true, "Added player to lobby", result);
            }
            catch (MySqlException ex)
            {
                // mysql error
                conn.Close();
                Console.WriteLine("ERROR - " + ex.Message + ": " + ex.StackTrace);
                return new SQLResponse(false, "An error occured at the server.", null);
            }
        }


        // removes player from lobby 
        public SQLResponse RemovePlayerFromLobby(string username)
        {
            try
            {
                // Escape Characters to Stop SQLI
                username = MySqlHelper.EscapeString(username);

                conn.Open();

                // set player table to null
                string query = "UPDATE `users` SET `table_number` = NULL WHERE `username` = '" + username + "';";
                MySqlDataAdapter adp = new MySqlDataAdapter(new MySqlCommand(query, conn));
                DataSet result = new DataSet();
                adp.Fill(result);
                conn.Close();

                return new SQLResponse(true, "Removed player from lobby", result);
            }
            catch (MySqlException ex)
            {
                // mysql error
                conn.Close();
                Console.WriteLine("ERROR - " + ex.Message + ": " + ex.StackTrace);
                return new SQLResponse(false, "An error occured at the server.", null);
            }
        }
    }




    // SQL Response Class for returning errors and successful results
    class SQLResponse
    {
        public bool success;                // if query was a success or not
        public string message;              // message from result, typically error message caught in try catch
        public DataSet data;                // the returned data from a successful query, null if unsuccessful

        public SQLResponse(bool _success = false, string _message = "", DataSet _data = null)
        {
            success = _success;
            message = _message;
            data = _data;
        }
    }

}
