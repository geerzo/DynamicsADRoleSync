using System;
using System.Collections.Generic;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Graph;
using System.Text.RegularExpressions;

namespace DynamicsADRoleSync.FunctionApp
{
    public static class ADRoleSyncTimer
    {
        [FunctionName("ADRoleSyncTimer")]
        public static void Run([TimerTrigger("0 */1 * * * *")]TimerInfo myTimer, TraceWriter log)
        {
            var userRoles = new Dictionary<string, string[]>();
            var uniqueRoles = new HashSet<string>();

            ADUserManager adUserMgr = new ADUserManager();
            log.Info($"Dynamics AD to Dynamics Role Sync function executed at: {DateTime.Now}");

            var dynMgr = new DynamicsUserManager();

            var provisionedUsers = adUserMgr.GetProvisionedUsers("CRM");

            var regex = new Regex(Settings.Get("GroupMatchRegEx"), RegexOptions.IgnoreCase);
            foreach (var user in provisionedUsers)
            {
                var roles = new List<string>();
                foreach (var group in adUserMgr.GetGroupsForUser(user))
                {
                    if (regex.IsMatch(group))
                    {
                        var dynRole = regex.Replace(group, "");
                        roles.Add(dynRole);
                        uniqueRoles.Add(dynRole);
                    }
                }
                if (roles.Count > 0)
                {
                    var dynUserID = dynMgr.GetUserID(user.UserPrincipalName);
                    userRoles.Add(dynUserID, roles.ToArray());
                    log.Info(user.DisplayName + ": " + string.Join(", ", userRoles[dynUserID]));
                }

                log.Info(user.DisplayName + ": No Dynamics Roles");

            }

        }
    }
}
