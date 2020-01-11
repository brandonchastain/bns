using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Core
{
    public class UdpMessage
    {
        public UdpMessage(byte[] buffer, IPEndPoint endpoint)
        {
            Buffer = buffer;
            Source = endpoint;
        }

        public byte[] Buffer { get; set; }
        public IPEndPoint Source { get; set; }
    }
}
