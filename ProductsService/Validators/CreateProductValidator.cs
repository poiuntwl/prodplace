using System.Text.Json;
using FluentValidation;
using ProductsService.Handlers;

namespace ProductsService.Validators;

public class CreateProductValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.Product.Name)
            .NotEmpty().WithMessage("Name for product is required.")
            .Length(2, 100).WithMessage("Name for product must be between 2 and 100 characters.");

        RuleFor(x => x.Product.Description)
            .MaximumLength(5000).WithMessage("Name for product must be less than or equal to 5000 characters.");

        RuleFor(x => x.Product.CustomFields).Must(BeValidJson);
    }

    private static bool BeValidJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return true;
        }

        try
        {
            JsonDocument.Parse(json);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}