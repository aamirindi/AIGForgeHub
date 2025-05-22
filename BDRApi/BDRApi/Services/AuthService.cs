using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BDRApi.Data;
using BDRApi.DTOs;
using BDRApi.Models;
using BDRApi.Repository;
using Microsoft.EntityFrameworkCore;

namespace BDRApi.Services
{
    public class AuthService : IAuth
    {
        private readonly ApplicationDbContext _db;
        private readonly IJwt _jwt;
        // private readonly IConfiguration _configuration;

        public AuthService(ApplicationDbContext _db, IJwt _jwt)
        {
            this._db = _db;
            this._jwt = _jwt;
            // this._configuration = _configuration;
        }

        public async Task<AuthResponse> GetUserData(string email, string pass)
        {

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(pass))
            {
                return new AuthResponse("Email and Password are required!");
            }

            var user = await _db.Users
                .FirstOrDefaultAsync(u => u.userEmail == email && u.userPass == pass);

            if (user == null)
            {
                return new AuthResponse("Invalid email or password!");
            }

            return new AuthResponse("Login successful!", user);
        }


        public async Task<AuthResponse> Authentication(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                return new AuthResponse("Email and Password are required!");
            }

            var user = await _db.Users
                .FirstOrDefaultAsync(u => u.userEmail == email && u.userPass == password);

            if (user == null)
            {
                return new AuthResponse("Invalid email or password!");
            }

            var data = new AuthUserDTO
            {
                userId = user.userId,
                userName = user.userName,
                userEmail = user.userEmail,
                Pass = user.userPass,
                userRole = user.userRole
            };

            return new AuthResponse("Login successful!", data);
        }

        public List<User> GetUsers()
        {
            var userData = _db.Users.ToList();
            return userData;
        }

        public  void UpdateTwoFactorAuth(User u,int id)
        {

            var userData = _db.Users.Find(id);
            userData.TwoFactorAuth = "Yes";
            _db.SaveChangesAsync();
        }
    }
}