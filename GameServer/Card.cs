using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameServer
{
    // enum of suits for easy tracking of card suite
    public enum Suites
    {
        Clubs = 0,
        Diamonds,
        Hearts,
        Spades
    }

    // card class
    class Card
    {
        private Suites Suite;
        private int Value;

        public Card(Suites _suite, int _value)
        {
            Value = _value;
            Suite = _suite;
        }

        // return suite of card
        public Suites GetSuite()
        {
            return Suite;
        }


        // return value of card
        public int GetValue()
        {
            return Value;
        }

        // get the suite in string format
        public string NamedSuite()
        {
            string suite;
            switch ((int)Suite)
            {
                case (0):
                    suite = "Clubs";
                    break;
                case (1):
                    suite = "Diamonds";
                    break;
                case (2):
                    suite = "Hearts";
                    break;
                case (3):
                    suite = "Spades";
                    break;
                default:
                    suite = "Clubs";
                    break;
            }

            return suite;
        }

        // get named value of card (eg King, Ace, etc)
        public string NamedValue()
        {
            string name;
            switch (Value)
            {
                case (14):
                    name = "Ace";
                    break;
                case (13):
                    name = "King";
                    break;
                case (12):
                    name = "Queen";
                    break;
                case (11):
                    name = "Jack";
                    break;
                default:
                    name = Value.ToString();
                    break;
            }

            return name;
        }
    }

    // deck class to set up and create decks of playing cards
    class Deck
    {
        public Stack<Card> Cards = new Stack<Card>();

        // constructor
        public Deck()
        {
            ShuffleDeck();
        }


        // resets the deck of cards 
        public void ShuffleDeck()
        {
            // clear deck
            Cards.Clear();

            // populate deck with all 52 cards
            for (int i = 0; i < 52; i++)
            {
                Suites suite = (Suites)(Math.Floor((decimal)i / 13));
                //Add 2 to value as a cards range from 2 to 14(Ace)
                int val = i % 13 + 2;
                Cards.Push(new Card(suite, val));
            }


            // randomly shuffle deck
            Card[] tempArray = Cards.ToArray();
            var rand = new Random();
            for (int i = tempArray.Length - 1; i > 0; i--)
            {
                int n = rand.Next(i + 1);
                Card temp = tempArray[i];
                tempArray[i] = tempArray[n];
                tempArray[n] = temp;
            }
            Cards = new Stack<Card>(tempArray);
        }

       

        // draws a card from the top of the deck, removing it from the stack
        public Card DrawCard()
        {
            return Cards.Pop();
        }
    }
}
