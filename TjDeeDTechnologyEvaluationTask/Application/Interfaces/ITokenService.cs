using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities.Domain.Entities;

namespace Application.Interfaces
{
    public interface ITokenService
    {
        string GenerateAccessToken(User user);
    }
}
