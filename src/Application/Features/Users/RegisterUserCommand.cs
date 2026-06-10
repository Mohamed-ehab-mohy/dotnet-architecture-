using Application.Common.Models;
using MediatR;

namespace Application.Features.Users;

public record RegisterUserCommand(
    string Name,
    string Email,
    string Password,
    string Role
) : IRequest<Result<int>>;
