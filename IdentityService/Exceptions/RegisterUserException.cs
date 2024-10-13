namespace IdentityService.Exceptions;

public class RegisterUserException : Exception
{
    public ICollection<string> Errors { get; set; }

    public RegisterUserException(ICollection<string> errors)
    {
        Errors = errors;
    }
}