using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Helper
{
    public static class PasswordHelper
    {
        
        public static void CreatePasswordHash(string password, out string passwordHash, out string passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = Convert.ToBase64String(hmac.Key); 
                passwordHash = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(password))); 
            }
        }

        
        public static bool VerifyPassword(string password, string storedHash, string storedSalt)
        {
            using (var hmac = new HMACSHA512(Convert.FromBase64String(storedSalt))) 
            {
                var computedHash = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(password))); 
                return computedHash == storedHash; 
            }
        }
    }
}
