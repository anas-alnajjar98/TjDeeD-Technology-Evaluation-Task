using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Application;
using System.Threading.Tasks;
using Application.Interfaces;
using Application.DTOs.UserDTOS;
using Infrastructure.Data;
using Microsoft.Extensions.Logging;
using Domain.Entities.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Azure.Core;
using Infrastructure.Helper;
using System.Text.RegularExpressions;
using Domain.Entities;

namespace Infrastructure.Services
{
    public class UserService:IUserService
    {
        private readonly DbContextTask _context;
        private readonly ILogger<UserService> _logger;
        public UserService(DbContextTask context, ILogger<UserService> logger)
        {
            _context = context;
            _logger = logger;
        }
        public async Task<string> RegisterUser(UserRegistrationDto registrationDto)
        {
            _logger.LogInformation("Attempting to register user: {Username}", registrationDto.Username);

            try
            {
                
                if (await _context.Users.AnyAsync(u => u.Email == registrationDto.Email || u.Username == registrationDto.Username))
                {
                    _logger.LogWarning("Registration failed: Username or email already exists. Username: {Username}, Email: {Email}",
                        registrationDto.Username, registrationDto.Email);
                    return "Username or email is already taken.";
                }

                
                bool isEmailValid = Regex.IsMatch(registrationDto.Email,
                    @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z",
                    RegexOptions.IgnoreCase);
                bool isPasswordValid = Regex.IsMatch(registrationDto.Password, @"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[!@#$%^&*(),.?\"":{}|<>]).{8,}$");

                if (!isEmailValid)
                {
                    _logger.LogWarning("Registration failed: Invalid email format. Email: {Email}", registrationDto.Email);
                    return "Invalid email format.";
                }
                if (!isPasswordValid)
                {
                    _logger.LogWarning("Registration failed: Invalid password format. Password: {Password}", registrationDto.Password);
                    return "Invalid password format.";
                }

                
                PasswordHelper.CreatePasswordHash(registrationDto.Password, out string passwordHash, out string passwordSalt);

               
                var user = new User
                {
                    Username = registrationDto.Username,
                    Email = registrationDto.Email,
                    PasswordHash = passwordHash,
                    Password = registrationDto.Password,
                    PasswordSalt = passwordSalt,
                    FullName = registrationDto.FullName
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync(); 

               
                var defaultRole = await _context.UserRoles
                    .FirstOrDefaultAsync(r => r.RoleName == "User"); 

                if (defaultRole == null)
                {
                    _logger.LogWarning("Default role 'User' not found.");
                    return "Default role 'User' not found.";
                }

                var userRoleUser = new UserRoleUser
                {
                    UserId = user.Id, 
                    RoleId = defaultRole.Id 
                };

                _context.Add(userRoleUser);
                await _context.SaveChangesAsync();

                _logger.LogInformation("User registered successfully: {Username}", registrationDto.Username);
                return "User registered successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while registering user: {Username}", registrationDto.Username);
                return "An error occurred during registration. Please try again.";
            }
        }

    }
}
