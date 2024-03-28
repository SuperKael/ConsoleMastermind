using System.Diagnostics;

namespace ConsoleMastermind
{
    public class SimpleConsoleGameInterface : IGameInterface
    {
        private GameSession? game;

        /// <summary>
        /// Plays a session of the game.
        /// This method only returns once the game has ended.
        /// </summary>
        public void Play()
        {
            game = new GameSession();

            Console.Clear();
            Console.WriteLine("Welcome to Console Mastermind!");
            Console.WriteLine();
            Console.WriteLine($"Type a {game.DigitCount}-digit number and press Enter/Return to submit a guess.");
            Console.WriteLine("Each guess with result in a corresponding hint.");
            Console.WriteLine($"You win if you are able to guess the hidden number within {game.GuessesRemaining} guesses.");
            Console.WriteLine();
            Console.WriteLine("Good luck! (Enter 'quit' at any time to exit)");
            Console.WriteLine();

            // Game loop that continues until the game has ended.
            while (HandleInput());

            // Handle a game that ended with a win or loss. If the player chose to quit the game
            // before completion, this does not occur.
            if (game.IsGameOver)
            {
                Console.WriteLine(
                    (game.IsGameWon ? "You win! You correctly guessed the number: " : "You lose - the number was: ")
                    + string.Concat(game.GetAnswerDigits().Select(d => (char)('0' + d))));
                Console.WriteLine();
                Console.WriteLine("Thank you for playing.");
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine("Press any key to exit.");

                // Block for a key press before exiting completely.
                Console.ReadKey();
            }
        }

        /// <summary>
        /// Blocks and handles input from the player. Also returns a value indicating whether
        /// the game should continue after handling the player's input.
        /// </summary>
        /// <returns>false if the game should end, otherwise true</returns>
        private bool HandleInput()
        {
            Console.Write("Your Guess: ");
            string? input = Console.ReadLine();
            if (input == null) return false; // Caused by entering Ctrl+Z followed by Enter/Return, and terminates console input.
            return ProcessInput(input);
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
                Console.WriteLine($"You must enter a {game.DigitCount}-digit number!");
                return true;
            }

            if (input.Any(c => c <= '0' || c > (char)('0' + game.DigitMaxValue)))
            {
                Console.WriteLine($"All digits must be between 1 and {game.DigitMaxValue}!");
                return true;
            }

            GameSession.GuessResponse guessResponse = game.Guess(input.Select(c => (byte)(c - '0')).ToArray());

            if (!guessResponse.IsWin)
            {
                Console.WriteLine(new string('+', guessResponse.ExactMatches) + new string('-', guessResponse.DigitMatches));
                Console.WriteLine();
                Console.WriteLine($"You have {game.GuessesRemaining} guesses remaining.");
            }

            return !game.IsGameOver;
        }
    }
}
