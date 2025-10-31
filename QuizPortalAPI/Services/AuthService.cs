using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using QuizPortalAPI.Data;
using QuizPortalAPI.DTOs.Auth;
using QuizPortalAPI.Models;

namespace QuizPortalAPI.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<AuthService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IUserService _userService;

        public AuthService(
            AppDbContext context,
            ILogger<AuthService> logger,
            IConfiguration configuration,
            IUserService userService)
        {
            _context = context;
            _logger = logger;
            _configuration = configuration;
            _userService = userService;
        }

        public async Task<AuthResponseDTO> LoginAsync(LoginDTO loginDTO)
        {
            try
            {
                // Find user by email
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == loginDTO.Email);
                if (user == null)
                {
                    _logger.LogWarning($"Login attempt with non-existent email: {loginDTO.Email}");
                    return new AuthResponseDTO
                    {
                        Success = false,
                        Message = "Invalid email or password"
                    };
                }

                // Verify password
                if (!BCrypt.Net.BCrypt.Verify(loginDTO.Password, user.Password))
                {
                    _logger.LogWarning($"Failed login attempt for user: {user.Email}");
                    return new AuthResponseDTO
                    {
                        Success = false,
                        Message = "Invalid email or password"
                    };
                }

                // Revoke old refresh tokens
                await RevokeAllRefreshTokensAsync(user.UserID);

                // Generate tokens
                var accessToken = GenerateAccessToken(user);
                var refreshToken = await GenerateAndStoreRefreshTokenAsync(user.UserID);

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
                    RefreshToken = refreshToken,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(
                        int.Parse(_configuration["JwtSettings:ExpirationMinutes"] ?? "15")),
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
                // Check if email already exists
                if (await _userService.UserExistsByEmailAsync(registerDTO.Email))
                {
                    _logger.LogWarning($"Registration attempt with existing email: {registerDTO.Email}");
                    return new AuthResponseDTO
                    {
                        Success = false,
                        Message = "Email already registered"
                    };
                }

                // Parse role from DTO (Teacher or Student)
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

                // Create new user with selected role
                var user = new User
                {
                    FullName = registerDTO.FullName,
                    Email = registerDTO.Email,
                    Password = BCrypt.Net.BCrypt.HashPassword(registerDTO.Password),
                    Role = userRole,  // Use role from registration request
                    IsDefaultPassword = false,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Users.Add(user); 
                await _context.SaveChangesAsync();

                _logger.LogInformation($"New user registered as {userRole}: {user.Email}");

                // Generate tokens for auto-login after registration
                var accessToken = GenerateAccessToken(user);
                var refreshToken = await GenerateAndStoreRefreshTokenAsync(user.UserID);

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
                    RefreshToken = refreshToken,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(
                        int.Parse(_configuration["JwtSettings:ExpirationMinutes"] ?? "15")),
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

        public async Task<AuthResponseDTO> RefreshTokenAsync(string refreshTokenString)
        {
            try
            {
                // Find refresh token in database
                var refreshToken = await _context.RefreshTokens
                    .Include(rt => rt.User)
                    .FirstOrDefaultAsync(rt => rt.Token == refreshTokenString);

                if (refreshToken == null || !refreshToken.IsValid)
                {
                    _logger.LogWarning("Invalid refresh token attempt");
                    return new AuthResponseDTO
                    {
                        Success = false,
                        Message = "Invalid or expired refresh token"
                    };
                }

                var user = refreshToken.User;
                if (user == null)
                {
                    _logger.LogWarning("User not found for refresh token");
                    return new AuthResponseDTO
                    {
                        Success = false,
                        Message = "User not found"
                    };
                }

                // Generate new access token
                var accessToken = GenerateAccessToken(user);

                // Optionally generate new refresh token (rotate refresh tokens)
                refreshToken.RevokedAt = DateTime.UtcNow;
                _context.RefreshTokens.Update(refreshToken);

                var newRefreshToken = await GenerateAndStoreRefreshTokenAsync(user.UserID);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Token refreshed for user: {user.Email}");

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
                    Message = "Token refreshed successfully",
                    AccessToken = accessToken,
                    RefreshToken = newRefreshToken,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(
                        int.Parse(_configuration["JwtSettings:ExpirationMinutes"] ?? "15")),
                    User = userInfo
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during token refresh: {ex.Message}");
                return new AuthResponseDTO
                {
                    Success = false,
                    Message = "An error occurred during token refresh"
                };
            }
        }

        public async Task<AuthResponseDTO> ChangePasswordAsync(int userId, ChangePasswordDTO changePasswordDTO)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning($"Change password attempt for non-existent user: {userId}");
                    return new AuthResponseDTO
                    {
                        Success = false,
                        Message = "User not found"
                    };
                }

                // Verify current password
                if (!BCrypt.Net.BCrypt.Verify(changePasswordDTO.CurrentPassword, user.Password))
                {
                    _logger.LogWarning($"Incorrect current password for user: {user.Email}");
                    return new AuthResponseDTO
                    {
                        Success = false,
                        Message = "Current password is incorrect"
                    };
                }

                // Update password
                user.Password = BCrypt.Net.BCrypt.HashPassword(changePasswordDTO.NewPassword);
                user.IsDefaultPassword = false;

                _context.Users.Update(user);

                // Revoke all refresh tokens (user must login again)
                await RevokeAllRefreshTokensAsync(userId);

                await _context.SaveChangesAsync();

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

        public async Task<bool> LogoutAsync(int userId)
        {
            try
            {
                await RevokeAllRefreshTokensAsync(userId);
                _logger.LogInformation($"User logged out: {userId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during logout: {ex.Message}");
                return false;
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
                    ClockSkew = TimeSpan.Zero
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
       
        // Access token and refresh token generations
        private string GenerateAccessToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["JwtSettings:SecretKey"] ?? "");

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim("IsDefaultPassword", user.IsDefaultPassword.ToString())
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(
                    int.Parse(_configuration["JwtSettings:ExpirationMinutes"] ?? "60")),
                Issuer = _configuration["JwtSettings:Issuer"],
                Audience = _configuration["JwtSettings:Audience"],
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private async Task<string> GenerateAndStoreRefreshTokenAsync(int userId)
        {
            var refreshToken = new RefreshToken
            {
                UserId = userId,
                Token = GenerateRandomToken(),
                ExpiresAt = DateTime.UtcNow.AddDays(
                    int.Parse(_configuration["JwtSettings:RefreshTokenExpirationDays"] ?? "7")),
                CreatedAt = DateTime.UtcNow
            };

            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();

            return refreshToken.Token;
        }

        private async Task RevokeAllRefreshTokensAsync(int userId)
        {
            var tokens = await _context.RefreshTokens
                .Where(rt => rt.UserId == userId && rt.RevokedAt == null)
                .ToListAsync();

            foreach (var token in tokens)
            {
                token.RevokedAt = DateTime.UtcNow;
            }

            if (tokens.Count > 0)
            {
                _context.RefreshTokens.UpdateRange(tokens);
                await _context.SaveChangesAsync();
            }
        }

      
        private string GenerateRandomToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }
    }
}
