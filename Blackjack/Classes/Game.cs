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
        private readonly Deck _deck = new();
        private bool _dealerReveal = false;
        private readonly UIManager _ui = new();
        public event Action<GameStateChangedEvent>? OnStateChanged;
        private Guid _activeHandId;

        public Game()
        {
            _deck = new Deck();
            _deck.Shuffle();
        }

        private void NotifyUI()
        {
            OnStateChanged?.Invoke(new GameStateChangedEvent
            {
                Players = _players,
                Dealer = _dealer,
                DealerReveal = _dealerReveal,
                ActiveHandId = _activeHandId
            });
        }

        public void Start()
        {
            _players.Add(new Player("Player1"));

            OnStateChanged += _ui.Table;

            _deck.Shuffle();

            bool playAgain = true;

            while (playAgain)
            {
                PlayRound();

                playAgain = AskPlayAgain();

                if (playAgain)
                {
                    ResetRound();
                }
            }

            Console.WriteLine("Thanks for playing!");
        }

        private void PlayRound()
        {
            if (_deck.ShouldReshuffle)
            {
                Console.WriteLine("Reshuffling deck...");
                _deck.RebuildAndShuffle();
            }

            DealInitialCards();
            NotifyUI();

            foreach (var player in _players)
            {
                PlayerTurn(player);
            }

            DealerTurn();
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
                var hand = new Hand();

                hand.AddCard(_deck.DrawCard());
                hand.AddCard(_deck.DrawCard());

                player.Hands.Add(hand);
            }

            _dealer.Hand.AddCard(_deck.DrawCard());
            _dealer.Hand.AddCard(_deck.DrawCard());
        }

        private void PlayerTurn(Player player)
        {
            Queue<Hand> queue = new();

            for (int i = 0; i < player.Hands.Count; i++)
            {
                queue.Enqueue((player.Hands[i]));
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
                        choices.Add("Split");

                    var choice = AnsiConsole.Prompt(
                        new SelectionPrompt<string>()
                            .Title($"What do you want to do? (Hand {index + 1})")
                            .AddChoices(choices));

                    if (choice == "Hit")
                    {
                        hand.AddCard(_deck.DrawCard());

                        if (hand.IsBust)
                            hand.State = HandState.Bust;

                        NotifyUI();
                    }
                    else if (choice == "Split")
                    {
                        var newHand = hand.Split(_deck);

                        player.Hands.Add(newHand);
                        queue.Enqueue(newHand);

                        NotifyUI();
                    }
                    else
                    {
                        hand.State = HandState.Finished;
                        NotifyUI();
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

        private void ShowResults()
        {
            HandEvaluator.Evaluate(_players, _dealer);

            Console.WriteLine("\nResults:\n");

            Console.WriteLine($"Dealer score: ({_dealer.Hand.GetValue()})\n");

            foreach (var player in _players)
            {
                Console.WriteLine(player.Name);

                foreach (var hand in player.Hands)
                {
                    Console.WriteLine($"Hand ({hand.GetValue()}): {hand.Result}");
                }

                Console.WriteLine();
            }

        }
    }
}