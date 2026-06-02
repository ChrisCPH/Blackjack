using Blackjack.Enums;
using Spectre.Console;

namespace Blackjack.Classes
{
    public class Bets
    {
        public void TakeBets(List<Player> players)
        {
            foreach (var player in players)
            {
                var bet = AskBet(player);

                player.RemoveMoney(bet);

                var hand = player.Hands[0];
                hand.Bet = bet;
            }
        }

        private decimal AskBet(Player player)
        {
            decimal bet = AnsiConsole.Ask<decimal>(
                $"[yellow]{player.Name}[/] - Balance: [green]{player.Balance}[/]\nEnter bet:");

            while (bet <= 0 || bet > player.Balance)
            {
                bet = AnsiConsole.Ask<decimal>(
                    $"Invalid bet. Try again ({player.Name}) - Balance: {player.Balance}");
            }

            return bet;
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

                        case HandResult.Lose:
                        case HandResult.Bust:
                            payout = 0;
                            break;
                    }

                    hand.Payout = payout;
                    player.AddMoney(payout);
                }
            }
        }
    }
}