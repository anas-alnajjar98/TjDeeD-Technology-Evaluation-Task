using Application.DTOs.UserDTOS;
using Application.Interfaces;
using Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserController> _logger;
        private readonly IAuthService _authService;
        public UserController(IUserService userService, ILogger<UserController> logger, IAuthService authService)
        {
            _userService = userService;
            _logger = logger;
            _authService = authService; 
        }
        /// <summary>
        /// Registers a new user with the provided registration data.
        /// </summary>
        /// <param name="registrationDto">The registration data containing the user's username, email, password, and full name.</param>
        /// <returns>Returns a success message if registration is successful, or an error message if registration fails.</returns>
        /// <response code="200">User registered successfully.</response>
        /// <response code="400">Bad request if registration data is invalid or the user already exists.</response>
        /// <response code="500">Internal server error if an unexpected error occurs during registration.</response>
        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser([FromBody] UserRegistrationDto registrationDto)
        {
            if (registrationDto == null)
            {
                _logger.LogWarning("Registration failed: Registration data is null.");
                return BadRequest("Invalid registration data.");
            }


            var result = await _userService.RegisterUser(registrationDto);

            if (result == "User registered successfully.")
            {
                _logger.LogInformation("User registered successfully: {Username}", registrationDto.Username);
                return Ok(result);
            }

            _logger.LogWarning("Registration failed for user: {Username}", registrationDto.Username);
            return BadRequest(result);
        }
        /// <summary>
        /// Authenticates a user with the provided login credentials (email/username and password).
        /// </summary>
        /// <param name="loginRequest">The login request containing the user's email/username and password.</param>
        /// <returns>Returns an authentication response containing the access token and refresh token if login is successful.</returns>
        /// <response code="200">Successfully authenticated, returns access token and refresh token.</response>
        /// <response code="401">Unauthorized if credentials are invalid.</response>
        /// <response code="500">Internal server error if an unexpected error occurs during authentication.</response>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] Application.DTOs.LoginREQ_RES.LoginRequest loginRequest)
        {
            try
            {
                var loginResponse = await _authService.LoginAsync(loginRequest);
                return Ok(loginResponse); 
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized("Invalid credentials");
            }
        }
        /// <summary>
        /// Refreshes the user's access token using the provided refresh token.
        /// </summary>
        /// <param name="refreshToken">The refresh token used to obtain a new access token.</param>
        /// <returns>Returns a new access token if the refresh token is valid and not expired.</returns>
        /// <response code="200">Successfully refreshed the access token.</response>
        /// <response code="401">Unauthorized if the refresh token is invalid or expired.</response>
        /// <response code="500">Internal server error if an unexpected error occurs during the refresh process.</response>
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] string refreshToken)
        {
            try
            {
                var newAccessToken = await _authService.RefreshAccessTokenAsync(refreshToken);
                return Ok(new { AccessToken = newAccessToken });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized("Invalid or expired refresh token.");
            }
        }

    }
}
