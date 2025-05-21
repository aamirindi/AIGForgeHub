using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BDRApi.Models
{
    public class User
    {
        [Key]
        public int userId { get; set; }
        public string userName { get; set; }
        public string userEmail { get; set; }
        public string userPass { get; set; }
        public string userRole { get; set; }
        public string userPhone { get; set; }
        public string TwoFactorAuth { get; set; } = "No";
    }
}