using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BDRApi.DTOs
{
    public class OtpVerificationRequest
    {

        public string Email { get; set; }
        public string Token { get; set; }
    }
}