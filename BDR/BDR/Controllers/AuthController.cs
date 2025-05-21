using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using BDR.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace BDR.Controllers
{
    public class AuthController : Controller
    {
        private readonly ILogger<AuthController> _logger;
        HttpClient _httpClient;

        public AuthController(ILogger<AuthController> logger, HttpClient httpClient)
        {
            this._logger = logger;

            HttpClientHandler handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };

            _httpClient = new HttpClient(handler);
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
                var result = await response.Content.ReadAsStringAsync();
                var user = JsonConvert.DeserializeObject<User>(result);

                if (user != null)
                {
                    // session
                    HttpContext.Session.SetString("UserId", user.userId.ToString());
                    HttpContext.Session.SetString("UserName", user.userName);
                    HttpContext.Session.SetString("UserEmail", user.userEmail);
                    HttpContext.Session.SetString("UserRole", user.userRole);

                    // redirect based on role
                    string redirectUrl = user.userRole switch
                    {
                        "SuperAdmin" => Url.Action("SuperAdminDashboard", "Home"),
                        "Admin" => Url.Action("AdminDashboard", "Home"),
                        "Vendor" => Url.Action("VendorDashboard", "Home"),
                        "Buyer" => Url.Action("BuyerDashboard", "Home"),
                        _ => null
                    };

                    if (redirectUrl != null)
                        TempData["success"] = $"{user.userName} logged in successfully";
                    return Json(new { success = true, redirectUrl });
                }
            }

            return Json(new { success = false, message = "Error during login" });

        }


        // otp view
        public IActionResult Otp()
        {
            return View();
        }

        // Qr view
        public IActionResult Qr()
        {
            return View();
        }

    }
}