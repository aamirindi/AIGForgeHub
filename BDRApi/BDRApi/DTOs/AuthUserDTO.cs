using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BDRApi.DTOs
{
    public class AuthUserDTO
    {
        public int userId { get; set; }
        public string userName { get; set; }
        public string userEmail { get; set; }
        public string Pass { get; set; }
        public string userRole { get; set; }
    }
}