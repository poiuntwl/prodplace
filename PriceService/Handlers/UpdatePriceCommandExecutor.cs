using MediatR;
using PriceService.Commands;
using PriceService.Interfaces;

namespace PriceService.Handlers;

public class UpdatePriceCommandExecutor : IRequestHandler<UpdatePriceCommand, bool>
{
    private readonly IPricesRepository _pricesRepository;

    public UpdatePriceCommandExecutor(IPricesRepository pricesRepository)
    {
        _pricesRepository = pricesRepository;
    }

    public async Task<bool> Handle(UpdatePriceCommand request, CancellationToken cancellationToken)
    {
        return await _pricesRepository.UpdatePriceOldAsync(request.ProductId, request.PriceAmount, cancellationToken);
    }
}