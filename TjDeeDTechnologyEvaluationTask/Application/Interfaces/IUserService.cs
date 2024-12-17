using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs.UserDTOS;

namespace Application.Interfaces
{
    public interface IUserService
    {
        Task<string> RegisterUser(UserRegistrationDto Request);
    }
}
