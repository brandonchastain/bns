using System;
using System.Net;
using Bns.Dns.ResourceRecords;
using Bns.Dns.Serialization;
using NUnit.Framework;

namespace Dns.Test
{
    public class RrSerializationTests
    {
        [Test]
        public void SerializeARecord()
        {
            var serializer = GetRrSerializer();
            var aRecord = new ARecord()
            {
                Address = IPAddress.Parse("192.168.1.1"),
                Name = "www.",
                TimeToLive = 343,
            };

            byte[] expected = { 0x03, 0x77, 0x77, 0x77, 0x00,
                                0x00, 0x01,
                                0x00, 0x01,
                                0x00, 0x00, 0x01, 0x57,
                                0x00, 0x04,
                                0xc0, 0xa8, 0x01, 0x01};

            var serialized = serializer.ToByteArray(aRecord);
            Assert.AreEqual(expected, serialized);
        }

        [Test]
        public void SerializeCNameRecord()
        {
            var serializer = GetRrSerializer();
            var cname = new CNameRecord()
            {
                Name = "www.",
                TimeToLive = 343,
                CName = "aaa.",
            };

            byte[] expected = { 0x03, 0x77, 0x77, 0x77, 0x00,
                                0x00, 0x05,
                                0x00, 0x01,
                                0x00, 0x00, 0x01, 0x57,
                                0x00, 0x05,
                                0x03, 0x61, 0x61, 0x61, 0x00};

            var serialized = serializer.ToByteArray(cname);
            Assert.AreEqual(expected, serialized);
        }

        [Test]
        public void SerializeNSRecord()
        {
            var serializer = GetRrSerializer();
            var ns = new NSRecord()
            {
                Name = "www.",
                TimeToLive = 343,
                DName = "aaa."
            };

            byte[] expected = { 0x03, 0x77, 0x77, 0x77, 0x00,
                                0x00, 0x02,
                                0x00, 0x01,
                                0x00, 0x00, 0x01, 0x57,
                                0x00, 0x05,
                                0x03, 0x61, 0x61, 0x61, 0x00};

            var serialized = serializer.ToByteArray(ns);
            Assert.AreEqual(expected, serialized);
        }

        //[Test]
        //public void TestRead4BytesAsInt()
        //{
        //    var serializer = GetRrSerializer();
        //    var ns = new NSRecord()
        //    {
        //        Name = "www.",
        //        TimeToLive = 343,
        //        DName = "aaa."
        //    };

        //    byte[] expected = { 0x02 };

        //    var serialized = serializer.Read4BytesAsInt(ns);
        //    Assert.AreEqual(expected, serialized);
        //}

        private static ResourceRecordBinarySerializer GetRrSerializer()
        {
            var dnsQSerializer = new DnsQuestionBinarySerializer();
            //var serializer = new ResourceRecordBinarySerializerTestAdapter(dnsQSerializer);
            var serializer = new ResourceRecordBinarySerializer(dnsQSerializer);

            return serializer;
        }
    }
}