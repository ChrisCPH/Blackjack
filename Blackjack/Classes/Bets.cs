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
            const decimal minimumBet = 10m;

            decimal bet = AnsiConsole.Ask<decimal>(
                $"[yellow]{player.Name}[/] - Balance: [green]{player.Balance:F2}[/]\nEnter bet (min {minimumBet}):");

            while (bet < minimumBet || bet > player.Balance || HasTooManyDecimals(bet))
            {
                string reason = bet < minimumBet
                    ? $"Minimum bet is {minimumBet}."
                    : bet > player.Balance
                        ? $"Insufficient balance."
                        : "Bets can only have up to 2 decimal places.";

                bet = AnsiConsole.Ask<decimal>(
                    $"{reason} Try again ({player.Name}) - Balance: {player.Balance:F2} Enter bet:");
            }

            return bet;
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
                }
            }
        }
    }
}