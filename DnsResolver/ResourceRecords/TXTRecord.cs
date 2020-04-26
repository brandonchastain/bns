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
            var bytes = new List<byte>();
            var textDataBytes = Encoding.ASCII.GetBytes(this.TextData);

            if (this.Length != textDataBytes.Length)
            {
                Console.WriteLine("TXT record length mismatch detected during serialization.");
            }

            bytes.AppendIntAs2Bytes(this.Length);
            bytes.AddRange(textDataBytes);

            return bytes.ToArray();
        }
    }
}
