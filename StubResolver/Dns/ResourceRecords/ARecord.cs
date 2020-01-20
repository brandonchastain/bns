using System;
using System.Collections.Generic;
using System.Text;

namespace Bns.StubResolver.Dns.ResourceRecords
{
    public class ARecord : ResourceRecord
    {
        public byte[] Address { get; set; }

        public override RecordType GetRecordType() => RecordType.A;

        public override byte[] ToByteArray()
        {
            var bytes = new List<byte>(base.ToByteArray());
            bytes.AddRange(new List<byte>(this.Address));
            return bytes.ToArray();
        }
    }
}
