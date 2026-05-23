using Microsoft.Extensions.Configuration;
using Onyx.Oms.Core.Common.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace Onyx.Oms.Infrastructure.Security
{
    internal class AesCryptoService : ICryptoService
    {
        private readonly byte[] _key;

        public AesCryptoService(IConfiguration configuration)
        {
            var keyString = configuration["Security:MasterEncryptionKey"];
            if(string.IsNullOrWhiteSpace(keyString) || keyString.Length < 32)
            {
                throw new InvalidOperationException("A 32-character (256-bit) Master Encryption Key must be configured.");
            }

            // Ensure to extract exactly 32 bytes for AES-256
            _key = Encoding.UTF8.GetBytes(keyString.Substring(0,32));
        }

        public string Decrypt(string cipherText)
        {
            if(string.IsNullOrEmpty(cipherText))
                return string.Empty;

            var fullCipher = Convert.FromBase64String(cipherText);

            using var aes = Aes.Create();
            aes.Key = _key;

            var iv = new byte[aes.BlockSize / 8]; // 16 bytes for AES
            var cipher = new byte[fullCipher.Length - iv.Length];

            // Extract the IV and actual ciphertext from the raw byte array
            Buffer.BlockCopy(fullCipher, 0, iv, 0, iv.Length);
            Buffer.BlockCopy(fullCipher, iv.Length, cipher, 0, cipher.Length);

            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream(cipher);
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var sr = new StreamReader(cs);

            return sr.ReadToEnd();
        }

        public string Encrypt(string planeText)
        {
            if(string.IsNullOrWhiteSpace(planeText))
                return string.Empty;

            using var aes = Aes.Create();
            aes.Key = _key;
            aes.GenerateIV(); // Unique random IV per encryption event

            using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream();

            // Prepend the raw IV to the stream so Decrypt knows what IV was used
            ms.Write(aes.IV, 0, aes.IV.Length);

            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            using (var sw = new StreamWriter(cs))
            {
                sw.Write(planeText);
            }

            return Convert.ToBase64String(ms.ToArray());
        }
    }
}
