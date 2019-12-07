using System;
using System.Threading.Tasks;
using Core;
using Microsoft.Extensions.Configuration;

namespace StubResolverApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var rr = new StubResolver();
            await rr.StartListener();
        }
    }
}
