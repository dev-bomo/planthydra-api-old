using System;
using System.Linq;
using System.Security.Claims;

namespace api.Utils
{
    /// <summary>
    /// Misc utilities
    /// </summary>
    public class Misc
    {
        /// <summary>
        /// Gets the ID from the claims principal
        /// </summary>
        /// <param name="cp">The claims principal</param>
        public static string GetIdFromClaimsPrincipal(ClaimsPrincipal cp)
        {
            var id = cp.Claims.First(c => c.Type == "id");
            if (id == null)
            {
                throw new Exception("The claims principal doesn't have an Id");
            }
            else
            {
                return id.Value;
            }
        }
    }
}