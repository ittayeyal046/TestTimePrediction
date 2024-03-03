using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using Microsoft.Extensions.Options;

namespace TTPService.IntegrationTests.Authorization
{
    internal static class TokenHelper
    {
        private static readonly TokenGenerator TokenGenerator;

        static TokenHelper()
        {
            TokenGenerator = new TokenGenerator(new OptionsWrapper<AuthenticationOptions>(
                new AuthenticationOptions
                {
                    IssuerSigningKey = "superSecretKey@345",
                    ExpireTimeInHours = 1,
                    Issuer = "ceres",
                    Audience = "ceres-users",
                }));
        }

        public static string GetToken()
        {
            return TokenGenerator.GenerateToken(new List<Claim>() { new Claim("role", "gdlusers") }).Token;
        }

        public static HttpClient WithAuthorization(this HttpClient client)
        {
            var token = TokenHelper.GetToken();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            return client;
        }
    }
}