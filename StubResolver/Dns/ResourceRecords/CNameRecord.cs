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

        public override byte[] ToByteArray()
        {
            var bytes = new List<byte>(base.ToByteArray());
            var qBytes = new DnsQuestionSerializer().SerializeQName(this.CName);
            
            bytes.Add((byte)(qBytes.Count >> 8));
            bytes.Add((byte)qBytes.Count);
            bytes.AddRange(qBytes);

            return bytes.ToArray();
        }
    }
}
