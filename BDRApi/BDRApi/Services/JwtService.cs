using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using BDRApi.DTOs;
using BDRApi.Repository;
using Microsoft.IdentityModel.Tokens;

namespace BDRApi.Services
{
    public class JwtService : IJwt
    {
        private readonly string _secretKey;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly int _expirationMinutes;
        private readonly IConfiguration _configuration;
        public JwtService(IConfiguration _configuration)
        {
            this._configuration = _configuration;
            this._secretKey = _configuration["Jwt:Key"] ?? throw new ArgumentException("Secret key is not configured in appsetting");
            this._issuer = _configuration["Jwt:Issuer"] ?? throw new ArgumentException("Issuer is not configured in appsetting");
            this._audience = _configuration["Jwt:Audience"] ?? throw new ArgumentException("Audience is not configured in appsetting");
            this._expirationMinutes = int.Parse(_configuration["Jwt:ExpirationMinutes"] ?? "60");
        }


        // this is used to generate JWT token using the provided parameters
        public string GenerateToken(int userId, string userName, string userEmail, string userPass, string userRole)
        {
            // tokenHandelet we use to create and write the token
            var tokenHandler = new JwtSecurityTokenHandler();
            // key
            var key = Encoding.ASCII.GetBytes(_secretKey);

            // token descriptor to add the subject and exipration minutes and creadentils 
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(
                    new[]{
                        new Claim("userId",  userId.ToString()),
                        new Claim("userName", userName),
                        new Claim("userEmail", userEmail),
                        new Claim("userPass",  userPass),
                        new Claim(ClaimTypes.Role, userRole)
                        // new Claim("userRole", userRole)
                    }
                ),
                Expires = DateTime.UtcNow.AddMinutes(_expirationMinutes),

                // signing creadentails == key + algo
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }


        public string GetTokenFromHeader(string authorizationHeader)
        {
            if (string.IsNullOrEmpty(authorizationHeader) || !authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                throw new SecurityTokenException("Authorization header is missing or invalid.");
            }
            var token = authorizationHeader.Substring("Bearer ".Length).Trim();

            if (string.IsNullOrEmpty(token))
            {
                throw new SecurityTokenException("Token is missing in the Authorization header.");
            }

            return token;
        }

        public async Task<UserClaims> ReadTokenData(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);

            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type.Equals("userId"));

            var userData = new UserClaims()
            {
                userId = userIdClaim != null && int.TryParse(userIdClaim.Value, out int parsedUserId) ? parsedUserId : 0,
                userName = jwtToken.Claims.FirstOrDefault(c => c.Type.Equals("userName"))?.Value,
                userEmail = jwtToken.Claims.FirstOrDefault(c => c.Type.Equals("userEmail"))?.Value,
                userPass = jwtToken.Claims.FirstOrDefault(c => c.Type.Equals("userPass"))?.Value,
                userRole = jwtToken.Claims.FirstOrDefault(c =>
                    c.Type == ClaimTypes.Role || c.Type == "role")?.Value
            };

            return userData;
        }


        public JwtSecurityToken VertifyToken(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                throw new SecurityTokenException("Token is missing.");
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_secretKey);

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                RoleClaimType = ClaimTypes.Role
            };

            try
            {
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken validatedToken);

                if (validatedToken is JwtSecurityToken jwtToken &&
                    jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    return jwtToken;
                }

                throw new SecurityTokenException("Token is invalid.");
            }
            catch (Exception ex)
            {
                throw new SecurityTokenException($"Token validation failed: {ex.Message}");
            }
        }

    }
}