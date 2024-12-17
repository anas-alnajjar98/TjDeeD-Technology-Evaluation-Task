using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Application.Interfaces
{
    public interface IImageService
    {
        Task<string> SaveImageAsync(IFormFile image);

    }
}
