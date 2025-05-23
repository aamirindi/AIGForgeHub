using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using BDR.Models;
using Google.Authenticator;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using static QRCoder.PayloadGenerator;

namespace BDR.Controllers
{
    public class AuthController : Controller
    {
        private readonly ILogger<AuthController> _logger;
        HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly string _key;



        public AuthController(ILogger<AuthController> logger, HttpClient httpClient, IConfiguration configuration)
        {
            this._logger = logger;

            HttpClientHandler handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };

            _httpClient = new HttpClient(handler);
            _configuration = configuration;
            _key = configuration["TwoAuth:Key"] ?? throw new ArgumentException("Key is not configured in app settings");
        }


        // login view
        public IActionResult Login()
        {
            //if (Request.Cookies["jwtToken"] != null && HttpContext.Session.GetString("UserRole") != null)
            //{
            //    string role = HttpContext.Session.GetString("UserRole");
            //    return RedirectToAction(role + "Dashboard", "Home");
            //}
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {

            var loginDto = new Login
            {
                email = email,
                pass = password
            };

            string url = $"http://localhost:5056/api/Auth/Login";

            var json = JsonConvert.SerializeObject(loginDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            Console.WriteLine("JsonConver: " + json);

            var response = await _httpClient.PostAsync(url, content);


            if (response.IsSuccessStatusCode)
            {
                //var result = await response.Content.ReadAsStringAsync();
                //var user = JsonConvert.DeserializeObject<User>(result);

                var result = await response.Content.ReadAsStringAsync();
                var loginResponse = JsonConvert.DeserializeObject<LoginResponse>(result);

                var user = loginResponse?.result?.data;


                //Console.WriteLine(user.userName);

                if (user != null)
                {
                    // session
                    HttpContext.Session.SetString("UserId", user.userId.ToString());
                    HttpContext.Session.SetString("UserName", user.userName);
                    HttpContext.Session.SetString("UserEmail", user.userEmail);
                    HttpContext.Session.SetString("UserRole", user.userRole);
                    HttpContext.Session.SetString("Password", user.userPass);
                    HttpContext.Session.SetString("TwoFactorAuth", user.twoFactorAuth);


                    if (user.twoFactorAuth.Equals("No"))
                    {
                        return RedirectToAction("Qr");
                    }
                    else

                    {
                        return RedirectToAction("Otp");
                    }
                }
            }

            return View();
        }

        public IActionResult Otp()
        {
            // If JWT exists, user is logged in — prevent OTP access
            //if (Request.Cookies["jwtToken"] != null && HttpContext.Session.GetString("UserRole") != null)
            //{
            //    string role = HttpContext.Session.GetString("UserRole");
            //    return RedirectToAction(role + "Dashboard", "Home");
            //}

            // Else allow
            if (HttpContext.Session.GetString("UserEmail") == null)
            {
                return RedirectToAction("Login");
            }

            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Otp(IFormCollection fc)
        {
            TwoFactorAuthenticator tfa = new TwoFactorAuthenticator();
            string useruniqueKey = HttpContext.Session.GetString("UserEmail") + _key;
            bool isValid = tfa.ValidateTwoFactorPIN(useruniqueKey, fc["otp"]);

            if (!isValid)
            {
                TempData["error"] = "Invalid OTP";
                return View();
            }

            int userId = int.Parse(HttpContext.Session.GetString("UserId"));
            string userEmail = HttpContext.Session.GetString("UserEmail");
            string userPass = HttpContext.Session.GetString("Password");
            string userRole = HttpContext.Session.GetString("UserRole");
            string userName = HttpContext.Session.GetString("UserName");

            // Update user status
            string updateUrl = $"http://localhost:5056/api/Auth/UpdateUser/{userId}";
            await _httpClient.PatchAsync(updateUrl, null);

            // Get JWT Token
            var loginDto = new Login { email = userEmail, pass = userPass };
            string jwtUrl = "http://localhost:5056/api/Auth/jwt-login";
            var content = new StringContent(JsonConvert.SerializeObject(loginDto), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(jwtUrl, content);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                var tokenResponse = JsonConvert.DeserializeObject<OtpTokenResponse>(result);

                if (!string.IsNullOrEmpty(tokenResponse?.token))
                {
                  
                    CookieOptions options = new CookieOptions
                    {
                        HttpOnly = true,
                        Expires = DateTime.Now.AddHours(1)
                    };
                    Response.Cookies.Append("jwtToken", tokenResponse.token, options);
                }
            }

          
            string redirectUrl = userRole switch
            {
                "SuperAdmin" => "SuperAdminDashboard",
                "Admin" => "AdminDashboard",
                "Vendor" => "VendorDashboard",
                "Buyer" => "BuyerDashboard",
                _ => "Login"
            };

            TempData["success"] = $"{userName} logged in successfully";
            return RedirectToAction(redirectUrl, "Home");
        }




        // Qr view

        //public IActionResult Qr()
        //{
        //    if (HttpContext.Session.GetString("UserEmail") != null)
        //    {
        //        TwoFactorAuthenticator tfa = new TwoFactorAuthenticator();

        //        string useruniqueKey = Convert.ToString(HttpContext.Session.GetString("UserEmail")) + _key;
        //        var setupCode = tfa.GenerateSetupCode("Google Authenticator", HttpContext.Session.GetString("UserEmail"), useruniqueKey, false);
        //        ViewBag.QrCodeImageUrl = setupCode.QrCodeSetupImageUrl;
        //        ViewBag.SetupCode = setupCode.ManualEntryKey;
        //        return View();
        //    }
        //    else
        //    {
        //        return RedirectToAction("login");
        //    }
        //}


        public IActionResult Qr()
        {
            var email = HttpContext.Session.GetString("UserEmail");
            var twoFA = HttpContext.Session.GetString("TwoFactorAuth");

            if (string.IsNullOrEmpty(email))
                return RedirectToAction("Login");

            if(twoFA == "Yes")
                return RedirectToAction("Otp");



            TwoFactorAuthenticator tfa = new TwoFactorAuthenticator();
            string useruniqueKey = email + _key;
            var setupCode = tfa.GenerateSetupCode("Google Authenticator", email, useruniqueKey, false);

            ViewBag.QrCodeImageUrl = setupCode.QrCodeSetupImageUrl;
            ViewBag.SetupCode = setupCode.ManualEntryKey;

            return View();
        }


        [HttpPost]
        public async Task<IActionResult> OtpGetting()
        {
            if (HttpContext.Session.GetString("UserEmail") != null)
            {
                string email = HttpContext.Session.GetString("UserEmail");
                string url = $"http://localhost:5056/api/Auth/SendMail/{email}";

                var response = await _httpClient.PostAsync(url, null);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();

                    var otpResponse = JsonConvert.DeserializeObject<OtpTokenResponse>(result);

                    if (otpResponse != null && otpResponse.success)
                    {

                        TempData["otp"] = otpResponse.token;
                        return RedirectToAction("OtpEmail");
                    }
                }

                return RedirectToAction("Login");
            }

            return RedirectToAction("Login");
        }


        public IActionResult OtpEmail()
        {
            if (HttpContext.Session.GetString("UserEmail") != null)
            {
                return View();
            }
            else
            {
                return RedirectToAction("login");
            }
        }

        [HttpPost]
        public async Task<IActionResult> OtpEmail(IFormCollection fc)
        {
            var token = fc["otp"];
            int userId = int.Parse(HttpContext.Session.GetString("UserId"));
            var tempOtp = TempData["otp"];

            if (token.Equals(tempOtp))
            {
                string url = $"http://localhost:5056/api/Auth/UpdateUser/{userId}";
                HttpResponseMessage res = await _httpClient.PatchAsync(url, null);

                return RedirectToAction("Qr");
            }
            else
            {
                TempData["error"] = "Incorrect Otp";
                return View();
            }
        }

    }
}