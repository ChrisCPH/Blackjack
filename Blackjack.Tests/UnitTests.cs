using Blackjack.Classes;
using Blackjack.Enums;

namespace Blackjack.Tests
{
    public class UnitTests
    {
        [Fact]
        public void GetValue_TwoNumberCards_ReturnsSum()
        {
            var hand = new Hand();

            hand.AddCard(new Card("Hearts", "5", 5));
            hand.AddCard(new Card("Spades", "7", 7));

            Assert.Equal(12, hand.GetValue());
        }

        [Fact]
        public void GetValue_AceAndSix_Returns17()
        {
            var hand = new Hand();

            hand.AddCard(new Card("Hearts", "Ace", 11));
            hand.AddCard(new Card("Spades", "6", 6));

            Assert.Equal(17, hand.GetValue());
        }

        [Fact]
        public void IsSoft_AceAndSix_ReturnsTrue()
        {
            var hand = new Hand();

            hand.AddCard(new Card("Hearts", "Ace", 11));
            hand.AddCard(new Card("Spades", "6", 6));

            Assert.True(hand.IsSoft());
        }

        [Fact]
        public void IsSoft_AceSixTen_ReturnsFalse()
        {
            var hand = new Hand();

            hand.AddCard(new Card("Hearts", "Ace", 11));
            hand.AddCard(new Card("Spades", "6", 6));
            hand.AddCard(new Card("Clubs", "10", 10));

            Assert.False(hand.IsSoft());
        }

        [Fact]
        public void Split_CreatesTwoHands()
        {
            var player = new Player("Test");

            var hand = new Hand();
            player.Hands.Add(hand);

            hand.AddCard(new Card("Hearts", "8", 8));
            hand.AddCard(new Card("Spades", "8", 8));

            var deck = new Deck();

            var newHand = hand.Split(deck);

            player.Hands.Add(newHand);

            Assert.Equal(2, player.Hands.Count);
        }

        [Fact]
        public void CanSplit_TwoEights_ReturnsTrue()
        {
            var hand = new Hand();

            hand.AddCard(new Card("Hearts", "8", 8));
            hand.AddCard(new Card("Spades", "8", 8));

            Assert.True(hand.CanSplit());
        }

        [Fact]
        public void CanSplit_EightAndNine_ReturnsFalse()
        {
            var hand = new Hand();

            hand.AddCard(new Card("Hearts", "8", 8));
            hand.AddCard(new Card("Spades", "9", 9));

            Assert.False(hand.CanSplit());
        }

        [Fact]
        public void CanSplit_ThreeCards_ReturnsFalse()
        {
            var hand = new Hand();

            hand.AddCard(new Card("Hearts", "8", 8));
            hand.AddCard(new Card("Spades", "8", 8));
            hand.AddCard(new Card("Clubs", "2", 2));

            Assert.False(hand.CanSplit());
        }

        [Fact]
        public void ShouldHit_Soft17_ReturnsTrue()
        {
            var dealer = new Dealer();

            dealer.Hand.AddCard(new Card("Hearts", "Ace", 11));
            dealer.Hand.AddCard(new Card("Spades", "6", 6));

            Assert.True(dealer.ShouldHit());
        }

        [Fact]
        public void ShouldHit_Hard17_ReturnsFalse()
        {
            var dealer = new Dealer();

            dealer.Hand.AddCard(new Card("Hearts", "10", 10));
            dealer.Hand.AddCard(new Card("Spades", "7", 7));

            Assert.False(dealer.ShouldHit());
        }

        [Fact]
        public void Split_AllowsFourHands()
        {
            var player = new Player("Test");

            var hand = new Hand();
            player.Hands.Add(hand);

            hand.AddCard(new Card("H", "8", 8));
            hand.AddCard(new Card("S", "8", 8));

            var deck = new Deck();

            var h2 = hand.Split(deck);
            player.Hands.Add(h2);

            var h3 = h2.Split(deck);
            player.Hands.Add(h3);

            var h4 = h3.Split(deck);
            player.Hands.Add(h4);

            Assert.Equal(4, player.Hands.Count);
        }

        [Fact]
        public void GetValue_AceSixTen_Returns17()
        {
            var hand = new Hand();

            hand.AddCard(new Card("Hearts", "Ace", 11));
            hand.AddCard(new Card("Spades", "6", 6));
            hand.AddCard(new Card("Clubs", "10", 10));

            Assert.Equal(17, hand.GetValue());
        }

        [Fact]
        public void GetValue_TwoAcesAndNine_Returns21()
        {
            var hand = new Hand();

            hand.AddCard(new Card("Hearts", "Ace", 11));
            hand.AddCard(new Card("Spades", "Ace", 11));
            hand.AddCard(new Card("Clubs", "9", 9));

            Assert.Equal(21, hand.GetValue());
        }

        [Fact]
        public void IsBust_TwentyTwo_ReturnsTrue()
        {
            var hand = new Hand();

            hand.AddCard(new Card("Hearts", "10", 10));
            hand.AddCard(new Card("Spades", "10", 10));
            hand.AddCard(new Card("Clubs", "2", 2));

            Assert.True(hand.IsBust);
        }

        [Fact]
        public void ShouldHit_Sixteen_ReturnsTrue()
        {
            var dealer = new Dealer();

            dealer.Hand.AddCard(new Card("Hearts", "10", 10));
            dealer.Hand.AddCard(new Card("Spades", "6", 6));

            Assert.True(dealer.ShouldHit());
        }
    }
}