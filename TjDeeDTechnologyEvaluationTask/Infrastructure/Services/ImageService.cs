using Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class ImageService : IImageService
    {
        private readonly ILogger<ImageService> _logger;

        public ImageService(ILogger<ImageService> logger)
        {
            _logger = logger;
        }

        public async Task<string> SaveImageAsync(IFormFile image)
        {
            
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image), "Image file is null.");
            }

            if (image.Length == 0)
            {
                throw new ArgumentException("Image file is empty.", nameof(image));
            }

           
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");

           
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

           
            var uniqueFileName = Guid.NewGuid() + "_" + Path.GetFileName(image.FileName);
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

           
            try
            {
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(fileStream);
                }
            }
            catch (Exception ex)
            {
                throw new IOException("An error occurred while saving the image file.", ex);
            }

            
            return $"/images/{uniqueFileName}";
        }

    }
}
