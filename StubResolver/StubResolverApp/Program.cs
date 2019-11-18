using System;
using Core;
using Microsoft.Extensions.Configuration;

namespace StubResolverApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var rr = new StubResolver();
            rr.StartListener();
        }
    }
}
