namespace ConsoleMastermind
{
    public class GameSession
    {
        public const int DefaultDigitCount = 4;
        public const int DefaultDigitMaxValue = 6;
        public const int DefaultMaxGuesses = 10;

        private readonly byte[] answerDigits;
        private readonly int[] answerDigitCounts;
        private readonly GuessResponse[] guessHistory;
        
        public int DigitCount { get; }
        public byte DigitMaxValue { get; }
        public int GuessCount { get; private set; }
        public int MaxGuesses => guessHistory.Length;
        public int GuessesRemaining => MaxGuesses - GuessCount;
        public bool IsGameWon => GuessCount > 0 && guessHistory[GuessCount - 1].IsWin;
        public bool IsGameOver => GuessesRemaining <= 0 || IsGameWon;

        /// <summary>
        /// Represents a session of a Mastermind game, with the specified settings.
        /// </summary>
        /// <param name="digitCount">The number of digits to include in the answerDigits, and expect from the guesses</param>
        /// <param name="digitMaxValue">The maximum value that a given digit can be, inclusive</param>
        /// <param name="maxGuesses">The maximum amount of guesses permitted</param>
        /// <param name="randomSeed"></param>
        public GameSession(int digitCount = DefaultDigitCount, byte digitMaxValue = DefaultDigitMaxValue, int maxGuesses = DefaultMaxGuesses, int? randomSeed = null)
        {
            DigitCount = digitCount;
            DigitMaxValue = digitMaxValue;
            answerDigits = new byte[digitCount];
            answerDigitCounts = new int[digitMaxValue];

            Random rand = randomSeed.HasValue ? new Random(randomSeed.Value) : new Random();

            // Generates an answerDigits with `digitCount` digits from 1 to `digitMaxValue`
            for (int i = 0; i < digitCount; i++)
            {
                byte digit = (byte)(rand.Next(digitMaxValue) + 1);
                answerDigits[i] = digit;
                answerDigitCounts[digit - 1]++;
            }

            guessHistory = new GuessResponse[maxGuesses];
        }

        /// <summary>
        /// Submits a guess, and returns a response indicating the results of the guess.
        /// </summary>
        /// <param name="guessDigits">An array of digits to guess</param>
        /// <returns>The response to the submitted guess</returns>
        /// <exception cref="ArgumentOutOfRangeException">If the number of digits is not correct</exception>
        /// <exception cref="InvalidOperationException">If the game has already ended</exception>
        /// <exception cref="ArgumentException">If one or more of the digits are not within the acceptable range</exception>
        public GuessResponse Guess(byte[] guessDigits)
        {
            ArgumentNullException.ThrowIfNull(guessDigits);
            if (guessDigits.Length != DigitCount)
                throw new ArgumentOutOfRangeException(nameof(guessDigits), $"Guess must have {DigitCount} digits!");

            if (IsGameOver) throw new InvalidOperationException("The game is already over!");

            int exactMatches = 0;
            int digitMatches = 0;

            int[] guessedDigits = new int[DigitMaxValue];

            // Loops through first to check for exact digit matches
            for (int i = 0; i < DigitCount; i++)
            {
                byte digit = guessDigits[i];
                if (digit == 0) throw new ArgumentException("Guess digits cannot be 0!", nameof(guessDigits));
                if (digit > DigitMaxValue) throw new ArgumentException($"Guess digits cannot be greater than {DigitMaxValue}!", nameof(guessDigits));

                if (digit == answerDigits[i])
                {
                    // Tracks how many times the given digit has appeared in the guessedDigits.
                    guessedDigits[digit - 1]++;
                    exactMatches++;
                }
            }

            // Loops through a second time to check for digits that do not exactly match
            for (int i = 0; i < DigitCount; i++)
            {
                byte digit = guessDigits[i];

                // Skip over exact matchess
                if (digit == answerDigits[i]) continue;

                byte digitIndex = (byte) (digit - 1);

                // Uses tracked digit counts in guessedDigits to determine if the digit it needed somewhere else
                if (answerDigitCounts[digitIndex] >= ++guessedDigits[digitIndex])
                {
                    digitMatches++;
                }
            }

            GuessResponse guessResponse = new(guessDigits, exactMatches, digitMatches, DigitCount, GuessesRemaining);
            guessHistory[GuessCount++] = guessResponse;
            return guessResponse;
        }

        /// <summary>
        /// Returns a copy of the guess history. The length of the array will match the total number
        /// of guesses allowed, no matter how many guesses have been submitted so far.
        /// </summary>
        /// <returns>The guess history</returns>
        public GuessResponse[] GetGuessHistory()
        {
            return (GuessResponse[])guessHistory.Clone();
        }

        /// <summary>
        /// Returns the digits of the answer.
        /// </summary>
        /// <returns>The answer digits</returns>
        internal byte[] GetAnswerDigits()
        {
            return (byte[])answerDigits.Clone();
        }

        /// <summary>
        /// Represents the game state at the time of a guess, including the hint that corresponds to the guess,
        /// and whether the guess was correct.
        /// </summary>
        public readonly struct GuessResponse
        {
            public readonly byte[]? GuessDigits;
            public readonly int ExactMatches;
            public readonly int DigitMatches;
            public readonly int DigitCount;
            public readonly int GuessesRemaining;

            public bool IsWin => ExactMatches == DigitCount;

            internal GuessResponse(byte[] guessDigits, int exactMatches, int digitMatches, int digitCount, int guessesRemaining)
            {
                GuessDigits = guessDigits;
                ExactMatches = exactMatches;
                DigitMatches = digitMatches;
                DigitCount = digitCount;
                GuessesRemaining = guessesRemaining;
            }
        }
    }
}
