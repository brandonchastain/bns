using Bns.StubResolver.Dns;
using Bns.StubResolver.Dns.ResourceRecords;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Bns.StubResolver.Core
{
    internal abstract class ResolutionStrategy
    {
        public abstract Task<List<ResourceRecord>> ResolveAsync(Question question);
    }
}
