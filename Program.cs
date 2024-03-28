namespace ConsoleMastermind
{
    public class Program
    {
        private static void Main()
        {
            Console.Write("Would you like to enable advanced unicode graphics? (y/n): ");
            ConsoleKeyInfo keyInfo = Console.ReadKey();

            while (keyInfo.KeyChar is not ('y' or 'Y' or 'n' or 'N'))
            {
                Console.WriteLine();
                Console.Write("Please enter either 'y' or 'n': ");
                keyInfo = Console.ReadKey();
            }

            IGameInterface gameInterface = keyInfo.KeyChar is 'y' or 'Y' ?
                new AdvancedConsoleGameInterface() :
                new SimpleConsoleGameInterface();

            gameInterface.Play();
        }
    }
}