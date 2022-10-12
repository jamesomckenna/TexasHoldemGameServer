using System;
using System.Collections.Generic;
using System.Text;

namespace GameServer
{
    // not used enum currenbt as the roles are tracked by server
    public enum PlayerRole
    {
        Player = 0,
        SmallBlind,
        BigBlind,
        Dealer
    }

    class Player
    {
        public int id;              // players game ID
        public int tokens;          // how many tokens the player currently possess

        public Card card1;          // first card in hand
        public Card card2;          // second card in hand

        public int bet;             // current bet for this round
        public bool folded;         // if the player has folded
        public bool played;          // if the player has called the last bet

        public BestHandClass BestHand;

        public Player(int _id, int _startingTokens)
        {
            id = _id;
            tokens = _startingTokens;                  // change to customisable value
            card1 = null;
            card2 = null;
            BestHand = null;
        }

        // set up player for new round
        public void InitNewGame()
        {
            bet = 0;
            folded = false;
            played = false;
            BestHand = null;
        }

        public void DealCards(Card _card1, Card _card2)
        {
            card1 = _card1;
            card2 = _card2;
        }

        // set up player for new round
        public void InitNewRound()
        {
            bet = 0;
        }
    }
}
