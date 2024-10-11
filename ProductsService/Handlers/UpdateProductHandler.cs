using MediatR;
using ProductsService.Dtos.Product;

namespace ProductsService.Handlers;

// ReSharper disable once UnusedType.Global
public class UpdateProductHandler : IRequestHandler<UpdateProductRequest>
{
    public Task Handle(UpdateProductRequest request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}

public record UpdateProductRequest(int Id, UpdateProductRequestDto Dto) : IRequest;