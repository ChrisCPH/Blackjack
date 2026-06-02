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

            foreach (var player in players)
            {
                foreach (var hand in player.Hands)
                {
                    int playerScore = hand.GetValue();

                    if (hand.IsBust)
                    {
                        hand.Result = HandResult.Bust;
                    }
                    else if (dealerBust)
                    {
                        hand.Result = HandResult.Win;
                    }
                    else if (playerScore > dealerScore)
                    {
                        hand.Result = HandResult.Win;
                    }
                    else if (playerScore < dealerScore)
                    {
                        hand.Result = HandResult.Lose;
                    }
                    else
                    {
                        hand.Result = HandResult.Push;
                    }

                    if (playerScore == 21 && hand.Cards.Count == 2)
                    {
                        hand.Result = HandResult.Blackjack;
                    }
                }
            }
        }
    }
}