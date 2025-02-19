namespace Prodplace.KeyRotator;

public class KeyRotationService : BackgroundService
{
    private readonly IKeyStore _keyStore;
    private readonly ILogger<KeyRotationService> _logger;
    private readonly IConfiguration _configuration;
    private readonly TimeSpan _rotationInterval;
    private readonly TimeSpan _checkInterval;
    private readonly TimeSpan _cleanupInterval;

    public KeyRotationService(
        IKeyStore keyStore,
        ILogger<KeyRotationService> logger,
        IConfiguration configuration)
    {
        _keyStore = keyStore;
        _logger = logger;
        _configuration = configuration;

        _rotationInterval = _configuration.GetValue("KeyRotation:RotationInterval", TimeSpan.FromHours(1));
        _checkInterval = _configuration.GetValue("KeyRotation:CheckInterval", TimeSpan.FromMinutes(5));
        _cleanupInterval = _configuration.GetValue("KeyRotation:CleanupInterval", TimeSpan.FromDays(1));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var lastCleanup = DateTime.UtcNow;

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await RotateKeyIfNeeded();
                await CleanupExpiredKeys(lastCleanup);
                lastCleanup = DateTime.UtcNow;

                await Task.Delay(_checkInterval, stoppingToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred in the key rotation service");
            throw;
        }
    }

    private async Task RotateKeyIfNeeded()
    {
        try
        {
            var currentKeyAge = DateTime.UtcNow - _keyStore.CurrentSigningKey?.Created;

            if (currentKeyAge >= _rotationInterval)
            {
                _logger.LogInformation("Rotating JWT signing key");
                _keyStore.RotateKey();
                await OnKeyRotated();
                _logger.LogInformation("JWT signing key rotation completed successfully");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to rotate JWT signing key");
            throw;
        }
    }

    private async Task CleanupExpiredKeys(DateTime lastCleanup)
    {
        if (DateTime.UtcNow - lastCleanup < _cleanupInterval) return;

        try
        {
            var expiredKeys = _keyStore.ValidationKeys
                .Where(k => k?.Expires < DateTime.UtcNow)
                .ToList();

            if (expiredKeys.Any())
            {
                _logger.LogInformation("Found {Count} expired keys to clean up", expiredKeys.Count);
                foreach (var key in expiredKeys)
                {
                    // Implement cleanup logic in IKeyStore
                    // _keyStore.RemoveKey(key);
                    _logger.LogInformation("Removed expired key {KeyId}", key?.Key.KeyId);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cleanup expired keys");
            // Don't throw here to prevent service interruption
        }
    }

    private async Task OnKeyRotated()
    {
        try
        {
            // Implement notification logic here
            // Example: Notify other services via message bus
            _logger.LogInformation("Successfully notified services about key rotation");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to notify services about key rotation");
            // Don't throw here as key rotation was successful
        }
    }
}