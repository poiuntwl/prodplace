using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using NSubstitute;
using System.Security.Cryptography;

namespace Prodplace.KeyRotator.Tests;

public class FileKeyStoreTests : IDisposable
{
    private readonly string _testKeyFolder = Path.Combine(Path.GetTempPath(), "test-jwt-keys");
    private readonly FileKeyStore _store;

    public FileKeyStoreTests()
    {
        Directory.CreateDirectory(_testKeyFolder);
        _store = new FileKeyStore(_testKeyFolder);
    }

    [Fact]
    public void Constructor_ShouldCreateInitialKey()
    {
        // Assert
        _store.CurrentSigningKey.Should().NotBeNull();
        _store.CurrentSigningKey!.Key.Should().BeOfType<RsaSecurityKey>();
        _store.ValidationKeys.Should().ContainSingle();
        _store.ValidationKeys[0].Should().Be(_store.CurrentSigningKey);
    }

    [Fact]
    public void RotateKey_ShouldCreateNewKeyAndKeepOldOne()
    {
        // Arrange
        var initialKey = _store.CurrentSigningKey;
        var initialKeyCreateDate = initialKey!.Created;

        // Act
        var newKey = _store.RotateKey();

        // Assert
        newKey.Should().NotBeNull();
        newKey.Created.Should().BeAfter(initialKeyCreateDate);
        _store.CurrentSigningKey.Should().Be(newKey);
        _store.ValidationKeys.Should().HaveCount(2);
        _store.ValidationKeys.Should()
            .Contain(k => k.Created == initialKeyCreateDate, "previous key should remain valid");
    }

    [Fact]
    public void GenerateNewKey_ShouldCreateValidRsaKey()
    {
        // Act
        var key = _store.GenerateNewKey();

        // Assert
        key.Should().NotBeNull();
        key.Key.Should().BeOfType<RsaSecurityKey>();
        var rsaKey = (RsaSecurityKey)key.Key;
        rsaKey.Rsa.Should().NotBeNull();
        rsaKey.KeySize.Should().Be(2048);
        key.Created.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
        key.Expires.Should().BeCloseTo(DateTimeOffset.UtcNow.AddDays(90), TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Dispose_ShouldPreventFurtherAccess()
    {
        // Arrange
        _store.RotateKey(); // Create additional key

        // Act
        _store.Dispose();

        // Assert
        var act = () => _store.ValidationKeys;
        act.Should().Throw<ObjectDisposedException>();

        var act2 = () => _store.CurrentSigningKey;
        act.Should().Throw<ObjectDisposedException>();
    }

    [Fact]
    public void LoadExistingKeys_ShouldLoadAndDecryptKeys()
    {
        // Arrange
        var initialStore = new FileKeyStore(_testKeyFolder);
        initialStore.RotateKey(); // Create a second key
        var expectedKeyCount = initialStore.ValidationKeys.Count;

        // Act
        var newStore = new FileKeyStore(_testKeyFolder); // This will load existing keys

        // Assert
        newStore.ValidationKeys.Should().HaveCount(expectedKeyCount);
        newStore.CurrentSigningKey.Should().NotBeNull();
        newStore.CurrentSigningKey!.Key.Should().BeOfType<RsaSecurityKey>();
    }

    public void Dispose()
    {
        _store.Dispose();
        if (Directory.Exists(_testKeyFolder))
        {
            Directory.Delete(_testKeyFolder, true);
        }

        GC.SuppressFinalize(this);
    }
}

public class KeyRotationServiceTests : IDisposable
{
    private readonly IKeyStore _keyStore;
    private readonly ILogger<KeyRotationService> _logger;
    private readonly KeyRotationService _service;
    private readonly CancellationTokenSource _cts;

    public KeyRotationServiceTests()
    {
        _keyStore = Substitute.For<IKeyStore>();
        _logger = Substitute.For<ILogger<KeyRotationService>>();
        _service = new KeyRotationService(_keyStore, _logger);
        _cts = new CancellationTokenSource();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldRotateKeyWhenIntervalExceeded()
    {
        // Arrange
        var oldKey = new DatedSecurityKey
        {
            Key = new RsaSecurityKey(RSA.Create()),
            Created = DateTimeOffset.UtcNow.AddHours(-2), // Older than rotation interval
            Expires = DateTimeOffset.UtcNow.AddDays(90)
        };

        _keyStore.CurrentSigningKey.Returns(oldKey);

        var newKey = new DatedSecurityKey
        {
            Key = new RsaSecurityKey(RSA.Create()),
            Created = DateTimeOffset.UtcNow,
            Expires = DateTimeOffset.UtcNow.AddDays(90)
        };
        _keyStore.RotateKey().Returns(newKey);

        // Act
        var executeTask = _service.StartAsync(_cts.Token);
        await Task.Delay(TimeSpan.FromSeconds(1)); // Short delay to allow background task to run.
        _cts.Cancel();
        await executeTask; // Ensure the task finishes.

        // Assert
        _keyStore.Received(1).RotateKey();
        _logger.Received(1).Log(Arg.Any<LogLevel>(), Arg.Any<EventId>(), Arg.Any<Arg.AnyType>(),
            Arg.Any<Exception?>(), Arg.Any<Func<Arg.AnyType, Exception?, string>>());
    }

    [Fact]
    public async Task ExecuteAsync_ShouldNotRotateKeyWhenIntervalNotExceeded()
    {
        // Arrange
        var newKey = new DatedSecurityKey
        {
            Key = new RsaSecurityKey(RSA.Create()),
            Created = DateTimeOffset.UtcNow.AddMinutes(-30), // Newer than rotation interval
            Expires = DateTimeOffset.UtcNow.AddDays(90)
        };

        _keyStore.CurrentSigningKey.Returns(newKey);

        // Act
        var executeTask = _service.StartAsync(_cts.Token);
        await Task.Delay(TimeSpan.FromSeconds(1)); // Short delay to allow background task to run.
        await _cts.CancelAsync();
        await executeTask; // Ensure the task finishes and exceptions are caught.

        // Assert
        _keyStore.DidNotReceive().RotateKey();
    }

    public void Dispose()
    {
        _cts.Dispose();
        GC.SuppressFinalize(this);
    }
}