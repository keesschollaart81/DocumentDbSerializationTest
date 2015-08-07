using Newtonsoft.Json;

namespace DocumentDbSerializationTest
{
    public class Document
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("created")]
        public long Created { get; set; }
    }
}