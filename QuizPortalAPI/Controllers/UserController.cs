using Microsoft.AspNetCore.Mvc;
using QuizPortalAPI.DTOs.User;
using QuizPortalAPI.DTOs.Auth;
using QuizPortalAPI.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace QuizPortalAPI.Controllers
{
    [ApiController]
    [Route("api/users")] 
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserService userService, ILogger<UserController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        private int GetLoggedInUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out var userId))
                return userId;
            return 0;
        }


        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")] 
        public async Task<ActionResult<UserResponseDTO>> GetUserById(int id)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                {
                    _logger.LogWarning($"Admin attempted to access non-existent user");
                    return NotFound(new { message = "User not found" });
                }

                _logger.LogInformation($"Admin retrieved user details");
                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting user: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving the user" });
            }
        }

        [HttpGet("by-email/{email}")]
        [Authorize]
        public async Task<ActionResult<UserResponseDTO>> GetUserByEmail(string email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                    return BadRequest(new { message = "Email is required" });

                var user = await _userService.GetUserByEmailAsync(email);
                if (user == null)
                {
                    _logger.LogWarning($"Admin email lookup returned no results");
                    return NotFound(new { message = "User not found" });
                }

                _logger.LogInformation($"Admin retrieved user by email");
                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting user by email: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving the user" });
            }
        }

        //  SECURED: Users can only get their own profile
        [HttpGet("profile")]
        [Authorize]
        public async Task<ActionResult<UserResponseDTO>> GetProfile()
        {
            try
            {
                var userId = GetLoggedInUserId();
                if (userId == 0)
                    return Unauthorized(new { message = "Invalid user ID" });

                var user = await _userService.GetUserByIdAsync(userId);
                if (user == null)
                    return NotFound(new { message = "User not found" });

                _logger.LogInformation($"User retrieved their profile");
                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting profile: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving profile" });
            }
        }

        //  SECURED: Users can only update their own profile
        [HttpPut("profile")]
        [Authorize]
        public async Task<ActionResult<UserResponseDTO>> UpdateProfile([FromBody] UpdateUserDTO updateUserDTO)
        {
            try
            {
                //  Input validation
                if (updateUserDTO == null)
                    return BadRequest(new { message = "Invalid request data" });

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userId = GetLoggedInUserId();
                if (userId == 0)
                    return Unauthorized(new { message = "Invalid user ID" });

                var updatedUser = await _userService.UpdateUserAsync(userId, updateUserDTO);
                if (updatedUser == null)
                    return NotFound(new { message = "User not found" });

                _logger.LogInformation($"User updated their profile");
                return Ok(new { message = "Profile updated successfully", data = updatedUser });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning($"Profile update validation error");
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating profile: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while updating the profile" });
            }
        }

        //  SECURED: Users can only change their own password
        [HttpPut("change-password")]
        [Authorize]
        public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordDTO changePasswordDTO)
        {
            try
            {
                //  Input validation
                if (changePasswordDTO == null)
                    return BadRequest(new { message = "Invalid request data" });

                if (string.IsNullOrWhiteSpace(changePasswordDTO.CurrentPassword) ||
                    string.IsNullOrWhiteSpace(changePasswordDTO.NewPassword))
                    return BadRequest(new { message = "Current and new passwords are required" });

                if (changePasswordDTO.CurrentPassword == changePasswordDTO.NewPassword)
                    return BadRequest(new { message = "New password must be different from current password" });

                var userId = GetLoggedInUserId();
                if (userId == 0)
                    return Unauthorized(new { message = "Invalid user ID" });

                var result = await _userService.ChangeUserPasswordAsync(userId, changePasswordDTO);
                if (!result)
                    return BadRequest(new { message = "Current password is incorrect" });

                _logger.LogInformation($"User changed their password");
                return Ok(new { message = "Password changed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error changing password: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while changing password" });
            }
        }

        //  SECURED: Users can only delete their own account with password confirmation
        [HttpDelete("profile")]
        [Authorize]
        public async Task<IActionResult> DeleteAccount([FromBody] DeleteAccountDTO deleteAccountDTO)
        {
            try
            {
                //  Input validation
                if (deleteAccountDTO == null || string.IsNullOrWhiteSpace(deleteAccountDTO.Password))
                    return BadRequest(new { message = "Password is required to delete account" });

                var userId = GetLoggedInUserId();
                if (userId == 0)
                    return Unauthorized(new { message = "Invalid user ID" });

                //  Verify password before deletion
                var user = await _userService.GetUserByIdAsync(userId);
                if (user == null)
                    return NotFound(new { message = "User not found" });

                // Check password using service method
                var passwordVerified = await _userService.VerifyPasswordAsync(userId, deleteAccountDTO.Password);
                if (!passwordVerified)
                    return Unauthorized(new { message = "Invalid password - account not deleted" });

                var result = await _userService.DeleteUserAsync(userId);
                if (!result)
                    return NotFound(new { message = "User not found" });

                _logger.LogInformation($"User deleted their account");
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting account: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while deleting the account" });
            }
        }
    }
}