namespace ConsoleMastermind
{
    public interface IGameInterface
    {
        /// <summary>
        /// Plays a session of the game.
        /// This method only returns once the game has ended.
        /// </summary>
        void Play();
    }
}
