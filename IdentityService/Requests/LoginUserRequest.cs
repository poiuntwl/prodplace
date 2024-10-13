using IdentityService.Dtos;
using MediatR;

namespace IdentityService.Requests;

public record LoginUserRequest(LoginDto LoginDto) : IRequest<UserDataResult>;