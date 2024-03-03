namespace TTPService.IntegrationTests.Authorization
{
    public class AuthenticationOptions
    {
        public string IssuerSigningKey { get; set; }

        public string Issuer { get; set; }

        public string Audience { get; set; }

        public int ExpireTimeInHours { get; set; }
    }
}