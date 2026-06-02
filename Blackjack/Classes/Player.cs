using System;
using System.Collections.Generic;
using System.Text;

namespace Blackjack.Classes
{
    public class Player
    {
        public string Name { get; }
        public List<Hand> Hands { get; } = new();

        public Player(string name)
        {
            Name = name;
        }

        public void Reset()
        {
            Hands.Clear();
        }
    }
}