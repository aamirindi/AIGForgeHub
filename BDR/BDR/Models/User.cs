using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BDR.Models
{
    public class LoginResponse
    {
        public string message { get; set; }
        public LoginResult result { get; set; }
    }

    public class LoginResult
    {
        public string message { get; set; }
        public User data { get; set; }
    }

    public class User
    {
        public int userId { get; set; }
        public string userName { get; set; }
        public string userEmail { get; set; }
        public string userPass { get; set; }
        public string userRole { get; set; }
        public string userPhone { get; set; }
        public string twoFactorAuth { get; set; }

       
    }

}