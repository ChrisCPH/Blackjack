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
    }
}