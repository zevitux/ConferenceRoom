using ConferenceRoomApi.Models;

namespace ConferenceRoomApi.Repositories;

public interface IUserRepository
{
    Task<User?> GetUserByEmailAsync(string email);
    Task<User> GetUserById(int id);
    Task<List<User>> GetAllUsersAsync();
    Task<User> AddAsync(User user);
    Task<User> UpdateAsync(User user);
}