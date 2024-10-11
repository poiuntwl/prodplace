using ProductsService.Models.MongoDbModels;

namespace ProductsService.Interfaces;

public interface IBulkProductsUploader
{
    Task<(long Created, long Updated)> UploadAsync(ICollection<ProductModel> products, CancellationToken ct);
}