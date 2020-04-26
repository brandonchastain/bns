using Bns.Dns;
using Bns.Dns.Serialization;
using Bns.StubResolver.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Bns.DnsClient.App
{
    public class DnsClient : IDisposable
    {
        private static IConfiguration configuration;
        private static DnsClient client;

        private readonly IDnsMsgBinSerializer dnsSerializer;
        private readonly IOptionsMonitor<DnsClientOptions> options;

        private UdpClient udpClient;
        private IPEndPoint endpoint;
        private long intervalLatencySum;
        private int intervalQueryCount;

        public DnsClient(IDnsMsgBinSerializer dnsSerializer, IOptionsMonitor<DnsClientOptions> options)
        {
            this.dnsSerializer = dnsSerializer ?? throw new ArgumentNullException(nameof(dnsSerializer));
            this.options = options ?? throw new ArgumentNullException(nameof(options));
            this.intervalLatencySum = 0;
            this.intervalQueryCount = 0;
        }

        public static async Task Main(string[] args)
        {
            BuildClientConfigFromArgs(args);
            InitializeClient();
            await RunDnsLatencyTest().ConfigureAwait(false);
        }

        private static void BuildClientConfigFromArgs(string[] commandLineArgs)
        {
            configuration = new ConfigurationBuilder().AddCommandLine(commandLineArgs).Build();
        }

        private static void InitializeClient()
        {
            var services = ConfigureServices(configuration);
            client = services.GetRequiredService<DnsClient>();
            client.BuildNameserverEndpoint();
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

        private void BuildNameserverEndpoint()
        {
            var ipAddress = IPAddress.Parse(options.CurrentValue.NsIpAddress);
            var port = 53;
            this.endpoint = new IPEndPoint(ipAddress, port);
        }

        private static async Task RunDnsLatencyTest()
        {
            var sendQueries = SendQueriesAsync();
            var logReports = LogReportsAsync();
            await Task.WhenAny(sendQueries, logReports).ConfigureAwait(false);
        }

        private static Task SendQueriesAsync()
        {
            Console.WriteLine($"Sending DNS queries to {client.options.CurrentValue.NsIpAddress}... (press CTRL-C to quit)\n");
            return TaskUtil.RunAndWaitForCancel(
                client.SendQueryLoop(),
                CancellationToken.None,
                cleanup: null);
        }

        private static Task LogReportsAsync()
        {
            return TaskUtil.RunAndWaitForCancel(
                client.ReportLatencyAsync(),
                CancellationToken.None,
                cleanup: null);
        }

        private async Task SendQueryLoop()
        {
            while (true)
            {
                try
                {
                    await SendQuery().ConfigureAwait(false);
                }
                catch (SocketException ex)
                {
                    this.HandleSocketException(ex);
                }
            }
        }

        // TODO: Send a sliding window of 10 requests in parallel.
        private async Task SendQuery()
        {
            using (udpClient = new UdpClient())
            {
                await this.SendDnsMessage().ConfigureAwait(false);
                await this.MeasureTimeToReceiveResponse().ConfigureAwait(false);
            }
            udpClient = null;
        }

        private async Task<int> SendDnsMessage()
        {
            var dnsMessage = this.GetSampleDnsMessage();
            var bytesSent = await SendSerializedDnsMessage(dnsMessage).ConfigureAwait(false);
            return bytesSent;
        }

        private DnsMessage GetSampleDnsMessage()
        {
            var dnsMessage = new DnsMessage()
            {
                Header = MakeHeader(),
                Question = MakeQuestion()
            };
            return dnsMessage;
        }

        private Header MakeHeader()
        {
            return new Header()
            {
                Id = 2929,
                Opcode = HeaderOpCode.StandardQuery,
                QueryCount = 1,
                RecursionDesired = true,
                Z = 0,
            };
        }

        private Question MakeQuestion()
        {
            return new Question()
            {
                QClass = RecordClass.IN,
                QType = RecordType.A,
                QName = "mobile.pipe.aria.microsoft.com",
            };
        }

        private async Task<int> SendSerializedDnsMessage(DnsMessage dnsMessage)
        {
            var bytes = this.dnsSerializer.Serialize(dnsMessage);
            var bytesSent = await udpClient.SendAsync(bytes, bytes.Length, endpoint).ConfigureAwait(false);

            if (bytesSent != bytes.Length)
            {
                Console.WriteLine("Could not send entire query.");
            }

            return bytesSent;
        }

        private async Task MeasureTimeToReceiveResponse()
        {
            var stopwatch = Stopwatch.StartNew();
            var result = await ReceiveAsync().ConfigureAwait(false);
            if (result != null)
            {
                IncrementLatencyStats((int)stopwatch.ElapsedMilliseconds);
            }
        }

        private void HandleSocketException(SocketException ex)
        {
            Console.WriteLine("Encountered a SocketException when sending dns queries:");
            Console.WriteLine(ex);

            Console.WriteLine("Trying again...");
        }

        private async Task ReportLatencyAsync()
        {
            while (true)
            {
                await WaitForReportingInterval().ConfigureAwait(false);
                PrintLastIntervalLatency();
                ResetStats();
            }
        }

        private async Task WaitForReportingInterval()
        {
            var reportInterval = TimeSpan.FromSeconds(2);
            await Task.Delay(reportInterval).ConfigureAwait(false);
        }

        private void PrintLastIntervalLatency()
        {
            if (intervalQueryCount > 0)
            {
                Console.WriteLine($"[queryCount: {intervalQueryCount} | meanLatencyMs: {intervalLatencySum / intervalQueryCount}]");
            }
            else
            {
                Console.WriteLine("Latency stats queue is empty.");
            }
        }

        private void ResetStats()
        {
            Interlocked.Exchange(ref intervalQueryCount, 0);
            Interlocked.Exchange(ref intervalLatencySum, 0);
        }

        private async Task<UdpReceiveResult?> ReceiveAsync()
        {
            var maxDnsTimeout = TimeSpan.FromSeconds(6);
            var timeout = Task.Delay(maxDnsTimeout);
            Task completedTask = await Task.WhenAny(udpClient.ReceiveAsync(), timeout).ConfigureAwait(false);
            if (completedTask == timeout)
            {
                return null;
            }

            var udpTask = (Task<UdpReceiveResult>)completedTask;
            var result = await udpTask.ConfigureAwait(false);
            return result;
        }

        private void IncrementLatencyStats(int latencyMs)
        {
            Interlocked.Add(ref intervalLatencySum, latencyMs);
            Interlocked.Add(ref intervalQueryCount, 1);
        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    udpClient?.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
