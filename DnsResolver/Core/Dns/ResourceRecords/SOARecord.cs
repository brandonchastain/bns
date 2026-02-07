using Bns.Dns.Serialization;
using Dns.Serialization.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bns.Dns.ResourceRecords
{
    public class SOARecord : ResourceRecord
    {
        public string MName { get; set; }

        public string RName { get; set; }
        
        public uint Serial { get; set; }

        public int RefreshInterval { get; set; }

        public int RetryInterval { get; set; }

        public int ExpireInterval { get; set; }

        public uint Minimum { get; set; }

        public override RecordType GetRecordType() => RecordType.SOA;

        public override byte[] ToByteArray()
        {
            var bytes = this.SerializeCommonFields();
            var mnameBytes = QNameSerializer.SerializeQName(this.MName);
            var rnameBytes = QNameSerializer.SerializeQName(this.RName);

            var length = mnameBytes.Count + rnameBytes.Count + 20;
            bytes.AppendIntAs2Bytes(length);

            bytes.AddRange(mnameBytes);
            bytes.AddRange(rnameBytes);
            bytes.AppendIntAs4Bytes((int)this.Serial);
            bytes.AppendIntAs4Bytes(this.RefreshInterval);
            bytes.AppendIntAs4Bytes(this.RetryInterval);
            bytes.AppendIntAs4Bytes(this.ExpireInterval);
            bytes.AppendIntAs4Bytes((int)this.Minimum);

            return bytes.ToArray();
        }
    }
}
