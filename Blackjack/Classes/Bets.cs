using Blackjack.Enums;
using Blackjack.UI;
using Terminal.Gui.App;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

namespace Blackjack.Classes
{
    public class Bets
    {
        public async Task TakeBetsAsync(List<Player> players, UIManager ui)
        {
            foreach (var player in players)
            {
                var bet = await AskBetAsync(player, ui);
                player.RemoveMoney(bet);
                player.Hands[0].Bet = bet;
            }
        }

        private Task<decimal> AskBetAsync(Player player, UIManager ui)
        {
            const decimal minimumBet = 10m;
            var tcs = new TaskCompletionSource<decimal>();

            ui.App.Invoke(() => ShowBetScreen(player, minimumBet, ui, tcs));

            return tcs.Task;
        }

        private void ShowBetScreen(Player player, decimal minimumBet, UIManager ui, TaskCompletionSource<decimal> tcs, string? errorMessage = null)
        {
            var window = new Window
            {
                Title = "Place Your Bets",
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill()
            };

            var nameLabel = new Label
            {
                Text = $"{player.Name} — Balance: {player.Balance}",
                X = Pos.Center(),
                Y = Pos.Center() - 4,
                Width = Dim.Auto()
            };

            var promptLabel = new Label
            {
                Text = $"Enter bet (min {minimumBet}):",
                X = Pos.Center(),
                Y = Pos.Center() - 2,
                Width = Dim.Auto()
            };

            var betField = new TextField
            {
                X = Pos.Center(),
                Y = Pos.Center(),
                Width = 20,
                Text = minimumBet.ToString()
            };

            var errorLabel = new Label
            {
                Text = errorMessage ?? string.Empty,
                X = Pos.Center(),
                Y = Pos.Center() + 2,
                Width = Dim.Auto(),
                Visible = errorMessage != null
            };

            var okButton = new Button
            {
                Title = "OK",
                X = Pos.Center(),
                Y = Pos.Center() + 4,
                IsDefault = true
            };

            okButton.Accepting += (s, e) =>
            {
                if (!decimal.TryParse(betField.Text, out decimal bet))
                {
                    window.App?.RequestStop();
                    ui.App.Invoke(() => ShowBetScreen(player, minimumBet, ui, tcs, "Invalid amount. Please enter a number."));
                    return;
                }

                if (bet < minimumBet)
                {
                    window.App?.RequestStop();
                    ui.App.Invoke(() => ShowBetScreen(player, minimumBet, ui, tcs, $"Minimum bet is {minimumBet}."));
                    return;
                }

                if (bet > player.Balance)
                {
                    window.App?.RequestStop();
                    ui.App.Invoke(() => ShowBetScreen(player, minimumBet, ui, tcs, "Insufficient balance."));
                    return;
                }

                if (HasTooManyDecimals(bet))
                {
                    window.App?.RequestStop();
                    ui.App.Invoke(() => ShowBetScreen(player, minimumBet, ui, tcs, "Bets can only have up to 2 decimal places."));
                    return;
                }

                tcs.TrySetResult(bet);
                window.App?.RequestStop();
            };

            window.Add(nameLabel, promptLabel, betField, errorLabel, okButton);
            ui.App.Run(window);
            window.Dispose();
        }

        private bool HasTooManyDecimals(decimal value)
        {
            return value != Math.Round(value, 2, MidpointRounding.AwayFromZero);
        }

        public bool TryApplySameBets(List<Player> players, Dictionary<Player, decimal> previousBets)
        {
            foreach (var player in players)
            {
                if (!previousBets.TryGetValue(player, out decimal previousBet) || player.Balance < previousBet)
                    return false;
            }

            return true;
        }

        public bool CanAffordSplit(Player player, Hand hand)
        {
            return player.Balance >= hand.Bet;
        }

        public void TakeSplitBet(Player player, Hand originalHand, Hand newHand)
        {
            player.RemoveMoney(originalHand.Bet);
            newHand.Bet = originalHand.Bet;
        }

        public bool CanAffordDouble(Player player, Hand hand)
        {
            return player.Balance >= hand.Bet;
        }

        public void TakeDoubleBet(Player player, Hand hand)
        {
            player.RemoveMoney(hand.Bet);
            hand.Bet *= 2;
        }

        public void PayWinnings(List<Player> players, Dealer dealer)
        {
            foreach (var player in players)
            {
                foreach (var hand in player.Hands)
                {
                    decimal payout = 0;

                    switch (hand.Result)
                    {
                        case HandResult.Blackjack:
                            payout = hand.Bet * 2.5m;
                            break;
                        case HandResult.Win:
                            payout = hand.Bet * 2m;
                            break;
                        case HandResult.Push:
                            payout = hand.Bet;
                            break;
                        case HandResult.Surrender:
                            payout = hand.Bet * 0.5m;
                            break;
                        case HandResult.Lose:
                        case HandResult.Bust:
                            payout = 0;
                            break;
                    }

                    payout = Math.Round(payout, 2, MidpointRounding.AwayFromZero);
                    hand.Payout = payout;
                    player.AddMoney(payout);

                    if (hand.PairBet > 0)
                    {
                        decimal pairPayout = hand.PairResult switch
                        {
                            PairResult.PerfectPair => hand.PairBet * 26m,
                            PairResult.ColoredPair => hand.PairBet * 13m,
                            PairResult.MixedPair => hand.PairBet * 6m,
                            _ => 0m
                        };

                        pairPayout = Math.Round(pairPayout, 2, MidpointRounding.AwayFromZero);
                        hand.PairPayout = pairPayout;
                        player.AddMoney(pairPayout);
                    }

                    if (hand.InsuranceBet > 0)
                    {
                        decimal insurancePayout = hand.InsuranceResult
                            ? hand.InsuranceBet * 3m
                            : 0m;

                        insurancePayout = Math.Round(insurancePayout, 2, MidpointRounding.AwayFromZero);
                        hand.InsurancePayout = insurancePayout;
                        player.AddMoney(insurancePayout);
                    }
                }
            }
        }

        public async Task TakePairBetsAsync(List<Player> players, UIManager ui)
        {
            foreach (var player in players)
            {
                var bet = await AskPairBetAsync(player, ui);
                if (bet > 0)
                {
                    player.RemoveMoney(bet);
                    player.Hands[0].PairBet = bet;
                }
            }
        }

        private Task<decimal> AskPairBetAsync(Player player, UIManager ui)
        {
            var tcs = new TaskCompletionSource<decimal>();
            ui.App.Invoke(() => ui.ShowPairBetScreen(player, tcs));
            return tcs.Task;
        }

        public async Task TakeInsuranceBetsAsync(List<Player> players, UIManager ui)
        {
            foreach (var player in players)
            {
                var maxInsurance = Math.Floor(player.Hands[0].Bet / 2 * 100) / 100;

                if (player.Balance < 5m || maxInsurance < 5m)
                    continue;

                var bet = await ui.ShowBetInputAsync(
                    $"Dealer shows Ace — {player.Name} place insurance bet?",
                    5m,
                    maxInsurance,
                    player.Balance);

                if (bet > 0)
                {
                    player.RemoveMoney(bet);
                    player.Hands[0].InsuranceBet = bet;
                }
            }
        }
    }
}