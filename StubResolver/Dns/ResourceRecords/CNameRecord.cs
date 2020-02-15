using Bns.StubResolver.Dns.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bns.StubResolver.Dns.ResourceRecords
{
    public class CNameRecord : ResourceRecord
    {

        public string CName { get; set; } // max length of 255 octets

        public override RecordType GetRecordType() => RecordType.CNAME;

    }
}
