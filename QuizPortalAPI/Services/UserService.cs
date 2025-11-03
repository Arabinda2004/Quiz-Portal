using Microsoft.EntityFrameworkCore;
using QuizPortalAPI.Data;
using QuizPortalAPI.DTOs.User;
using QuizPortalAPI.DTOs.Auth;
using QuizPortalAPI.Models;

using System.Security.Claims;

namespace QuizPortalAPI.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<UserService> _logger;

        public UserService(AppDbContext context, ILogger<UserService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<UserResponseDTO>> GetAllUsersAsync()
        {
            try
            {
                var users = await _context.Users.ToListAsync();
                return users.Select(MapToResponseDTO).ToList(); // Select() is a LINQ projection method
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving all users: {ex.Message}");
                throw;
            }
        }

        public async Task<UserResponseDTO?> GetUserByIdAsync(int userId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                return user != null ? MapToResponseDTO(user) : null;
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"Error retrieving user {userId}: {ex.Message}");
                throw;
            }
        }


        public async Task<UserResponseDTO?> GetUserByEmailAsync(string email)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
                return user != null ? MapToResponseDTO(user) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving user by email: {ex.Message}");
                throw;
            }
        }


        public async Task<UserResponseDTO> CreateUserAsync(AdminCreateUserDTO createUserDTO)
        {
            try
            {
                // Check if email already exists
                if (await UserExistsByEmailAsync(createUserDTO.Email))
                    throw new InvalidOperationException("Email already exists");

                // Parse role // recheck needed
                if (!Enum.TryParse<UserRole>(createUserDTO.Role, true, out var role))
                    throw new InvalidOperationException($"Invalid role: {createUserDTO.Role}");

                var user = new User
                {
                    FullName = createUserDTO.FullName,
                    Email = createUserDTO.Email,
                    Password = BCrypt.Net.BCrypt.HashPassword(createUserDTO.Password),
                    Role = role,
                    IsDefaultPassword = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"User created successfully: {user.Email}");
                return MapToResponseDTO(user);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating user: {ex.Message}");
                throw;
            }
        }

        public async Task<UserResponseDTO?> UpdateUserAsync(int userId, UpdateUserDTO updateUserDTO)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                    return null;

                // Check if new email is unique (if provided)
                if (!string.IsNullOrEmpty(updateUserDTO.Email) && updateUserDTO.Email != user.Email)
                {
                    if (await UserExistsByEmailAsync(updateUserDTO.Email)) // checking if any user has the new email
                        throw new InvalidOperationException("Email already exists");
                    user.Email = updateUserDTO.Email;
                }

                if (!string.IsNullOrEmpty(updateUserDTO.FullName))
                    user.FullName = updateUserDTO.FullName;

                // // Handle role update (only for admins)
                // if (!string.IsNullOrEmpty(updateUserDTO.Role))
                // {
                //     if (!Enum.TryParse<UserRole>(updateUserDTO.Role, true, out var role))
                //         throw new InvalidOperationException($"Invalid role: {updateUserDTO.Role}");
                //     user.Role = role;
                // }
                // my change 

                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"User updated successfully: {user.Email}");
                return MapToResponseDTO(user);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating user {userId}: {ex.Message}");
                throw;
            }
        }

        public async Task<UserResponseDTO?> AdminUpdateUserAsync(int userId, AdminUpdateUserDTO updateUserDTO)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                    return null;

                // Check if new email is unique (if provided)
                if (!string.IsNullOrEmpty(updateUserDTO.Email) && updateUserDTO.Email != user.Email)
                {
                    if (await UserExistsByEmailAsync(updateUserDTO.Email)) // checking if any user has the new email
                        throw new InvalidOperationException("Email already exists");
                    user.Email = updateUserDTO.Email;
                }

                if (!string.IsNullOrEmpty(updateUserDTO.FullName))
                    user.FullName = updateUserDTO.FullName;

                // Handle role update (only for admins)
                if (!string.IsNullOrEmpty(updateUserDTO.Role))
                {
                    if (!Enum.TryParse<UserRole>(updateUserDTO.Role, true, out var role))
                        throw new InvalidOperationException($"Invalid role: {updateUserDTO.Role}");
                    user.Role = role;
                }
                

                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"User updated successfully: {user.Email}");
                return MapToResponseDTO(user);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating user {userId}: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> DeleteUserAsync(int userId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                    return false;

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"User deleted successfully: {userId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting user {userId}: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> UserExistsByEmailAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }

        public async Task<bool> CanUpdateRoleAsync(int adminUserId, int targetUserId)
        {
            try
            {
                // Cannot update own role through this path
                if (adminUserId == targetUserId)
                    return false;

                // Get the admin user
                var adminUser = await _context.Users.FindAsync(adminUserId);
                if (adminUser == null)
                {
                    _logger.LogWarning($"Admin user {adminUserId} not found");
                    return false;
                }

                // Only admins can update roles
                if (adminUser.Role != UserRole.Admin)
                {
                    _logger.LogWarning($"Non-admin user {adminUserId} attempted to update role of user {targetUserId}");
                    return false;
                }

                // Check if target user exists
                var targetUser = await _context.Users.FindAsync(targetUserId);
                if (targetUser == null)
                {
                    _logger.LogWarning($"Target user {targetUserId} not found");
                    return false;
                }

                _logger.LogInformation($"Admin {adminUserId} is authorized to update role of user {targetUserId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error checking role update authorization: {ex.Message}");
                return false;
            }
        }

        public async Task<UserRole?> GetUserRoleAsync(int userId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                return user?.Role;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving user role: {ex.Message}");
                return null;
            }
        }

        public async Task MarkPasswordAsDefaultAsync(int userId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning($"User {userId} not found for marking default password");
                    return;
                }

                user.IsDefaultPassword = true;
                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Marked password as default for user {userId}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error marking password as default for user {userId}: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> ChangeUserPasswordAsync(int userId, ChangePasswordDTO changePasswordDto)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                    return false;

                // Verify current password
                if (!BCrypt.Net.BCrypt.Verify(changePasswordDto.CurrentPassword, user.Password))
                    return false;

                // Hash and update to new password
                user.Password = BCrypt.Net.BCrypt.HashPassword(changePasswordDto.NewPassword);
                user.IsDefaultPassword = false;

                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Password changed successfully for user: {user.Email}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error changing password for user {userId}: {ex.Message}");
                throw;
            }
        }

        private static UserResponseDTO MapToResponseDTO(User user)
        {
            return new UserResponseDTO
            {
                UserID = user.UserID,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role.ToString(),
                IsDefaultPassword = user.IsDefaultPassword,
                CreatedAt = user.CreatedAt
            };
        }

        public async Task<bool> VerifyPasswordAsync(int userId, string password)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                    return false;

                return BCrypt.Net.BCrypt.Verify(password, user.Password);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error verifying password: {ex.Message}");
                return false;
            }
        }
    }
}
