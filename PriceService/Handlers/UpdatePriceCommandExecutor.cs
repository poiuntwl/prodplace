using MediatR;
using PriceService.Commands;
using PriceService.Interfaces;
using PriceService.Services;

namespace PriceService.Handlers;

public class UpdatePriceCommandExecutor : IRequestHandler<UpdatePriceCommand, bool>
{
    private readonly IPriceManager _priceManager;

    public UpdatePriceCommandExecutor(IPriceManager priceManager)
    {
        _priceManager = priceManager;
    }

    public async Task<bool> Handle(UpdatePriceCommand request, CancellationToken cancellationToken)
    {
        return await _priceManager.SetPriceAsync(request.ProductId, request.PriceAmount, cancellationToken);
    }
}