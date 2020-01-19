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
            var rr = new StubResolver();
            var cancellationToken = cts.Token;

            Console.CancelKeyPress += new ConsoleCancelEventHandler(sigintHandler);

            await rr.StartListener(cancellationToken).ConfigureAwait(false);
        }

        private static void sigintHandler(object sender, ConsoleCancelEventArgs args)
        {
            cts.Cancel();
            Console.WriteLine("Exiting.");
            args.Cancel = true;
        }
    }
}
