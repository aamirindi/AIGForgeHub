using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BDRApi.DTOs;
using BDRApi.Models;
using BDRApi.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OtpNet;
using QRCoder;

namespace BDRApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuth _auth;
        private readonly IJwt _jwt;

        public AuthController(IJwt _jwt, IAuth _auth)
        {
            this._jwt = _jwt;
            this._auth = _auth;
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO dto)
        {
            var result = await _auth.GetUserData(dto.Email, dto.Pass);

            if (result.Message == "Email and Password is Required!")
                return BadRequest(new { message = result.Message });

            if (result?.Message == "User not found!" || result?.Message == "Invalid email or password!")
                return Unauthorized(new { message = "Invalid email or password!" });

            return Ok(new { message = "Token Generated", result });
        }

        // jwt logins
        [HttpPost("jwt-login")]
        public async Task<IActionResult> JwtLogin([FromBody] LoginDTO dto)
        {
            var result = await _auth.Authentication(dto.Email, dto.Pass);

            if (result.Message == "Email and Password is Required!")
                return BadRequest(new { message = result.Message });

            if (result.Message == "User not found!" || result.Message == "Invalid email or password!" || result.Data == null)
                return Unauthorized(new { message = "Invalid email or password!" });

            var userInfo = result.Data as AuthUserDTO;

            var token = _jwt.GenerateToken(userInfo.userId, userInfo.userName, userInfo.userEmail, userInfo.Pass, userInfo.userRole);

            Response.Cookies.Append("token", token, new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.Strict,
                //Secure = true
            });

            return Ok(new { message = "Token Generated", token });
        }

        // get user Data 
        [Authorize]
        [Route("getUserData")]
        [HttpGet]
        public async Task<IActionResult> GetUserData()
        {
            var authHeader = HttpContext.Request.Headers["Authorization"].ToString();

            var token = authHeader.Substring("Bearer".Length).Trim();
            var result = await _jwt.ReadTokenData(token);
            return Ok(new { result });
        }

        // get all data
        [Authorize(Roles = "SuperAdmin")]
        [Route("fetchAllUsers")]
        [HttpGet]
        public IActionResult FetchAllUsers()
        {
            var users = _auth.GetUsers();
            return Ok(new { success = true, message = users });
        }


        // logout
        [Authorize]
        [Route("logout")]
        [HttpPost]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("token");
            return Ok(new { success = true, message = "Logout Successfully!" });
        }


        [Route("UpdateUser/{id}")]
        [HttpPatch]
        public async Task<IActionResult> UpdateTwoFactorAuth(int id)
        {
            var result = await _auth.UpdateTwoFactorAuthAsync(id);
            if (result.Message == "User not found!")
                return NotFound(new { message = result.Message });

            return Ok(new { success = true, message = result.Message });

        }

        [Route("SendMail/{email}")]
        [HttpPost]
        public async Task<IActionResult> ForgotQr(string email)
        {
            var result = await _auth.ForgotQr(email);
            if (result.Message != "Otp sent successfully")
                return NotFound(new { message = "Mail Not Send" });

            return Ok(new { success = true, message = result.Message, token = result.Data });
        }

    }
}