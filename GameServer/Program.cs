using System;
using System.Data;
using System.Threading;

namespace GameServer
{
    class Program
    {
        /********** customisable variables **********/
        private static int maxPlayer = 500;
        private static int maxLobbies = 100;
        private static int port = 26950;
        public static string dbserver = "127.0.0.1"; 
        public static string dbuser = "root";
        public static string dbpass = "";
        public static string dbname = "texasholdem";
        /********************************************/

        private static bool isRunning = false;

        // server start function
        static void Main(string[] args)
        {
            Console.Title = "Texas Hold'em Game Server";
            isRunning = true;

            Thread mainThread = new Thread(new ThreadStart(MainThread));
            mainThread.Start();

            Server.Start(port, maxPlayer, maxLobbies, dbserver, dbuser, dbpass, dbname);
        }

        private static void MainThread()
        {
            Console.WriteLine("NOTICE - Main thread started. Running at "+Constants.TICKS_PER_SEC+" ticks per second.");
            DateTime _nextLoop = DateTime.Now;

            DateTime _nextLobbyUpdate = DateTime.Now;
            DateTime _nextLobbyTimer = DateTime.Now;

            while (isRunning)
            {
                while (_nextLoop < DateTime.Now)
                {
                    // update lobby data from SQL every minute
                    if (_nextLobbyUpdate < DateTime.Now)
                    {
                        Server.updateServerLobbies();
                        _nextLobbyUpdate = _nextLobbyUpdate.AddMilliseconds(Constants.MS_PER_TICK * 30);
                    }

                    // increment lobby timers every second
                    if (_nextLobbyTimer < DateTime.Now)
                    {
                        Server.updateLobbyTimers();
                        _nextLobbyTimer = _nextLobbyTimer.AddMilliseconds(Constants.MS_PER_TICK);
                    }

                    // process queue of functions on the main thread
                    GameLogic.Update();

                    _nextLoop = _nextLoop.AddMilliseconds(Constants.MS_PER_TICK);
                    
                    if (_nextLoop > DateTime.Now)
                    {
                        Thread.Sleep(_nextLoop - DateTime.Now);
                    }
                }
            }
        }
    }
}
