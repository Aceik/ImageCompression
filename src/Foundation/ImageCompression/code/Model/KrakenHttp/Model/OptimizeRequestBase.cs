using Newtonsoft.Json;

namespace Kraken.Model
{
    public abstract class OptimizeRequestBase
    {
        [JsonProperty("lossy")]
        public bool Lossy { get; set; } = false;

        [JsonProperty("wait")]
        public bool Wait { get; set; } = false;

        [JsonProperty("webp")]
        public bool WebP { get; set; } = false;

        [JsonProperty("auto_orient")]
        public bool AutoOrient { get; set; } = false;
                                                               
        [JsonProperty("sampling_scheme")]
        internal string SamplingSchemeInternal { get; set; }

        [JsonProperty("auth")]
        public Authentication Authentication { get; } = new Authentication();

        [JsonProperty("dev")]
        public bool Dev { get; set; }

        [JsonProperty("quality")]
        public int? Quality { get; set; }
    }
}