using Blackjack.Enums;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Blackjack.Classes
{
    public static class HandEvaluator
    {
        public static void Evaluate(List<Player> players, Dealer dealer)
        {
            int dealerScore = dealer.Hand.GetValue();
            bool dealerBust = dealer.Hand.GetValue() > 21;
            bool dealerBlackjack = dealerScore == 21 && dealer.Hand.Cards.Count == 2;

            foreach (var player in players)
            {
                foreach (var hand in player.Hands)
                {
                    if (hand.Result != HandResult.Surrender)
                    {
                        int playerScore = hand.GetValue();
                        bool playerBlackjack = playerScore == 21 && hand.Cards.Count == 2;

                        if (playerBlackjack && dealerBlackjack)
                            hand.Result = HandResult.Push;
                        else if (playerBlackjack)
                            hand.Result = HandResult.Blackjack;
                        else if (dealerBlackjack)
                            hand.Result = HandResult.Lose;
                        else if (hand.IsBust)
                            hand.Result = HandResult.Bust;
                        else if (dealerBust)
                            hand.Result = HandResult.Win;
                        else if (playerScore > dealerScore)
                            hand.Result = HandResult.Win;
                        else if (playerScore < dealerScore)
                            hand.Result = HandResult.Lose;
                        else
                            hand.Result = HandResult.Push;
                    }

                    EvaluatePairBet(hand);
                    EvaluateInsurance(hand, dealerBlackjack);
                }
            }
        }

        private static void EvaluatePairBet(Hand hand)
        {
            if (hand.PairBet <= 0)
                return;

            if (hand.Cards.Count < 2)
            {
                hand.PairResult = PairResult.NoPair;
                return;
            }

            var card1 = hand.Cards[0];
            var card2 = hand.Cards[1];

            bool sameRank = card1.Rank == card2.Rank;

            if (!sameRank)
            {
                hand.PairResult = PairResult.NoPair;
                return;
            }

            bool sameSuit = card1.Suit == card2.Suit;
            bool sameColor = IsSameColor(card1.Suit, card2.Suit);

            if (sameSuit)
                hand.PairResult = PairResult.PerfectPair;
            else if (sameColor)
                hand.PairResult = PairResult.ColoredPair;
            else
                hand.PairResult = PairResult.MixedPair;
        }

        private static void EvaluateInsurance(Hand hand, bool dealerBlackjack)
        {
            if (hand.InsuranceBet <= 0)
                return;

            hand.InsuranceResult = dealerBlackjack;
        }

        private static bool IsSameColor(string suit1, string suit2)
        {
            bool IsRed(string suit) => suit == "Hearts" || suit == "Diamonds";
            return IsRed(suit1) == IsRed(suit2);
        }
    }
}