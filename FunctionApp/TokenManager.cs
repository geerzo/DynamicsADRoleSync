using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Globalization;
using System.Threading.Tasks;

namespace DynamicsADRoleSync.FunctionApp
{
    internal class TokenManager
    {
        public static async Task<string> GetBearerTokenAsync(string resource)
        {
            var aadInstance = "https://login.windows.net/{0}";
            var tenant = Settings.Get("Tenant");
            var clientId = Settings.Get("AppClientID");
            var authority = string.Format(CultureInfo.InvariantCulture, aadInstance, tenant);
            var clientSecret = Settings.Get("AppClientSecret");
            var credential = new ClientCredential(clientId, clientSecret);
            var authContext = new AuthenticationContext(authority);
            var authResult = await authContext.AcquireTokenAsync(resource, credential);

            return authResult.AccessToken;
        }
    }
}
