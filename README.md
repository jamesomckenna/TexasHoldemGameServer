# TexasHoldemGameServer
As a capstone project for our university degree, I helped develop a Texas Holdem poker smartphone game to help reduce the amount of money gamblers spend on poker games, by providing a money free alternative. 

This is the game server I developed to connect the smartphones and host LAN games for the smartphones across several lobbies. 
The game featured the ability to create, customize and join lobbies, send and receive friend requests, send messages to other players and log game histories to be viewed by the player.

Please note that running this application will have limited functionality as I currently cannot easily provide the mobile app it was designed to operate with.
Whilst I do provide instructions to set up and use the server, I encourage you to inspect the code and read the comments at your leisure to get a proper grasp at the app's full functionality.

# Requirements
To start the server, you need: 
- A PC running Windows OS
- .NET Core 3.1 SDK installed. The SDK can be installed from here: https://dotnet.microsoft.com/en-us/download/dotnet/3.1
- A MySQL server running on the local machine
- Username and password of the local MySQL server

# How to set up
- Clone the repo using: `git clone https://github.com/jamesomckenna/TexasHoldemgameServer.git`
- Copy the contents of the file `TexasHoldemGameServer/DatabaseSetup/GameServerDB.sql` and run it on the local MySQL server
- Run the application file `TexasHoldemGameServer/GameServer/bin/Debug/netcoreapp3.1/GameServer.exe` to start the server
- Enter the MySQL server username and password into the prompts 
- And congrats the server is running!
