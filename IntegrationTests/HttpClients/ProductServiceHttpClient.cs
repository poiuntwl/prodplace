extern alias ProductsServiceSUT;
using ProductsServiceSUT::ProductsService.Dtos.Product;

namespace IntegrationTests.HttpClients;

public interface IProductServiceHttpClient : IDisposable
{
    Task<ICollection<ProductDto>?> GetAll(string? jwt = null);
    Task<ICollection<ProductDto>?> GetById(string id);
    Task<ProductDto?> Create(CreateProductRequestDto dto, string? jwt = null);
    Task<ICollection<ProductDto>?> UpdatePrice(UpdateProductPriceRequestDto dto);
    Task<ICollection<ProductDto>?> UploadBulk(ICollection<ProductDto> dto);
}

public class ProductServiceHttpClient : IProductServiceHttpClient
{
    private readonly HttpClient _httpClient;

    public ProductServiceHttpClient(HttpClient client)
    {
        _httpClient = client;
    }

    public void Dispose()
    {
        _httpClient.Dispose();
        GC.SuppressFinalize(this);
    }

    public async Task<ICollection<ProductDto>?> GetAll(string? jwt = null)
    {
        return await _httpClient.SendRequestAsync<ICollection<ProductDto>>("api", jwt: jwt);
    }

    public async Task<ICollection<ProductDto>?> GetById(string id)
    {
        return await _httpClient.SendRequestAsync<ICollection<ProductDto>>($"api/{id}");
    }

    public async Task<ProductDto?> Create(CreateProductRequestDto dto, string? jwt = null)
    {
        return await _httpClient.SendRequestAsync<ProductDto>("api", HttpMethod.Post, dto, jwt);
    }

    public async Task<ICollection<ProductDto>?> UpdatePrice(UpdateProductPriceRequestDto dto)
    {
        return await _httpClient.SendRequestAsync<ICollection<ProductDto>>("api", HttpMethod.Put, dto);
    }

    public async Task<ICollection<ProductDto>?> UploadBulk(ICollection<ProductDto> dto)
    {
        return await _httpClient.SendRequestAsync<ICollection<ProductDto>>("api/upload");
    }
}