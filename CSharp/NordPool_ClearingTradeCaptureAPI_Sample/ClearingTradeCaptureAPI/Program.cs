namespace NordPool.ClearingTradeCaptureAPI.Sample
{
    using System;

    public class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length == 0 || !ResolveCommand(args[0].ToLower()))
            {
                string arg;
                do
                {
                    PrintHelp();
                    arg = Console.ReadLine();
                }
                while (!ResolveCommand(arg));
            }
        }

        private static bool ResolveCommand(string arg)
        {
            if (arg == "single" || arg == "s")
            {
                Console.WriteLine("'single' selected");
                SingleRequestClient singleRequestClient = new SingleRequestClient();
                singleRequestClient.MakeSingleRequest();
                return true;
            }
            if (arg == "repeated" || arg == "r")
            {
                Console.WriteLine("'repeated' selected");
                RepeatedRequestClient repeatedRequestClient = new RepeatedRequestClient();
                repeatedRequestClient.MakeRepeatedRequests();
                return true;
            }

            return false;
        }

        private static void PrintHelp()
        {
            Console.WriteLine("Usage: NordPool.ClearingTradeCaptureAPI.Sample [command]");
            Console.WriteLine("Commands:");
            Console.WriteLine("\ts[ingle]   - make single request and print the result trades on console one by one");
            Console.WriteLine("\tr[epeated] - make repeated requests every 1 minute and print results in compact tabular format");
            Console.WriteLine("If command is omitted, an interactive prompt is shown.");
        }
    }
}