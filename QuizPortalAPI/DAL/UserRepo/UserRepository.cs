using Microsoft.EntityFrameworkCore;
using QuizPortalAPI.Data;
using QuizPortalAPI.Models;


namespace QuizPortalAPI.DAL.UserRepo;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<User>> GetAllUsersAsync()
    {
        return await _context.Users.ToListAsync();
    }

    public async Task<User?> GetUserDetailsByIdAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        return user;
    }

    public async Task<User?> FindUserByEmailAsync(string email)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        return user;
    }

    public async Task<bool> IsUserExistsWithEmailAsync(string email)
    {
        return await _context.Users.AnyAsync(e => e.Email == email);
    }

    public async Task<UserRole?> GetUserRoleAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        return user?.Role;
    }

    public async Task CreateAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> DeleteAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return false;

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<User?> FindUserByIdAsync(int userId)
    {
        return await _context.Users.FindAsync(userId);
    }
}