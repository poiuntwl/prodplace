using System.Security.Cryptography;
using FluentAssertions;
using Microsoft.IdentityModel.Tokens;
using Prodplace.KeyRotator;

namespace ProdPlace.KeyRotator.IntegrationTests;

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
        _store.CurrentSigningKey.Should().NotBeNull();
        _store.CurrentSigningKey!.Key.Should().BeOfType<RsaSecurityKey>();
        _store.ValidationKeys.Should().ContainSingle();
        _store.ValidationKeys[0].Should().Be(_store.CurrentSigningKey);
    }

    [Fact]
    public void RotateKey_ShouldCreateNewKeyAndKeepOldOne()
    {
        var initialKey = _store.CurrentSigningKey;
        var initialKeyCreateDate = initialKey!.Created;


        var newKey = _store.RotateKey();


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
        var key = _store.GenerateNewKey();


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
        _store.RotateKey();


        _store.Dispose();


        var act = () => _store.ValidationKeys;
        act.Should().Throw<ObjectDisposedException>();

        var act2 = () => _store.CurrentSigningKey;
        act2.Should().Throw<ObjectDisposedException>();
    }

    [Fact]
    public void LoadExistingKeys_ShouldLoadAndDecryptKeys()
    {
        var initialStore = new FileKeyStore(_testKeyFolder);
        initialStore.RotateKey();
        var expectedKeyCount = initialStore.ValidationKeys.Count;


        var newStore = new FileKeyStore(_testKeyFolder);


        newStore.ValidationKeys.Should().HaveCount(expectedKeyCount);
        newStore.CurrentSigningKey.Should().NotBeNull();
        newStore.CurrentSigningKey!.Key.Should().BeOfType<RsaSecurityKey>();
    }

    [Fact]
    public void RemoveKey_ShouldRemoveExpiredKey()
    {
        var initialKey = _store.CurrentSigningKey;
        var newKey = _store.RotateKey();


        var result = _store.RemoveKey(initialKey!);


        result.Should().BeTrue();
        _store.ValidationKeys.Should().HaveCount(1);
        _store.ValidationKeys.Should().NotContain(initialKey);
        _store.CurrentSigningKey.Should().Be(newKey);
    }

    [Fact]
    public void RemoveKey_ShouldNotRemoveCurrentSigningKey()
    {
        var currentKey = _store.CurrentSigningKey;


        var result = _store.RemoveKey(currentKey!);


        result.Should().BeFalse();
        _store.ValidationKeys.Should().ContainSingle();
        _store.CurrentSigningKey.Should().Be(currentKey);
    }

    [Fact]
    public void RemoveKey_ShouldHandleNonExistentKey()
    {
        var rsa = RSA.Create();
        var nonExistentKey = new DatedSecurityKey
        {
            Key = new RsaSecurityKey(rsa)
            {
                KeyId = Guid.NewGuid().ToString()
            },
            Created = DateTimeOffset.UtcNow,
            Expires = DateTimeOffset.UtcNow.AddDays(90)
        };


        var result = _store.RemoveKey(nonExistentKey);


        result.Should().BeFalse();
        _store.ValidationKeys.Should().ContainSingle();
        rsa.Dispose();
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