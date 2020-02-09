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
        private readonly UdpListener listener;
        private ResolutionStrategy resolutionStrategy;

        public Resolver(ushort listenPort = 10053)
        {
            this.listener = new UdpListener(ProcessUdpMessage, listenPort);
            this.resolutionStrategy = new StubResolutionStrategy();
        }

        private async Task<IByteSerializable> ProcessUdpMessage(UdpMessage udpMessage)
        {
            var dnsMessage = DnsMessage.Parse(udpMessage.Buffer);

            var answers = await this.resolutionStrategy.ResolveAsync(dnsMessage.Question);
            dnsMessage.AddAnswers(answers);

            dnsMessage.Header.IsAuthoritativeAnswer = false;
            dnsMessage.Header.Rcode = ResponseCode.NoError;
            dnsMessage.Header.Opcode = HeaderOpCode.StandardQuery;
            dnsMessage.Header.RecursionAvailable = false;

            return dnsMessage;
        }

        public async Task StartListener(CancellationToken cancellationToken)
        {
            Console.WriteLine("Listening for DNS queries... (press CTRL-C to quit)\n");

            await this.listener.Start(cancellationToken).ConfigureAwait(false);
        }
    }
}
