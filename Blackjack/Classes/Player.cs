using System;
using System.Collections.Generic;
using System.Text;

namespace Blackjack.Classes
{
    public class Player
    {
        public string Name { get; }
        public List<Hand> Hands { get; } = new();
        public int ActiveHandIndex { get; set; } = 0;

        public Player(string name)
        {
            Name = name;
            Hands.Add(new Hand());
        }

        public bool CanSplit()
        {
            var hand = Hands[ActiveHandIndex];

            if (hand.Cards.Count != 2)
                return false;

            return hand.Cards[0].Rank == hand.Cards[1].Rank;
        }

        public void Split(Deck deck)
        {
            var hand = Hands[ActiveHandIndex];

            var card1 = hand.Cards[0];
            var card2 = hand.Cards[1];

            hand.Cards.Clear();
            hand.Cards.Add(card1);

            var newHand = new Hand();
            newHand.AddCard(card2);

            hand.AddCard(deck.DrawCard());
            newHand.AddCard(deck.DrawCard());

            Hands.Add(newHand);
        }
    }
}