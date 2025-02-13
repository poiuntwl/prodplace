namespace Prodplace.KeyRotator;

public class KeyRotationService : BackgroundService
{
    private readonly IKeyStore _keyStore;
    private readonly ILogger<KeyRotationService> _logger;
    private readonly TimeSpan _rotationInterval = TimeSpan.FromHours(1);
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(5);

    public KeyRotationService(IKeyStore keyStore, ILogger<KeyRotationService> logger)
    {
        _keyStore = keyStore;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var currentKeyAge = DateTime.UtcNow - _keyStore.CurrentSigningKey?.Created;

            if (currentKeyAge >= _rotationInterval)
            {
                _logger.LogInformation("Rotating JWT signing key");
                _keyStore.RotateKey();
                await OnKeyRotated();
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }
    }

    private Task OnKeyRotated()
    {
        // Optional: Notify other services/systems
        return Task.CompletedTask;
    }
}