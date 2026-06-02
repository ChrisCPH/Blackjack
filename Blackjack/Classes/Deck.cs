using System;
using System.Collections.Generic;
using System.Text;

namespace Blackjack.Classes
{
    public class Deck
    {
        private readonly List<Card> _cards = [];
        public int CardsLeft => _cards.Count;
        public bool ShouldReshuffle => _cards.Count < 15;

        public Deck()
        {
            CreateDeck();
            Shuffle();
        }

        private void CreateDeck()
        {
            _cards.Clear();

            string[] suits = ["Hearts", "Diamonds", "Clubs", "Spades"];
            string[] ranks =
            [
                "Ace", "2", "3", "4", "5",
                "6", "7", "8", "9", "10",
                "Jack", "Queen", "King"
            ];

            foreach (var suit in suits)
            {
                foreach (var rank in ranks)
                {
                    int value = rank switch
                    {
                        "Ace" => 11,
                        "Jack" or "Queen" or "King" => 10,
                        _ => int.Parse(rank)
                    };

                    _cards.Add(new Card(suit, rank, value));
                }
            }
        }

        public void Shuffle()
        {
            Random random = new();

            for (int i = _cards.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);

                (_cards[i], _cards[j]) = (_cards[j], _cards[i]);
            }
        }

        public Card DrawCard()
        {
            if (_cards.Count == 0)
            {
                throw new InvalidOperationException("Deck is empty!");
            }

            var card = _cards[0];
            _cards.RemoveAt(0);

            return card;
        }

        public void RebuildAndShuffle()
        {
            CreateDeck();
            Shuffle();
        }
    }
}