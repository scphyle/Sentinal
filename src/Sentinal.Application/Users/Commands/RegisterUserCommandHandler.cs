using MediatR;
using Sentinal.Application.Common.Interfaces;
using Sentinal.Infrastructure.Common.Security;

namespace Sentinal.Application.Users.Commands;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Guid>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public RegisterUserCommandHandler(IUserRepository userRepository, IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<Guid> Handle(RegisterUserCommand command, CancellationToken cancellationToken)
    {
        var hashedPassword = _passwordHasher.HashPassword(command.Password);

        return await _userRepository.CreateUser(command.Username, command.Email, hashedPassword);
    }
}