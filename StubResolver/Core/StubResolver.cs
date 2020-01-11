using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Dns;

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
            return dnsMessage;
        }

        public async Task StartListener()
        {
            Console.WriteLine("Waiting for broadcast");
            await this.listener.ListenAndProcessResponses();
            Console.WriteLine("Listener stopped.");
        }
    }
}
