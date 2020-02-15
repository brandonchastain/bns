using System;
using System.Collections.Generic;
using System.Text;

namespace Bns.StubResolver.Udp.Contracts
{
    public interface IByteSerializable
    {
        byte[] ToByteArray();
    }
}
