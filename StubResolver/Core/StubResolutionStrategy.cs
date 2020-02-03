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
    internal class StubResolutionStrategy : ResolutionStrategy
    {
        private UdpClient udpClient = new UdpClient();

        public override async Task<List<ResourceRecord>> ResolveAsync(Question question)
        {
            var message = new DnsMessage()
            {
                Header = new Header()
                {
                    Id = 3333,
                    QueryCount = 1,
                    RecursionDesired = true,
                    Z = 2
                },
                Question = question,
            };

            var dgram = message.ToByteArray();
            Console.WriteLine("Sending query:");
            Console.WriteLine(message);

            var endpoint = new IPEndPoint(IPAddress.Parse("8.8.8.8"), 53);
            await udpClient.SendAsync(dgram, dgram.Length, endpoint);
            var response = await udpClient.ReceiveAsync();

            // TODO: validate the response before parsing

            var responseDnsMessage = DnsMessage.Parse(response.Buffer);
            Console.WriteLine($"Response from {endpoint}: ");
            Console.WriteLine(responseDnsMessage);

            // TODO: Parse the actual resource records.

            var results = new List<ResourceRecord>();
            results.Add(new ARecord()
            {
                Address = new byte[] { 0x00, 0x01, 0x02, 0x03 },
                Name = "www.google1.com",
                TimeToLive = 77
            });
            return results;

            //return responseDnsMessage.Answers;
        }
    }
}
