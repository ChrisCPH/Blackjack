using Blackjack.Classes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Blackjack.UI
{
    public class GameStateChangedEvent
    {
        public required List<Player> Players { get; set; }
        public required Dealer Dealer { get; set; }
        public bool DealerReveal { get; set; }
        public Guid ActiveHandId { get; set; }
    }
}
