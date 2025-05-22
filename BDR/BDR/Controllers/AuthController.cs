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


                    if(user.twoFactorAuth.Equals("No"))
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


        // otp view
        public IActionResult Otp()
        {
            if(HttpContext.Session.GetString("UserEmail")==null)
            {
                return RedirectToAction("login");
            }

            return View();
        }

        [HttpPost]
        public IActionResult Otp(IFormCollection fc)
        {
            var token = fc["otp"];
            Console.WriteLine(token);
            TwoFactorAuthenticator tfa = new TwoFactorAuthenticator();
            string useruniqueKey = Convert.ToString(HttpContext.Session.GetString("UserEmail")) + _key;
            //Console.WriteLine(useruniqueKey);
            bool isValid = tfa.ValidateTwoFactorPIN(useruniqueKey, token);

            int userId = int.Parse(HttpContext.Session.GetString("UserId"));

            string userRole = HttpContext.Session.GetString("UserRole");
            string userName = HttpContext.Session.GetString("UserName");
            if (isValid)
            {


                HttpContext.Session.SetString("id", Convert.ToString(HttpContext.Session.GetString("UserEmail")) + _key);

                //string url = "http://localhost:5056/api/Auth/UpdateUser/{userId}";
                //var user = new User
                //{
                //    twoFactorAuth = "Yes"
                //};

                //var json = JsonConvert.SerializeObject(user);
                //StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                //HttpResponseMessage res = _httpClient.PatchAsync(url, content).Result;



                //redirect based on roleS
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
            TempData["error"] = "Invalid OTP";
            return RedirectToAction("login");
        }

        // Qr view

        public IActionResult Qr()
        {
            if (HttpContext.Session.GetString("UserEmail") != null)
            {
                TwoFactorAuthenticator tfa = new TwoFactorAuthenticator();
                string useruniqueKey = Convert.ToString(HttpContext.Session.GetString("UserEmail")) + _key;
                var setupCode = tfa.GenerateSetupCode("Google Authenticator", HttpContext.Session.GetString("UserEmail"), useruniqueKey, false);
                ViewBag.QrCodeImageUrl = setupCode.QrCodeSetupImageUrl;
                ViewBag.SetupCode = setupCode.ManualEntryKey;
                return View();
            }
            else
            {
                return RedirectToAction("login");
            }
        }


    }
}