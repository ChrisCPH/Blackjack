using Blackjack.Classes;
using System.Collections.ObjectModel;
using Terminal.Gui.App;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

namespace Blackjack.UI
{
    public class UIManager
    {
        private IApplication _app = null!;
        private Window _mainWindow = null!;
        private FrameView _dealerFrame = null!;
        private FrameView _playerFrame = null!;
        private FrameView _sidebarFrame = null!;
        private FrameView _actionsFrame = null!;

        private Label _dealerView = null!;
        private Label _playerView = null!;
        private Label _sidebarView = null!;

        private List<Button> _actionButtons = [];
        private TaskCompletionSource<string>? _actionPending;

        public IApplication App => _app;

        public void Init()
        {
            _app = Application.Create();
            _app.Init();

            _mainWindow = new Window
            {
                Title = "Blackjack",
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill()
            };

            _sidebarFrame = new FrameView
            {
                Title = "Balances",
                X = Pos.AnchorEnd(22),
                Y = 0,
                Width = 22,
                Height = Dim.Fill()
            };

            _sidebarView = new Label
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill()
            };

            _sidebarFrame.Add(_sidebarView);

            _dealerFrame = new FrameView
            {
                Title = "Dealer",
                X = 0,
                Y = 0,
                Width = Dim.Fill(22),
                Height = 12
            };

            _dealerView = new Label
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill()
            };

            _dealerFrame.Add(_dealerView);

            _playerFrame = new FrameView
            {
                Title = "Players",
                X = 0,
                Y = 12,
                Width = Dim.Fill(22),
                Height = Dim.Fill(4)
            };

            _playerFrame.ViewportSettings |= ViewportSettingsFlags.HasVerticalScrollBar;
            _playerFrame.SetContentHeight(80); // Forced height to allow scrolling

            _playerView = new Label
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill()
            };

            _playerFrame.Add(_playerView);

            _actionsFrame = new FrameView
            {
                Title = "Actions",
                X = 0,
                Y = Pos.AnchorEnd(4),
                Width = Dim.Fill(22),
                Height = 4
            };

            _mainWindow.Add(_sidebarFrame, _dealerFrame, _playerFrame, _actionsFrame);
        }

        public void Table(GameStateChangedEvent state)
        {
            _app.Invoke(() =>
            {
                UpdateDealer(state);
                UpdatePlayers(state);
                UpdateSidebar(state);
            });
        }

        private void UpdateDealer(GameStateChangedEvent state)
        {
            _dealerView.Text = HandString(
                state.Dealer.Hand.Cards,
                state.DealerReveal
                    ? state.Dealer.Hand.GetValue()
                    : GetVisibleDealerValue(state.Dealer.Hand.Cards),
                hideFirst: !state.DealerReveal
            );
        }

        private void UpdatePlayers(GameStateChangedEvent state)
        {
            var sb = new System.Text.StringBuilder();

            foreach (var player in state.Players)
            {
                for (int i = 0; i < player.Hands.Count; i++)
                {
                    var hand = player.Hands[i];
                    var isActive = state.ActiveHandId == hand.Id;

                    sb.AppendLine(isActive
                        ? $">>> {player.Name} - Hand {i + 1} (PLAYING) <<<"
                        : $"{player.Name} - Hand {i + 1}");

                    sb.AppendLine(HandString(hand.Cards, hand.GetValue()));
                }
            }

            _playerView.Text = sb.ToString();
        }

        private void UpdateSidebar(GameStateChangedEvent state)
        {
            var sb = new System.Text.StringBuilder();

            foreach (var player in state.Players)
            {
                sb.AppendLine(player.Name);
                sb.AppendLine($"${player.Balance:F2}");
                sb.AppendLine();
            }

            _sidebarView.Text = sb.ToString();
        }

        public async Task<string> ShowActionsAsync(string title, List<string> choices)
        {
            _actionPending = new TaskCompletionSource<string>();

            await InvokeAsync(() =>
            {
                _actionsFrame.RemoveAll();
                _actionButtons.Clear();

                int x = 1;

                foreach (var choice in choices)
                {
                    var label = choice;
                    var btn = new Button
                    {
                        Title = label,
                        X = x,
                        Y = 1
                    };

                    btn.Accepting += (s, e) =>
                    {
                        _actionPending?.TrySetResult(label);
                    };

                    _actionButtons.Add(btn);
                    _actionsFrame.Add(btn);
                    x += label.Length + 6;
                }

                _actionsFrame.Title = title;
            });

            return await _actionPending.Task;
        }

        public void ClearActions()
        {
            _app.Invoke(() =>
            {
                _actionsFrame.RemoveAll();
                _actionButtons.Clear();
            });
        }

        public void Run()
        {
            _app.Run(_mainWindow);
            _app.Dispose();
        }

        public Task InvokeAsync(Action action)
        {
            var tcs = new TaskCompletionSource();
            _app.Invoke(() =>
            {
                action();
                tcs.SetResult();
            });
            return tcs.Task;
        }

        private string HandString(List<Card> cards, int score, bool hideFirst = false)
        {
            var cardStrings = new List<string>();

            for (int i = 0; i < cards.Count; i++)
            {
                if (hideFirst && i == 1)
                    cardStrings.Add(HiddenCard());
                else
                    cardStrings.Add(CardString(cards[i]));
            }

            if (cardStrings.Count == 0)
                return "(No cards)";

            var cardLines = cardStrings
                .Select(c => c.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None))
                .ToList();

            int height = cardLines.Max(c => c.Length);
            var result = new System.Text.StringBuilder();

            for (int i = 0; i < height; i++)
            {
                foreach (var card in cardLines)
                    result.Append(i < card.Length ? card[i] + "  " : "            ");
                result.AppendLine();
            }

            result.AppendLine($"Score: {score}");
            return result.ToString();
        }

        private string CardString(Card card)
        {
            string suit = card.Suit switch
            {
                "Hearts" => "♥",
                "Diamonds" => "♦",
                "Clubs" => "♣",
                "Spades" => "♠",
                _ => "?"
            };

            string rank = card.Rank switch
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
                │ {rank,-2}      │
                │         │
                │    {suit}    │
                │         │
                │      {rank,2} │
                └─────────┘
                """;
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
            if (cards.Count == 0) return 0;
            return cards[0].Value;
        }

        public async Task<string> ShowSelectionAsync(string title, string[] choices)
        {
            var tcs = new TaskCompletionSource<string>();

            await InvokeAsync(() =>
            {
                var dialog = new Dialog
                {
                    Title = title,
                    Width = 40,
                    Height = choices.Length + 6
                };

                var listView = new ListView
                {
                    X = 1,
                    Y = 1,
                    Width = Dim.Fill(1),
                    Height = choices.Length,
                    Source = new ListWrapper<string>(new ObservableCollection<string>(choices))
                };

                listView.Accepting += (s, e) =>
                {
                    tcs.TrySetResult(choices[listView.SelectedItem ?? 0]);
                    dialog.App?.RequestStop();
                };

                dialog.Add(listView, listView);
                App.Run(dialog);
                dialog.Dispose();
            });

            return await tcs.Task;
        }

        public async Task<decimal> ShowBetInputAsync(string title, decimal min, decimal max, decimal balance, string? errorMessage = null)
        {
            var tcs = new TaskCompletionSource<decimal>();

            await InvokeAsync(() =>
            {
                var dialog = new Dialog
                {
                    Title = title,
                    Width = Math.Max(50, title.Length + 6),
                    Height = 10
                };

                var infoLabel = new Label
                {
                    Text = $"Balance: {balance}  |  Min: {min}  Max: {max}",
                    X = 1,
                    Y = 1,
                    Width = Dim.Fill(1)
                };

                var errorLabel = new Label
                {
                    Text = errorMessage ?? string.Empty,
                    X = 1,
                    Y = 2,
                    Width = Dim.Fill(1),
                    Visible = errorMessage != null
                };

                var betField = new TextField
                {
                    X = 1,
                    Y = 3,
                    Width = Dim.Fill(1),
                    Text = min.ToString()
                };

                var okButton = new Button
                {
                    Title = "OK",
                    X = Pos.Center() - 4,
                    Y = 5,
                    IsDefault = false
                };

                var skipButton = new Button
                {
                    Title = "Skip",
                    X = Pos.Center() + 4,
                    Y = 5,
                    IsDefault = true
                };

                okButton.Accepting += (s, e) =>
                {
                    if (!decimal.TryParse(betField.Text, out decimal bet))
                    {
                        dialog.App?.RequestStop();
                        _ = ShowBetInputAsync(title, min, max, balance, "Invalid amount.");
                        return;
                    }

                    if (bet < min)
                    {
                        dialog.App?.RequestStop();
                        _ = ShowBetInputAsync(title, min, max, balance, $"Minimum bet is {min}.");
                        return;
                    }

                    if (bet > max)
                    {
                        dialog.App?.RequestStop();
                        _ = ShowBetInputAsync(title, min, max, balance, $"Maximum bet is {max}.");
                        return;
                    }

                    if (bet > balance)
                    {
                        dialog.App?.RequestStop();
                        _ = ShowBetInputAsync(title, min, max, balance, "Insufficient balance.");
                        return;
                    }

                    if (bet != Math.Round(bet, 2, MidpointRounding.AwayFromZero))
                    {
                        dialog.App?.RequestStop();
                        _ = ShowBetInputAsync(title, min, max, balance, "Max 2 decimal places.");
                        return;
                    }

                    tcs.TrySetResult(bet);
                    dialog.App?.RequestStop();
                };

                skipButton.Accepting += (s, e) =>
                {
                    tcs.TrySetResult(0);
                    dialog.App?.RequestStop();
                };

                dialog.Add(infoLabel, errorLabel, betField, okButton, skipButton);
                _app.Run(dialog);
                dialog.Dispose();
            });

            return await tcs.Task;
        }

        public async Task ShowMessageAsync(string message)
        {
            var tcs = new TaskCompletionSource();

            await InvokeAsync(() =>
            {
                var lines = message.Split('\n');
                var width = Math.Min(80, lines.Max(l => l.Length) + 6);
                var height = Math.Min(20, lines.Length + 6);

                var dialog = new Dialog
                {
                    Title = "Blackjack",
                    Width = width,
                    Height = lines.Length + 10
                };

                var label = new Label
                {
                    Text = message,
                    X = 1,
                    Y = 1,
                    Width = Dim.Fill(1),
                    Height = lines.Length
                };

                var okButton = new Button
                {
                    Title = "OK",
                    X = Pos.Center(),
                    Y = Pos.Bottom(label) + 1,
                    IsDefault = true
                };

                okButton.Accepting += (s, e) =>
                {
                    tcs.TrySetResult();
                    dialog.App?.RequestStop();
                };

                dialog.Add(label, okButton);
                App.Run(dialog);
                dialog.Dispose();
            });

            await tcs.Task;
        }

        public void ShowPairBetScreen(Player player, TaskCompletionSource<decimal> tcs, string? errorMessage = null)
        {
            var window = new Window
            {
                Title = "Pair Side Bet",
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill()
            };

            var nameLabel = new Label
            {
                Text = $"{player.Name} — Balance: {player.Balance}",
                X = Pos.Center(),
                Y = Pos.Center() - 6,
                Width = Dim.Auto()
            };

            var infoLabel = new Label
            {
                Text = "Place a Pair side bet? (Mixed 5:1 | Colored 12:1 | Perfect 25:1)",
                X = Pos.Center(),
                Y = Pos.Center() - 4,
                Width = Dim.Auto()
            };

            var promptLabel = new Label
            {
                Text = "Enter bet amount (min 10) or skip:",
                X = Pos.Center(),
                Y = Pos.Center() - 2,
                Width = Dim.Auto()
            };

            var betField = new TextField
            {
                X = Pos.Center(),
                Y = Pos.Center(),
                Width = 20,
                Text = "10"
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
                Title = "Place Bet",
                X = Pos.Center() - 8,
                Y = Pos.Center() + 4
            };

            var skipButton = new Button
            {
                Title = "Skip",
                X = Pos.Center() + 4,
                Y = Pos.Center() + 4,
                IsDefault = true
            };

            okButton.Accepting += (s, e) =>
            {
                if (!decimal.TryParse(betField.Text, out decimal bet))
                {
                    window.App?.RequestStop();
                    App.Invoke(() => ShowPairBetScreen(player, tcs, "Invalid amount. Please enter a number."));
                    return;
                }

                if (bet < 10m)
                {
                    window.App?.RequestStop();
                    App.Invoke(() => ShowPairBetScreen(player, tcs, "Minimum pair bet is £10."));
                    return;
                }

                if (bet > player.Balance)
                {
                    window.App?.RequestStop();
                    App.Invoke(() => ShowPairBetScreen(player, tcs, "Insufficient balance."));
                    return;
                }

                if (HasTooManyDecimals(bet))
                {
                    window.App?.RequestStop();
                    App.Invoke(() => ShowPairBetScreen(player, tcs, "Bets can only have up to 2 decimal places."));
                    return;
                }

                tcs.TrySetResult(bet);
                window.App?.RequestStop();
            };

            skipButton.Accepting += (s, e) =>
            {
                tcs.TrySetResult(0);
                window.App?.RequestStop();
            };

            window.Add(nameLabel, infoLabel, promptLabel, betField, errorLabel, okButton, skipButton);
            App.Run(window);
            window.Dispose();
        }

        private bool HasTooManyDecimals(decimal value)
        {
            return value != Math.Round(value, 2, MidpointRounding.AwayFromZero);
        }
    }
}