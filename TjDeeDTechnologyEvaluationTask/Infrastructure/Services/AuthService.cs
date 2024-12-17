using System;
using System.Linq;
using System.Threading.Tasks;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Application.Interfaces;
using Application.DTOs.LoginREQ_RES;
using Infrastructure.Data;
using Microsoft.Extensions.Configuration;
using Infrastructure.Helper;
using Domain.Entities.Domain.Entities;

namespace Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly DbContextTask _context;
        private readonly IConfiguration _configuration;
        private readonly ITokenService _tokenService;
        private readonly IDateTimeProvider _dateTimeProvider;

        public AuthService(DbContextTask context, IConfiguration configuration, ITokenService tokenService, IDateTimeProvider dateTimeProvider)
        {
            _context = context;
            _configuration = configuration;
            _tokenService = tokenService;
            _dateTimeProvider = dateTimeProvider;
        }
        /// <summary>
        /// Validates the user's credentials and generates an access token and refresh token if the login is successful.
        /// </summary>
        /// <param name="loginRequest">The login request containing the user's email/username and password.</param>
        /// <returns>A response containing the access token and refresh token for authenticated users.</returns>
        /// <exception cref="UnauthorizedAccessException">Thrown when the credentials are invalid.</exception>
        public async Task<LoginResponse> LoginAsync(LoginRequest loginRequest)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == loginRequest.Email || u.Email == loginRequest.Email);

            if (user == null || !PasswordHelper.VerifyPassword(loginRequest.Password, user.PasswordHash, user.PasswordSalt))
            {
                throw new UnauthorizedAccessException("Invalid credentials.");
            }

            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshToken = await GenerateRefreshToken(user);

            return new LoginResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token
            };
        }

        private async Task<RefreshToken> GenerateRefreshToken(User user)
        {
            var refreshToken = new RefreshToken
            {
                UserId = user.Id,
                Token = Guid.NewGuid().ToString(),
                ExpiryDate = _dateTimeProvider.UtcNow.AddDays(2), 
                IsUsed = false,
                IsRevoked = false
            };

            await _context.RefreshTokens.AddAsync(refreshToken);
            await _context.SaveChangesAsync();

            return refreshToken;
        }
        /// <summary>
        /// Validates the provided refresh token and issues a new access token if valid.
        /// </summary>
        /// <param name="refreshToken">The refresh token to validate.</param>
        /// <returns>A new access token if the refresh token is valid and not expired.</returns>
        /// <exception cref="UnauthorizedAccessException">Thrown if the refresh token is invalid or expired.</exception>
        public async Task<string> RefreshAccessTokenAsync(string refreshToken)
        {
            var token = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken && !rt.IsRevoked && !rt.IsUsed);

            if (token == null || token.ExpiryDate < _dateTimeProvider.UtcNow)
            {
                throw new UnauthorizedAccessException("Invalid or expired refresh token.");
            }

            token.IsUsed = true;
            await _context.SaveChangesAsync();

            var user = await _context.Users.FindAsync(token.UserId);
            return _tokenService.GenerateAccessToken(user);
        }
    }
}
