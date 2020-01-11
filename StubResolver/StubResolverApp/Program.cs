using System;
using System.Threading.Tasks;
using Core;
using Microsoft.Extensions.Configuration;

namespace StubResolverApp
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var rr = new StubResolver();
            await rr.StartListener().ConfigureAwait(false);
        }
    }
}
