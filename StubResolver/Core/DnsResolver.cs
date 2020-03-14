using Bns.StubResolver.Udp;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Bns.Dns;
using Bns.Dns.Serialization;

namespace Bns.StubResolver.Core
{
    public class DnsResolver
    {
        private readonly IOptionsMonitor<ResolverOptions> options; // member is unused, but needed for DI in the ctor.
        private readonly IResolutionStrategy resolutionStrategy;
        private readonly IDnsMsgBinSerializer dnsSerializer;

        private readonly UdpListener listener;
        private readonly ushort listenPort;

        public DnsResolver(
            IOptionsMonitor<ResolverOptions> options,
            IResolutionStrategy resolutionStrategy,
            IDnsMsgBinSerializer dnsSerializer)
        {
            this.options = options ?? throw new ArgumentNullException(nameof(options));
            this.resolutionStrategy = resolutionStrategy ?? throw new ArgumentNullException(nameof(resolutionStrategy));
            this.dnsSerializer = dnsSerializer ?? throw new ArgumentNullException(nameof(dnsSerializer));

            this.listenPort = (ushort)options.CurrentValue.Port;
            this.listener = new UdpListener(ProcessRawQuery, this.listenPort);
        }

        public async Task StartListener(CancellationToken cancellationToken)
        {
            Console.WriteLine($"Listening for DNS queries on port {listenPort}... (press CTRL-C to quit)\n");

            await this.listener.Start(cancellationToken).ConfigureAwait(false);
        }

        private async Task<byte[]> ProcessRawQuery(UdpMessage udpMessage)
        {
            var dnsMessage = this.dnsSerializer.Deserialize(udpMessage.Buffer);
            var response = await this.resolutionStrategy.ResolveAsync(dnsMessage.Question);

            if (response.Answers.Count == 0)
            {
                Console.WriteLine($"No answer found for question from endpoint={udpMessage.Source}:");
                Console.WriteLine($"{new DnsJsonSerializer().ToJson(dnsMessage.Question)}");
            }

            dnsMessage.AddAnswersAndIncrementCount(response.Answers);
            dnsMessage.AddAuthorityAndIncrementCount(response.Authority);
            dnsMessage.AddAdditionalAndIncrementCount(response.Additional);
            dnsMessage.Header.IsResponse = true;
            dnsMessage.Header.IsAuthoritativeAnswer = false;
            dnsMessage.Header.RecursionAvailable = response.Header.RecursionAvailable;
            dnsMessage.Header.Rcode = response.Header.Rcode;
            dnsMessage.Header.Opcode = HeaderOpCode.StandardQuery;

            return this.dnsSerializer.Serialize(dnsMessage);
        }
    }
}
