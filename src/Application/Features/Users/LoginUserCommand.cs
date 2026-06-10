using Application.Common.Models;
using MediatR;

namespace Application.Features.Users;

public record LoginUserCommand(
    string Email,
    string Password
) : IRequest<Result<string>>;
