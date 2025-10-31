using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizPortalAPI.DTOs.User;
using QuizPortalAPI.Services;
using System.Security.Claims;

namespace QuizPortalAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/admin")]
    public class AdminController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<AdminController> _logger;

        public AdminController(IUserService userService, ILogger<AdminController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        /// <summary>
        /// GET: api/admin/users
        /// Get all users (Admin only)
        /// </summary>
        [HttpGet("users")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<UserResponseDTO>>> GetAllUsers()
        {
            try
            {
                var users = await _userService.GetAllUsersAsync();
                _logger.LogInformation("Admin retrieved all users successfully");
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting all users: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving users" });
            }
        }

        /// <summary>
        /// GET: api/admin/users/{id}
        /// Get a specific user by ID (Admin only)
        /// </summary>
        [HttpGet("users/{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UserResponseDTO>> GetUserById(int id)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                {
                    _logger.LogWarning($"Admin requested user with ID {id} not found");
                    return NotFound(new { message = "User not found" });
                }

                _logger.LogInformation($"Admin retrieved user {id} successfully");
                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting user {id}: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving the user" });
            }
        }

        /// <summary>
        /// POST: api/admin/users
        /// Create a new user (Admin only)
        /// </summary>
        [HttpPost("users")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UserResponseDTO>> CreateUser([FromBody] AdminCreateUserDTO adminCreateUserDTO)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid admin user creation request");
                    return BadRequest(ModelState);
                }

                // Convert AdminCreateUserDTO to CreateUserDTO
                var createUserDTO = new AdminCreateUserDTO
                {
                    FullName = adminCreateUserDTO.FullName,
                    Email = adminCreateUserDTO.Email,
                    Role = adminCreateUserDTO.Role,
                    // Use provided password or generate default
                    Password = adminCreateUserDTO.Password ?? GenerateDefaultPassword()
                };

                var createdUser = await _userService.CreateUserAsync(createUserDTO);

                // If we generated a default password, mark it as such
                if (string.IsNullOrEmpty(adminCreateUserDTO.Password))
                {
                    await _userService.MarkPasswordAsDefaultAsync(createdUser.UserID);
                }

                _logger.LogInformation($"Admin created user successfully: {createdUser.Email}");

                return CreatedAtAction(nameof(GetUserById),
                    new { id = createdUser.UserID },
                    createdUser);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning($"Conflict creating user: {ex.Message}");
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating user: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while creating the user" });
            }
        }

        /// <summary>
        /// PUT: api/admin/users/{id}
        /// Update a user (Admin only - can update role and other details)
        /// </summary>
        [HttpPut("users/{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UserResponseDTO>> UpdateUser(int id, [FromBody] AdminUpdateUserDTO adminUpdateUserDTO)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning($"Invalid admin user update request for user {id}");
                    return BadRequest(ModelState);
                }

                // Convert AdminUpdateUserDTO to UpdateUserDTO
                var updateUserDTO = new AdminUpdateUserDTO
                {
                    FullName = adminUpdateUserDTO.FullName,
                    Email = adminUpdateUserDTO.Email
                };

                // Handle role update separately with admin authorization
                if (!string.IsNullOrEmpty(adminUpdateUserDTO.Role))
                {
                    // Get current admin user ID from claims
                    var adminUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (string.IsNullOrEmpty(adminUserIdClaim) || !int.TryParse(adminUserIdClaim, out int adminUserId))
                    {
                        _logger.LogWarning("Invalid admin user ID in token");
                        return Unauthorized(new { message = "Invalid admin user ID" });
                    }

                    // Check if admin can update role
                    bool canUpdateRole = await _userService.CanUpdateRoleAsync(adminUserId, id);
                    if (!canUpdateRole)
                    {
                        _logger.LogWarning($"Admin {adminUserId} attempted to update role of user {id} without authorization");
                        return Forbid();
                    }

                    // Add role to update DTO
                    updateUserDTO.Role = adminUpdateUserDTO.Role;
                }

                var updatedUser = await _userService.AdminUpdateUserAsync(id, updateUserDTO);

                if (updatedUser == null)
                {
                    _logger.LogWarning($"User with ID {id} not found for admin update");
                    return NotFound(new { message = "User not found" });
                }

                _logger.LogInformation($"Admin updated user {id} successfully");
                return Ok(updatedUser);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning($"Conflict updating user {id}: {ex.Message}");
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating user {id}: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while updating the user" });
            }
        }

        /// <summary>
        /// DELETE: api/admin/users/{id}
        /// Delete a user (Admin only)
        /// </summary>
        [HttpDelete("users/{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                var result = await _userService.DeleteUserAsync(id);

                if (!result)
                {
                    _logger.LogWarning($"User with ID {id} not found for admin deletion");
                    return NotFound(new { message = "User not found" });
                }

                _logger.LogInformation($"Admin deleted user {id} successfully");
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting user {id}: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while deleting the user" });
            }
        }

        /// <summary>
        /// Generates a default password for new users
        /// </summary>
        private string GenerateDefaultPassword()
        {
            // Generate a secure default password
            // In production, you might want to use a more sophisticated approach
            return $"TempPass{Guid.NewGuid().ToString().Substring(0, 8)}!";
        }
    }
}