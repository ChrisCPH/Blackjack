using Blackjack.Classes;
using Blackjack.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Blackjack.Tests
{
    public class HandEvaluatorTests
    {
        [Fact]
        public void Evaluate_DealerBlackjack_Player21WithThreeCards_Lose()
        {
            var player = new Player("Test");
            var dealer = new Dealer();

            var hand = new Hand();
            player.Hands.Add(hand);

            hand.AddCard(new Card("H", "7", 7));
            hand.AddCard(new Card("S", "7", 7));
            hand.AddCard(new Card("D", "7", 7));

            dealer.Hand.AddCard(new Card("C", "Ace", 11));
            dealer.Hand.AddCard(new Card("D", "King", 10));

            HandEvaluator.Evaluate([player], dealer);

            Assert.Equal(HandResult.Lose, hand.Result);
        }

        [Fact]
        public void Evaluate_PlayerBlackjack_Dealer21WithThreeCards_Blackjack()
        {
            var player = new Player("Test");
            var dealer = new Dealer();

            var hand = new Hand();
            player.Hands.Add(hand);

            hand.AddCard(new Card("H", "Ace", 11));
            hand.AddCard(new Card("S", "King", 10));

            dealer.Hand.AddCard(new Card("C", "7", 7));
            dealer.Hand.AddCard(new Card("D", "7", 7));
            dealer.Hand.AddCard(new Card("H", "7", 7));

            HandEvaluator.Evaluate([player], dealer);

            Assert.Equal(HandResult.Blackjack, hand.Result);
        }

        [Fact]
        public void Evaluate_BothBlackjack_ReturnsPush()
        {
            var player = new Player("Test");
            var dealer = new Dealer();

            var hand = new Hand();
            player.Hands.Add(hand);

            hand.AddCard(new Card("H", "Ace", 11));
            hand.AddCard(new Card("S", "King", 10));

            dealer.Hand.AddCard(new Card("C", "Ace", 11));
            dealer.Hand.AddCard(new Card("D", "King", 10));

            HandEvaluator.Evaluate([player], dealer);

            Assert.Equal(HandResult.Push, hand.Result);
        }

        [Fact]
        public void Evaluate_DealerHigherThanPlayer_Lose()
        {
            var player = new Player("Test");
            var dealer = new Dealer();

            var hand = new Hand();
            player.Hands.Add(hand);

            hand.AddCard(new Card("Hearts", "10", 10));
            hand.AddCard(new Card("Spades", "7", 7));

            dealer.Hand.AddCard(new Card("Clubs", "10", 10));
            dealer.Hand.AddCard(new Card("Diamonds", "9", 9));

            HandEvaluator.Evaluate([player], dealer);

            Assert.Equal(HandResult.Lose, hand.Result);
        }

        [Fact]
        public void Evaluate_SameScore_Push()
        {
            var player = new Player("Test");
            var dealer = new Dealer();

            var hand = new Hand();
            player.Hands.Add(hand);

            hand.AddCard(new Card("Hearts", "10", 10));
            hand.AddCard(new Card("Spades", "8", 8));

            dealer.Hand.AddCard(new Card("Clubs", "10", 10));
            dealer.Hand.AddCard(new Card("Diamonds", "8", 8));

            HandEvaluator.Evaluate([player], dealer);

            Assert.Equal(HandResult.Push, hand.Result);
        }

        [Fact]
        public void Evaluate_DealerBust_PlayerWins()
        {
            var player = new Player("Test");
            var dealer = new Dealer();

            var hand = new Hand();
            player.Hands.Add(hand);

            hand.AddCard(new Card("Hearts", "10", 10));
            hand.AddCard(new Card("Spades", "7", 7));

            dealer.Hand.AddCard(new Card("Clubs", "10", 10));
            dealer.Hand.AddCard(new Card("Diamonds", "10", 10));
            dealer.Hand.AddCard(new Card("Hearts", "5", 5));

            HandEvaluator.Evaluate([player], dealer);

            Assert.Equal(HandResult.Win, hand.Result);
        }

        [Fact]
        public void Evaluate_NaturalBlackjack_ReturnsBlackjack()
        {
            var player = new Player("Test");
            var dealer = new Dealer();

            var hand = new Hand();
            player.Hands.Add(hand);

            hand.AddCard(new Card("Hearts", "Ace", 11));
            hand.AddCard(new Card("Spades", "King", 10));

            dealer.Hand.AddCard(new Card("Clubs", "10", 10));
            dealer.Hand.AddCard(new Card("Diamonds", "7", 7));

            HandEvaluator.Evaluate([player], dealer);

            Assert.Equal(HandResult.Blackjack, hand.Result);
        }

        [Fact]
        public void Evaluate_PlayerHigherThanDealer_Wins()
        {
            var player = new Player("Test");
            var dealer = new Dealer();

            var hand = new Hand();
            player.Hands.Add(hand);

            hand.AddCard(new Card("Hearts", "10", 10));
            hand.AddCard(new Card("Spades", "9", 9));

            dealer.Hand.AddCard(new Card("Clubs", "10", 10));
            dealer.Hand.AddCard(new Card("Diamonds", "7", 7));

            HandEvaluator.Evaluate([player], dealer);

            Assert.Equal(HandResult.Win, player.Hands[0].Result);
        }
    }
}
