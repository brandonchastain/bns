using Bns.Dns.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bns.Dns.ResourceRecords
{
    public abstract class ResourceRecord
    {
        public string Name { get; set; } // max length 255 octets
        public int TimeToLive { get; set; }

        public abstract RecordType GetRecordType();

        public RecordClass GetRecordClass() => RecordClass.IN;
    }
}
