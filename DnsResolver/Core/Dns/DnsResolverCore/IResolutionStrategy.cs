using Bns.Dns;
using System.Threading.Tasks;

namespace Bns.StubResolver.Core
{
    public interface IResolutionStrategy
    {
        Task<DnsMessage> ResolveAsync(Question question);
    }
}
