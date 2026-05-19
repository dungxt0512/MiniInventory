using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace MiniInventory.Helpers
{
    public interface JwtHelpers
    {
        string generateToken(int userId, string username, string role);
        bool validateToken(string token);
        ClaimsPrincipal GetClaimsPrincipal(string token);
    }
    public class JwtHelpersImpl : JwtHelpers
    {
        private readonly IConfiguration _config;
        public JwtHelpersImpl(IConfiguration config)
        {
            _config = config;
        }

        public string generateToken(int userId, string username, string role)
        {
            try
            {
                var jwtSettings = _config.GetSection("JwtSettings");
                var secretKey = jwtSettings["Secret"];
                var Issuer = jwtSettings["Issuer"];
                var audience = jwtSettings["Audience"];
                var ExpireHours = int.Parse(jwtSettings["ExpireHours"]!);
                var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(secretKey!));
                var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var claims = new[]
                {
                    new Claim("userId", userId.ToString()),
                    new Claim(ClaimTypes.Name, username),
                    new Claim(ClaimTypes.Role, role),
                    new Claim("iss", Issuer)
                };
                var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
                    issuer: Issuer,
                    audience: audience,
                    claims: claims,
                    expires: DateTime.Now.AddHours(ExpireHours),
                    signingCredentials: credentials
                );
                var tokenString = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().WriteToken(token);
                return tokenString;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Loi tao KWT token: {ex.Message}");
                throw;
            }
        }
        public ClaimsPrincipal GetClaimsPrincipal(string token)
        {
            try
            {
                var jwtSettings = _config.GetSection("JwtSettings");
                var secretKey = jwtSettings["Secret"];
                var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(secretKey!));
                var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"],
                    IssuerSigningKey = key,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);
                return principal;
            }
            catch
            {
                return null!;
            }
        }

        public bool validateToken(string token)
        {
            try
            {
                var jwtSettings = _config.GetSection("JwtSettings");
                var secretKey = jwtSettings["Secret"];
                var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(secretKey));
                var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"],
                    IssuerSigningKey = key,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);
                return validatedToken != null;
            }
            catch
            {
                return false;
            }
        }
    }
}
