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

            services.AddSingleton<DnsQuestionBinarySerializer>();
            services.AddSingleton<ResourceRecordBinarySerializer>();
            services.AddSingleton<IDnsMsgBinSerializer, DnsMessageBinarySerializer>();
            services.AddSingleton<IResolutionStrategy, StubResolutionStrategy>();
            services.AddSingleton<DnsResolver>();
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
            var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Development";
            
            return new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile($"appsettings.{environment}.json", optional: true)
                .AddCommandLine(args)
                .Build();
        }
        
        private static void SigintHandler(object sender, ConsoleCancelEventArgs args)
        {
            cts.Cancel();
            Console.WriteLine("Ctrl + C signal received. Exiting.");
            args.Cancel = true;
        }
    }
}
