using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Filters;
using BgTaxi.Attributes.Results;

using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Security.Claims;
using BgTaxi.Models;

namespace BgTaxi.Attributes
{
    public class IdentityBasicAuthenticationAttribute : BasicAuthenticationAttribute
    {
        protected override async Task<IPrincipal> AuthenticateAsync(string userName, string password, CancellationToken cancellationToken)
        {
            UserManager<ApplicationUser> userManager = CreateUserManager();

            cancellationToken.ThrowIfCancellationRequested(); // Unfortunately, UserManager doesn't support CancellationTokens.
            ApplicationUser user = await userManager.FindAsync(userName, password);
            
            if (user == null)
            {
                // No user with userName/password exists.
                return null;
                
            }

            // Create a ClaimsIdentity with all the claims for this user.
            cancellationToken.ThrowIfCancellationRequested(); // Unfortunately, IClaimsIdenityFactory doesn't support CancellationTokens.
            ClaimsIdentity identity = await userManager.ClaimsIdentityFactory.CreateAsync(userManager, user, "Basic");
            return new ClaimsPrincipal(identity);
        }

        private static UserManager<ApplicationUser> CreateUserManager()
        {
            return new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext()));
        }
    }
}