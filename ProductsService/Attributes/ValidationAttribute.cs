namespace ProductsService.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class ValidationAttribute(Type requestType) : Attribute
{
    public Type RequestType { get; } = requestType;
};