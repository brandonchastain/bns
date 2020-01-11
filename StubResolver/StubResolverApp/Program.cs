using System;
using System.Threading;
using System.Threading.Tasks;
using Core;
using Microsoft.Extensions.Configuration;

namespace StubResolverApp
{
    public class Program
    {
        private static CancellationTokenSource cts = new CancellationTokenSource();

        public static async Task Main(string[] args)
        {
            Console.Clear();
            Console.CancelKeyPress += new ConsoleCancelEventHandler(sigintHandler);

            var cancellationToken = cts.Token;

            var rr = new StubResolver();
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
