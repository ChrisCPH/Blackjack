using Blackjack.Classes;
using Blackjack.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Blackjack.Tests
{
    public class BetsTests
    {
        [Fact]
        public void CanAffordSplit_ReturnsTrue_WhenPlayerHasEnoughMoney()
        {
            var player = new Player("TestPlayer");
            var bets = new Bets();

            var hand = new Hand { Bet = 50 };
            player.Hands.Add(hand);

            player.RemoveMoney(hand.Bet);

            var result = bets.CanAffordSplit(player, player.Hands[0]);

            Assert.True(result);
        }

        [Fact]
        public void CanAffordSplit_ReturnsFalse_WhenPlayerDoesNotHaveEnoughMoney()
        {
            var player = new Player("TestPlayer");
            var bets = new Bets();

            var hand = new Hand { Bet = 550 };
            player.Hands.Add(hand);

            player.RemoveMoney(hand.Bet);

            var result = bets.CanAffordSplit(player, player.Hands[0]);

            Assert.False(result);
        }

        [Fact]
        public void TakeSplitBet_RemovesMoneyAndCopiesBet()
        {
            var player = new Player("TestPlayer");
            var bets = new Bets();

            var hand = new Hand();
            player.Hands.Add(hand);

            var originalHand = new Hand
            {
                Bet = 25
            };

            var newHand = new Hand();

            bets.TakeSplitBet(player, originalHand, newHand);

            Assert.Equal(975, player.Balance);
            Assert.Equal(25, newHand.Bet);
        }

        [Fact]
        public void CanAffordDouble_ReturnsTrue_WhenPlayerCanAffordBet()
        {
            var player = new Player("TestPlayer");
            var bets = new Bets();

            var hand = new Hand { Bet = 50 };
            player.Hands.Add(hand);

            player.RemoveMoney(hand.Bet);

            Assert.True(bets.CanAffordDouble(player, player.Hands[0]));
        }

        [Fact]
        public void CanAffordDouble_ReturnsFalse_WhenPlayerCannotAffordBet()
        {
            var player = new Player("TestPlayer");
            var bets = new Bets();

            var hand = new Hand { Bet = 550 };
            player.Hands.Add(hand);

            player.RemoveMoney(hand.Bet);

            Assert.False(bets.CanAffordDouble(player, hand));
        }

        [Fact]
        public void TakeDoubleBet_DoublesBetAndRemovesMoney()
        {
            var player = new Player("TestPlayer");
            var bets = new Bets();

            var hand = new Hand { Bet = 50 };
            player.Hands.Add(hand);

            player.RemoveMoney(hand.Bet);

            bets.TakeDoubleBet(player, hand);

            Assert.Equal(900, player.Balance);
            Assert.Equal(100, hand.Bet);
        }

        [Fact]
        public void TryApplySameBets_ReturnsTrue_WhenAllPlayersCanAffordPreviousBet()
        {
            var bets = new Bets();

            var player1 = new Player("TestPlayer1");
            var player2 = new Player("TestPlayer2");

            var hand = new Hand { Bet = 50 };
            player1.Hands.Add(hand);
            player2.Hands.Add(hand);

            player1.RemoveMoney(hand.Bet);
            player2.RemoveMoney(hand.Bet);

            var previousBets = new Dictionary<Player, decimal>
            {
                { player1, hand.Bet },
                { player2, hand.Bet }
            };

            var result = bets.TryApplySameBets(
                [player1, player2],
                previousBets);

            Assert.True(result);
        }

        [Fact]
        public void TryApplySameBets_ReturnsFalse_WhenPlayerCannotAffordPreviousBet()
        {
            var player = new Player("TestPlayer");
            var bets = new Bets();

            var hand = new Hand { Bet = 550 };
            player.Hands.Add(hand);

            player.RemoveMoney(hand.Bet);

            var previousBets = new Dictionary<Player, decimal>
            {
                { player, hand.Bet }
            };

            var result = bets.TryApplySameBets(
                [player],
                previousBets);

            Assert.False(result);
        }

        [Fact]
        public void TryApplySameBets_ReturnsFalse_WhenPlayerHasNoPreviousBet()
        {
            var player = new Player("TestPlayer");
            var bets = new Bets();

            var hand = new Hand();
            player.Hands.Add(hand);

            var result = bets.TryApplySameBets(
                [player],
                []);

            Assert.False(result);
        }

        [Fact]
        public void PayWinnings_PaysRegularWin()
        {
            var player = new Player("TestPlayer");
            var bets = new Bets();
            var dealer = new Dealer();

            var hand = new Hand { Bet = 50 };
            player.Hands.Add(hand);

            player.RemoveMoney(hand.Bet);

            player.Hands[0].Result = HandResult.Win;

            bets.PayWinnings([player], dealer);

            Assert.Equal(1050, player.Balance);
            Assert.Equal(100, player.Hands[0].Payout);
        }

        [Fact]
        public void PayWinnings_PaysBlackjackAtThreeToTwo()
        {
            var player = new Player("TestPlayer");
            var bets = new Bets();
            var dealer = new Dealer();

            var hand = new Hand { Bet = 50 };
            player.Hands.Add(hand);

            player.RemoveMoney(hand.Bet);

            player.Hands[0].Result = HandResult.Blackjack;

            bets.PayWinnings([player], dealer);

            Assert.Equal(1075, player.Balance);
            Assert.Equal(125, player.Hands[0].Payout);
        }

        [Fact]
        public void PayWinnings_ReturnsBetOnPush()
        {
            var player = new Player("TestPlayer");
            var bets = new Bets();
            var dealer = new Dealer();

            var hand = new Hand { Bet = 50 };
            player.Hands.Add(hand);

            player.RemoveMoney(hand.Bet);

            player.Hands[0].Result = HandResult.Push;

            bets.PayWinnings([player], dealer);

            Assert.Equal(1000, player.Balance);
            Assert.Equal(50, player.Hands[0].Payout);
        }

        [Fact]
        public void PayWinnings_ReturnsHalfBetOnSurrender()
        {
            var player = new Player("TestPlayer");
            var bets = new Bets();
            var dealer = new Dealer();

            var hand = new Hand { Bet = 50 };
            player.Hands.Add(hand);

            player.RemoveMoney(hand.Bet);

            player.Hands[0].Result = HandResult.Surrender;

            bets.PayWinnings([player], dealer);

            Assert.Equal(975, player.Balance);
            Assert.Equal(25, player.Hands[0].Payout);
        }

        [Fact]
        public void PayWinnings_PaysPerfectPair()
        {
            var player = new Player("TestPlayer");
            var bets = new Bets();
            var dealer = new Dealer();

            var hand = new Hand { PairBet = 50 };
            player.Hands.Add(hand);

            player.RemoveMoney(hand.PairBet);

            player.Hands[0].PairResult = PairResult.PerfectPair;

            bets.PayWinnings([player], dealer);

            Assert.Equal(2250, player.Balance);
            Assert.Equal(1300, player.Hands[0].PairPayout);
        }

        [Fact]
        public void PayWinnings_PaysWinningInsurance()
        {
            var player = new Player("TestPlayer");
            var bets = new Bets();
            var dealer = new Dealer();

            var hand = new Hand { InsuranceBet = 50 };
            player.Hands.Add(hand);

            player.RemoveMoney(hand.InsuranceBet);

            player.Hands[0].InsuranceResult = true;

            bets.PayWinnings([player], dealer);

            Assert.Equal(1100, player.Balance);
            Assert.Equal(150, player.Hands[0].InsurancePayout);
        }
    }
}
