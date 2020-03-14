using System;
using System.Threading;
using System.Threading.Tasks;
using Bns.Dns.Serialization;
using Bns.StubResolver.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace StubResolverApp
{
    public class Program
    {
        private const ushort DefaultUdpPort = 53;

        private static CancellationTokenSource cts = new CancellationTokenSource();

        private static IConfiguration Configuration { get; set; }

        public static async Task Main(string[] args)
        {
            if (args.Length == 0 || !ushort.TryParse(args[0], out ushort port))
            {
                port = DefaultUdpPort;
            }

            var config = BuildConfig(args);
            var services = new ServiceCollection();
            ConfigureServices(services, config);

            await Start(services.BuildServiceProvider());
        }

        private static IConfiguration BuildConfig(string[] args)
        {
            return new ConfigurationBuilder().AddCommandLine(args).Build();
        }

        private static void ConfigureServices(IServiceCollection services, IConfiguration config)
        {
            services.AddTransient<DnsResolver>();
            services.AddOptions<ResolverOptions>();
            services.Configure<ResolverOptions>(config);
            services.AddTransient<DnsQuestionBinarySerializer>();
            services.AddTransient<ResourceRecordBinarySerializer>();
            services.AddTransient<IDnsMsgBinSerializer, DnsMessageBinarySerializer>();
            services.AddTransient<IResolutionStrategy, StubResolutionStrategy>();
        }

        private static async Task Start(IServiceProvider services)
        {
            var resolver = services.GetRequiredService<DnsResolver>();
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
