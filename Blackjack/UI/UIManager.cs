using Blackjack.Classes;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Blackjack.UI
{
    public class UIManager
    {
        public void Table(GameStateChangedEvent state)
        {
            AnsiConsole.Clear();

            var grid = new Grid();
            grid.AddColumn();

            var dealerPanel = new Panel(
                HandString(
                    "Dealer",
                    state.Dealer.Hand.Cards,
                    state.DealerReveal
                        ? state.Dealer.Hand.GetValue()
                        : GetVisibleDealerValue(state.Dealer.Hand.Cards),
                    hideFirst: !state.DealerReveal
                )
            )
            .Header("[bold red]Dealer[/]")
            .Border(BoxBorder.Rounded);

            grid.AddRow(dealerPanel);

            foreach (var player in state.Players)
            {
                for (int i = 0; i < player.Hands.Count; i++)
                {
                    var hand = player.Hands[i];

                    var isActive = state.ActiveHandId == hand.Id;

                    var title = isActive
                        ? $"{player.Name} - Hand {i + 1} (Playing)"
                        : $"{player.Name} - Hand {i + 1}";


                    var playerPanel = new Panel(
                        HandString(
                            title,
                            hand.Cards,
                            hand.GetValue()
                        )
                    )
                    .Header(isActive
                        ? $"[bold yellow]{title}[/]"
                        : $"[bold cyan]{title}[/]")
                    .Border(BoxBorder.Rounded);

                    grid.AddRow(playerPanel);
                }
            }

            AnsiConsole.Write(grid);
        }

        private string Card(Card card)
        {
            string suitSymbol = card.Suit switch
            {
                "Hearts" => "♥",
                "Diamonds" => "♦",
                "Clubs" => "♣",
                "Spades" => "♠",
                _ => "?"
            };

            string displayRank = card.Rank switch
            {
                "Ace" => "A",
                "Jack" => "J",
                "Queen" => "Q",
                "King" => "K",
                _ => card.Rank
            };

            return
                $"""
                ┌─────────┐
                │ {displayRank,-2}      │
                │         │
                │    {suitSymbol}    │
                │         │
                │      {displayRank,2} │
                └─────────┘
                """;
        }

        private string HandString(string title, List<Card> cards, int score, bool hideFirst = false)
        {
            var cardStrings = new List<string>();

            for (int i = 0; i < cards.Count; i++)
            {
                if (hideFirst && i == 1)
                {
                    cardStrings.Add(HiddenCard());
                }
                else
                {
                    cardStrings.Add(Card(cards[i]));
                }
            }

            var cardLines = cardStrings
                .Select(c => c.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None))
                .ToList();

            int height = cardLines.Max(c => c.Length);

            var result = new StringBuilder();

            for (int i = 0; i < height; i++)
            {
                foreach (var card in cardLines)
                {
                    if (i < card.Length)
                        result.Append(card[i] + "  ");
                    else
                        result.Append("            ");
                }

                result.AppendLine();
            }

            result.AppendLine($"Score: {score}");

            return result.ToString();
        }

        private string HiddenCard()
        {
            return
                """
                ┌─────────┐
                │ ???     │
                │         │
                │   🂠     │
                │         │
                │     ??? │
                └─────────┘
                """;
        }

        private int GetVisibleDealerValue(List<Card> cards)
        {
            if (cards.Count == 0)
                return 0;

            return cards[0].Value;
        }
    }
}