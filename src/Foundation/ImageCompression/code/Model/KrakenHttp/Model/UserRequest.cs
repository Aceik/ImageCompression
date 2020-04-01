using Newtonsoft.Json;

namespace Kraken.Model
{
    public class UserRequest : OptimizeRequestBase
    {
        [JsonIgnore]
        public bool Dev { get; set; }
    }
}