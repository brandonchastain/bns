using Bns.Dns;
using Bns.Dns.Serialization;
using Bns.StubResolver.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Bns.DnsClient.App
{
    public class DnsClient : IDisposable
    {
        private static IConfiguration Configuration { get; set; }

        private readonly IDnsMsgBinSerializer dnsSerializer;
        private readonly IOptionsMonitor<DnsClientOptions> options;

        private SemaphoreSlim statLock = new SemaphoreSlim(1, 1);
        private long latencySum = 0;
        private int queryCount = 0;

        public DnsClient(IDnsMsgBinSerializer dnsSerializer, IOptionsMonitor<DnsClientOptions> options)
        {
            this.dnsSerializer = dnsSerializer ?? throw new ArgumentNullException(nameof(dnsSerializer));
            this.options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public static async Task Main(string[] args)
        {
            Configuration = new ConfigurationBuilder().AddCommandLine(args).Build();

            var services = ConfigureServices(Configuration);
            using (var client = services.GetRequiredService<DnsClient>())
            {
                Console.WriteLine($"Sending DNS queries to {client.options.CurrentValue.NsIpAddress}... (press CTRL-C to quit)\n");

                var sendTasks = new List<Task>();

                for (int i = 0; i < 1; i++)
                {
                    sendTasks.Add(TaskUtil.RunAndWaitForCancel(
                                client.SendQueriesAsync(),
                                CancellationToken.None,
                                cleanup: null));
                }

                var reportTask = TaskUtil.RunAndWaitForCancel(
                    client.ReportLatencyAsync(),
                    CancellationToken.None,
                    cleanup: null);

                await Task.WhenAny(Task.WhenAny(sendTasks), reportTask).ConfigureAwait(false);
            }
        }

        public void Dispose()
        {
            // Dispose of unmanaged resources.
            Dispose(true);
            // Suppress finalization.
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                statLock.Dispose();
            }
        }

        private static IServiceProvider ConfigureServices(IConfiguration config)
        {
            var services = new ServiceCollection();
            services.AddOptions<DnsClientOptions>();
            services.Configure<DnsClientOptions>(config);

            services.AddTransient<DnsQuestionBinarySerializer>();
            services.AddTransient<ResourceRecordBinarySerializer>();
            services.AddTransient<IDnsMsgBinSerializer, DnsMessageBinarySerializer>();
            services.AddTransient<DnsClient>();

            return services.BuildServiceProvider();
        }

        private async Task ReportLatencyAsync()
        {
            while(true)
            {
                await Task.Delay(2000).ConfigureAwait(false);
                await statLock.WaitAsync().ConfigureAwait(false);

                if (queryCount > 0)
                {
                    Console.WriteLine($"[queryCount: {queryCount} | meanLatencyMs: {latencySum / queryCount}]");
                }
                else
                {
                    Console.WriteLine("Latency stats queue is empty.");
                }

                queryCount = 0;
                latencySum = 0;

                statLock.Release();
            }
        }

        private async Task SendQueriesAsync()
        {
            while (true)
            {
                try
                {
                    using (var udpClient = new UdpClient())
                    {
                        await SendDnsQueryAsync(udpClient).ConfigureAwait(false);
                        //await Task.Delay(10);

                    }
                }
                catch (SocketException ex)
                {
                    Console.WriteLine("Encountered a SocketException when sending dns queries:");
                    Console.WriteLine(ex);

                    Console.WriteLine("Trying again...");
                }
            }
        }

        // TODO: Send a sliding window of 10 requests in parallel.
        private async Task SendDnsQueryAsync(UdpClient udpClient)
        {
            var ipAddress = IPAddress.Parse(options.CurrentValue.NsIpAddress);
            var port = 53;
            var endpoint = new IPEndPoint(ipAddress, port);
            var dnsMessage = new DnsMessage();

            dnsMessage.Header = new Header()
            {
                Id = 2929,
                Opcode = HeaderOpCode.StandardQuery,
                QueryCount = 1,
                RecursionDesired = true,
                Z = 0,
            };

            dnsMessage.Question = new Question()
            {
                QClass = RecordClass.IN,
                QType = RecordType.A,
                QName = "mobile.pipe.aria.microsoft.com",
            };

            var bytes = this.dnsSerializer.Serialize(dnsMessage);
            var bytesSentTask = udpClient.SendAsync(bytes, bytes.Length, endpoint).ConfigureAwait(false);
            var stopwatch = Stopwatch.StartNew();

            var bytesSent = await bytesSentTask;
            if (bytesSent != bytes.Length)
            {
                Console.WriteLine("Could not send entire query.");
            }

            var timeout = Task.Delay(6000);
            Task completedTask = await Task.WhenAny(udpClient.ReceiveAsync(), timeout).ConfigureAwait(false);
            if (completedTask == timeout)
            {
                return;
            }

            var udpTask = (Task<UdpReceiveResult>)completedTask;
            var result = await udpTask.ConfigureAwait(false);
            var ms = stopwatch.ElapsedMilliseconds;

            var dnsResponseMessage = this.dnsSerializer.Deserialize(result.Buffer);

            await statLock.WaitAsync().ConfigureAwait(false);
            latencySum += ms;
            queryCount++;
            statLock.Release();
        }
    }
}
