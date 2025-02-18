using System.Security.Cryptography;
using System.Text;
using MoodTracker_back.Application.Interfaces;

namespace MoodTracker_back.Infrastructure.Adapters.Security;

public class CryptographService : ICryptographService
{
    private readonly byte[] _key = new byte[32];
    private readonly byte[] _iv = new byte[16];
    private readonly ILoggingService _logger;

    public CryptographService(ILoggingService logger)
    {
        _logger = logger;
        try
        {
            var keyString = Environment.GetEnvironmentVariable("DATA_STORAGE_ENCRYPTION_KEY");
            var ivString = Environment.GetEnvironmentVariable("DATA_STORAGE_ENCRYPTION_KEY_IV");

            if (string.IsNullOrWhiteSpace(keyString))
            {
                throw new ArgumentNullException(nameof(keyString));
            }

            if (string.IsNullOrWhiteSpace(ivString))
            {
                throw new ArgumentNullException(nameof(ivString));
            }

            _key = Convert.FromBase64String(keyString);
            _iv = Convert.FromBase64String(ivString);
        }
        catch (Exception ex)
        {
            _logger.LogErrorAsync(ex, "Erro ao inicializar chaves de criptografia.");
            throw;
        }
    }

    
    public string Encrypt(string plaintext)
    {
        if (plaintext == null)
            throw new ArgumentNullException(nameof(plaintext));

        try
        {
            using var aes = Aes.Create();
            aes.Key = _key;
            aes.IV = _iv;

            var data = Encoding.UTF8.GetBytes(plaintext);
            using var encryptor = aes.CreateEncryptor();
            byte[] encryptedBytes = encryptor.TransformFinalBlock(data, 0, data.Length);

            return Convert.ToBase64String(encryptedBytes);
        }
        catch (Exception ex)
        {
            _logger.LogErrorAsync(ex, "Erro ao criptografar texto.");
            throw new ArgumentException("Falha ao criptografar os dados.", ex);
        }
    }

    public string Decrypt(string ciphertext)
    {
        if (string.IsNullOrWhiteSpace(ciphertext) || 
            ciphertext.Length % 4 != 0 || 
            ciphertext.Any(c => !Base64Chars.Contains(c)))
        {
            throw new ArgumentException("Texto criptografado inválido.");
        }
        try
        {
            if (!IsBase64String(ciphertext))
                throw new FormatException("O texto criptografado não é um Base64 válido.");

            using var aes = Aes.Create();
            aes.Key = _key;
            aes.IV = _iv;

            var encryptedBytes = Convert.FromBase64String(ciphertext);
            using var decryptor = aes.CreateDecryptor();
            byte[] decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);

            return Encoding.UTF8.GetString(decryptedBytes);
        }
        catch (Exception ex)
        {
            _logger.LogErrorAsync(ex, "Erro ao descriptografar texto.");
            throw new ArgumentException("Falha ao descriptografar os dados.", ex);
        }
    }
  
    private static readonly HashSet<char> Base64Chars = new HashSet<char>(
        "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/="
    );

    private static bool IsBase64String(string base64)
    {
        Span<byte> buffer = new Span<byte>(new byte[base64.Length]);
        return Convert.TryFromBase64String(base64, buffer, out _);
    }
}