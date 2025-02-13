#pragma warning disable CA1416
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;

namespace Prodplace.KeyRotator;

public interface IKeyStore
{
    DatedSecurityKey? CurrentSigningKey { get; }
    IReadOnlyList<DatedSecurityKey?> ValidationKeys { get; }
    DatedSecurityKey RotateKey();
}

public class FileKeyStore : IKeyStore, IDisposable
{
    private readonly string _keyFolder;
    private static readonly TimeSpan KeyLifetime = TimeSpan.FromDays(90);
    private readonly List<DatedSecurityKey?> _keys = new();

    private bool _disposed;
    private DatedSecurityKey? _currentSigningKey;

    public DatedSecurityKey? CurrentSigningKey
    {
        get
        {
            ThrowIfDisposed();
            return _currentSigningKey;
        }
        private set => _currentSigningKey = value;
    }

    public IReadOnlyList<DatedSecurityKey?> ValidationKeys
    {
        get
        {
            ThrowIfDisposed();
            return _keys.AsReadOnly();
        }
    }

    public FileKeyStore(string keyFolder = "jwt-keys")
    {
        _keyFolder = keyFolder;
        Directory.CreateDirectory(_keyFolder);
        LoadExistingKeys();
        CurrentSigningKey ??= GenerateNewKey();
    }

    private void LoadExistingKeys()
    {
        foreach (var keyFile in Directory.GetFiles(_keyFolder, "*.key"))
        {
            var encrypted = File.ReadAllBytes(keyFile);
            var decrypted = ProtectedData.Unprotect(encrypted, null, DataProtectionScope.CurrentUser);

            using var ms = new MemoryStream(decrypted);
            using var reader = new BinaryReader(ms);

            // Read metadata
            var createdTicks = reader.ReadInt64();
            var keyXml = reader.ReadString();

            var rsa = RSA.Create();
            rsa.FromXmlString(keyXml);

            var dateTimeOffset = new DateTimeOffset(createdTicks, TimeSpan.Zero);
            var key = new DatedSecurityKey
            {
                Key = new RsaSecurityKey(rsa),
                Created = dateTimeOffset,
                Expires = dateTimeOffset.Add(KeyLifetime)
            };

            _keys.Add(key);
        }

        CurrentSigningKey = _keys.MaxBy(k => File.GetCreationTimeUtc($"{k?.Key.KeyId}.key"));
    }

    public DatedSecurityKey GenerateNewKey()
    {
        var rsa = RSA.Create();
        var key = new DatedSecurityKey
        {
            Key = new RsaSecurityKey(rsa)
            {
                KeyId = Guid.NewGuid().ToString() // Unique identifier
            },
            Created = DateTimeOffset.UtcNow,
            Expires = DateTimeOffset.UtcNow.Add(KeyLifetime)
        };

        using var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms);

        // Write metadata and key together
        writer.Write(key.Created.Ticks);
        writer.Write(rsa.ToXmlString(true));

        var encrypted = ProtectedData.Protect(
            ms.ToArray(),
            null,
            DataProtectionScope.CurrentUser
        );

        File.WriteAllBytes(
            Path.Combine(_keyFolder, $"{key.Key.KeyId}.key"),
            encrypted
        );

        _keys.Add(key);
        return key;
    }

    public DatedSecurityKey RotateKey() => CurrentSigningKey = GenerateNewKey();

    private void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(FileKeyStore));
        }
    }

    public void Dispose()
    {
        if (_disposed) return;

        foreach (var key in _keys.Where(x => x?.Key != null))
        {
            if (key!.Key is RsaSecurityKey rsaKey)
            {
                rsaKey.Rsa?.Dispose();
            }
        }

        _disposed = true;
        GC.SuppressFinalize(this);
    }
}

public class DatedSecurityKey
{
    public required SecurityKey Key { get; init; }
    public required DateTimeOffset Created { get; init; }
    public DateTimeOffset Expires { get; init; }
}