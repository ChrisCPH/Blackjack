using Blackjack.UI;
using System.Collections.ObjectModel;
using Terminal.Gui.App;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

namespace Blackjack.Classes
{
    public class GameSetup
    {
        private readonly UIManager _ui;

        public GameSetup(UIManager ui)
        {
            _ui = ui;
        }

        public async Task<int> AskPlayerCount() =>
            await ShowSelectionScreenAsync(
                "Welcome to Blackjack",
                "How many players?",
                ["1", "2", "3", "4", "5", "6", "7"]
            );

        public async Task<int> AskDeckCount() =>
            await ShowSelectionScreenAsync(
                "Game Setup",
                "How many decks?",
                ["1", "2", "4", "6", "8"]
            );

        private async Task<int> ShowSelectionScreenAsync(string title, string prompt, string[] options)
        {
            int result = int.Parse(options[0]);

            await _ui.InvokeAsync(() =>
            {
                var window = new Window
                {
                    Title = title,
                    X = 0,
                    Y = 0,
                    Width = Dim.Fill(),
                    Height = Dim.Fill()
                };

                var promptLabel = new Label
                {
                    Text = prompt,
                    X = Pos.Center(),
                    Y = Pos.Center() - 2,
                    Width = 20
                };

                var listView = new ListView
                {
                    X = Pos.Center() - 5,
                    Y = Pos.Bottom(promptLabel) + 1,
                    Width = 10,
                    Height = options.Length,
                    Source = new ListWrapper<string>(
                        new ObservableCollection<string>(options)
                    )
                };

                var okButton = new Button
                {
                    Title = "OK",
                    X = Pos.Center(),
                    Y = Pos.Bottom(listView) + 1,
                    IsDefault = true
                };

                okButton.Accepting += (s, e) =>
                {
                    result = int.Parse(options[listView.SelectedItem ?? 0]);
                    window.App?.RequestStop();
                };

                window.Add(promptLabel, listView, okButton);

                _ui.App.Run(window);
                window.Dispose();
            });

            return result;
        }
    }
}