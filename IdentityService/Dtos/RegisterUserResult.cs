﻿namespace IdentityService.Dtos;

public class RegisterUserResult
{
    public string Email { get; set; }
    public string Username { get; set; }
    public string Token { get; set; }
}