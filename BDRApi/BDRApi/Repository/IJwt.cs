using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using BDRApi.DTOs;

namespace BDRApi.Repository
{
    public interface IJwt
    {
        // create Jwt token
        string GenerateToken(int userId, string userName, string userEmail, string userPass, string userRole);

        // verification token
        JwtSecurityToken VertifyToken(string token);

        // extract the token from a request header
        string GetTokenFromHeader(string authorizationHeader);

        // reads data (claims) from the token
        Task<UserClaims> ReadTokenData(string token);
    }
}