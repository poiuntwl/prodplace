#pragma warning disable CA1416
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;

namespace Prodplace.KeyRotator;

public interface IKeyStore
{
    DatedSecurityKey? CurrentSigningKey { get; }
    IReadOnlyList<DatedSecurityKey?> ValidationKeys { get; }
    void RotateKey();
}

public class FileKeyStore : IKeyStore, IDisposable
{
    private const string KeyFolder = "jwt-keys";
    private static readonly TimeSpan KeyLifetime = TimeSpan.FromDays(90);
    private readonly List<DatedSecurityKey?> _keys = new();

    public DatedSecurityKey? CurrentSigningKey { get; private set; }
    public IReadOnlyList<DatedSecurityKey?> ValidationKeys => _keys.AsReadOnly();

    public FileKeyStore()
    {
        Directory.CreateDirectory(KeyFolder);
        LoadExistingKeys();
        CurrentSigningKey ??= GenerateNewKey();
    }

    private void LoadExistingKeys()
    {
        foreach (var keyFile in Directory.GetFiles(KeyFolder, "*.key"))
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

            var key = new DatedSecurityKey
            {
                Key = new RsaSecurityKey(rsa.ExportParameters(true)),
                Created = new DateTimeOffset(createdTicks, TimeSpan.Zero),
                Expires = DateTimeOffset.UtcNow.Add(KeyLifetime)
            };

            _keys.Add(key);
        }

        CurrentSigningKey = _keys.MaxBy(k => File.GetCreationTimeUtc($"{k?.Key.KeyId}.key"));
    }

    public DatedSecurityKey GenerateNewKey()
    {
        var rsa = RSA.Create(2048);
        var key = new DatedSecurityKey()
        {
            Key = new RsaSecurityKey(rsa.ExportParameters(true)),
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
            Path.Combine(KeyFolder, $"{key.Key.KeyId}.key"),
            encrypted
        );

        _keys.Add(key);
        return key;
    }

    public void RotateKey() => CurrentSigningKey = GenerateNewKey();

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        foreach (var key in _keys.Where(x => x?.Key != null)
                     .Select(x => x!.Key)
                     .OfType<RsaSecurityKey>())
        {
            key.Rsa?.Dispose();
        }
    }
}