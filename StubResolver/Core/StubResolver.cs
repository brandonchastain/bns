using Bns.StubResolver.Udp;
using Bns.StubResolver.Udp.Contracts;
using Dns;
using Dns.ResourceRecords;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Bns.StubResolver.Core
{
    public class StubResolver
    {
        private readonly UdpListener listener;

        public StubResolver(ushort listenPort = 10053)
        {
            this.listener = new UdpListener(ProcessUdpMessage, listenPort);
        }

        private static IByteSerializable ProcessUdpMessage(UdpMessage udpMessage)
        {
            var dnsMessage = DnsMessage.Parse(udpMessage.Buffer);
            
            Console.WriteLine(dnsMessage);

            Resolve(dnsMessage);

            return dnsMessage;
        }

        private static void Resolve(DnsMessage dnsMessage)
        {
            var rr = new CNameRecord()
            {
                Name = "www.google1.com.",
                TimeToLive = 33,
                CName = "www.google.com."
            };

            dnsMessage.AddAnswer(rr);

            //dnsMessage.AddAnswer(new ARecord()
            //{
            //    Address = new byte[] { 0x00, 0x01, 0x02, 0x03 },
            //    Name = "www.google1.com",
            //    TimeToLive = 77
            //});
        }

        public async Task StartListener(CancellationToken cancellationToken)
        {
            Console.WriteLine("Listening for DNS queries...");
            await this.listener.ListenAndProcessDatagrams(cancellationToken).ConfigureAwait(false);
        }
    }
}
