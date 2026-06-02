namespace Blackjack.Tests;
using Blackjack.Classes;
using Blackjack.Enums;

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

        player.Hands[0].AddCard(new Card("Hearts", "8", 8));
        player.Hands[0].AddCard(new Card("Spades", "8", 8));

        var deck = new Deck();

        player.Split(deck);

        Assert.Equal(2, player.Hands.Count);
    }

    [Fact]
    public void CanSplit_TwoEights_ReturnsTrue()
    {
        var player = new Player("Test");

        player.Hands[0].AddCard(new Card("Hearts", "8", 8));
        player.Hands[0].AddCard(new Card("Spades", "8", 8));

        Assert.True(player.CanSplit());
    }

    [Fact]
    public void CanSplit_EightAndNine_ReturnsFalse()
    {
        var player = new Player("Test");

        player.Hands[0].AddCard(new Card("Hearts", "8", 8));
        player.Hands[0].AddCard(new Card("Spades", "9", 9));

        Assert.False(player.CanSplit());
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
    public void Evaluate_PlayerHigherThanDealer_Wins()
    {
        var player = new Player("Test");
        var dealer = new Dealer();

        player.Hands[0].AddCard(new Card("Hearts", "10", 10));
        player.Hands[0].AddCard(new Card("Spades", "9", 9));

        dealer.Hand.AddCard(new Card("Clubs", "10", 10));
        dealer.Hand.AddCard(new Card("Diamonds", "7", 7));

        HandEvaluator.Evaluate([player], dealer);

        Assert.Equal(HandResult.Win, player.Hands[0].Result);
    }
}
