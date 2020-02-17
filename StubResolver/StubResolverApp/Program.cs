using System;
using System.Threading;
using System.Threading.Tasks;
using Bns.StubResolver.Core;

namespace StubResolverApp
{
    public class Program
    {
        private const ushort DefaultUdpPort = 53;

        private static CancellationTokenSource cts = new CancellationTokenSource();

        public static async Task Main(string[] args)
        {
            ushort port;
            if (args.Length == 0 || !ushort.TryParse(args[0], out port))
            {
                port = DefaultUdpPort;
            }

            await Start(port);
        }

        private static async Task Start(ushort port)
        {
            var resolver = new Resolver(port);
            var cancellationToken = cts.Token;

            Console.CancelKeyPress += new ConsoleCancelEventHandler(sigintHandler);

            await resolver.StartListener(cancellationToken).ConfigureAwait(false);
        }

        private static void sigintHandler(object sender, ConsoleCancelEventArgs args)
        {
            cts.Cancel();
            Console.WriteLine("Ctrl + C signal received. Exiting.");
            args.Cancel = true;
        }
    }
}
