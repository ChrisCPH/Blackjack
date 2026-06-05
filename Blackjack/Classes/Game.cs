using Blackjack.Enums;
using Blackjack.UI;
using System.Collections.ObjectModel;
using System.Text;
using Terminal.Gui.App;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

namespace Blackjack.Classes
{
    public class Game
    {
        private readonly List<Player> _players = [];
        private readonly Dealer _dealer = new();
        private Deck _deck = new();
        private bool _dealerReveal = false;
        private readonly UIManager _ui = new();
        public event Action<GameStateChangedEvent>? OnStateChanged;
        private Guid _activeHandId;
        private readonly Bets _bets = new();
        private bool _betsAlreadyPlaced = false;
        private Dictionary<Player, decimal> _previousBets = new();


        public Game()
        {
            OnStateChanged += _ui.Table;
        }

        private async Task NotifyUI()
        {
            var playerSnapshot = _players.ToList();
            var dealer = _dealer;
            var dealerReveal = _dealerReveal;
            var activeHandId = _activeHandId;

            await _ui.InvokeAsync(() =>
            {
                OnStateChanged?.Invoke(new GameStateChangedEvent
                {
                    Players = playerSnapshot,
                    Dealer = dealer,
                    DealerReveal = dealerReveal,
                    ActiveHandId = activeHandId,
                    Balance = playerSnapshot.Sum(p => p.Balance)
                });
            });
        }

        public void Start()
        {
            _ui.Init();

            _ui.App.AddTimeout(TimeSpan.Zero, () =>
            {
                Task.Run(async () => await RunGameLoop());
                return false;
            });

            _ui.Run();
        }

        private async Task RunGameLoop()
        {
            bool restart = true;

            while (restart)
            {
                _players.Clear();

                var setup = new GameSetup(_ui);
                var playerCount = await setup.AskPlayerCount();
                var deckCount = await setup.AskDeckCount();
                _deck = new Deck(deckCount);

                for (int i = 1; i <= playerCount; i++)
                    _players.Add(new Player($"Player{i}"));

                bool playAgain = true;

                while (playAgain)
                {
                    RemoveEliminatedPlayers();

                    if (_players.Count == 0)
                    {
                        await ShowMessageAsync("All players have been eliminated!");
                        break;
                    }

                    await PlayRoundAsync();

                    var choice = await ShowSelectionAsync("Same bets?", ["Yes", "No", "Quit"]);

                    if (choice == "Quit")
                    {
                        playAgain = false;
                    }
                    else if (choice == "Yes")
                    {
                        if (!_bets.TryApplySameBets(_players, _previousBets))
                            await ShowMessageAsync("One or more players can't afford their previous bet. Taking new bets.");
                        else
                            _betsAlreadyPlaced = true;
                    }
                }

                var restartChoice = await ShowSelectionAsync("Return to start screen?", ["Yes", "No exit game"]);
                restart = restartChoice == "Yes";
            }

            await ShowMessageAsync("Thanks for playing!");
            _ui.App.RequestStop();
        }

        private async Task PlayRoundAsync()
        {
            ResetRound();

            if (_deck.ShouldReshuffle)
            {
                await ShowMessageAsync("Reshuffling deck...");
                _deck.RebuildAndShuffle();
            }

            if (_betsAlreadyPlaced)
            {
                foreach (var player in _players)
                {
                    player.Hands[0].Bet = _previousBets[player];
                    player.RemoveMoney(_previousBets[player]);
                }
            }
            else
            {
                await _bets.TakeBetsAsync(_players, _ui);
            }

            _betsAlreadyPlaced = false;

            foreach (var player in _players)
                _previousBets[player] = player.Hands[0].Bet;

            if (_players.Count == 0)
            {
                await ShowMessageAsync("No players remaining. Game over!");
                return;
            }

            DealInitialCards();
            await NotifyUI();

            foreach (var player in _players)
                await PlayerTurnAsync(player);

            DealerTurn();

            HandEvaluator.Evaluate(_players, _dealer);

            _bets.PayWinnings(_players, _dealer);

            await NotifyUI();

            await ShowResultsAsync();
        }

        private void ResetRound()
        {
            foreach (var player in _players)
                player.Reset();

            _dealer.Hand = new Hand();
            _dealerReveal = false;
            _activeHandId = Guid.Empty;
        }

        private void DealInitialCards()
        {
            foreach (var player in _players)
            {
                var hand = player.Hands[0];
                hand.AddCard(_deck.DrawCard());
                hand.AddCard(_deck.DrawCard());
            }

            _dealer.Hand.AddCard(_deck.DrawCard());
            _dealer.Hand.AddCard(_deck.DrawCard());
        }

        private async Task PlayerTurnAsync(Player player)
        {
            Queue<Hand> queue = new();

            for (int i = 0; i < player.Hands.Count; i++)
                queue.Enqueue(player.Hands[i]);

            while (queue.Count > 0)
            {
                var hand = queue.Dequeue();
                var index = player.Hands.IndexOf(hand);

                while (hand.State == HandState.Active && !hand.IsBust)
                {
                    _activeHandId = hand.Id;
                    await NotifyUI();

                    var choices = new List<string> { "Hit", "Stand" };

                    if (hand.CanSplit())
                        choices.Add(_bets.CanAffordSplit(player, hand) ? "Split" : "Split (can't afford)");

                    if (hand.CanDouble())
                        choices.Add(_bets.CanAffordDouble(player, hand) ? "Double" : "Double (can't afford)");

                    if (hand.CanSurrender())
                        choices.Add("Surrender");

                    var choice = await _ui.ShowActionsAsync(
                        $"{player.Name} - Hand {index + 1}",
                        choices);

                    switch (choice)
                    {
                        case "Hit":
                            hand.AddCard(_deck.DrawCard());
                            if (hand.IsBust)
                                hand.State = HandState.Bust;
                            await NotifyUI();
                            break;

                        case "Split":
                            var newHand = hand.Split(_deck);
                            _bets.TakeSplitBet(player, hand, newHand);
                            player.Hands.Add(newHand);
                            queue.Enqueue(newHand);
                            await NotifyUI();
                            break;

                        case "Double":
                            _bets.TakeDoubleBet(player, hand);
                            hand.AddCard(_deck.DrawCard());
                            hand.State = hand.IsBust ? HandState.Bust : HandState.Finished;
                            await NotifyUI();
                            break;

                        case "Surrender":
                            hand.Result = HandResult.Surrender;
                            hand.State = HandState.Finished;
                            await NotifyUI();
                            break;

                        case "Stand":
                            hand.State = HandState.Finished;
                            await NotifyUI();
                            break;
                    }
                }
            }
        }

        private void DealerTurn()
        {
            while (_dealer.ShouldHit())
            {
                _dealer.Hand.AddCard(_deck.DrawCard());

                _dealerReveal = true;
            } 
        }

        private void RemoveEliminatedPlayers()
        {
            var eliminated = _players.Where(p => p.Balance < 10m).ToList();

            foreach (var player in eliminated)
                _players.Remove(player);
        }

        private async Task ShowResultsAsync()
        {
            var sb = new StringBuilder();

            sb.AppendLine($"Dealer score: {_dealer.Hand.GetValue()}\n");

            foreach (var player in _players)
            {
                sb.AppendLine($"{player.Name} - Balance: {player.Balance:C}");

                foreach (var hand in player.Hands)
                {
                    sb.AppendLine(
                        $"Hand ({hand.GetValue()}): {hand.Result}\n" +
                        $"Bet: {hand.Bet}  Payout: {hand.Payout}  Net: {hand.Net}"
                    );
                }

                sb.AppendLine();
            }

            _dealerReveal = true;
            await NotifyUI();

            sb.AppendLine("Round finished. Continue?");
            await ShowMessageAsync(sb.ToString());
        }

        private async Task<string> ShowSelectionAsync(string title, string[] choices)
        {
            var tcs = new TaskCompletionSource<string>();

            await _ui.InvokeAsync(() =>
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
                _ui.App.Run(dialog);
                dialog.Dispose();
            });

            return await tcs.Task;
        }

        private async Task ShowMessageAsync(string message)
        {
            var tcs = new TaskCompletionSource();

            await _ui.InvokeAsync(() =>
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
                _ui.App.Run(dialog);
                dialog.Dispose();
            });

            await tcs.Task;
        }
    }
}