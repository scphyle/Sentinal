using Sentinal.Application.Users.Commands;

namespace Sentinal.Application.Common.Interfaces;

public interface IUserRepository
{
    public Task<Guid> CreateUser(string username, string email, string passwordHash);
}