using Application.Common.Models;
using Domain.Entities;
using Domain.Interfaces;
using MediatR;

namespace Application.Features.Users;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Result<int>>
{
    private readonly IRepository<User> _repository;

    public RegisterUserCommandHandler(IRepository<User> repository)
    {
        _repository = repository;
    }

    public async Task<Result<int>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var passwordHash = BCryptHelper.HashPassword(request.Password);
        var user = new User(request.Name, request.Email, passwordHash, request.Role);
        await _repository.AddAsync(user);
        return Result<int>.Success(user.Id);
    }
}

internal static class BCryptHelper
{
    public static string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }
}
