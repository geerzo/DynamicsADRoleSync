using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using RestSharp.Authenticators;

namespace DynamicsADRoleSync.FunctionApp
{
    internal class DynamicsUserManager
    {
        private RestClient client = null;

        public DynamicsUserManager()
        {
            var resource = Settings.Get("DynamicsRootUrl");
            var token = TokenManager.GetBearerTokenAsync(resource).Result;
            client = new RestClient(resource + "/api/data/v9.0")
            {
                Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(token, "Bearer")
            };
        }

        public string GetUserID(string username)
        {
            var req = new RestRequest("systemusers");
            req.AddQueryParameter("$filter", string.Format("domainname eq '{0}'", username));

            var resp = client.Get(req);
            if (resp.StatusCode == System.Net.HttpStatusCode.OK && !string.IsNullOrEmpty(resp.Content))
            {
                JObject json = JsonConvert.DeserializeObject<JObject>(resp.Content);
                if (json.Value<JArray>("value").Count > 0)
                {
                    return json["value"][0].Value<string>("systemuserid");
                }
            }

            return null;
        }

        public void LogSystemUsers(TraceWriter log)
        {
            //var req = new RestRequest("systemusers", Method.GET);
            //var resp = client.Get(req);

            //JObject json = JsonConvert.DeserializeObject<JObject>(resp.Content);

            //log.Info(resp.Content);
        }
    }
}
