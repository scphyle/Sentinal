namespace Sentinal.Application.Users.DTOs;

public record UserAuthDto(Guid Id, string Username, string Email, string Token);