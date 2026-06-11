using Microsoft.EntityFrameworkCore;
using Sentinal.Application.Common.Interfaces;
using Sentinal.Domain.Users;
using Sentinal.Infrastructure.Common.Persistence;

namespace Sentinal.Infrastructure.Users.Persistence;

public class UserRepository : IUserRepository
{
    private readonly SentinalDbContext _context;

    public UserRepository(SentinalDbContext context)
    {
        _context = context;
    }

    public async Task<UserEntity> CreateUserAsync(string username, string email, string passwordHash)
    {
        var newUser = new UserEntity()
        {
            Email = email,
            PasswordHash = passwordHash,
            Username = username,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        var newEntity = await _context.Users.AddAsync(newUser);
        if(await _context.SaveChangesAsync() > 0)
            return newEntity.Entity;
        throw new InvalidOperationException("Failed to create user");
    }

    #region GetGroup

    public async Task<UserEntity?> GetUserByUsernameAsync(string username)
    {
        return await _context.Users.FirstOrDefaultAsync(x => x.Username == username && !x.MarkedForDeletion);
    }

    public async Task<UserEntity?> GetUserByEmailAsync(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(x => x.Email == email);
    }

    public async Task<UserEntity?> GetUserByIdAsync(Guid id)
    {
        return await _context.Users.FirstOrDefaultAsync(x => x.Id == id);
    }

    #endregion

    #region Existence Checks

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _context.Users.AnyAsync(x => x.Email == email && !x.MarkedForDeletion);
    }

    public async Task<bool> UsernameExistsAsync(string username)
    {
        return await _context.Users.AnyAsync(x => x.Username == username && !x.MarkedForDeletion);
    }

    #endregion

    #region UpdateGroup

    public async Task<UserEntity> UpdateUserDataAsync(UserEntity user)
    {
        user.UpdatedAt = DateTime.UtcNow;
        _context.Users.Update(user);
        if (await _context.SaveChangesAsync() > 0)
            return user;
        throw new InvalidOperationException("Failed to update user");
    }

    public async Task<bool> MarkUserAsDeletedAsync(Guid id)
    {
        var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == id);
        if (user == null)
            throw new InvalidOperationException("User not found");
        user.MarkedForDeletion = true;
        user.UpdatedAt = DateTime.UtcNow;
        user.DeletedAt = DateTime.UtcNow;
        _context.Users.Update(user);
        return await _context.SaveChangesAsync() > 0;
    }

    #endregion



}