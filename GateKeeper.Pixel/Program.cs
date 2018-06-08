using System;

using Serilog;

namespace GateKeeper.Pixel
{
    class Program
    {

        private static bool LoggingEnabled { get; set; }

        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to GateKeeper Pixel!");
            Initialize(args);
        }

        static void Initialize (string[] args)
        {
            foreach (string arg in args)
                if (arg.Contains("-log"))
                    LoggingEnabled = true;

            ConfigureSerilog();
        }

        static void ConfigureSerilog()
        {
            var logConfiguration = new LoggerConfiguration()
                .WriteTo.Console();
            if (LoggingEnabled)
                logConfiguration.WriteTo.RollingFile("log-.txt");

            var computers = NetworkBrowser.GetNetworkComputers();
            var computers2 = NativeMethods.GetNetworkComputerNames();
            var computers3 = NetApi32.GetServerList(NetApi32.SV_101_TYPES.SV_TYPE_ALL);
        }

        private static string NormalizePath(string str)
        {
            str = str.Replace('\\', '/');
            if (str.EndsWith("/"))
            {

            }
            return "hi";
        }
    }
}
