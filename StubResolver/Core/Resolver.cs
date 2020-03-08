using Bns.StubResolver.Udp;
using Bns.StubResolver.Udp.Contracts;
using Bns.StubResolver.Dns;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Bns.StubResolver.Core
{
    public class Resolver
    {
        private readonly ushort listenPort;
        private readonly UdpListener listener;
        private IResolutionStrategy resolutionStrategy;

        public Resolver(ushort listenPort)
        {
            this.listenPort = listenPort;
            this.listener = new UdpListener(ProcessUdpMessage, listenPort);
            this.resolutionStrategy = new StubResolutionStrategy();
        }

        public async Task StartListener(CancellationToken cancellationToken)
        {
            Console.WriteLine($"Listening for DNS queries on port {this.listenPort}... (press CTRL-C to quit)\n");

            await this.listener.Start(cancellationToken).ConfigureAwait(false);
        }

        private async Task<byte[]> ProcessUdpMessage(UdpMessage udpMessage)
        {
            var dnsMessage = DnsMessage.Parse(udpMessage.Buffer);

            var response = await this.resolutionStrategy.ResolveAsync(dnsMessage.Question);

            if (response.Answers.Count == 0)
            {
                Console.WriteLine($"No answer found for query from endpoint={udpMessage.Source}");
            }

            dnsMessage.AddAnswersAndIncrementCount(response.Answers);
            dnsMessage.AddAuthorityAndIncrementCount(response.Authority);
            dnsMessage.AddAdditionalAndIncrementCount(response.Additional);

            dnsMessage.Header.IsResponse = true;
            dnsMessage.Header.IsAuthoritativeAnswer = false;
            dnsMessage.Header.RecursionAvailable = response.Header.RecursionAvailable;
            dnsMessage.Header.Rcode = response.Header.Rcode;
            dnsMessage.Header.Opcode = HeaderOpCode.StandardQuery;

            return dnsMessage.ToByteArray();
        }
    }
}
