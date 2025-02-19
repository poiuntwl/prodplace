using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using NSubstitute;
using Prodplace.KeyRotator;

namespace ProdPlace.KeyRotator.IntegrationTests;

public class KeyRotationServiceTests : IDisposable
{
    private readonly IKeyStore _keyStore;
    private readonly ILogger<KeyRotationService> _logger;
    private readonly KeyRotationService _service;
    private readonly CancellationTokenSource _cts;
    private readonly RsaSecurityKey _testKey1;
    private readonly RsaSecurityKey _testKey2;

    public KeyRotationServiceTests()
    {
        _testKey1 = new RsaSecurityKey(RSA.Create());
        _testKey2 = new RsaSecurityKey(RSA.Create());

        _keyStore = Substitute.For<IKeyStore>();
        _logger = Substitute.For<ILogger<KeyRotationService>>();

        var configuration = Substitute.For<IConfiguration>();
        var configSection = Substitute.For<IConfigurationSection>();
        configSection.Value.Returns("01:00:00");
        configuration.GetSection("KeyRotation:RotationInterval").Returns(configSection);

        var checkSection = Substitute.For<IConfigurationSection>();
        checkSection.Value.Returns("00:05:00");
        configuration.GetSection("KeyRotation:CheckInterval").Returns(checkSection);

        var cleanupSection = Substitute.For<IConfigurationSection>();
        cleanupSection.Value.Returns("1.00:00:00");
        configuration.GetSection("KeyRotation:CleanupInterval").Returns(cleanupSection);

        _service = new KeyRotationService(_keyStore, _logger, configuration);
        _cts = new CancellationTokenSource();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldRotateKeyWhenIntervalExceeded()
    {
        var oldKey = new DatedSecurityKey
        {
            Key = _testKey1,
            Created = DateTimeOffset.UtcNow.AddHours(-2),
            Expires = DateTimeOffset.UtcNow.AddDays(90)
        };

        _keyStore.CurrentSigningKey.Returns(oldKey);

        var newKey = new DatedSecurityKey
        {
            Key = _testKey2,
            Created = DateTimeOffset.UtcNow,
            Expires = DateTimeOffset.UtcNow.AddDays(90)
        };
        _keyStore.RotateKey().Returns(newKey);

        var executeTask = _service.StartAsync(_cts.Token);
        await Task.Delay(100);
        _cts.Cancel();
        await executeTask;


        _keyStore.Received(1).RotateKey();


        _logger.Received(1).Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString() == "Rotating JWT signing key"),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task ExecuteAsync_ShouldNotRotateKeyWhenIntervalNotExceeded()
    {
        var newKey = new DatedSecurityKey
        {
            Key = _testKey1,
            Created = DateTimeOffset.UtcNow.AddMinutes(-30),
            Expires = DateTimeOffset.UtcNow.AddDays(90)
        };

        _keyStore.CurrentSigningKey.Returns(newKey);

        var executeTask = _service.StartAsync(_cts.Token);
        await Task.Delay(100);
        await _cts.CancelAsync();
        await executeTask;

        _keyStore.DidNotReceive().RotateKey();
    }

    public void Dispose()
    {
        _cts.Dispose();
        (_testKey1.Rsa as IDisposable)?.Dispose();
        (_testKey2.Rsa as IDisposable)?.Dispose();
        GC.SuppressFinalize(this);
    }
}