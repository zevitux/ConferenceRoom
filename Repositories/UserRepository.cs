using ConferenceRoomApi.Data;
using ConferenceRoomApi.Models;
using Microsoft.EntityFrameworkCore;

namespace ConferenceRoomApi.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<UserRepository> _logger;

    public UserRepository(AppDbContext context, ILogger<UserRepository> logger)
    {
        _context = context;
        _logger = logger;
    }
    
    public async Task<User?> GetUserByEmailAsync(string email)
    {
        try
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }
        catch (Exception)
        {
            _logger.LogError("Error searching user!");
            throw;
        }
    }

    public Task<User> GetUserById(int id)
    {
        try
        {
            return _context.Users.FirstOrDefaultAsync(u => u.Id == id)!;
        }
        catch (Exception)
        {
            _logger.LogError("Error searching user with ID: {id}", id);
            throw;
        }
        
    }

    public Task<List<User>> GetAllUsersAsync()
    {
        try
        {
            return _context.Users.ToListAsync();
        }
        catch (Exception)
        {
            _logger.LogError("Error searching users!");
            throw;
        }
    }

    public async Task<User> AddAsync(User user)
    {
        try
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }
        catch (Exception)
        {
            _logger.LogError("Error adding user!");
            throw;
        }
    }

    public async Task<User> UpdateAsync(User user)
    {
        try
        {
            var existingUser = await _context.Users.FindAsync(user.Id);
            if (existingUser == null)
                throw new KeyNotFoundException("User not found!");
            
            _context.Entry(existingUser).CurrentValues.SetValues(user);
            await _context.SaveChangesAsync();
            _context.Entry(existingUser).State = EntityState.Detached;
            return existingUser;
        }
        catch
        {
            _logger.LogError("Error updating user with ID: {Id}", user.Id);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var user = await _context.Users
                .Include(u => u.Bookings)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null) return false;

            if (user.Bookings?.Any() == true)
                _context.Bookings.RemoveRange(user.Bookings);

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return true;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            _logger.LogError("Error deleting user with ID: {Id}", id);
            throw;
        }
    }
}