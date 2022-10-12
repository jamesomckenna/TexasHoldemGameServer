# TexasHoldemGameServer
LAN server for a Texas Hold'em mobile game. 



Please note that running this application will have limited functionality as I currently cannot easily provide the mobile app it was designed to work with.
Whilst I do provide instructions to set up and use the server, I encourage you to inspect the code and read the comments at your leisure to get a proper grasp at the app's full functionality.

# Requirements
To start the server, you need: 
- A PC running Windows OS that supports the .NET Core 3.1 framework
- A MySQL server running on the local machine
- Username and password of the local MySQL server

# How to set up
- Clone the repo using: `git clone https://github.com/SPOBS/TexasHoldemgameServer.git`
- Copy the contents of the file `TexasHoldemGameServer/DatabaseSetup/GameServerDB.sql` and run it on the local MySQL server
- Run the application file `TexasHoldemGameServer/GameServer/bin/Debug/netcoreapp3.1/GameServer.exe` to start the server
- Enter the MySQL server username and password into the prompts 
- And congrats the server is running!
