using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizPortalAPI.DTOs.Auth;
using QuizPortalAPI.Services;
using System.Security.Claims;

namespace QuizPortalAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// POST: api/auth/register
        /// Register new user (Teacher/Student)
        /// </summary>
        [HttpPost("register")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AuthResponseDTO>> Register([FromBody] RegisterDTO registerDTO)
        {
            try
            {
                if (!ModelState.IsValid) // keeps track of validation errors
                {
                    _logger.LogWarning("Invalid registration request");
                    return BadRequest(ModelState);
                }

                var result = await _authService.RegisterAsync(registerDTO);

                if (!result.Success)
                {
                    _logger.LogWarning($"Registration failed for email: {registerDTO.Email}");
                    return Conflict(result);
                }

                // Set JWT token in HttpOnly cookie
                SetAuthCookies(result.AccessToken);

                _logger.LogInformation($"New user registered: {registerDTO.Email}");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during registration: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new AuthResponseDTO
                    {
                        Success = false,
                        Message = "An error occurred during registration"
                    });
            }
        }

        /// <summary>
        /// POST: api/auth/login
        /// Authenticate user and return JWT token
        /// </summary>
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AuthResponseDTO>> Login([FromBody] LoginDTO loginDTO)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid login request");
                    return BadRequest(ModelState);
                }

                var result = await _authService.LoginAsync(loginDTO);

                if (!result.Success)
                {
                    _logger.LogWarning($"Login failed for email: {loginDTO.Email}");
                    return Unauthorized(result);
                }

                // Set JWT token in HttpOnly cookie
                SetAuthCookies(result.AccessToken);

                _logger.LogInformation($"User logged in successfully: {loginDTO.Email}");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during login: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new AuthResponseDTO
                    {
                        Success = false,
                        Message = "An error occurred during login"
                    });
            }
        }

        /// <summary>
        /// POST: api/auth/logout
        /// Clear JWT cookies
        /// </summary>
        [HttpPost("logout")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult Logout()
        {
            try
            {
                // Clear cookie
                Response.Cookies.Delete("accessToken");

                _logger.LogInformation("User logged out successfully");
                return Ok(new { success = true, message = "Logged out successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during logout: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { success = false, message = "An error occurred during logout" });
            }
        }

        /// <summary>
        /// Helper method to set JWT access token in HttpOnly cookie
        /// </summary>
        private void SetAuthCookies(string? accessToken)
        {
            if (string.IsNullOrEmpty(accessToken))
                return;

            var accessTokenCookieOptions = new CookieOptions
            {
                HttpOnly = true,  // Not accessible via JavaScript
                Secure = !HttpContext.Request.IsHttps ? false : true,  // HTTPS only in production
                SameSite = SameSiteMode.Strict,  // CSRF protection
                Expires = DateTimeOffset.UtcNow.AddMinutes(60)  // 60 minutes
            };

            Response.Cookies.Append("accessToken", accessToken, accessTokenCookieOptions);
        }

        /// <summary>
        /// Helper method to clear JWT cookies
        /// </summary>
        private void ClearAuthCookies()
        {
            Response.Cookies.Delete("accessToken");
        }
    }
}
