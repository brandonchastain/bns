using Bns.Dns;
using System.Threading.Tasks;

namespace Bns.StubResolver.Core
{
    public class EmptyResolutionStrategy : IResolutionStrategy
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
