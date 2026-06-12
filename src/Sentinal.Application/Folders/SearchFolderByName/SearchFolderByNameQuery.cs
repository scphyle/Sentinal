using FluentResults;
using MediatR;
using Sentinal.Application.Folders.DTOs;

namespace Sentinal.Application.Folders.SearchFolderByName;

public record SearchFolderByNameQuery(string Name, Guid UserId) : IRequest<Result<List<FolderDto>>>;