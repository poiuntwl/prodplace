using IdentityService.Dtos;
using MediatR;

namespace IdentityService.Requests;

public record RegisterUserRequest(RegisterDto RegisterDto) : IRequest<NewUserResult>;