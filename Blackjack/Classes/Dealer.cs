using System;
using System.Collections.Generic;
using System.Text;

namespace Blackjack.Classes
{
    public class Dealer
    {
        public Hand Hand { get; set; } = new();

        public bool IsBust => Hand.GetValue() > 21;

        // Dealer must hit on 16 or less, and must hit on soft 17
        public bool ShouldHit()
        {
            int value = Hand.GetValue();

            return value < 17 ||
                   (value == 17 && Hand.IsSoft());
        }
    }
}