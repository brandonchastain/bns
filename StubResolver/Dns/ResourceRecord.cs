using Dns.RecordData;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dns
{
    public abstract class ResourceRecord
    {
        public string Name { get; set; } // max length 255 octets
        public int TimeToLive { get; set; }

        public abstract RecordType GetRecordType();

        public RecordClass GetRecordClass() => RecordClass.IN;

        public virtual byte[] ToByteArray()
        {
            var bytes = new List<byte>();
            var dnsSerializer = new DnsQuestionSerializer();
            var qNameBytes = dnsSerializer.SerializeQName(this.Name);
            bytes.AddRange(qNameBytes);

            int rrTypeNum = ((int)this.GetRecordType()) + 1;
            bytes.Add((byte)(rrTypeNum >> 8));
            bytes.Add((byte)rrTypeNum);

            var rrClassNum = ((int)this.GetRecordClass()) + 1;
            bytes.Add((byte)(rrClassNum >> 8));
            bytes.Add((byte)rrClassNum);

            bytes.Add((byte)(this.TimeToLive >> 24));
            bytes.Add((byte)(this.TimeToLive >> 16));
            bytes.Add((byte)(this.TimeToLive >> 8));
            bytes.Add((byte)this.TimeToLive);

            return bytes.ToArray();
        }
    }
}
