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

        public Game()
        {
            _deck = new Deck();
            _deck.Shuffle();
        }


        public void Start()
        {
            _players.Add(new Player("Player1"));

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
            _ui.Table(_players, _dealer, _dealerReveal);

            foreach (var player in _players)
            {
                PlayerTurn(player);
            }

            DealerTurn();
            _ui.Table(_players, _dealer, _dealerReveal);

            ShowResults();
        }

        private void ResetRound()
        {
            foreach (var player in _players)
            {
                foreach (var hand in player.Hands)
                {
                    hand.Cards.Clear();
                }

                player.Hands.Clear();
                player.Hands.Add(new Hand());
                player.ActiveHandIndex = 0;
            }

            _dealerReveal = false;
            _dealer.Hand.Cards.Clear();
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
                if (player.Hands.Count == 0)
                    player.Hands.Add(new Hand());

                var hand = player.Hands[0];
                hand.AddCard(_deck.DrawCard());
                hand.AddCard(_deck.DrawCard());
            }

            _dealer.Hand.AddCard(_deck.DrawCard());
            _dealer.Hand.AddCard(_deck.DrawCard());
        }

        private void PlayerTurn(Player player)
        {
            for (int h = 0; h < player.Hands.Count; h++)
            {
                player.ActiveHandIndex = h;

                var hand = player.Hands[h];

                while (true)
                {
                    if (hand.IsBust)
                    {
                        Console.WriteLine("BUST!");
                        break;
                    }

                    var choices = new List<string> { "Hit", "Stand" };

                    if (player.CanSplit())
                        choices.Add("Split");

                    var choice = AnsiConsole.Prompt(
                        new SelectionPrompt<string>()
                            .Title($"What do you want to do? (Hand {h + 1})")
                            .AddChoices(choices));

                    if (choice == "Split")
                    {
                        player.Split(_deck);
                        _ui.Table(_players, _dealer, _dealerReveal);
                        continue;
                    }

                    if (choice == "Hit")
                    {
                        hand.AddCard(_deck.DrawCard());
                        _ui.Table(_players, _dealer, _dealerReveal);
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        private void DealerTurn()
        {
            _ui.Table(_players, _dealer, _dealerReveal);

            while (_dealer.ShouldHit())
            {
                var card = _deck.DrawCard();
                _dealer.Hand.AddCard(card);

                _ui.Table(_players, _dealer, _dealerReveal);
            }

            _dealerReveal = true;
            _ui.Table(_players, _dealer, _dealerReveal);
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