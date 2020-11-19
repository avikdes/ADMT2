using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IdentityModel.Claims;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Owin;
using ADMT.Models;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Newtonsoft.Json;
using System.IO;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Owin.Security.Notifications;

namespace ADMT
{
    public partial class Startup
    {
        private static string clientId = ConfigurationManager.AppSettings["ida:ClientId"];
        private static string appKey = ConfigurationManager.AppSettings["ida:ClientSecret"];
        private static string aadInstance = ConfigurationManager.AppSettings["ida:AADInstance"];
        private static string tenantId = ConfigurationManager.AppSettings["ida:TenantId"];
        private static string postLogoutRedirectUri = ConfigurationManager.AppSettings["ida:PostLogoutRedirectUri"];
        private static string RedirectUri = ConfigurationManager.AppSettings["ida:RedirectUri"];

        //public static readonly string Authority = aadInstance + tenantId;
        public static readonly string Authority = aadInstance + "common/v2.0";

        // This is the resource ID of the AAD Graph API.  We'll need this to request a token to call the Graph API.
        string graphResourceId = "https://graph.windows.net/";

        public void ConfigureAuth(IAppBuilder app)
        {
            var ids = TenantConfigMgr.ValidTenantIds;

            ApplicationDbContext db = new ApplicationDbContext();

            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);

            app.UseCookieAuthentication(new CookieAuthenticationOptions());

            var options = new OpenIdConnectAuthenticationOptions
            {
                ClientId = clientId,
                Authority = Authority,
                RedirectUri = RedirectUri,
                PostLogoutRedirectUri = postLogoutRedirectUri,
                Scope = OpenIdConnectScope.OpenIdProfile,
                // ResponseType is set to request the id_token - which contains basic information about the signed-in user
                ResponseType = OpenIdConnectResponseType.IdToken,
                // ValidateIssuer set to false to allow personal and work accounts from any organization to sign in to your application
                // To only allow users from a single organizations, set ValidateIssuer to true and 'tenant' setting in web.config to the tenant name
                // To allow users from only a list of specific organizations, set ValidateIssuer to true and use ValidIssuers parameter 
                
                Notifications = new OpenIdConnectAuthenticationNotifications()
                {
                    // If there is a code in the OpenID Connect response, redeem it for an access token and refresh token, and store those away.
                    AuthorizationCodeReceived = (context) =>
                    {
                        var code = context.Code;
                        ClientCredential credential = new ClientCredential(clientId, appKey);
                        string signedInUserID = context.AuthenticationTicket.Identity.FindFirst(ClaimTypes.NameIdentifier).Value;
                        AuthenticationContext authContext = new AuthenticationContext(Authority, new ADALTokenCache(signedInUserID));
                        return authContext.AcquireTokenByAuthorizationCodeAsync(
                           code, new Uri(HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Path)), credential, graphResourceId);
                    },
                    RedirectToIdentityProvider = RedirectToIdentityProvider1,
                    AuthenticationFailed = AuthFaile
                }
            };
            options.TokenValidationParameters = new TokenValidationParameters()
            {
                ValidateIssuer = true,
                ValidIssuers = ids
               
            };
            app.UseOpenIdConnectAuthentication(options);
        }

        private Task AuthFaile(AuthenticationFailedNotification<OpenIdConnectMessage, OpenIdConnectAuthenticationOptions> arg)
        {
            return Task.FromResult(0);

        }

        private Task RedirectToIdentityProvider1(RedirectToIdentityProviderNotification<OpenIdConnectMessage, OpenIdConnectAuthenticationOptions> arg)
        {
            arg.ProtocolMessage.SetParameter("prompt", "consent");
            //arg.ProtocolMessage.IssuerAddress = arg.ProtocolMessage.
            return Task.CompletedTask;
        }
        public static string ValidateIssuers(string issuer, SecurityToken securityToken, TokenValidationParameters validationParameters)
        {
            if(securityToken is JwtSecurityToken jwt)
            {
                //securityToken
            }
            return "issuer";
        }
    }
}
