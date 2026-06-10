using Domain.Entities;
using MediatR;

namespace Application.Features.Users;

public record GetUserByIdQuery(int Id) : IRequest<User?>;
