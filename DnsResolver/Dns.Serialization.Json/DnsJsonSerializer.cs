using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Nodes;

namespace Bns.Dns.Serialization
{
    public class DnsJsonSerializer : IJsonSerializer
    {
        private static JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions
        {
            Converters = { new JsonStringEnumConverter() },
            WriteIndented = false
        };

        private static JsonSerializerOptions prettyPrintOptions = new JsonSerializerOptions
        {
            Converters = { new JsonStringEnumConverter() },
            WriteIndented = true
        };

        public string ToJson(object o)
        {
            return JsonSerializer.Serialize(o, jsonSerializerOptions);
        }

        public string PrettyPrint(string json)
        {
            var node = JsonNode.Parse(json);
            return JsonSerializer.Serialize(node, prettyPrintOptions);
        }
    }
}
