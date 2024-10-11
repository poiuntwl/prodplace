using System.Text.Json;
using FluentValidation;
using ProductsService.Models.MongoDbModels;

namespace ProductsService.Validators;

public class CreateProductValidator : AbstractValidator<ProductModel>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name for product is required.")
            .Length(2, 100).WithMessage("Name for product must be between 2 and 100 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(5000).WithMessage("Name for product must be less than or equal to 5000 characters.");

        RuleFor(x => x.CustomFields)
            .Must(BeValidJson);
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