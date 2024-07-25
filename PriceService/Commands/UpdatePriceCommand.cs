using PriceService.Interfaces;

namespace PriceService.Commands;

public record UpdatePriceCommand(int ProductId, decimal PriceAmount) : ICommand<bool>;