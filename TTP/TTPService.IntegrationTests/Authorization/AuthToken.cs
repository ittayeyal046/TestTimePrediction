using System;

namespace TTPService.IntegrationTests.Authorization
{
    public class AuthToken
    {
        public string Token { get; set; }

        public string TokenType => "Bearer";

        public DateTime Expiration { get; set; }
    }
}