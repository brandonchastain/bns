using System;
using System.Threading;
using System.Threading.Tasks;
using Bns.StubResolver.Core;

namespace StubResolverApp
{
    public class Program
    {
        private static CancellationTokenSource cts = new CancellationTokenSource();

        public static async Task Main(string[] args)
        {
            var resolver = new Resolver();
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
