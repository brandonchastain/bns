using System;
using System.Collections.Generic;
using System.Text;

namespace Dns
{
    class ResourceRecord
    {
        public string Name { get; set; } // max length 255 octets
        public RecordType RrType { get; set; }
        public RecordClass RrClass { get; set; }
        public uint TimeToLive { get; set; }
        public ushort RdataLength { get; set; }
        public RData.RData Rdata { get; set; }
    }
}
