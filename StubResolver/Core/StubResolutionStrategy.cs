using Bns.StubResolver.Dns;
using Bns.StubResolver.Dns.ResourceRecords;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Bns.StubResolver.Core
{
    internal class StubResolutionStrategy : IResolutionStrategy
    {
        private UdpClient udpClient = new UdpClient();

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

            var dgram = message.ToByteArray();
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

            var responseDnsMessage = DnsMessage.Parse(udpResponse.Buffer);
            //Console.WriteLine($"Response from {endpoint}: ");
            //Console.WriteLine(responseDnsMessage);

            return responseDnsMessage;
        }
    }
}
