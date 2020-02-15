﻿using Bns.StubResolver.Dns;
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
                    Z = 0
                },
                Question = question,
            };

            var dgram = message.ToByteArray();
            Console.WriteLine("Sending query:");
            Console.WriteLine(message);

            var endpoint = new IPEndPoint(IPAddress.Parse("10.0.1.29"), 53);
            await udpClient.SendAsync(dgram, dgram.Length, endpoint);
            var response = await udpClient.ReceiveAsync();

            // TODO: validate the response before parsing

            var responseDnsMessage = DnsMessage.Parse(response.Buffer);
            Console.WriteLine($"Response from {endpoint}: ");
            Console.WriteLine(responseDnsMessage);

            // TODO: Parse the actual resource records.

            var results = responseDnsMessage.Answers;
            return results;

            //return responseDnsMessage.Answers;
        }
    }
}
