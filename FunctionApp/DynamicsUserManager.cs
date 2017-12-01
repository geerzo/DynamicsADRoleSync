using GcmSharp.Serialization;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using RestSharp.Authenticators;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DynamicsADRoleSync.FunctionApp
{
    internal class DynamicsUserManager
    {
        private RestClient client = null;
        private string baseUrl = null;

        public DynamicsUserManager()
        {
            var resource = Settings.Get("DynamicsRootUrl");
            var token = TokenManager.GetBearerTokenAsync(resource).Result;
            baseUrl = resource + "/api/data/v9.0";
            client = new RestClient(baseUrl)
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
                var json = JsonConvert.DeserializeObject<JObject>(resp.Content);
                if (json.Value<JArray>("value").Count > 0)
                {
                    return json["value"][0].Value<string>("systemuserid");
                }
            }

            return null;
        }

        public DynUser GetUser(string username)
        {
            var req = new RestRequest(string.Format("systemusers({0})", GetUserID(username)));
            //req.AddQueryParameter("$filter", string.Format("domainname eq '{0}'", username));
            req.AddQueryParameter("$expand", "systemuserroles_association($select=roleid,name)");

            var resp = client.Get(req);
            if (resp.StatusCode == System.Net.HttpStatusCode.OK && !string.IsNullOrEmpty(resp.Content))
            {
                return JsonConvert.DeserializeObject<DynUser>(resp.Content);
            }

            return null;
        }

        public List<DynRole> GetRoles()
        {
            var req = new RestRequest("roles");
            req.AddQueryParameter("$select", "roleid,name");

            var resp = client.Get(req);
            if (resp.StatusCode == System.Net.HttpStatusCode.OK && !string.IsNullOrEmpty(resp.Content))
            {
                var json = JsonConvert.DeserializeObject<JObject>(resp.Content);
                return json["value"].ToObject<List<DynRole>>();
            }

            return null;
        }

        public void AddRolesToUser(string userid, List<string> roleIDs)
        {
            foreach (string roleID in roleIDs)
            {
                var req = new RestRequest(string.Format("systemusers({0})/systemuserroles_association/$ref", userid));
                req.JsonSerializer = NewtonsoftJsonSerializer.Default;
                req.AddJsonBody(new RoleAssociation()
                {
                    id = baseUrl + string.Format("/roles({0})", roleID)
                });

                var resp = client.Post(req);
                if (resp.StatusCode != System.Net.HttpStatusCode.OK && resp.StatusCode != System.Net.HttpStatusCode.NoContent)
                {
                    throw new System.Exception();
                }
            }
        }
    }

    public class DynUser
    {
        public string systemuserid;
        public string fullname;
        public string domainname;
        public List<DynRole> systemuserroles_association;
    }

    public class DynRole
    {
        public string roleid;
        public string name;
    }

    public class RoleAssociation
    {
        [DataMember(Name = "@odata.id")]
        [JsonProperty(PropertyName = "@odata.id")]
        public string id;
    }
}
