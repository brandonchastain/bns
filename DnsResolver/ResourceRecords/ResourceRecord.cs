using Bns.Dns.Serialization;
using Dns.Serialization.Utils;
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
        public abstract byte[] ToByteArray();

        public RecordClass GetRecordClass() => RecordClass.IN;

        protected List<byte> SerializeCommonFields()
        {
            var bytes = new List<byte>();
            var qNameBytes = QNameSerializer.SerializeQName(this.Name);
            bytes.AddRange(qNameBytes);

            int rrTypeNum = ((int)this.GetRecordType()) + 1;
            bytes.AppendIntAs2Bytes(rrTypeNum);

            var rrClassNum = ((int)this.GetRecordClass()) + 1;
            bytes.AppendIntAs2Bytes(rrClassNum);

            bytes.AppendIntAs4Bytes(this.TimeToLive);
            return bytes;
        }
    }
}
