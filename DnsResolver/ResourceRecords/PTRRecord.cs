using Bns.Dns.Serialization;
using Dns.Serialization.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bns.Dns.ResourceRecords
{
    public class PTRRecord : ResourceRecord
    {
        public string PtrDName { get; set; }

        public override RecordType GetRecordType() => RecordType.PTR;

        public override byte[] ToByteArray()
        {
            var bytes = this.SerializeCommonFields();
            var qnameBytes = QNameSerializer.SerializeQName(this.PtrDName);
            var numBytes = qnameBytes.Count;
            bytes.AppendIntAs2Bytes(numBytes);
            bytes.AddRange(qnameBytes);
            return bytes.ToArray();
        }
    }
}
