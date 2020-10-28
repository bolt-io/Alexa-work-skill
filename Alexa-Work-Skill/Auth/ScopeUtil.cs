using System;
using System.Linq;

namespace Alexa_Work_Skill.Auth
{
    // see: https://github.com/Azure/azure-sdk-for-net/blob/master/sdk/identity/Azure.Identity/src/ScopeUtilities.cs
    public static class ScopeUtil
    {
        public static string GetResourceFromScope(string[] scopes)
        {
            var defaultSuffix = "/.default";

            if (!scopes.Any())
            {
                throw new ArgumentNullException(nameof(scopes));
            }

            if (scopes.Length > 1)
            {
                throw new ArgumentException(nameof(scopes));
            }

            var scope = scopes[0];

            if (!scope.EndsWith(defaultSuffix, StringComparison.Ordinal))
            {
                return scope;
            }
            return scope.Remove(scope.LastIndexOf(defaultSuffix, StringComparison.Ordinal));
        }
    }
}