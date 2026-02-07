using Bns.Dns.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bns.Dns.ResourceRecords
{
    public class TXTRecord : ResourceRecord
    {
        public string TextData { get; set; }

        public int Length { get; set; }

        public override RecordType GetRecordType() => RecordType.TXT;

        public override byte[] ToByteArray()
        {
            var bytes = this.SerializeCommonFields();
            var textDataBytes = Encoding.ASCII.GetBytes(this.TextData);

            // Build RDATA: 1-byte length prefix followed by character-string
            var rdata = new List<byte>();
            rdata.Add((byte)textDataBytes.Length);
            rdata.AddRange(textDataBytes);

            // Append RDLENGTH (2 bytes) then RDATA
            bytes.AppendIntAs2Bytes(rdata.Count);
            bytes.AddRange(rdata);

            return bytes.ToArray();
        }
    }
}
