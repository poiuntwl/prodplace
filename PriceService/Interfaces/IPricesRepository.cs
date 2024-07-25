namespace PriceService.Interfaces;

public interface IPricesRepository
{
    Task<bool> UpdatePriceAsync(int productId, decimal priceAmount, CancellationToken ct,
        bool createIfNotExists = false);

    Task<bool> UpdatePriceOldAsync(int productId, decimal priceAmount, CancellationToken ct,
        bool createIfNotExists = false);
}