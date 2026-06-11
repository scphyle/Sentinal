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

    public async Task<Guid> CreateUser(string username, string email, string passwordHash)
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

        await _context.SaveChangesAsync();
        return newEntity.Entity.Id;

    }
}