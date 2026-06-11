using Sentinal.Domain.Users;

namespace Sentinal.Application.Common.Interfaces;

public interface IUserRepository
{
    Task<UserEntity> CreateUserAsync(string username, string email, string passwordHash);
    Task<UserEntity?> GetUserByUsernameAsync(string username);
    Task<UserEntity?> GetUserByEmailAsync(string email);
    Task<UserEntity?> GetUserByIdAsync(Guid id);
    Task<UserEntity> UpdateUserDataAsync(UserEntity user);
    Task<bool> MarkUserAsDeletedAsync(Guid id);
    Task<bool> EmailExistsAsync(string email);
    Task<bool> UsernameExistsAsync(string username);
}