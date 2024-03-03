namespace TTPService.Authorization
{
    public class AuthorizationOptions
    {
        public string IssuerSigningKey { get; set; }

        public string Issuer { get; set; }

        public string Audience { get; set; }

        public string AuthenticationTokenEndPoint { get; set; }
    }
}