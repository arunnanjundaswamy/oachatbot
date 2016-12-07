using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace OAChatBot.Luis
{
    [Serializable]
    public class LuisResponse
    {
        public string Query { get; set; }

        [JsonProperty("intents")]
        public List<Intent> Intents { get; set; }

        [JsonProperty("entities")]
        public List<Entity> Entities { get; set; }
    }

    [Serializable]
    public sealed class Intent
    {
        [JsonProperty("intent")]
        public string Name { get; set; }

        [JsonProperty("score")]
        public double Score { get; set; }

        public bool IsNone()
        {
            return Name.Equals("None", StringComparison.InvariantCultureIgnoreCase);
        }
    }

    [Serializable]
    public sealed class Entity
    {
        [JsonProperty("entity")]
        public string Value { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }
}