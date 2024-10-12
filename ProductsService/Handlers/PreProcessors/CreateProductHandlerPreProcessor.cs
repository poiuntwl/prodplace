using FluentValidation;
using MediatR.Pipeline;

namespace ProductsService.Handlers.PreProcessors;

public class CreateProductHandlerPreProcessor : IRequestPreProcessor<CreateProductRequest>
{
    private readonly IValidator<CreateProductRequest> _validator;

    public CreateProductHandlerPreProcessor(IValidator<CreateProductRequest> validator)
    {
        _validator = validator;
    }

    public async Task Process(CreateProductRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (validationResult.IsValid == false)
        {
            throw new ValidationException(validationResult.Errors);
        }
    }
}