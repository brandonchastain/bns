using System;
using System.Collections.Generic;
using System.Text;

namespace Dns
{
    class ResourceRecord
    {
        string name; // max length 255 octets
        RecordType rrType;
        RecordClass rrClass;
        uint timeToLive;
        ushort rdataLength;
        RData.RData rdata;
    }
}
