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
        private static CancellationTokenSource cts = new CancellationTokenSource();

        private static IConfiguration Configuration { get; set; }

        public static async Task Main(string[] args)
        {
            Configuration = BuildConfig(args);

            var services = new ServiceCollection();
            ConfigureServices(services, Configuration);

            await Start(services.BuildServiceProvider());
        }

        private static void ConfigureServices(IServiceCollection services, IConfiguration config)
        {
            services.AddOptions<ResolverOptions>();
            services.Configure<ResolverOptions>(config);

            services.AddTransient<DnsQuestionBinarySerializer>();
            services.AddTransient<ResourceRecordBinarySerializer>();
            services.AddTransient<IDnsMsgBinSerializer, DnsMessageBinarySerializer>();
            services.AddTransient<IResolutionStrategy, StubResolutionStrategy>();
            services.AddTransient<DnsResolver>();
        }

        private static async Task Start(IServiceProvider services)
        {
            var resolver = services.GetRequiredService<DnsResolver>();
            var cancellationToken = cts.Token;

            Console.CancelKeyPress += new ConsoleCancelEventHandler(SigintHandler);

            await resolver.StartListener(cancellationToken).ConfigureAwait(false);
        }

        private static IConfiguration BuildConfig(string[] args)
        {
            return new ConfigurationBuilder().AddCommandLine(args).Build();
        }
        
        private static void SigintHandler(object sender, ConsoleCancelEventArgs args)
        {
            cts.Cancel();
            Console.WriteLine("Ctrl + C signal received. Exiting.");
            args.Cancel = true;
        }
    }
}
