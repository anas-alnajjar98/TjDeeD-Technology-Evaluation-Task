using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs.LoginREQ_RES;

namespace Application.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponse> LoginAsync(LoginRequest loginRequest);
        Task<string> RefreshAccessTokenAsync(string refreshToken);
    }
}
