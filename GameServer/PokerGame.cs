using System;
using System.Collections.Generic;
using System.Text;

namespace GameServer
{

    public enum Stages
    {
        Preflop = 0,
        Flop,
        Turn,
        River
    }
    class PokerGame
    {
        // game settings
        public string roomName;                                                             // lobby's name
        public string roomCode;                                                             // lobby's unique identifier
        public bool isRunning;                                                              // state to check is the game is in progress or not
        public int gameCounter;                                                             // number of games played ion this lobby
        public int startingTokens;                                                          // number tokens each player starts with when they join the lobby
        public int turnLimit;                                                               // time limit each  player has to make thier turn
        public int maxPlayers;                                                              // maximum amount of players allowed in lobby. Value shuld be between 2 and 5
        public int minPlayers;                                                              // minimum amount of players allowed in to start a game. Value shuld be between 2 and 5
        public bool privateGame;                                                            // if private game, value is set to true, if public value is false
        public Dictionary<int, Player> playersInLobby = new Dictionary<int, Player>();      // number of players within in the lobby
        public List<Player> playersInGame = new List<Player>();                             // list of players currently in game

        // current game values 
        public int pot;                                                                     // total pot for 
        public int highestBet;                                                              // the highest bet in the round
        public int smallBlindAmount;                                                        // the amount to bet as the Small Blind
        public int bigBlindAmount;                                                          // the amount to bet as the Big Blind
        public int dealerId;                                                                // player ID of the dealer
        public int smallBlindId;                                                            // player Small Blind of the dealer
        public int bigBlindId;                                                              // player Big Blind of the dealer
        private int currentTurn;                                                            // player ID of whose turn is currently is
        private int currentTurnTimer;                                                       // tracks the time remaining of the current players turn
        private Stages currentStage;                                                        // current stage of the game. See enum Stages

        public Card[] flop = new Card[5];                                                   // cards of the flop
        private Deck deck = new Deck();                                                     // shuffled deck of cards


        public PokerGame(string _roomname, string _roomcode, int _startingTokens, int _maxPlayers, int _minPlayers, int _turnLimit, bool _privateGame)
        {
            gameCounter = 0;
            isRunning = false;

            roomName = _roomname;
            roomCode = _roomcode;
            startingTokens = _startingTokens;
            turnLimit = _turnLimit;
            maxPlayers = _maxPlayers;
            minPlayers = _minPlayers;
            privateGame = _privateGame;
           
            // initialises open slots for lobbies 
            for (int i = 1; i <= maxPlayers; i++)
            {
                playersInLobby.Add(i, null);
            }
        }

        public void StartGame(int _clientId)
        {
            if (isRunning)
            {
                // if game is already running, return message
                ServerSend.StartGameResponse(_clientId, false, "Game is already running.");
            }
            else
            {
                playersInGame.Clear();
                // add players into the game
                // different from player in lobby

                foreach (Player _player in playersInLobby.Values)
                {
                    if (_player != null)
                    {
                        playersInGame.Add(_player);
                    }
                }

                // check fopr valid group size
                if (playersInGame.Count >= minPlayers)
                {
                    isRunning = true;
                    ServerSend.StartGameResponse(_clientId, true, "Game is starting.");

                    // initialise game data
                    gameCounter++;
                    pot = 0;
                    highestBet = 0;
                    smallBlindAmount = 20;
                    bigBlindAmount = smallBlindAmount * 2;
                    currentStage = Stages.Preflop;

                    // set dealers to increment on the new round
                    dealerId = gameCounter - 1;
                    if (dealerId >= playersInGame.Count)
                    {
                        dealerId = 0;
                    }

                    smallBlindId = dealerId + 1;
                    if (smallBlindId >= playersInGame.Count)
                    {
                        smallBlindId = 0;
                    }

                    bigBlindId = smallBlindId + 1;
                    if (bigBlindId >= playersInGame.Count)
                    {
                        bigBlindId = 0;
                    }

                    // create random deck of cards
                    deck.ShuffleDeck();

                    // send each client thier tokens and blind roles
                    foreach (Player p in playersInGame)
                    {
                        // initialise player data and set their hand
                        p.InitNewGame();
                        p.DealCards(deck.DrawCard(), deck.DrawCard());
                        ServerSend.StartGameResult(p.id, p.tokens, playersInGame[dealerId], playersInGame[smallBlindId], playersInGame[bigBlindId]);
                    }

                    // set cards for flop
                    for (int i = 0; i < flop.Length; i++)
                    {
                        flop[i] = deck.DrawCard();
                    }

                    currentTurn = smallBlindId;
                    currentTurnTimer = 0;

                    foreach (Player p in playersInLobby.Values)
                    {
                        if (p != null)
                        {
                            ServerSend.SmallBlindTurn(p.id, playersInGame[smallBlindId], smallBlindAmount);
                        }
                    }
                }
                else
                {
                    ServerSend.StartGameResponse(_clientId, false, "Not enough players.");
                }
            }
        }

        // Raise a players bet
        public void Raise(int _fromClientId, int _bet)
        {
            if (isRunning)
            {
                // check if player requesting raise exists
                int playerId = GetGameIDFromClientID(_fromClientId);
                if (playerId > -1)
                {
                    // check that the raise is a legal move
                    if (currentTurn == playerId)
                    {
                        if (!IsBlindRound(playerId))
                        {
                            if (playersInGame[playerId].tokens >= _bet && _bet > 0)
                            {
                                if (playersInGame[playerId].bet + _bet > highestBet)
                                {
                                    // rasie the players bet 
                                    playersInGame[playerId].tokens -= _bet;
                                    playersInGame[playerId].bet += _bet;

                                    //set bet as the highest bet to mach in the game
                                    highestBet = playersInGame[playerId].bet;
                                    SetAllPlayersToUnplayed();
                                    playersInGame[playerId].played = true;

                                    // return successful result and start next turn
                                    ServerSend.RaiseResponse(_fromClientId, true, "Success");                                 
                                    foreach (Player p in playersInLobby.Values)
                                    {
                                        if (p != null)
                                        {
                                            ServerSend.RaiseResult(p.id, playersInGame[playerId], _bet);
                                        }
                                    }
                                    NextTurn();
                                }
                                else
                                {
                                    ServerSend.RaiseResponse(_fromClientId, false, "You have not bet enough to raise.");
                                }
                            }
                            else
                            {
                                ServerSend.RaiseResponse(_fromClientId, false, "Invalid bet quantity.");
                            }
                        } 
                        else
                        {
                            if(playerId == bigBlindId)
                            {
                                if (_bet >= bigBlindAmount && _bet >= highestBet)
                                {
                                    // set the players bet 
                                    playersInGame[playerId].tokens -= _bet;
                                    playersInGame[playerId].bet = _bet;

                                    // set bet as the highest bet to mach in the game
                                    highestBet = playersInGame[playerId].bet;
                                    SetAllPlayersToUnplayed();
                                    playersInGame[playerId].played = true;

                                    // return successful result and deal cards
                                    ServerSend.RaiseResponse(_fromClientId, true, "Success");
                                    foreach (Player p in playersInLobby.Values)
                                    {
                                        if (p != null)
                                        {
                                            ServerSend.RaiseResult(p.id, playersInGame[playerId], _bet);
                                        }
                                    }
                                    DealCards();
                                }
                                else
                                {
                                    ServerSend.RaiseResponse(_fromClientId, false, "You have not bet enough to match the big blind.");
                                }
                            } 
                            else if(playerId == smallBlindId)
                            {
                                if(_bet >= smallBlindAmount)
                                {
                                    // set the players bet 
                                    playersInGame[playerId].tokens -= _bet;
                                    playersInGame[playerId].bet = _bet;

                                    //set bet as the highest bet to mach in the game
                                    highestBet = playersInGame[playerId].bet;
                                    playersInGame[playerId].played = true;

                                    // return successful result and start the big blind
                                    ServerSend.RaiseResponse(_fromClientId, true, "Success");
                                    
                                    currentTurn = bigBlindId;
                                    currentTurnTimer = 0;

                                    if (_bet > bigBlindAmount)
                                    {
                                        bigBlindAmount = _bet;
                                    }
                                    
                                    foreach (Player p in playersInLobby.Values)
                                    {
                                        if (p != null)
                                        {
                                            ServerSend.RaiseResult(p.id, playersInGame[playerId], _bet);
                                            ServerSend.BigBlindTurn(p.id, playersInGame[currentTurn], bigBlindAmount);
                                        }
                                    }
                                } 
                                else
                                {
                                    ServerSend.RaiseResponse(_fromClientId, false, "You have not bet enough to match the small blind.");
                                }
                            }
                            else
                            {
                                ServerSend.RaiseResponse(_fromClientId, false, "An error has occured, this should not happen.");
                            }
                        }
                    }
                    else
                    {
                        ServerSend.RaiseResponse(_fromClientId, false, "It is not your turn.");
                    }
                }
                else
                {
                    ServerSend.RaiseResponse(_fromClientId, false, "You are not apart of this game.");
                }
            }
            else
            {
                ServerSend.RaiseResponse(_fromClientId, false, "Game has not started.");
            }

        }

        // match the highest bet
        public void Call(int _fromClientId)
        {
            if (isRunning)
            {
                // check if player requesting call exists
                int playerId = GetGameIDFromClientID(_fromClientId);
                if (playerId > -1)
                {
                    // check that the call is a legal move
                    if (currentTurn == playerId)
                    {
                        if (!IsBlindRound(playerId))
                        {
                            if (playersInGame[playerId].bet < highestBet)
                            {
                                int amountToCall = highestBet - playersInGame[playerId].bet;
                                int bet;

                                // if player cannot match the highest, go all in
                                if (amountToCall > playersInGame[playerId].tokens)
                                {
                                    bet = playersInGame[playerId].tokens;
                                }
                                else
                                {
                                    bet = amountToCall;
                                }

                                // set player tokens and bet
                                playersInGame[playerId].tokens -= bet;
                                playersInGame[playerId].bet += bet;
                                playersInGame[playerId].played = true;

                                // return successful result and start next turn
                                ServerSend.CallResponse(_fromClientId, true, "Success");
                                foreach (Player p in playersInLobby.Values)
                                {
                                    if (p != null)
                                    {
                                        ServerSend.CallResult(p.id, playersInGame[playerId]);
                                    }
                                }
                                NextTurn();
                            }
                            else
                            {
                                ServerSend.CallResponse(_fromClientId, false, "You already have matched the highest bet.");
                            }
                        }
                        else
                        {
                            ServerSend.CallResponse(_fromClientId, false, "You cannot call as the blind.");
                        }
                    }
                    else
                    {
                        ServerSend.CallResponse(_fromClientId, false, "It is not your turn.");
                    }
                }
                else
                {
                    ServerSend.CallResponse(_fromClientId, false, "You are not apart of this game.");
                }
            }
            else
            {
                ServerSend.CallResponse(_fromClientId, false, "Game has not started.");
            }

        }

        // forfiet the round
        public void Fold(int _fromClientId)
        {
            if (isRunning)
            {
                // check if player requesting to fold exists
                int playerId = GetGameIDFromClientID(_fromClientId);
                if (playerId > -1)
                {
                    // check that the fold is a legal move
                    if (currentTurn == playerId)
                    {
                        if (!IsBlindRound(playerId))
                        {
                            // return successful result and start next turn
                            playersInGame[playerId].folded = true;
                            ServerSend.FoldResponse(_fromClientId, true, "Success");                          
                            foreach (Player p in playersInLobby.Values)
                            {
                                if (p != null)
                                {
                                    ServerSend.FoldResult(p.id, playersInGame[playerId]);
                                }
                            }
                            NextTurn();
                        }
                        else
                        {
                            ServerSend.FoldResponse(_fromClientId, false, "You cannot fold as the blind.");
                        }
                    }
                    else
                    {
                        ServerSend.FoldResponse(_fromClientId, false, "It is not your turn.");
                    }
                }
                else
                {
                    ServerSend.FoldResponse(_fromClientId, false, "You are not apart of this game.");
                }
            }
            else
            {
                ServerSend.FoldResponse(_fromClientId, false, "Game has not started.");
            }
        }

        // make a bet of 0 tokens
        public void Check(int _fromClientId)
        {
            if (isRunning)
            {
                // check if player requesting to check exists
                int playerId = GetGameIDFromClientID(_fromClientId);
                if (playerId > -1)
                {
                    // check that the check is a legal move
                    if (currentTurn == playerId)
                    {
                        if (highestBet == 0)
                        {
                            if (currentStage != Stages.Preflop)
                            {
                                // return successful result and start next turn
                                playersInGame[playerId].played = true;
                                ServerSend.CheckResponse(_fromClientId, true, "Success");
                                foreach (Player p in playersInLobby.Values)
                                {
                                    if (p != null)
                                    {
                                        ServerSend.CheckResult(p.id, playersInGame[playerId]);
                                    }
                                }
                                NextTurn();
                            }
                            else
                            {
                                ServerSend.CheckResponse(_fromClientId, false, "You cannot check during the pre-flop.");
                            }
                        }
                        else
                        {
                            ServerSend.CheckResponse(_fromClientId, false, "You cannot check if a bet was been placed by another player.");
                        }
                    }
                    else
                    {
                        ServerSend.CheckResponse(_fromClientId, false, "It is not your turn.");
                    }
                }
                else
                {
                    ServerSend.CheckResponse(_fromClientId, false, "You are not apart of this game.");
                }
            }
            else
            {
                ServerSend.CheckResponse(_fromClientId, false, "Game has not started.");
            }
        }

        // loop through each player starting from the current turn
        private void NextTurn()
        {
            bool _turnFound = false;
            int _activePlayers = 0;

            // check the number of active players (eg, player who have not folded or gone all in)
            foreach (Player p in playersInGame)
            {
                if(!p.folded)
                {
                    _activePlayers++;
                }
            }

            // if only one player remains, the game ends and they are declared the winner
            if (_activePlayers > 1)
            {
                // loop through all the available players to find the next turn
                // start loop from id of players turn
                int id = currentTurn + 1;
                if (id >= playersInGame.Count){id = 0;}

                do
                {
                    // check if folded or has already played
                    if (!playersInGame[id].folded && !playersInGame[id].played && playersInGame[id].tokens > 0)
                    {
                        currentTurn = id;
                        currentTurnTimer = 0;
                        foreach (Player p in playersInLobby.Values)
                        {
                            if (p != null)
                            {
                                ServerSend.NewTurn(p.id, playersInGame[currentTurn]);
                            }
                        }
                        _turnFound = true;
                        break;
                    }

                    // check the next player
                    id++;
                    // if i exceeds player count, i has reached the end of the list searched the players at the start of the list
                    if (id >= playersInGame.Count)
                    {
                        id = 0;
                    }

                } while (id != currentTurn);

                if (!_turnFound)
                {
                    // advance game
                    AddToPot();

                    switch (currentStage)
                    {
                        case (Stages.Preflop):
                            foreach (Player p in playersInLobby.Values)
                            {
                                if (p != null)
                                {
                                    ServerSend.RevealFlop(p.id, flop[0], flop[1], flop[2], pot);
                                }
                            }
                            currentStage = Stages.Flop;
                            SetAllPlayersToUnplayed();
                            NextTurn();
                            break;
                        case (Stages.Flop):
                            foreach (Player p in playersInLobby.Values)
                            {
                                if (p != null)
                                {
                                    ServerSend.RevealTurn(p.id, flop[3], pot);
                                }
                            }
                            currentStage = Stages.Turn;
                            SetAllPlayersToUnplayed();
                            NextTurn();
                            break;
                        case (Stages.Turn):
                            foreach (Player p in playersInLobby.Values)
                            {
                                if (p != null)
                                {
                                    ServerSend.RevealRiver(p.id, flop[4], pot);
                                }
                            }
                            currentStage = Stages.River;
                            SetAllPlayersToUnplayed();
                            NextTurn();
                            break;
                        case (Stages.River):
                            // the last stage is complete, end the game
                            EndGame();
                            break;
                    }
                }
            }
            else
            {
                // not enough players exist in the lobby, the remaining player wins
                EndGame();
            }
        }


        // returns game player id from server client id
        private int GetGameIDFromClientID(int _playerId)
        {
            for (int i = 0; i < playersInGame.Count; i++)
            {
                if (playersInGame[i].id == _playerId)
                {
                    return i;
                }
            }
            return -1;
        }

        // returns lobby player id from server client id
        private int GetLobbyIDFromClientID(int _playerId)
        {
            for (int i = 1; i <= maxPlayers; i++)
            {
                if(playersInLobby[i] != null)
                {
                    if (playersInLobby[i].id == _playerId)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

        // checks if the blinds are playing
        private bool IsBlindRound(int _playerId)
        {
            if (
                (_playerId == smallBlindId || _playerId == bigBlindId)
                && currentStage == Stages.Preflop
                && playersInGame[_playerId].bet == 0
                )
            {
                return true;
            }
            return false;
        }

        // notifies the server that the all players have not yet had their turn
        private void SetAllPlayersToUnplayed()
        {
            for (int i = 0; i < playersInGame.Count; i++)
            {
                playersInGame[i].played = false;
            }
        }


        // deal cards to players and the flop
        public void DealCards()
        {
            // send each client their hand
            foreach (Player p in playersInGame)
            {
                ServerSend.DealCards(p.id, p.card1, p.card2);
            }

            // initiate first turn                  
            NextTurn();
        }

        // take all players bets and transfer them into the pot
        public void AddToPot()
        {
            for (int i = 0; i < playersInGame.Count; i++)
            {
                pot += playersInGame[i].bet;
                playersInGame[i].bet = 0;
                highestBet = 0;
            }
        }

        // ends the game and declares the winner
        #region EndGame
        private void EndGame()
        {
            isRunning = false;

            // Check for any remaining bets if the game was ended earlier than intended.
            AddToPot();

            if (playersInGame.Count > 0)
            {
                //Initialising Lists
                List<Player> PlayersSeen = new List<Player>();
                List<Player> EqualHands = new List<Player>();
                List<Player> Winners = new List<Player>();

                //Initialising Others
                int HighHandVal = 0;
                int InitBestHand;
                int SplitPot;


                //Generate a list of remaining players and a count value of these players.
                foreach (Player p in playersInGame)
                {
                    if (p.folded == false)
                    {
                        PlayersSeen.Add(p);
                    }
                }

                //Perform for all players still in the hand.
                for (int i = 0; i < PlayersSeen.Count; i++)
                {
                    //Calculate the best five cards of the player and return an integer value representation of the value of those cards as well as their best hand in an array form.
                    PlayersSeen[i].BestHand = EvaluateHand(PlayersSeen[i]);

                    //Find the highest valued hand from the round.
                    if (HighHandVal < PlayersSeen[i].BestHand.Value)
                    {
                        HighHandVal = PlayersSeen[i].BestHand.Value;
                    }
                }

                //Create a list of the players who share the hand value of the maximum value and a count of these players.
                for (int i = 0; i < PlayersSeen.Count; i++)
                {
                    if (PlayersSeen[i].BestHand.Value == HighHandVal)
                    {
                        EqualHands.Add(PlayersSeen[i]);
                    }
                }

                //Create a list of hands which are the highest type from the current hand and compare for high cards and filter accordingly.
                if (EqualHands.Count > 1) //If more than one equal hand.
                {
                    if (EqualHands.Count > 2)
                    {
                        InitBestHand = CompareHand(EqualHands[0].BestHand.Hand, EqualHands[1].BestHand.Hand);
                        Winners.Add(EqualHands[InitBestHand]);
                        for (int i = 2; i < EqualHands.Count; i++)
                        {
                            if (CompareHand(Winners[0].BestHand.Hand, EqualHands[i].BestHand.Hand) == 0)//Old hand is better
                            {
                                //Do nothing - check next hand.
                            }
                            else if (CompareHand(Winners[0].BestHand.Hand, EqualHands[i].BestHand.Hand) == 1) //New hand is better
                            {
                                Winners.Clear();
                                Winners.Add(EqualHands[i]);
                            }
                            else if (CompareHand(Winners[0].BestHand.Hand, EqualHands[i].BestHand.Hand) == 2) //Hands are equal
                            {
                                Winners.Add(EqualHands[i]);
                            }
                        }
                    }
                    else if (EqualHands.Count == 2) //Only two hands so we can just find the better one of the two.
                    {
                        InitBestHand = CompareHand(EqualHands[0].BestHand.Hand, EqualHands[1].BestHand.Hand);
                        if (InitBestHand == 2)
                        {
                            Winners.Add(EqualHands[0]);
                            Winners.Add(EqualHands[1]);
                        }
                        else
                        {
                            Winners.Add(EqualHands[InitBestHand]);
                        }
                    }
                }
                else
                {
                    //There is only one equal hand, therefore this hand wins.
                    Winners.Add(EqualHands[0]);
                }

                SplitPot = pot / Winners.Count;

                //Calculate pot sizes for winners, 
                foreach (Player p in Winners)
                {
                    //Add pot to winners balance.
                    int id = GetGameIDFromClientID(p.id);
                    playersInGame[id].tokens += SplitPot;
                }


                List<PlayerResultClass> PlayerResultList = new List<PlayerResultClass>();
                foreach (Player p in PlayersSeen)
                {
                    PlayerResultClass playerResult = new PlayerResultClass(Server.clients[p.id].username, p.card1, p.card2, false, p.BestHand.Value, p.tokens, 0);
                    for (int i = 0; i < Winners.Count; i++)
                    {
                        if(p.id == Winners[i].id)
                        {
                            playerResult.winnings = SplitPot;
                            playerResult.win = true;
                        }
                    }
                    PlayerResultList.Add(playerResult);
                }
                 

                //Send data per winner - packet can maybe use count value to set up size limit on client side correctly. - NOTE
                foreach (Player client in playersInLobby.Values)
                {
                    if (client != null)
                    {                       
                        ServerSend.GameResult(client.id, PlayerResultList);
                    }
                }
            }
        }

        #endregion

        #region Evaluate & Compare Hand
        public BestHandClass EvaluateHand(Player _player)
        {
            //Initialise Variables
            Card[] AvailCards = new Card[7];
            BestHandClass BestHand = new BestHandClass(null, 0);
            int FiveCounter = 0;

            //Set players cards
            AvailCards[0] = _player.card1;
            AvailCards[1] = _player.card2;
            AvailCards[2] = flop[0];
            AvailCards[3] = flop[1];
            AvailCards[4] = flop[2];
            AvailCards[5] = flop[3];
            AvailCards[6] = flop[4];

            //Body
            //Combination Generation
            for (int i = 0; i < AvailCards.Length; i++)
            {
                for (int j = i+1; j < AvailCards.Length; j++)
                {
                    //Initialise For New Combination - Internal Loop Initialisation
                    Card[] FiveCards = new Card[5];
                    Card[] FiveCardsSorted = new Card[5];
                    FiveCounter = 0;

                    //Generate A New Combination
                    for (int k = 0; k < AvailCards.Length; k++) //Loop For 7
                    {
                        if (k != i && k != j) //Where Specific Combination Values Are NOT Met.
                        {
                            FiveCards[FiveCounter] = AvailCards[k];
                            FiveCounter += 1;
                        }
                    }

                    //Check Combination For Hand Types
                    if(FiveCards[0] != null)
                    {
                        FiveCardsSorted = HighSort(FiveCards);

                        //Straight Flush
                        if (StraightFlush(FiveCards) == true)
                        {
                            FiveCardsSorted = HighSort(FiveCards); //Straight Sorted in Descending Order
                            if (BestHand.Value < 8) //Check if is best hand so far.
                            {
                                BestHand.Hand = FiveCardsSorted;
                                BestHand.Value = 8; //Val 8 = StraightFlush
                            }
                            else if (BestHand.Value == 8 && CompareHand(BestHand.Hand, FiveCardsSorted) == 1) //Check If Higher StraightFlush
                            {
                                BestHand.Hand = FiveCardsSorted;
                                BestHand.Value = 8; //Val 8 = StraightFlush
                            }
                        }
                        //Four of A Kind
                        else if (FourKind(FiveCards).success == true)
                        {
                            FourKindClass FourKindResponse = FourKind(FiveCards);
                            FiveCardsSorted = FourKindSort(FiveCards, FourKindResponse); //Sort by four of a kind sort/order.
                            if (BestHand.Value < 7) //Check if is best hand so far.
                            {
                                BestHand.Hand = FiveCardsSorted;
                                BestHand.Value = 7; //Val 7 = Four of a Kind
                            }
                            else if (BestHand.Value == 7 && CompareHand(BestHand.Hand, FiveCardsSorted) == 1) //Check if higher four of a kind.
                            {
                                BestHand.Hand = FiveCardsSorted;
                                BestHand.Value = 7; //Val 7 = Four of a Kind
                            }
                        }

                        //Full House
                        else if (FullHouse(FiveCards).success == true)
                        {
                            FullHouseClass FullHouseResponse = FullHouse(FiveCards);
                            FiveCardsSorted = FullHouseSort(FiveCards, FullHouseResponse); //Sort by Full House order.
                            if (BestHand.Value < 6)
                            {
                                BestHand.Hand = FiveCardsSorted;
                                BestHand.Value = 6; //Val 7 = Full House
                            }
                            else if (BestHand.Value == 6 && CompareHand(BestHand.Hand, FiveCards) == 1)
                            {
                                BestHand.Hand = FiveCardsSorted;
                                BestHand.Value = 6; //Val 7 = Full House
                            }
                        }

                        //Flush
                        else if (Flush(FiveCards) == true)
                        {
                            FiveCardsSorted = HighSort(FiveCards); //Straight Sorted in Descending Order
                            if (BestHand.Value < 5) //Check if is best hand so far.
                            {
                                BestHand.Hand = FiveCardsSorted;
                                BestHand.Value = 5; //Val 5 = Flush
                            }
                            else if (BestHand.Value == 5 && CompareHand(BestHand.Hand, FiveCardsSorted) == 1) //Check If Higher Flush
                            {
                                BestHand.Hand = FiveCardsSorted;
                                BestHand.Value = 5; //Val 4 = Flush
                            }
                        }

                        //Straight
                        else if (Straight(FiveCards) == true)
                        {
                            FiveCardsSorted = HighSort(FiveCards); //Straight Sorted in Descending Order
                            if (BestHand.Value < 4) //Check if is best hand so far.
                            {
                                BestHand.Hand = FiveCardsSorted;
                                BestHand.Value = 4; //Val 4 = Straight
                            }
                            else if (BestHand.Value == 4 && CompareHand(BestHand.Hand, FiveCardsSorted) == 1) //Check If Higher Straight
                            {
                                BestHand.Hand = FiveCardsSorted;
                                BestHand.Value = 4; //Val 4 = Straight
                            }
                        }

                        //Three of a Kind
                        else if (ThreeKind(FiveCards).success == true)
                        {
                            ThreeKindClass ThreeKindResponse = ThreeKind(FiveCards);
                            FiveCardsSorted = ThreeKindSort(FiveCards, ThreeKindResponse);
                            if (BestHand.Value < 3)
                            {
                                BestHand.Hand = FiveCardsSorted;
                                BestHand.Value = 3; //Val 3 = ThreeKind
                            }
                            else if (BestHand.Value == 3 && CompareHand(BestHand.Hand, FiveCardsSorted) == 1)
                            {
                                BestHand.Hand = FiveCardsSorted;
                                BestHand.Value = 3; //Val 3 = ThreeKind
                            }
                        }

                        //Two Pair
                        else if (TwoPair(FiveCards).success == true)
                        {
                            TwoPairClass TwoPairResponse = TwoPair(FiveCards);
                            FiveCardsSorted = TwoPairSort(FiveCards, TwoPairResponse);
                            if (BestHand.Value < 2)
                            {
                                BestHand.Hand = FiveCardsSorted;
                                BestHand.Value = 2; //Val 2 = TwoPair
                            }
                            else if (BestHand.Value == 2 && CompareHand(BestHand.Hand, FiveCardsSorted) == 1)
                            {
                                BestHand.Hand = FiveCardsSorted;
                                BestHand.Value = 2; //Val 2 = TwoPair
                            }
                        }

                        //Pair
                        else if (Pair(FiveCards).success == true)
                        {
                            PairClass PairResponse = Pair(FiveCards);
                            FiveCardsSorted = PairSort(FiveCards, PairResponse);
                            if (BestHand.Value < 1)
                            {
                                BestHand.Hand = FiveCardsSorted;
                                BestHand.Value = 1; //Val 1 = Pair
                            }
                            else if (BestHand.Value == 1 && CompareHand(BestHand.Hand, FiveCardsSorted) == 1)
                            {
                                BestHand.Hand = FiveCardsSorted;
                                BestHand.Value = 1; //Val 1 = Pair
                            }
                        }
                        else if (BestHand.Value == 0 && CompareHand(BestHand.Hand, FiveCardsSorted) == 1)
                        {
                            BestHand.Hand = FiveCardsSorted;
                            BestHand.Value = 0;
                        }
                    }

                }

            }
            return BestHand;
        }

        //Compare two equally weighted hands to check for a higher hand in the same class.
        // NOTE Best hand not yet set
        public int CompareHand(Card[] _h1, Card[] _h2)
        {
            if (_h1 == null)
            {
                return 1;
            }

            int BetterHand = 2;
            for (int i = 0; i < _h1.Length; i++)
            {
                if (_h1[i].GetValue() > _h2[i].GetValue())
                {
                    BetterHand = 0;
                    return BetterHand;
                }
                else if (_h1[i].GetValue() < _h2[i].GetValue())
                {
                    BetterHand = 1;
                    return BetterHand;
                }
            }
            return BetterHand;
        }
        #endregion

        #region Sort Hands Functions
        public Card[] HighSort(Card[] _Cards)
        {
            //Definitions
            //string Order = "ABCDE"; Where A > B > C > D > E

            //Initialise
            Card[] Response = null;
            Card TempCard = null;

            //Body - Linear Sort
            for (int i = 0; i < _Cards.Length; i++)
            {
                for (int j = i + 1; j < _Cards.Length; j++)
                {
                    if (_Cards[j].GetValue() > _Cards[i].GetValue()) //Swap elements if later value is greater, otherwise remain as is.
                    {
                        TempCard = _Cards[i];
                        _Cards[i] = _Cards[j];
                        _Cards[j] = TempCard;
                    }
                }
            }

            //Response
            Response = _Cards;
            return Response;
        }

        public Card[] FourKindSort(Card[] _Cards, FourKindClass _Pos)
        {
            //Definitions
            //string Order = "XXXXY";
            //string XXXX = "Quads"; XXXX = p1p2p3p4
            //string Y = "High Card";

            //Initialise
            Card[] Response = null;
            Card TempCard = null;

            //Body
            for (int i = 0; i < _Cards.Length; i++)
            {
                if (i != _Pos.p1 && i != _Pos.p2 && i != _Pos.p3 && i != _Pos.p4) //Find "Odd One Out" - value which is not part of the quads.
                {
                    TempCard = _Cards[i];
                    _Cards[i] = _Cards[4]; //Swap value to the last position in the array
                    _Cards[4] = TempCard; //Order Now = XXXXY
                }
            }

            //Response
            Response = _Cards;
            return Response;
        }


        public Card[] FullHouseSort(Card[] _Cards, FullHouseClass _Pos)
        {
            //Definitions
            //string Order = "XXXYY";
            //string XXX = "Set"; XXX = p1p2p3
            //string YY = "Bottom Pair"; YY = q1q2

            //Initialise
            Card[] Response = null;
            Card[] CardCopy = _Cards; //We have all five values of the relevant positions already, so we can create a copy of the array and then draw values from that without having to worry about overwriting values.

            //Body
            _Cards[0] = CardCopy[_Pos.p1]; //X
            _Cards[1] = CardCopy[_Pos.p2]; //X
            _Cards[2] = CardCopy[_Pos.p3]; //X
            _Cards[3] = CardCopy[_Pos.q1]; //Y
            _Cards[4] = CardCopy[_Pos.q2]; //Y

            //Response
            Response = _Cards;
            return Response;
        }

        public Card[] ThreeKindSort(Card[] _Cards, ThreeKindClass _Pos)
        {
            //Definitions
            //string Order = "XXXYZ";
            //string XXX = "Set"; XXX = p1p2p3
            //string Y = "High Card";
            //string Z = "BottomCard";

            //Initialise
            Card[] Response = null;
            Card[] CardCopy = _Cards;
            Card[] RemCards = new Card[2];

            //Body
            for (int i = 0; i < _Cards.Length; i++)
            {
                for (int j = i + 1; j < _Cards.Length; j++)
                {
                    if (i != _Pos.p1 && i != _Pos.p2 && i != _Pos.p3 && j != _Pos.p1 && j != _Pos.p2 && j != _Pos.p3) //Find values which are not part of the set.
                    {
                        //Seperate: "Non Set Cards"
                        RemCards[0] = CardCopy[i]; //Could be Y or Z
                        RemCards[1] = CardCopy[j]; //Could be Y or Z

                        //Sort: "Non Set Cards"
                        RemCards = HighSort(RemCards);

                        //Assign set cards to first three positions
                        _Cards[0] = CardCopy[_Pos.p1]; //X
                        _Cards[1] = CardCopy[_Pos.p2]; //X
                        _Cards[2] = CardCopy[_Pos.p3]; //X

                        //Assign now ordered remaining cards to the remaining two index positions.
                        _Cards[3] = RemCards[0]; //Y
                        _Cards[4] = RemCards[1]; //Z
                    }
                }
            }

            //Response
            Response = _Cards;
            return Response;
        }

        public Card[] TwoPairSort(Card[] _Cards, TwoPairClass _Pos)
        {
            //Definitions
            //string Order = "XXYYZ";
            //string XX = "Top Pair"; XX = p1p2
            //string YY = "Bottom Pair"; YY = q1q2
            //string Z = "High Card"; Z = _Cards[i]/TempCard

            //Initialise
            Card[] Response = null;
            Card TempCard = null;
            Card[] CardCopy = _Cards;

            //Body
            for (int i = 0; i < _Cards.Length; i++)
            {
                if (i != _Pos.p1 && i != _Pos.p2 && i != _Pos.q1 && i != _Pos.q2) //Find Z - High Card
                {
                    TempCard = _Cards[i];
                    _Cards[i] = _Cards[4]; //Swap high card with card in 4'th position.
                    _Cards[4] = TempCard; //Z
                }
            }

            //Assign Pair Values - From TwoPair() p1,p2 are always > q1,q2 as higher pair value is assigned to p1,p2 slot.
            _Cards[0] = CardCopy[_Pos.p1]; //X
            _Cards[1] = CardCopy[_Pos.p2]; //X
            _Cards[2] = CardCopy[_Pos.q1]; //Y
            _Cards[3] = CardCopy[_Pos.p2]; //Y

            //Response
            Response = _Cards;
            return Response;
        }

        public Card[] PairSort(Card[] _Cards, PairClass _Pos)
        {
            //Definitions
            //string Order = "XXABC";
            //string XX = "Set"; XX = p1p2
            //string A = "High Card";
            //string B = "Middle Card";
            //string C = "Low Card";

            //Initialise
            Card[] Response;
            Card[] CardCopy = _Cards;
            Card[] RemCards = new Card[3];

            //Body
            for (int i = 0; i < _Cards.Length; i++)
            {
                for (int j = i + 1; j < _Cards.Length; j++)
                {
                    for (int n = j + 1; n < _Cards.Length; n++)
                    {
                        if (i != _Pos.p1 && i != _Pos.p2 && j != _Pos.p1 && j != _Pos.p2 && n != _Pos.p1 && n != _Pos.p2) //Find values which are not part of the pair.
                        {
                            //Seperate: "Non Set Cards"
                            RemCards[0] = CardCopy[i]; //Could be A, B or C
                            RemCards[1] = CardCopy[j]; //Could be A, B or C
                            RemCards[2] = CardCopy[n];

                            //Sort: "Non Set Cards"
                            RemCards = HighSort(RemCards);

                            //Assign set cards to first three positions
                            _Cards[0] = CardCopy[_Pos.p1]; //X
                            _Cards[1] = CardCopy[_Pos.p2]; //X

                            //Assign now ordered remaining cards to the remaining two index positions.
                            _Cards[2] = RemCards[0]; //A
                            _Cards[3] = RemCards[1]; //B
                            _Cards[4] = RemCards[2]; //C
                        }
                    }
                }
            }

            //Response
            Response = _Cards;
            return Response;
        }
        #endregion

        #region Hand Type Functions
        public bool StraightFlush(Card[] _CurrHand)
        {
            bool StraightFlush = false;

            if (_CurrHand[0].GetSuite() == _CurrHand[1].GetSuite() && _CurrHand[0].GetValue() == (_CurrHand[1].GetValue() + 1))
            {
                if (_CurrHand[0].GetSuite() == _CurrHand[2].GetSuite() && _CurrHand[0].GetValue() == (_CurrHand[2].GetValue() + 2))
                {
                    if (_CurrHand[0].GetSuite() == _CurrHand[3].GetSuite() && _CurrHand[0].GetValue() == (_CurrHand[3].GetValue() + 3))
                    {
                        if (_CurrHand[0].GetSuite() == _CurrHand[4].GetSuite() && _CurrHand[0].GetValue() == (_CurrHand[4].GetValue() + 4))
                        {
                            StraightFlush = true;
                            return StraightFlush;
                        }
                    }
                }
            }
            return StraightFlush;
        }

        public FourKindClass FourKind(Card[] _CurrHand)
        {
            FourKindClass Response = new FourKindClass(-1, -1, -1, -1, false);

            for (int n = 0; n < _CurrHand.Length; n++)
            {
                //Generate Remainder Integers.
                int i1 = (n + 1) % _CurrHand.Length;
                int i2 = (n + 2) % _CurrHand.Length;
                int i3 = (n + 3) % _CurrHand.Length;
                int i4 = (n + 4) % _CurrHand.Length;

                //Check all values of array match except the current n value.
                if (_CurrHand[i1].GetValue() == _CurrHand[i2].GetValue() && _CurrHand[i1].GetValue() == _CurrHand[i3].GetValue() && _CurrHand[i1].GetValue() == _CurrHand[i4].GetValue())
                {
                    //Fill response.
                    Response.p1 = i1;
                    Response.p2 = i2;
                    Response.p3 = i3;
                    Response.p4 = i4;
                    Response.success = true;

                    return Response;
                }
            }
            return Response;
        }

        public FullHouseClass FullHouse(Card[] _CurrHand)
        {
            //Initialise Response
            FullHouseClass Response = new FullHouseClass(-1, -1, -1, -1, -1, false);

            //Array
            Card[] RemCards = new Card[2];

            //Counters
            int RemCounter = 0;

            //Body
            //Check if a set exists in this combination.
            ThreeKindClass Set = ThreeKind(_CurrHand);
            if (Set.success == true) //If set exists.
            {
                for (int n = 0; n < _CurrHand.Length; n++)
                {
                    if (n != Set.p1 && n != Set.p2 && n != Set.p3) //Generate new array of cards contianing the three cards which were not identified as a set.
                    {
                        RemCards[RemCounter] = _CurrHand[n];
                        RemCounter += 1; //Internal Loop Counter
                    }
                }
                PairClass RemPair = Pair(RemCards);     //Check new array of remaining cards for an additional pair.
                if (RemPair.success == true)            //If pair exists.
                {
                    PairClass RemPairOrig = OriginalPos(_CurrHand, RemCards[RemPair.p1].GetValue()); //Find original position of second pair values from the original array positions.

                    //Store Higher Value Pair
                    Response.p1 = Set.p1;
                    Response.p2 = Set.p2;
                    Response.p3 = Set.p3;

                    //Store Lower Value Pair
                    Response.q1 = RemPairOrig.p1;
                    Response.q2 = RemPairOrig.p2;

                    //Store Success Response
                    Response.success = true;

                    return Response;
                }
            }
            return Response;
        }

        public bool Flush(Card[] _CurrHand)
        {
            bool Flush = false;
            if (_CurrHand[0].GetSuite() == _CurrHand[1].GetSuite())
            {
                if (_CurrHand[0].GetSuite() == _CurrHand[2].GetSuite())
                {
                    if (_CurrHand[0].GetSuite() == _CurrHand[3].GetSuite())
                    {
                        if (_CurrHand[0].GetSuite() == _CurrHand[4].GetSuite())
                        {
                            Flush = true;
                            return Flush;
                        }
                    }
                }
            }
            return Flush;
        }

        public bool Straight(Card[] _CurrHand)
        {
            bool Straight = false;
            if (_CurrHand[0].GetValue() == _CurrHand[1].GetValue() + 1)
            {
                if (_CurrHand[0].GetValue() == _CurrHand[2].GetValue() + 2)
                {
                    if (_CurrHand[0].GetValue() == _CurrHand[3].GetValue() + 3)
                    {
                        if (_CurrHand[0].GetValue() == _CurrHand[4].GetValue() + 4)
                        {
                            Straight = true;
                            return Straight;
                        }
                    }
                }

            }
            return Straight;
        }

        public ThreeKindClass ThreeKind(Card[] _CurrHand)
        {
            //Initialise Response
            ThreeKindClass Response = new ThreeKindClass(-1, -1, -1, false);

            //Body
            for (int i = 0; i < _CurrHand.Length; i++)
            {
                for (int j = i+1; j < _CurrHand.Length; j++)
                {
                    for (int n = j+1; n < _CurrHand.Length; n++) //Loop to find all combinations of three cards from a given 5.
                    {
                        if (_CurrHand[i].GetValue() == _CurrHand[j].GetValue() && _CurrHand[i].GetValue() == _CurrHand[n].GetValue())//Check if given three cards they are a three of a kind.
                        {
                            //Fill response with card positions.
                            Response.p1 = i;
                            Response.p2 = j;
                            Response.p3 = n;
                            Response.success = true;

                            return Response;
                        }
                    }
                }
            }
            return Response;
        }

        public TwoPairClass TwoPair(Card[] _CurrHand)
        {
            //Initialisation
            //Response
            TwoPairClass Response = new TwoPairClass(-1, -1, -1, -1, false);

            //Array
            Card[] RemCards = new Card[3];

            //Counters
            int RemCounter = 0;

            //Body
            //Check if a pair exists in this combination.
            PairClass FirstPair = Pair(_CurrHand);
            if (FirstPair.success == true) //If pair exists.
            {
                for (int n = 0; n < _CurrHand.Length; n++)
                {
                    if (n != FirstPair.p1 && n != FirstPair.p2) //Generate new array of cards contianing the three cards which were not identified as a pair.
                    {
                        RemCards[RemCounter] = _CurrHand[n];
                        RemCounter += 1; //Internal Loop Counter
                    }
                }
                PairClass SecondPair = Pair(RemCards); //Check new array of remaining cards for an additional pair.
                if (SecondPair.success == true)//If pair exists.
                {
                    PairClass SecondPairOrig = OriginalPos(_CurrHand, RemCards[SecondPair.p1].GetValue()); //Find original position of second pair values from the original array positions.
                    if (_CurrHand[FirstPair.p1].GetValue() > _CurrHand[SecondPairOrig.p1].GetValue()) //Check which pair is larger, as it will need to be sorted to be positioned first.
                    {
                        //Store Higher Value Pair
                        Response.p1 = FirstPair.p1;
                        Response.p2 = FirstPair.p2;

                        //Store Lower Value Pair
                        Response.q1 = SecondPairOrig.p1;
                        Response.q2 = SecondPairOrig.p2;

                        //Store Success Response
                        Response.success = true;
                        return Response; //Fill Response and return.
                    }
                    else
                    {
                        //Store Higher Value Pair
                        Response.p1 = SecondPairOrig.p1;
                        Response.p2 = SecondPairOrig.p2;

                        //Store Lower Value Pair
                        Response.q1 = FirstPair.p1;
                        Response.q2 = FirstPair.p2;

                        //Store Success Response
                        Response.success = true;
                        return Response; //Fill Response and return.
                    }
                }
            }
            return Response; //Return Empty/False Response.
        }

        public PairClass Pair(Card[] _CurrHand)
        {
            //Initialise Response
            PairClass Response = new PairClass(-1, -1, false);

            //Loop through every pair combination of cards
            for (int i = 0; i < _CurrHand.Length; i++)
            {
                for (int j = i+1; j < _CurrHand.Length; j++)
                {
                    if (_CurrHand[i].GetValue() == _CurrHand[j].GetValue())//If the pair combinations are a mathcing value (i.e if they are a pair).
                    {
                        //Fill response with card positions.
                        Response.p1 = i;
                        Response.p2 = j;
                        Response.success = true;

                        return Response;
                    }
                }
            }
            return Response; //Return Empty.
        }


        public PairClass OriginalPos(Card[] _Cards, int _Value) //Returns the original position of pair values in original lists given remainder arrays.
        {
            //Initialise Response
            PairClass Response = new PairClass(-1, -1, false);

            //Body
            for (int i = 0; i < _Cards.Length; i++)
            {
                if (_Cards[i].GetValue() == _Value)//Find pair values
                {
                    if (Response.p1 != -1)//If first value has been found
                    {
                        //Fill second value and return response
                        Response.p2 = i;
                        Response.success = true;
                        return Response;
                    }
                    else //Otherwise
                    {
                        Response.p1 = i; //Fill first value
                    }
                }
            }
            return Response;
        }
        #endregion

        // add player to the lobby                          
        public void AddPlayer(Player _player)
        {
            // search for an available slot in the 
            bool slotFound = false;

            for (int i = 1; i <= maxPlayers; i++)
            {
                if (playersInLobby[i] == null)
                {
                    playersInLobby[i] = _player;
                    slotFound = true;
                    break;
                }
            }

            if (slotFound)
            {
                // join lobby
                ServerSend.JoinLobbyResponse(_player.id, true, "Success", roomCode);

                // send each existing player to new player
                foreach (Player p in playersInLobby.Values)
                {
                    if (p != null)
                    {
                        if (p.id != _player.id)
                        {
                            ServerSend.JoinLobbyResult(_player.id, p);
                        }
                    }
                }

                // send new player data to existing players including themselves
                foreach (Player p in playersInLobby.Values)
                {
                    if (p != null)
                    {
                        ServerSend.JoinLobbyResult(p.id, _player);
                    }
                }
            }
            else
            {
                ServerSend.JoinLobbyResponse(_player.id, false, "No slots are available", "");
            }
        }


        // remove player when disconnect occurs
        public void RemovePlayer(Player _leavingPlayer)
        {
            // record player results
            Server.db.AddGameRecord(Server.clients[_leavingPlayer.id].username, _leavingPlayer.tokens - startingTokens, roomName);
            Console.WriteLine("NOTICE - Removing player from lobby");

            // notify each player in lobby of player exiting
            foreach (Player p in playersInLobby.Values)
            {
                if (p != null)
                {
                    if (p.id != _leavingPlayer.id)
                    {
                        ServerSend.LeaveLobbyResult(p.id, _leavingPlayer);
                    }
                }
            }

            // remove player from InGame list
            int gamePlayerId = GetGameIDFromClientID(_leavingPlayer.id);
            if (gamePlayerId != -1)
            {
                if (playersInGame[gamePlayerId] != null)
                {
                    // remove player from players list 
                    playersInGame.RemoveAt(gamePlayerId);

                    // if it is players current turn, move to next turn
                    if (currentTurn == gamePlayerId && isRunning)
                    {
                        NextTurn();
                    }
                }
                else
                {
                    // remove player from players list 
                    playersInGame.RemoveAt(gamePlayerId);
                }

                
            }

            // remove player from InLobby list
            int lobbyPlayerID = GetLobbyIDFromClientID(_leavingPlayer.id);
            if (lobbyPlayerID != -1)
            {
                playersInLobby[lobbyPlayerID] = null;
            }
        }


        public int numPlayersInLobby()
        {
            int counter = 0;
            foreach(Player p in playersInLobby.Values)
            {
                if (p != null)
                {
                    counter++;
                }
            }
            return counter;
        }

        public void incrementTimer()
        {
            if (isRunning)
            {
                currentTurnTimer += 1;
                if(currentTurnTimer > turnLimit + 3)                    // extra 3 seconds are added to help any slow connections between the client and server
                {
                    Console.WriteLine("NOTICE - Player " + playersInGame[currentTurn].id  + " in room " + roomCode + " has ran out of time.");

                    if (IsBlindRound(currentTurn))
                    {
                        if (currentTurn == smallBlindId)
                        {
                            // default blind move is to raise to the minimum bet
                            int bet = smallBlindAmount;
                            if(smallBlindAmount > playersInGame[currentTurn].tokens)
                            {
                                bet = playersInGame[currentTurn].tokens;
                            }

                            // set the players bet 
                            playersInGame[currentTurn].tokens -= bet;
                            playersInGame[currentTurn].bet = bet;

                            //set bet as the highest bet to mach in the game
                            highestBet = playersInGame[currentTurn].bet;
                            playersInGame[currentTurn].played = true;

                            // return successful result and start the big blind
                            ServerSend.RaiseResponse(playersInGame[currentTurn].id, true, "Player has run out of time.");

                            currentTurn = bigBlindId;
                            currentTurnTimer = 0;

                            if (bet > bigBlindAmount)
                            {
                                bigBlindAmount = bet;
                            }

                            foreach (Player p in playersInLobby.Values)
                            {
                                if (p != null)
                                {
                                    ServerSend.RaiseResult(p.id, playersInGame[smallBlindId], bet);
                                    ServerSend.BigBlindTurn(p.id, playersInGame[currentTurn], bigBlindAmount);
                                }
                            }
                        }
                        else if (currentTurn == bigBlindId)
                        {
                            // default blind move is to raise to the minimum bet
                            int bet = bigBlindAmount;
                            if (bigBlindAmount > playersInGame[currentTurn].tokens)
                            {
                                bet = playersInGame[currentTurn].tokens;
                            }

                            // set the players bet 
                            playersInGame[currentTurn].tokens -= bet;
                            playersInGame[currentTurn].bet = bet;

                            // set bet as the highest bet to mach in the game
                            highestBet = playersInGame[currentTurn].bet;
                            SetAllPlayersToUnplayed();
                            playersInGame[currentTurn].played = true;

                            // return successful result and deal cards
                            ServerSend.RaiseResponse(playersInGame[currentTurn].id, true, "Player has run out of time.");
                            foreach (Player p in playersInLobby.Values)
                            {
                                if (p != null)
                                {
                                    ServerSend.RaiseResult(p.id, playersInGame[currentTurn], bet);
                                }
                            }

                            // after the big blind, players are shown their cards
                            DealCards();
                        }
                        else
                        {
                            // this code should not be ran
                            // player folds
                            playersInGame[currentTurn].folded = true;
                            ServerSend.FoldResponse(playersInGame[currentTurn].id, true, "Player has run out of time.");
                            foreach (Player p in playersInLobby.Values)
                            {
                                if (p != null)
                                {
                                    ServerSend.FoldResult(p.id, playersInGame[currentTurn]);
                                }
                            }
                            NextTurn();
                        }
                    }
                    else
                    {
                        // player folds
                        playersInGame[currentTurn].folded = true;
                        ServerSend.FoldResponse(playersInGame[currentTurn].id, true, "Player has run out of time.");
                        foreach (Player p in playersInLobby.Values)
                        {
                            if (p != null)
                            {
                                ServerSend.FoldResult(p.id, playersInGame[currentTurn]);
                            }
                        }
                        NextTurn();
                    }
                }
            }
        }
    }



    #region Class Objbects
    class BestHandClass
    {
        public Card[] Hand = new Card[5];
        public int Value;

        public BestHandClass(Card[] _hand, int _value)
        {
            Hand = _hand;
            Value = _value;
        }
    }

    class PairClass
    {
        public int p1 = -1;
        public int p2 = -1;
        public bool success = false;

        public PairClass(int _p1, int _p2, bool _success)
        {
            p1 = _p1;
            p2 = _p2;
            success = _success;
        }
    }

    class TwoPairClass
    {
        public int p1 = -1;
        public int p2 = -1;
        public int q1 = -1;
        public int q2 = -1;
        public bool success = false;

        public TwoPairClass(int _p1, int _p2, int _q1, int _q2, bool _success)
        {
            p1 = _p1;
            p2 = _p2;
            q1 = _p1;
            q2 = _p2;
            success = _success;
        }
    }

    class ThreeKindClass
    {
        public int p1 = -1;
        public int p2 = -1;
        public int p3 = -1;
        public bool success = false;

        public ThreeKindClass(int _p1, int _p2, int _p3, bool _success)
        {
            p1 = _p1;
            p2 = _p2;
            p3 = _p3;
            success = _success;
        }
    }

    class FullHouseClass
    {
        public int p1 = -1;
        public int p2 = -1;
        public int p3 = -1;
        public int q1 = -1;
        public int q2 = -1;
        public bool success = false;
        public FullHouseClass(int _p1, int _p2, int _p3, int _q1, int _q2, bool _success)
        {
            p1 = _p1;
            p2 = _p2;
            p3 = _p3;
            q1 = _p1;
            q2 = _p2;
            success = _success;
        }
    }

    class FourKindClass
    {
        public int p1 = -1;
        public int p2 = -1;
        public int p3 = -1;
        public int p4 = -1;
        public bool success = false;

        public FourKindClass(int _p1, int _p2, int _p3, int _p4, bool _success)
        {
            p1 = _p1;
            p2 = _p2;
            p3 = _p3;
            p4 = _p4;
            success = _success;
        }
    }


    class PlayerResultClass
    {
        public string username = "";
        public Card card1 = null;
        public Card card2 = null;
        public bool win = false;
        public string handname = "";
        public int tokens = 0;
        public int winnings = 0;

        public PlayerResultClass(string _username, Card _card1, Card _card2, bool _win, int _handvalue, int _tokens, int _winnings)
        {
            username = _username;
            card1 = _card1;
            card2 = _card2;
            win = _win;
            tokens = _tokens;
            winnings = _winnings;

            switch (_handvalue)
            {
                case 8:
                    handname = "Straight Flush";
                    break;
                case 7:
                    handname = "Four of a Kind";
                    break;
                case 6:
                    handname = "Full House";
                    break;
                case 5:
                    handname = "Flush";
                    break;
                case 4:
                    handname = "Straight";
                    break;
                case 3:
                    handname = "Three of a Kind";
                    break;
                case 2:
                    handname = "Two Pair";
                    break;
                case 1:
                    handname = "Pair";
                    break;
                default:
                    handname = "High Card";
                    break;
            }
    }
    }
    #endregion
}
