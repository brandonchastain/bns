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

        private async Task<IByteSerializable> ProcessUdpMessage(UdpMessage udpMessage)
        {
            var dnsMessage = DnsMessage.Parse(udpMessage.Buffer);

            var answers = await this.resolutionStrategy.ResolveAsync(dnsMessage.Question);
            dnsMessage.AddAnswersAndIncrementCount(answers);

            dnsMessage.Header.IsResponse = true;
            dnsMessage.Header.IsAuthoritativeAnswer = false;
            dnsMessage.Header.RecursionAvailable = true;
            dnsMessage.Header.Rcode = ResponseCode.NoError;
            dnsMessage.Header.Opcode = HeaderOpCode.StandardQuery;

            return dnsMessage;
        }
    }
}
