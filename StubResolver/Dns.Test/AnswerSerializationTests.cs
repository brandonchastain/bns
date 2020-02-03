using Bns.StubResolver.Dns.ResourceRecords;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dns.Test
{
    public class AnswerSerializationTests
    {
        [Test]
        public static void ARecordTest()
        {
            var rr = new ARecord();
            rr.Address = new byte[] { 0x01, 0x01, 0x01, 0x01 };
            rr.Name = "www.";
            rr.TimeToLive = 11;

            var bytes = rr.ToByteArray();

            var expected = new byte[] { 0x03, 0x77, 0x77, 0x77,  0x00, 0x00, 0x01, 0x00, 0x01, 0x00, 0x00, 0x00, 0x0b, 0x00, 0x04, 0x01, 0x01, 0x01, 0x01};
            Assert.AreEqual(expected, bytes);
        }
    }
}
