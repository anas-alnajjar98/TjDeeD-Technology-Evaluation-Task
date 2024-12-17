using Application.Interfaces;
using Domain.Entities.Domain.Entities;
using Infrastructure.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

using System.Text;

/// <summary>
/// Provides methods for generating JWT access tokens and refresh tokens.
/// </summary>
public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;
    private readonly DbContextTask _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="TokenService"/> class.
    /// </summary>
    /// <param name="configuration">The configuration containing JWT settings.</param>
    /// <param name="context">The database context for accessing user roles.</param>
    public TokenService(IConfiguration configuration, DbContextTask context)
    {
        _configuration = configuration;
        _context = context;
    }

    /// <summary>
    /// Generates an access token for the specified user.
    /// </summary>
    /// <param name="user">The user for whom the access token is being generated.</param>
    /// <returns>The generated JWT access token.</returns>
    public string GenerateAccessToken(User user)
    {
        var secretKey = _configuration["Jwt:SecretKey"];
        var issuer = _configuration["Jwt:Issuer"];
        var audience = _configuration["Jwt:Audience"];

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, string.Join(",", _context.UserRoles
                .Where(x => x.Id == _context.UserRoleUsers
                    .Where(y => y.UserId == user.Id)
                    .SingleOrDefault().RoleId)
                .SingleOrDefault().RoleName)),
            new Claim("userId", user.Id.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            expires: DateTime.Now.AddMinutes(30),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Generates a refresh token for the specified user.
    /// </summary>
    /// <param name="user">The user for whom the refresh token is being generated.</param>
    /// <returns>A newly generated refresh token.</returns>
    public string GenerateRefreshToken(User user)
    {
        return Guid.NewGuid().ToString();
    }
}
