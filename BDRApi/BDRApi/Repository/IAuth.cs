using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BDRApi.DTOs;
using BDRApi.Models;

namespace BDRApi.Repository
{
    public interface IAuth
    {
        Task<AuthResponse> Authentication(string email, string password);
        List<User> GetUsers();
        Task<AuthResponse> GetUserData(string email, string pass);
    }
}