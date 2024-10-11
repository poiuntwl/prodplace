using MediatR;
using ProductsService.Interfaces;
using ProductsService.Models.MongoDbModels;

namespace ProductsService.Handlers;

// ReSharper disable once UnusedType.Global
public class
    CreateOrUpdateProductsHandler : IRequestHandler<CreateOrUpdateProductsRequest, (long Created, long Updated)>
{
    private readonly IBulkProductsUploader _bulkProductsUploader;

    public CreateOrUpdateProductsHandler(IBulkProductsUploader bulkProductsUploader)
    {
        _bulkProductsUploader = bulkProductsUploader;
    }

    public async Task<(long Created, long Updated)> Handle(CreateOrUpdateProductsRequest request,
        CancellationToken cancellationToken)
    {
        return await _bulkProductsUploader.UploadAsync(request.Products, cancellationToken);
    }
}

public record CreateOrUpdateProductsRequest(ICollection<ProductModel> Products)
    : IRequest<(long Created, long Updated)>;