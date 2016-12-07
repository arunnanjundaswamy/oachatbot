using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace OAChatBot.Luis
{
    public class LuisClient
    {
        public static LuisClient Create()
        {
            string applicationId = ConfigurationManager.AppSettings["LuisApplicationId"];
            string subscriptionKey = ConfigurationManager.AppSettings["LuisSubscriptionKey"];
            string apiBaseUrl = ConfigurationManager.AppSettings["LuisApiBaseUrl"];

            return new LuisClient(applicationId, subscriptionKey, apiBaseUrl);
        }
        public static LuisClient Create(string applicationId)
        {
            string apiBaseUrl = ConfigurationManager.AppSettings["LuisApiBaseUrl"];
            string subscriptionKey = ConfigurationManager.AppSettings["LuisSubscriptionKey"];

            return new LuisClient(applicationId, subscriptionKey, apiBaseUrl);
        }

        private readonly string _apiBaseUrl = "?";

        private readonly string _applicationId;
        private readonly string _subscriptionKey;

        public LuisClient(string applicationId, string subscriptionKey, string apiBaseUrl)
        {
            _applicationId = applicationId;
            _subscriptionKey = subscriptionKey;
            _apiBaseUrl = apiBaseUrl;
        }

        public async Task<LuisResponse> SendQuery(string query)
        {
            string requestUrl = $"{_apiBaseUrl}?id={_applicationId}&subscription-key={_subscriptionKey}&q={query}";

            using (HttpClient client = new HttpClient())
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
                HttpResponseMessage response = await client.SendAsync(request);
                string content = await response.Content.ReadAsStringAsync();

                LuisResponse result = JsonConvert.DeserializeObject<LuisResponse>(content);
                result.Query = query;
                return result;
            }
        }
    }


}