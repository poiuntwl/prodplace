using PriceService.Interfaces;

namespace PriceService.Services;

public interface IPriceManager
{
    Task<bool> SetPriceAsync(int productId, decimal price, CancellationToken ct);
}

public class PriceManager : IPriceManager
{
    private readonly IPricesRepository _pricesRepository;

    public PriceManager(IPricesRepository pricesRepository)
    {
        _pricesRepository = pricesRepository;
    }

    public async Task<bool> SetPriceAsync(int productId, decimal price, CancellationToken ct)
    {
        return await _pricesRepository.UpdatePriceAsync(productId, price, ct);
    }
}