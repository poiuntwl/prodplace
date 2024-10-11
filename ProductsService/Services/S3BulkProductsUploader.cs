using ProductsService.Interfaces;
using ProductsService.Models.MongoDbModels;

namespace ProductsService.Services;

public class S3BulkProductsUploader : IBulkProductsUploader
{
    public async Task<(long Created, long Updated)> UploadAsync(ICollection<ProductModel> products,
        CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}