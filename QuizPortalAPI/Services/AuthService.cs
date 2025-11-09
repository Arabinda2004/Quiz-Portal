using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using QuizPortalAPI.Data;
using QuizPortalAPI.DTOs.Auth;
using QuizPortalAPI.Models;
using QuizPortalAPI.Helpers;
using QuizPortalAPI.DAL.UserRepo;


namespace QuizPortalAPI.Services
{
    public class AuthService : IAuthService
    {
        private readonly ILogger<AuthService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IUserService _userService;
        private readonly AuthHelper _jwtHelper;
        private readonly IUserRepository _userRepository;

        public AuthService(
            ILogger<AuthService> logger,
            IConfiguration configuration,
            IUserService userService,
            AuthHelper jwtHelper,
            IUserRepository userRepository)
        {
            _logger = logger;
            _configuration = configuration;
            _userService = userService;
            _jwtHelper = jwtHelper;
            _userRepository = userRepository;
        }

        public async Task<AuthResponseDTO> LoginAsync(LoginDTO loginDTO)
        {
            try
            {
                var user = await _userRepository.FindUserByEmailAsync(loginDTO.Email);
                if (user == null)
                {
                    _logger.LogWarning($"Login attempt with non-existent email: {loginDTO.Email}");
                    return new AuthResponseDTO
                    {
                        Success = false,
                        Message = "Invalid email or password"
                    };
                }

                if (!BCrypt.Net.BCrypt.Verify(loginDTO.Password, user.Password))
                {
                    _logger.LogWarning($"Failed login attempt for user: {user.Email}");
                    return new AuthResponseDTO
                    {
                        Success = false,
                        Message = "Invalid email or password"
                    };
                }

                var accessToken = _jwtHelper.GenerateAccessToken(user);

                var userInfo = new UserInfoDTO
                {
                    UserID = user.UserID,
                    FullName = user.FullName,
                    Email = user.Email,
                    Role = user.Role.ToString(),
                    IsDefaultPassword = user.IsDefaultPassword,
                    CreatedAt = user.CreatedAt
                };

                _logger.LogInformation($"User logged in successfully: {user.Email}");

                return new AuthResponseDTO
                {
                    Success = true,
                    Message = "Login successful",
                    AccessToken = accessToken,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(
                        int.Parse(_configuration["JwtSettings:ExpirationMinutes"] ?? "60")),
                    User = userInfo
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during login: {ex.Message}");
                return new AuthResponseDTO
                {
                    Success = false,
                    Message = "An error occurred during login"
                };
            }
        }

        public async Task<AuthResponseDTO> RegisterAsync(RegisterDTO registerDTO)
        {
            try
            {
                if (await _userRepository.IsUserExistsWithEmailAsync(registerDTO.Email))
                {
                    _logger.LogWarning($"Registration attempt with existing email: {registerDTO.Email}");
                    return new AuthResponseDTO
                    {
                        Success = false,
                        Message = "Email already registered"
                    };
                }

                if (!Enum.TryParse<UserRole>(registerDTO.Role, out var userRole) || 
                    userRole == UserRole.Admin)
                {
                    _logger.LogWarning($"Invalid role attempted during registration: {registerDTO.Role}");
                    return new AuthResponseDTO
                    {
                        Success = false,
                        Message = "Invalid role. Only 'Teacher' or 'Student' allowed"
                    };
                }

                var user = new User
                {
                    FullName = registerDTO.FullName,
                    Email = registerDTO.Email,
                    Password = BCrypt.Net.BCrypt.HashPassword(registerDTO.Password),
                    Role = userRole,
                    IsDefaultPassword = false,
                    CreatedAt = DateTime.UtcNow
                };

                await _userRepository.CreateAsync(user);

                _logger.LogInformation($"New user registered as {userRole}: {user.Email}");

                var accessToken = _jwtHelper.GenerateAccessToken(user);

                var userInfo = new UserInfoDTO
                {
                    UserID = user.UserID,
                    FullName = user.FullName,
                    Email = user.Email,
                    Role = user.Role.ToString(),
                    IsDefaultPassword = user.IsDefaultPassword,
                    CreatedAt = user.CreatedAt
                };

                return new AuthResponseDTO
                {
                    Success = true,
                    Message = "Registration successful",
                    AccessToken = accessToken,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(
                        int.Parse(_configuration["JwtSettings:ExpirationMinutes"] ?? "60")),
                    User = userInfo
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during registration: {ex.Message}");
                return new AuthResponseDTO
                {
                    Success = false,
                    Message = "An error occurred during registration"
                };
            }
        }

        public async Task<AuthResponseDTO> ChangePasswordAsync(int userId, ChangePasswordDTO changePasswordDTO)
        {
            try
            {
                var user = await _userRepository.GetUserDetailsByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning($"Change password attempt for non-existent user: {userId}");
                    return new AuthResponseDTO
                    {
                        Success = false,
                        Message = "User not found"
                    };
                }

                if (!BCrypt.Net.BCrypt.Verify(changePasswordDTO.CurrentPassword, user.Password))
                {
                    _logger.LogWarning($"Incorrect current password for user: {user.Email}");
                    return new AuthResponseDTO
                    {
                        Success = false,
                        Message = "Current password is incorrect"
                    };
                }

                user.Password = BCrypt.Net.BCrypt.HashPassword(changePasswordDTO.NewPassword);
                user.IsDefaultPassword = false;

                await _userRepository.UpdateAsync(user);

                _logger.LogInformation($"Password changed for user: {user.Email}");

                return new AuthResponseDTO
                {
                    Success = true,
                    Message = "Password changed successfully. Please login again."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during password change: {ex.Message}");
                return new AuthResponseDTO
                {
                    Success = false,
                    Message = "An error occurred during password change"
                };
            }
        }


        public Task<bool> ValidateTokenAsync(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_configuration["JwtSettings:SecretKey"] ?? "");

                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["JwtSettings:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = _configuration["JwtSettings:Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero // Removes default 5-min grace period
                }, out SecurityToken validatedToken);

                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Token validation failed: {ex.Message}");
                return Task.FromResult(false);
            }
        }

        public async Task<UserInfoDTO?> GetUserFromTokenAsync(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);

                var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                    return null;

                var userResponse = await _userService.GetUserByIdAsync(userId);
                if (userResponse == null)
                    return null;

                return new UserInfoDTO
                {
                    UserID = userResponse.UserID,
                    FullName = userResponse.FullName,
                    Email = userResponse.Email,
                    Role = userResponse.Role,
                    IsDefaultPassword = userResponse.IsDefaultPassword,
                    CreatedAt = userResponse.CreatedAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error extracting user from token: {ex.Message}");
                return null;
            }
        }
    }
}
