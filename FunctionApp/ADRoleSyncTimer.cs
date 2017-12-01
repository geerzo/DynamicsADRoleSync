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
            var userRoles = new Dictionary<string, List<string>>();
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
                        var dynRole = regex.Replace(group, "").Trim();
                        roles.Add(dynRole);
                        uniqueRoles.Add(dynRole);
                    }
                }
                if (roles.Count > 0)
                {
                    userRoles.Add(user.UserPrincipalName, roles);
                    log.Info(user.DisplayName + ": " + string.Join(", ", roles));
                }
                else
                {
                    log.Info(user.DisplayName + ": No Dynamics Roles");
                }
            }

            var dynRoles = dynMgr.GetRoles();

            foreach (var username in userRoles.Keys)
            {
                var dynUser = dynMgr.GetUser(username);

                var adRoles = userRoles[username];
                var roleIDsToAdd = new List<string>();
                foreach (var dynRole in dynUser.systemuserroles_association)
                {
                    if (adRoles.Contains(dynRole.name)) {
                        adRoles.Remove(dynRole.name);
                    }
                }
                foreach (var adRole in adRoles)
                {
                    foreach (var dynRole in dynRoles)
                    {
                        if (dynRole.name.Equals(adRole))
                        {
                            roleIDsToAdd.Add(dynRole.roleid);
                            break;
                        }
                    }
                }
                if (roleIDsToAdd.Count > 0)
                {
                    log.Info(string.Format("Adding the following roles to {0}: {1}", username, string.Join(", ", adRoles)));
                    dynMgr.AddRolesToUser(dynUser.systemuserid, roleIDsToAdd);
                } else
                {
                    log.Info(string.Format("{0} already has all the appropriate roles", username));
                }
            }
        }
    }
}
