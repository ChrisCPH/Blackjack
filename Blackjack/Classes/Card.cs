using System;
using System.Collections.Generic;
using System.Text;

namespace Blackjack.Classes
{
    public class Card
    {
        public string Suit { get; }
        public string Rank { get; }
        public int Value { get; }

        public Card(string suit, string rank, int value)
        {
            Suit = suit;
            Rank = rank;
            Value = value;
        }

        public override string ToString()
        {
            return $"{Rank} of {Suit}";
        }
    }
}