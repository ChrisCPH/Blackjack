using System;
using System.Collections.Generic;
using System.Text;

namespace Blackjack.Classes
{
    public class Player
    {
        public string Name { get; }
        public List<Hand> Hands { get; } = new();
        public decimal Balance { get; private set; }

        public Player(string name)
        {
            Name = name;
            Balance = 1000m; //Hardcoded starting balance for simplicity
        }

        public void AddMoney(decimal amount)
        {
            Balance += amount;
        }

        public bool RemoveMoney(decimal amount)
        {
            if (Balance < amount)
                return false;

            Balance -= amount;
            return true;
        }

        public void Reset()
        {
            Hands.Clear();
            Hands.Add(new Hand());
        }
    }
}