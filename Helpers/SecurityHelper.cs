using System.Security.Cryptography;
using System.Text;

namespace HutechStore.Helpers;

/// <summary>
/// AES-256-CBC Encryption/Decryption helper for securing
/// map tracking coordinates and shipper chat messages.
/// </summary>
public static class SecurityHelper
{
    private const int KeySize = 256;    // AES-256
    private const int IvSize  = 128;    // Block size
    private const int Iterations = 10000;

    private static readonly byte[] Salt = Encoding.UTF8.GetBytes("HutechStoreSalt2025");

    /// <summary>
    /// Derives a deterministic encryption key from an order number + session identifier.
    /// This ensures each order has a unique key tied to the user's session.
    /// </summary>
    public static string GetOrderSecretKey(string orderNumber, string sessionId)
    {
        var raw = $"HutechStore:{orderNumber}:{sessionId}:MapTrackingSecure";
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
        return Convert.ToBase64String(hash);
    }

    /// <summary>
    /// Derive key bytes from password using PBKDF2 (static API, no warnings).
    /// </summary>
    private static byte[] DeriveKey(string password)
    {
        return Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            Salt,
            Iterations,
            HashAlgorithmName.SHA256,
            KeySize / 8
        );
    }

    /// <summary>
    /// Encrypts plaintext with AES-256-CBC using a password-derived key.
    /// Returns Base64(IV + CipherText).
    /// </summary>
    public static string Encrypt(string plainText, string password)
    {
        if (string.IsNullOrEmpty(plainText)) return string.Empty;

        var plainBytes = Encoding.UTF8.GetBytes(plainText);

        using var aes = Aes.Create();
        aes.KeySize = KeySize;
        aes.Mode    = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        aes.Key     = DeriveKey(password);
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor();
        var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

        // Prepend IV to ciphertext for decryption
        var result = new byte[aes.IV.Length + cipherBytes.Length];
        Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
        Buffer.BlockCopy(cipherBytes, 0, result, aes.IV.Length, cipherBytes.Length);

        return Convert.ToBase64String(result);
    }

    /// <summary>
    /// Decrypts Base64(IV + CipherText) with AES-256-CBC.
    /// </summary>
    public static string Decrypt(string cipherText, string password)
    {
        if (string.IsNullOrEmpty(cipherText)) return string.Empty;

        var fullCipher = Convert.FromBase64String(cipherText);

        using var aes = Aes.Create();
        aes.KeySize = KeySize;
        aes.Mode    = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        aes.Key     = DeriveKey(password);

        // Extract IV (first 16 bytes)
        var ivBytes = new byte[IvSize / 8];
        Buffer.BlockCopy(fullCipher, 0, ivBytes, 0, ivBytes.Length);
        aes.IV = ivBytes;

        // Extract cipher bytes
        var cipherBytes = new byte[fullCipher.Length - ivBytes.Length];
        Buffer.BlockCopy(fullCipher, ivBytes.Length, cipherBytes, 0, cipherBytes.Length);

        using var decryptor = aes.CreateDecryptor();
        var plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
        return Encoding.UTF8.GetString(plainBytes);
    }

    /// <summary>
    /// Returns the PBKDF2-derived key bytes as Hex string, for sending to the client
    /// (via a secure, one-time-use endpoint) so CryptoJS can decrypt.
    /// </summary>
    public static (string KeyHex, string SaltHex) GetKeyMaterialForClient(string password)
    {
        var keyBytes = DeriveKey(password);
        return (Convert.ToHexString(keyBytes), Convert.ToHexString(Salt));
    }
}
