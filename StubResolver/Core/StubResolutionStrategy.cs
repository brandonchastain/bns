using Bns.Dns;
using Bns.Dns.Serialization;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Bns.StubResolver.Core
{
    public class StubResolutionStrategy : IResolutionStrategy
    {
        private readonly IDnsMsgBinSerializer dnsSerializer;
        private UdpClient udpClient = new UdpClient();

        public StubResolutionStrategy(IDnsMsgBinSerializer dnsSerializer)
        {
            this.dnsSerializer = dnsSerializer ?? throw new ArgumentNullException(nameof(dnsSerializer));
        }

        public async Task<DnsMessage> ResolveAsync(Question question)
        {
            var message = new DnsMessage()
            {
                Header = new Header()
                {
                    Id = 3333,
                    QueryCount = 1,
                    RecursionDesired = true,
                    Z = 0
                },
                Question = question,
            };

            var dgram = this.dnsSerializer.Serialize(message);
            //Console.WriteLine("Sending query:");
            //Console.WriteLine(message);

            var endpoint = new IPEndPoint(IPAddress.Parse("8.8.8.8"), 53);
            Task<UdpReceiveResult> udpTask = null;
            int retryCount = 0;
            var maxRetries = 10;
            while (retryCount < maxRetries && udpTask == null)
            {
                await udpClient.SendAsync(dgram, dgram.Length, endpoint);
                var timeout = Task.Delay(2000);
                Task completedTask = await Task.WhenAny(udpClient.ReceiveAsync(), timeout);

                if (completedTask != timeout)
                {
                    udpTask = (Task<UdpReceiveResult>)completedTask;
                    break;
                }

                retryCount++;

                Console.WriteLine($"Never received a udp response from server. Retrying {retryCount} of {maxRetries} times.");
            }

            if (retryCount >= maxRetries)
            {
                return message;
            }


            var udpResponse = await udpTask;

            // TODO: validate the response before parsing

            var responseDnsMessage = this.dnsSerializer.Deserialize(udpResponse.Buffer);
            //Console.WriteLine($"Response from {endpoint}: ");
            //Console.WriteLine(responseDnsMessage);

            return responseDnsMessage;
        }
    }
}
