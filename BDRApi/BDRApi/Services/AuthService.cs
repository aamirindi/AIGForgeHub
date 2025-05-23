using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Threading.Tasks;
using BDRApi.Data;
using BDRApi.DTOs;
using BDRApi.Models;
using BDRApi.Repository;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

namespace BDRApi.Services
{
    public class AuthService : IAuth
    {
        private readonly ApplicationDbContext _db;
        private readonly IJwt _jwt;
        private readonly IConfiguration _config;
        private readonly string _emailSender;
        private readonly string _appPassword;

        public AuthService(ApplicationDbContext _db, IJwt _jwt, IConfiguration config)
        {
            this._db = _db;
            this._jwt = _jwt;
            _config = config;
            _emailSender = config["Email:email"] ?? throw new ArgumentException("email is not configured in app settings");
            _appPassword = config["Email:password"] ?? throw new ArgumentException("appPassword is not configured in app settings");
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

        public async Task<AuthResponse> UpdateTwoFactorAuthAsync(int id)
        {
            var userData = await _db.Users.FindAsync(id);
            if (userData != null)
            {
                if (userData.TwoFactorAuth.Equals("Yes"))
                {
                    userData.TwoFactorAuth = "No";
                    await _db.SaveChangesAsync();
                    return new AuthResponse("Update Succesfully");
                }
                else
                {
                    userData.TwoFactorAuth = "Yes";
                    await _db.SaveChangesAsync();
                    return new AuthResponse("Update Succesfully");

                }
            }
            return new AuthResponse("User not found!");
        }


        public async Task<AuthResponse> ForgotQr(string email)
        {
            string subject = "Forgot Otp Coming";
            Random random = new Random();
            string token = random.Next(100000, 999999).ToString();
            string body = $"Your Otp is{token}";
            var s = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(_emailSender, _appPassword),
                EnableSsl = true
            };
            var MailMessage = new MailMessage
            {
                From = new MailAddress(_emailSender),
                IsBodyHtml = true,
                Subject = subject,
                Body = body
            };
            MailMessage.To.Add(email);
            s.Send(MailMessage);
            return new AuthResponse("Otp sent successfully", token);
        }

    }
}