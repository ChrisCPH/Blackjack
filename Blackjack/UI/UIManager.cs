using Terminal.Gui.App;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;
using Blackjack.Classes;

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
    }
}