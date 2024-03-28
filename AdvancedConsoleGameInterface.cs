namespace ConsoleMastermind
{
    public class AdvancedConsoleGameInterface : IGameInterface
    {
        public const int MessageBoxWidth = 60;

        private List<string> messageBox = new();

        private GameSession? game;

        /// <summary>
        /// Plays a session of the game.
        /// This method only returns once the game has ended.
        /// </summary>
        public void Play()
        {
            // Create a new game session
            game = new GameSession();

            SetMessageBoxText(
                "Welcome to Console Mastermind!",
                "", 
               $"Enter a {game.DigitCount}-digit number in the box in the corner",
                "and press Enter/Return to submit a guess.",
                "Your guess will be recorded in the left column, and",
                "a corresponding hint will appear in the right column.",
                "",
                "You win if you are able to guess the hidden number within",
               $"{game.GuessesRemaining} guesses.",
                "",
                "Good luck!",
                "(Enter 'quit' at any time to exit)".PadLeft(MessageBoxWidth)
            );

            // Game loop that continues until the game has ended.
            do
            {
                // Render game 'window' along with all of the current game state information
                Render();
                // Wait for and handle an input from the player
            } while (HandleInput());

            // Handle a game that ended with a win or loss. If the player chose to quit the game
            // before completion, this does not occur.
            if (game.IsGameOver)
            {
                SetMessageBoxText(
                    (game.IsGameWon ?
                        "You win! You correctly guessed the number: " :
                        "You lose - the number was: ")
                    + string.Concat(game.GetAnswerDigits().Select(d => (char)('0' + d))),
                    "",
                    "Thank you for playing.",
                    "",
                    "",
                    "Press any key to exit."
                );
                // Render one final time to ensure that the outcome of the game is displayed
                Render();

                // Block for a key press before exiting completely.
                Console.ReadKey();
            }

            // Return text cursor to normal position after ending game
            Console.SetCursorPosition(0, 4 + game.MaxGuesses);
        }

        /// <summary>
        /// Renders the game 'window' onto the console using unicode characters.
        /// </summary>
        private void Render()
        {
            int width = game!.DigitCount;

            Console.Clear();
            Console.WriteLine($"╔{new string('═', width)}╤{new string('═', width)}╦{new string('═', MessageBoxWidth)}╗");

            GameSession.GuessResponse[] guessHistory = game.GetGuessHistory();
            for (int i = 0; i < guessHistory.Length; i++)
            {
                GameSession.GuessResponse guessResponse = guessHistory[i];
                string guessString = guessResponse.GuessDigits != null ? 
                    string.Concat(guessResponse.GuessDigits.Select(d => (char)('0' + d))) : 
                    new string('·', width);

                string hintString =
                    (new string('+', guessResponse.ExactMatches) + new string('-', guessResponse.DigitMatches))
                    .PadRight(width, '·');

                Console.WriteLine($"║{guessString}│{hintString}║{(messageBox.Count > i ? messageBox[i] : ""),-MessageBoxWidth}║");
            }

            Console.WriteLine($"╟{new string('─', width)}┼{new string('─', width)}╢{(messageBox.Count > guessHistory.Length ? messageBox[guessHistory.Length] : ""),-MessageBoxWidth}║");
            Console.WriteLine($"║{new string('_', width)}│{game.GuessesRemaining.ToString().PadRight(game.DigitCount)}║{(messageBox.Count > guessHistory.Length + 1 ? messageBox[guessHistory.Length + 1] : ""),-MessageBoxWidth}║");
            Console.WriteLine($"╚{new string('═', width)}╧{new string('═', width)}╩{new string('═', MessageBoxWidth)}╝");
        }

        /// <summary>
        /// Blocks and handles input from the player. Also returns a value indicating whether
        /// the game should continue after handling the player's input.
        /// </summary>
        /// <returns>false if the game should end, otherwise true</returns>
        private bool HandleInput()
        {
            // Position cursor inside box to await input
            Console.SetCursorPosition(1, 2 + game!.MaxGuesses);

            List<char> inputChars = new(game.DigitCount);
            // Continue until the player has pressed enter/return
            while (true)
            {
                int cursorPos = Console.CursorLeft;
                ConsoleKeyInfo input = Console.ReadKey();
                // Handles typing within the entry box, without letting the text cursor 'escape' the box.
                switch (input.KeyChar)
                {
                    case '\r':
                    case '\n':
                        // Escapes the loop by returning from the method here
                        return ProcessInput(string.Concat(inputChars)) && !game.IsGameOver;
                    case '\b':
                        if (inputChars.Count > 0)
                        {
                            inputChars.RemoveAt(inputChars.Count - 1);
                            Console.Write('_');
                            cursorPos--;
                        }
                        break;
                    default:
                        if (char.IsControl(input.KeyChar)) continue;
                        if (inputChars.Count < game.DigitCount)
                        {
                            inputChars.Add(input.KeyChar);
                            cursorPos++;
                        }
                        else
                        {
                            Console.CursorLeft = cursorPos;
                            Console.Write('│');
                        }
                        break;
                }
                Console.CursorLeft = cursorPos;
            }
        }

        /// <summary>
        /// Processes a string input by the player, and acts accordingly. Also returns
        /// a value indicating whether the game should continue after processing the input.
        /// </summary>
        /// <param name="input">The string input by the player to process</param>
        /// <returns>false if the game should end, otherwise true</returns>
        private bool ProcessInput(string input)
        {
            if (input.ToLower() == "quit")
            {
                return false;
            }

            if (input.Length != game!.DigitCount || !input.All(char.IsDigit))
            {
                SetMessageBoxText(
                    $"You must enter a {game.DigitCount}-digit number!"
                );
                return true;
            }

            if (input.Any(c => c <= '0' || c > (char)('0' + game.DigitMaxValue)))
            {
                SetMessageBoxText(
                    $"All digits must be between 1 and {game.DigitMaxValue}!"
                );
                return true;
            }

            if (!game.Guess(input.Select(c => (byte)(c - '0')).ToArray()).IsWin)
            {
                SetMessageBoxText(
                    $"You have {game.GuessesRemaining} guesses remaining."
                );
            }

            return !game.IsGameOver;
        }

        private void SetMessageBoxText(params string[] lines)
        {
            messageBox = lines.ToList();
        }
    }
}
