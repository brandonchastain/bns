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
    internal class EmptyResolutionStrategy : IResolutionStrategy
    {
        private static readonly DnsMessage response = new DnsMessage()
        {
            Header = new Header()
            {
                IsResponse = true,
                Rcode = ResponseCode.NoError,
                QueryCount = 1,
            },
            Question = new Question()
            {
                QName = "www.microsoft.com.",
                QClass = RecordClass.IN,
                QType = RecordType.A
            }
        };

        public Task<DnsMessage> ResolveAsync(Question question)
        {
            return Task.FromResult(response);
        }
    }
}
