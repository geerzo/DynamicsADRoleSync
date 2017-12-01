using System;
using System.Diagnostics.CodeAnalysis;

namespace DynamicsADRoleSync.FunctionApp
{
    [ExcludeFromCodeCoverage]
    internal class Settings
    {
        public static string Get(string name)
        {
            return Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
        }
    }
}
