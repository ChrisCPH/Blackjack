using Blackjack.Enums;
using Blackjack.UI;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

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

        public Game()
        {
            OnStateChanged += _ui.Table;
        }

        private void NotifyUI()
        {
            OnStateChanged?.Invoke(new GameStateChangedEvent
            {
                Players = _players,
                Dealer = _dealer,
                DealerReveal = _dealerReveal,
                ActiveHandId = _activeHandId,
                Balance = _players.Sum(p => p.Balance)
            });
        }

        public void Start()
        {
            bool restart = true;

            while (restart)
            {
                _players.Clear();

                var setup = new GameSetup();
                var playerCount = setup.AskPlayerCount();
                var deckCount = setup.AskDeckCount();
                _deck = new Deck(deckCount);

                for (int i = 1; i <= playerCount; i++)
                {
                    _players.Add(new Player($"Player{i}"));
                }

                bool playAgain = true;

                while (playAgain)
                {
                    PlayRound();

                    if (_players.Count == 0)
                    {
                        AnsiConsole.MarkupLine("[red]All players have been eliminated![/]");
                        break;
                    }

                    playAgain = AskPlayAgain();
                }

                restart = AnsiConsole.Confirm("Return to start screen?");
            }

            Console.WriteLine("Thanks for playing!");
        }

        private void PlayRound()
        {
            ResetRound();
            RemoveEliminatedPlayers();

            if (_players.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]No players remaining. Game over![/]");
                return;
            }

            if (_deck.ShouldReshuffle)
            {
                Console.WriteLine("Reshuffling deck...");
                _deck.RebuildAndShuffle();
            }

            _bets.TakeBets(_players);

            DealInitialCards();
            NotifyUI();

            foreach (var player in _players)
            {
                PlayerTurn(player);
            }

            DealerTurn();

            HandEvaluator.Evaluate(_players, _dealer);

            _bets.PayWinnings(_players, _dealer);

            NotifyUI();

            ShowResults();
        }

        private void ResetRound()
        {
            foreach (var player in _players)
            {
                player.Reset();
            }

            _dealer.Hand = new Hand();
            _dealerReveal = false;
            _activeHandId = Guid.Empty;
        }

        private bool AskPlayAgain()
        {
            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Play again?")
                    .AddChoices("Yes", "No"));
            AnsiConsole.Clear();
            return choice == "Yes";
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

        private void PlayerTurn(Player player)
        {
            Queue<Hand> queue = new();

            for (int i = 0; i < player.Hands.Count; i++)
            {
                queue.Enqueue(player.Hands[i]);
            }

            while (queue.Count > 0)
            {
                var hand = queue.Dequeue();
                var index = player.Hands.IndexOf(hand);

                while (hand.State == HandState.Active && !hand.IsBust)
                {
                    _activeHandId = hand.Id;
                    NotifyUI();

                    var choices = new List<string> { "Hit", "Stand" };

                    if (hand.CanSplit())
                        choices.Add(_bets.CanAffordSplit(player, hand) ? "Split" : "[grey]Split (can't afford)[/]");

                    if (hand.CanDouble())
                        choices.Add(_bets.CanAffordDouble(player, hand) ? "Double" : "[grey]Double (can't afford)[/]");

                    if (hand.CanSurrender())
                        choices.Add("Surrender");

                    var choice = AnsiConsole.Prompt(
                        new SelectionPrompt<string>()
                            .Title($"What do you want to do? (Hand {index + 1})")
                            .AddChoices(choices));

                    switch (choice)
                    {
                        case "Hit":
                            hand.AddCard(_deck.DrawCard());
                            if (hand.IsBust)
                                hand.State = HandState.Bust;
                            NotifyUI();
                            break;

                        case "Split":
                            var newHand = hand.Split(_deck);
                            _bets.TakeSplitBet(player, hand, newHand);
                            player.Hands.Add(newHand);
                            queue.Enqueue(newHand);
                            NotifyUI();
                            break;

                        case "Double":
                            _bets.TakeDoubleBet(player, hand);
                            hand.AddCard(_deck.DrawCard());
                            hand.State = hand.IsBust ? HandState.Bust : HandState.Finished;
                            NotifyUI();
                            break;

                        case "Surrender":
                            hand.Result = HandResult.Surrender;
                            hand.State = HandState.Finished;
                            NotifyUI();
                            break;

                        case "Stand":
                            hand.State = HandState.Finished;
                            NotifyUI();
                            break;

                        case "[grey]Split (can't afford)[/]":

                        case "[grey]Double (can't afford)[/]":
                            break;
                    }
                }
            }
        }

        private void DealerTurn()
        {

            while (_dealer.ShouldHit())
            {
                var card = _deck.DrawCard();
                _dealer.Hand.AddCard(card);

                NotifyUI();
            }

            _dealerReveal = true;
        }

        private void RemoveEliminatedPlayers()
        {
            var eliminated = _players.Where(p => p.Balance < 10m).ToList();

            foreach (var player in eliminated)
            {
                AnsiConsole.MarkupLine($"[red]{player.Name} has been eliminated with a balance of {player.Balance}![/]");
                _players.Remove(player);
            }
        }

        private void ShowResults()
        {
            AnsiConsole.WriteLine("\nResults:\n");

            AnsiConsole.WriteLine($"Dealer score: ({_dealer.Hand.GetValue()})\n");

            foreach (var player in _players)
            {
                AnsiConsole.MarkupLine($"[yellow]{player.Name}[/] - Balance: [green]{player.Balance}[/]");

                foreach (var hand in player.Hands)
                {
                    AnsiConsole.MarkupLine(
                        $"Hand ({hand.GetValue()}): {hand.Result}\n" +
                        $"Bet: [bold green]{hand.Bet}[/]\n" +
                        $"Payout: [bold green]{hand.Payout}[/]\n" +
                        $"Net: [bold green]{hand.Net}[/]\n"
                    );
                }

                AnsiConsole.WriteLine();
            }
        }
    }
}