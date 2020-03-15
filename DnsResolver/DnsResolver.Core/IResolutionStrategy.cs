using Bns.Dns;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Bns.StubResolver.Core
{
    public interface IResolutionStrategy
    {
        Task<DnsMessage> ResolveAsync(Question question);
    }
}
