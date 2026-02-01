using System.Security.Cryptography;
using System.Text;

namespace WebApplication1.Services
{
    public interface IEncryptionService
    {
        string Encrypt(string plainText);
        string Decrypt(string cipherText);
    }

    public class EncryptionService : IEncryptionService
    {
        private readonly byte[] _key;
        private readonly byte[] _iv;

  public EncryptionService(IConfiguration configuration)
        {
        // Get encryption key from configuration
   // In production, use Azure Key Vault or similar secure storage
  var encryptionKey = configuration["EncryptionSettings:Key"] 
          ?? throw new InvalidOperationException("Encryption key not configured");
      var encryptionIV = configuration["EncryptionSettings:IV"] 
    ?? throw new InvalidOperationException("Encryption IV not configured");

            _key = Convert.FromBase64String(encryptionKey);
            _iv = Convert.FromBase64String(encryptionIV);

      // Validate key size (256-bit for AES-256)
            if (_key.Length != 32)
          throw new InvalidOperationException("Encryption key must be 256 bits (32 bytes)");
    if (_iv.Length != 16)
         throw new InvalidOperationException("Encryption IV must be 128 bits (16 bytes)");
        }

        public string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
      return string.Empty;

        using var aes = Aes.Create();
            aes.Key = _key;
      aes.IV = _iv;
       aes.Mode = CipherMode.CBC;
    aes.Padding = PaddingMode.PKCS7;

       using var encryptor = aes.CreateEncryptor();
  byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
   byte[] encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

            return Convert.ToBase64String(encryptedBytes);
  }

   public string Decrypt(string cipherText)
        {
 if (string.IsNullOrEmpty(cipherText))
   return string.Empty;

  using var aes = Aes.Create();
            aes.Key = _key;
  aes.IV = _iv;
   aes.Mode = CipherMode.CBC;
    aes.Padding = PaddingMode.PKCS7;

     using var decryptor = aes.CreateDecryptor();
       byte[] cipherBytes = Convert.FromBase64String(cipherText);
            byte[] decryptedBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);

            return Encoding.UTF8.GetString(decryptedBytes);
        }

    // Static helper to generate new keys (for initial setup)
        public static (string Key, string IV) GenerateNewKeys()
        {
            using var aes = Aes.Create();
         aes.KeySize = 256;
     aes.GenerateKey();
            aes.GenerateIV();

    return (Convert.ToBase64String(aes.Key), Convert.ToBase64String(aes.IV));
        }
    }
}
