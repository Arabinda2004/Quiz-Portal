using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizPortalAPI.DTOs.Auth;
using QuizPortalAPI.Services;

namespace QuizPortalAPI.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDTO>> Register([FromBody] RegisterDTO registerDTO)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid registration request");
                    return BadRequest(ModelState);
                }

                var result = await _authService.RegisterAsync(registerDTO);

                if (!result.Success)
                {
                    _logger.LogWarning($"Registration failed for email: {registerDTO.Email}");
                    return Conflict(result); // if email already exists or role invalid
                }

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

        [HttpPost("login")]
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

        [HttpPost("logout")]
        [Authorize]
        public IActionResult Logout()
        {
            try
            {
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
    }
}
