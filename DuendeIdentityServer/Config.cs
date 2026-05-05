using Duende.IdentityServer.Models;
using Duende.IdentityServer.Test;
using System.Security.Claims;

namespace DuendeIdentityServer
{
    public static class Config
    {
        public static IEnumerable<IdentityResource> IdentityResources =>
            new IdentityResource[]
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResources.Email()
            };

        public static IEnumerable<ApiScope> ApiScopes =>
            new ApiScope[]
                { };

        public static IEnumerable<Client> Clients =>
            new Client[]
                { 
                    new Client
                    {
                        ClientId = "mvc_client", // this must match the client_id in the request
                    ClientSecrets = { new Secret("secret".Sha256()) },
                    AllowedGrantTypes = GrantTypes.Code,
                    RedirectUris =
                    {
                        "https://localhost:5002/signin-oidc" // your MVC app redirect URI
                    },
                    PostLogoutRedirectUris =
                    {
                        "https://localhost:5002/signout-callback-oidc"
                    },
                    AllowedScopes =
                    {
                        "openid",
                        "profile",
                        "email"
                    },
                    AlwaysIncludeUserClaimsInIdToken = true
                    }
                };
        public static IEnumerable<TestUser> TestUsers =>
            new List<TestUser>
            {
                new TestUser
                {
                    SubjectId = "1",
                    Username = "Ben",
                    Password = "Ben",
                    Claims =
                    {
                        new Claim("sub", "1"),
                        new Claim("given_name", "Ben"),
                        new Claim("family_name", "Lambrechts"),
                        new Claim("email", "ben@pxl.be"),
                        new Claim("name", "Ben")
                    }
                }
            };
    }
}