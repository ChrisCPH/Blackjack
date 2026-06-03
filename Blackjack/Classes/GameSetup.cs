using Spectre.Console;

namespace Blackjack.Classes
{
    public class GameSetup
    {
        public int AskPlayerCount()
        {
            var count = AnsiConsole.Prompt(
                new SelectionPrompt<int>()
                    .Title("How many players?")
                    .AddChoices(1, 2, 3, 4, 5, 6, 7));

            return count;
        }

        public int AskDeckCount()
        {
            var count = AnsiConsole.Prompt(
                new SelectionPrompt<int>()
                    .Title("How many decks?")
                    .AddChoices(1, 2, 6, 8));

            return count;
        }
    }
}