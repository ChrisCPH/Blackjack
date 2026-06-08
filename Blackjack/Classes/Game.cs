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
                    if (_players.Count == 0)
                    {
                        await _ui.ShowMessageAsync("All players have been eliminated!");
                        break;
                    }

                    await PlayRoundAsync();

                    var eliminated = RemoveEliminatedPlayers();

                    foreach (var player in eliminated)
                    {
                        await _ui.ShowMessageAsync($"{player.Name} has been eliminated!");
                    }

                    if (_players.Count == 0)
                    {
                        await _ui.ShowMessageAsync("All players have been eliminated!");
                        break;
                    }

                    var choice = await _ui.ShowSelectionAsync("Same bets?", ["Yes", "No", "Quit"]);

                    if (choice == "Quit")
                    {
                        playAgain = false;
                    }
                    else if (choice == "Yes")
                    {
                        if (!_bets.TryApplySameBets(_players, _previousBets))
                            await _ui.ShowMessageAsync("One or more players can't afford their previous bet. Taking new bets.");
                        else
                            _betsAlreadyPlaced = true;
                    }
                }

                var restartChoice = await _ui.ShowSelectionAsync("Return to start screen?", ["Yes", "Exit game"]);
                restart = restartChoice == "Yes";
            }

            await _ui.ShowMessageAsync("Thanks for playing!");
            _ui.App.RequestStop();
        }

        private async Task PlayRoundAsync()
        {
            ResetRound();

            if (_deck.ShouldReshuffle)
            {
                await _ui.ShowMessageAsync("Reshuffling deck...");
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

            await _bets.TakePairBetsAsync(_players, _ui);

            if (_players.Count == 0)
            {
                await _ui.ShowMessageAsync("No players remaining. Game over!");
                return;
            }

            DealInitialCards();
            await NotifyUI();

            if (_dealer.Hand.Cards[0].Rank == "Ace")
                await _bets.TakeInsuranceBetsAsync(_players, _ui);

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

        private List<Player> RemoveEliminatedPlayers()
        {
            var eliminated = _players
                .Where(p => p.Balance < 10m)
                .ToList();

            foreach (var player in eliminated)
            {
                _players.Remove(player);
            }

            return eliminated;
        }

        private async Task ShowResultsAsync()
        {
            var sb = new StringBuilder();

            sb.AppendLine($"Dealer score: {_dealer.Hand.GetValue()}\n");

            foreach (var player in _players)
            {
                sb.AppendLine($"{player.Name} - Balance: {player.Balance}");

                foreach (var hand in player.Hands)
                {
                    sb.AppendLine(
                        $"Hand ({hand.GetValue()}): {hand.Result}\n" +
                        $"Bet: {hand.Bet}  Payout: {hand.Payout}  Net: {hand.Net}"
                    );

                    if (hand.PairBet > 0)
                        sb.AppendLine($"Pair Bet: {hand.PairBet}  Result: {hand.PairResult}  Payout: {hand.PairPayout}");

                    if (hand.InsuranceBet > 0)
                        sb.AppendLine($"Insurance: {hand.InsuranceBet}  Result: {(hand.InsuranceResult ? "Win" : "Lose")}  Payout: {hand.InsurancePayout}");
                }

                sb.AppendLine();
            }

            _dealerReveal = true;
            await NotifyUI();

            sb.AppendLine("Round finished. Continue?");
            await _ui.ShowMessageAsync(sb.ToString());
        }
    }
}