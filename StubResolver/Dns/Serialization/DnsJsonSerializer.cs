using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bns.StubResolver.Dns.Serialization
{
    public class DnsJsonSerializer : IJsonSerializer
    {
        private static JsonSerializer jsonSerializer = new JsonSerializer();

        static DnsJsonSerializer()
        {
            jsonSerializer.Converters.Add(new StringEnumConverter());
        }

        public string ToJson(object o)
        {
            var json = JToken.FromObject(o, jsonSerializer);
            return json.ToString();
        }

        public string PrettyPrint(string json)
        {
            var parsed = JToken.Parse(json);
            var beautified = parsed.ToString(Formatting.Indented);
            return beautified;
        }
    }
}
