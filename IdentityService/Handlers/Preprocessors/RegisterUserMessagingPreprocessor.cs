﻿using IdentityService.Dtos;
using IdentityService.Models.Messaging;
using IdentityService.Requests;
using IdentityService.Services;
using MediatR.Pipeline;

namespace IdentityService.Handlers.Preprocessors;

public class RegisterUserMessagingPreprocessor : IRequestPostProcessor<RegisterUserRequest, UserDataResult>
{
    private readonly IRabbitMqService _rabbitMqService;

    public RegisterUserMessagingPreprocessor(IRabbitMqService rabbitMqService)
    {
        _rabbitMqService = rabbitMqService;
    }

    public async Task Process(RegisterUserRequest request, UserDataResult response, CancellationToken cancellationToken)
    {
        _rabbitMqService.SendMessageAsync(new RegisterUserMessage(request.RegisterDto.Email, request.RegisterDto.Username));
    }
}