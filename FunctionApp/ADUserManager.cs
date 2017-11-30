using System;
using System.Linq;

using Microsoft.Graph;
using System.Net.Http.Headers;
using System.Collections.Generic;

namespace DynamicsADRoleSync.FunctionApp
{
    internal class ADUserManager
    {
        private static string apiUrl = "https://graph.windows.net";
        private GraphServiceClient client = null;

        public ADUserManager()
        {
            client = new GraphServiceClient("https://graph.windows.net/" + Settings.Get("Tenant"), new DelegateAuthenticationProvider(
                async (requestMessage) =>
                {
                    string accessToken = await TokenManager.GetBearerTokenAsync(apiUrl);
                    requestMessage.RequestUri = new Uri(requestMessage.RequestUri.ToString() + (String.IsNullOrEmpty(requestMessage.RequestUri.Query) ? "?" : "&") +  "api-version=1.6");
                    requestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", accessToken);
                }));
        }

        public User[] GetUsers()
        {
            return client.Users.Request().GetAsync().Result.ToArray<User>();
        }

        public User GetUser()
        {
            return client.Users.Request().GetAsync().Result.First<User>();
        }

        public string[] GetGroupsForUser(User user)
        {
            var groups = new List<string>();
            foreach (var group in client.Users[user.UserPrincipalName].MemberOf.Request().GetAsync().Result)
            {
                if (group.AdditionalData["objectType"].Equals("Group") && group.AdditionalData["securityEnabled"].Equals(true)) {
                    groups.Add(group.AdditionalData["displayName"].ToString());
                }
            }
            return groups.ToArray();
        }

        public User[] GetProvisionedUsers(string type)
        {
            var licensedUsers = new List<User>();
            foreach (var user in GetUsers())
            {
                foreach (var system in user.ProvisionedPlans)
                {
                    if (system.Service.Equals(type) && system.CapabilityStatus.Equals("Enabled"))
                    {
                        licensedUsers.Add(user);
                        break;
                    }
                }
            }

            return licensedUsers.ToArray();
        }
    }
}
