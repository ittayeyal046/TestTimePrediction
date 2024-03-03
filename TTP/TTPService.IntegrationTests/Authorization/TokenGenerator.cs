using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace TTPService.IntegrationTests.Authorization
{
    public class TokenGenerator
    {
        private readonly AuthenticationOptions _authenticationOptions;

        public TokenGenerator(IOptions<AuthenticationOptions> authenticationOptions)
        {
            _authenticationOptions = authenticationOptions.Value;
        }

        public AuthToken GenerateToken(IEnumerable<Claim> claims)
        {
            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_authenticationOptions.IssuerSigningKey));
            var signingCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

            var expiration = DateTime.UtcNow.AddHours(_authenticationOptions.ExpireTimeInHours);

            var tokeOptions = new JwtSecurityToken(
                issuer: _authenticationOptions.Issuer,
                audience: _authenticationOptions.Audience,
                claims: claims,
                expires: expiration,
                signingCredentials: signingCredentials);

            var tokenString = new JwtSecurityTokenHandler().WriteToken(tokeOptions);

            var authToken = new AuthToken()
            {
                Expiration = expiration,
                Token = tokenString,
            };
            return authToken;
        }
    }
}