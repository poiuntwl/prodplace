namespace CurrencyRatesService.Exceptions;

public class CurrencyRateNotAvailableException(string code) : Exception
{
    public string Code { get; } = code;
}