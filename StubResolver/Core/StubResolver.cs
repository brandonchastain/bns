using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dns;
using Dns.ResourceRecords;

namespace Core
{
    public class StubResolver
    {
        private readonly UdpListener listener;

        public StubResolver(ushort listenPort = 10053)
        {
            this.listener = new UdpListener(ProcessUdpMessage, listenPort);
        }

        private static DnsMessage ProcessUdpMessage(UdpMessage udpMessage)
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
            await this.listener.ListenAndProcessDatagrams(cancellationToken);
        }
    }
}
