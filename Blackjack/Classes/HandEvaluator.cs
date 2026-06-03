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

            bool dealerBlackjack =
                dealerScore == 21 &&
                dealer.Hand.Cards.Count == 2;

            foreach (var player in players)
            {
                foreach (var hand in player.Hands)
                {
                    if (hand.Result == HandResult.Surrender)
                        continue;

                    int playerScore = hand.GetValue();

                    bool playerBlackjack =
                        playerScore == 21 &&
                        hand.Cards.Count == 2;

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
            }
        }
    }
}