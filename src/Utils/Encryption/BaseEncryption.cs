using System.Security.Cryptography;
using System.Text;

using AskMeNowBot.Configuration;
using AskMeNowBot.Exceptions;

namespace AskMeNowBot.Utils.Encryption;

public class BaseEncryption : IEncryption
{
    private readonly byte[] _key;

    public BaseEncryption(IConfig config)
    {
        _key = Convert.FromBase64String(config.Security.EncryptionKey);

        if (_key.Length == 32)
        {
            return;
        }

        throw new InvalidEncryptionKeyException();
    }

    public string Encrypt(string value)
    {
        using var aes = Aes.Create();
        aes.Key = _key;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor();
        var bytes = Encoding.UTF8.GetBytes(value);
        var encryptedBytes = encryptor.TransformFinalBlock(bytes, 0, bytes.Length);

        return Convert.ToBase64String(aes.IV.Concat(encryptedBytes).ToArray());
    }

    public string Decrypt(string value)
    {
        var bytes = Convert.FromBase64String(value);
        var iv = bytes.Take(16).ToArray();
        var text = bytes.Skip(16).ToArray();

        using var aes = Aes.Create();
        aes.Key = _key;
        aes.IV = iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var decryptor = aes.CreateDecryptor();
        var decryptedBytes = decryptor.TransformFinalBlock(text, 0, text.Length);

        return Encoding.UTF8.GetString(decryptedBytes);
    }
}
