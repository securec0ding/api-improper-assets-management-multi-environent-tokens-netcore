using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Backend
{
    public static class JwtConfiguration
    {
        public const string SigningKey = "BHE1oRODZ7JpsvQ8Uzz89Eg43m69jOB8TIdB6Kkk";
        public const string Audience = "Hunter2 Bank API Clients";
        public const string Issuer = "Hunter2 Bank API";
        public const int ExpireSeconds = int.MaxValue;

        public static TokenValidationParameters GetTokenValidationParameters()
        {
            return new TokenValidationParameters
            {
                ValidIssuer = Issuer,
                ValidAudience = Audience,
                ValidateAudience = true,
                ValidateIssuer = true,
                ValidateLifetime = false,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SigningKey)),
            };
        }

        public static byte[] SigningKeyBytes
        {
            get
            {
                return Encoding.UTF8.GetBytes(SigningKey);
            }
        }
    }
}