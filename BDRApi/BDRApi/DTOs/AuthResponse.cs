using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BDRApi.DTOs
{
    public class AuthResponse
    {
        public string Message { get; set; }
        public object? Data { get; set; }
        public AuthResponse(string message, object? data = null)
        {
            Message = message;
            Data = data;
        }
    }
}