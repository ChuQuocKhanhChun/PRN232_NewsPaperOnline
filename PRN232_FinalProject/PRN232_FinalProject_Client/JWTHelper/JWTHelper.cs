using System.IdentityModel.Tokens.Jwt;

namespace PRN232_FinalProject_Client.JWTHelper
{
    public static class JwtHelper
    {
        public static string? GetClaimFromToken(string token, string claimType)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            return jwt.Claims.FirstOrDefault(c => c.Type == claimType)?.Value;
        }
    }
}
