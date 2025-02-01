using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Domain.Entities;
using Microsoft.IdentityModel.Tokens;
using MoodTracker_back.Application.Dtos;
using MoodTracker_back.Application.Interfaces;

namespace MoodTracker_back.Infrastructure.Adapters;

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;
    private readonly byte[] _key = new byte[32];
    private readonly byte[] _iv = new byte[16];

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
        var keyString = Environment.GetEnvironmentVariable("TOKEN_ENCRYPTION_KEY");
        var ivString = Environment.GetEnvironmentVariable("TOKEN_ENCRYPTION_KEY_IV");
        
        _key = Convert.FromBase64String(keyString);
        _iv = Convert.FromBase64String(ivString);
    }

    public string GenerateJwtToken(User user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT_KEY")!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: Environment.GetEnvironmentVariable("JWT_ISSUER"),
            audience: Environment.GetEnvironmentVariable("JWT_AUDIENCE"),
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(120),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    public string GeneratePasswordResetToken(int userId, string email)
    {
        using var aes = Aes.Create();
    
        aes.GenerateIV();
        byte[] iv = aes.IV;

        var tokenData = new
        {
            UserId = userId,
            Email = email,
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(4).ToUnixTimeSeconds(), 
            Random = GenerateRandomString(16) 
        };

        var json = JsonSerializer.Serialize(tokenData);
        byte[] jsonBytes = Encoding.UTF8.GetBytes(json);

        aes.Key = _key;
        aes.IV = iv; 

        byte[] encryptedData;
        using (var encryptor = aes.CreateEncryptor())
        using (var msEncrypt = new MemoryStream())
        {
            using (var cryptoStream = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
            {
                cryptoStream.Write(jsonBytes, 0, jsonBytes.Length);
                cryptoStream.FlushFinalBlock();
            }
            encryptedData = msEncrypt.ToArray();
        }

        byte[] combinedData = new byte[iv.Length + encryptedData.Length];
        Buffer.BlockCopy(iv, 0, combinedData, 0, iv.Length); 
        Buffer.BlockCopy(encryptedData, 0, combinedData, iv.Length, encryptedData.Length); 

        return ToBase64UrlString(combinedData);
    }
    
    public (bool isValid, int userId, string email) ValidatePasswordResetToken(string token)
    {
        try
        {
            var combinedBytes = FromBase64UrlString(token);
            
            if (combinedBytes.Length < 16)
            {
                return (false, 0, string.Empty);
            }
            
            byte[] iv = new byte[16];
            byte[] encryptedBytes = new byte[combinedBytes.Length - iv.Length];
            Buffer.BlockCopy(combinedBytes, 0, iv, 0, iv.Length);
            Buffer.BlockCopy(combinedBytes, iv.Length, encryptedBytes, 0, encryptedBytes.Length);

            using var aes = Aes.Create();
            aes.Key = _key;
            aes.IV = iv;

            using var msDecrypt = new MemoryStream(encryptedBytes);
            using var cryptoStream = new CryptoStream(msDecrypt, aes.CreateDecryptor(), CryptoStreamMode.Read);
            using var reader = new StreamReader(cryptoStream);
        
            var json = reader.ReadToEnd();
            var tokenData = JsonSerializer.Deserialize<TokenData>(json);

            // Validações explícitas
            if (tokenData == null || 
                tokenData.UserId <= 0 || 
                string.IsNullOrEmpty(tokenData.Email) || 
                DateTimeOffset.UtcNow.ToUnixTimeSeconds() > tokenData.ExpiresAt)
            {
                return (false, 0, string.Empty);
            }

            return (true, tokenData.UserId, tokenData.Email);
        }
        catch (Exception ex)
        {
            // Logar o erro para diagnóstico
            Console.WriteLine($"Erro ao validar token: {ex.Message}");
            return (false, 0, string.Empty);
        }
    }

    private static string GenerateRandomString(int length)
    {
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[length];
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }

    private static string ToBase64UrlString(byte[] bytes)
    {
        return Convert.ToBase64String(bytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }
    
    private static byte[] FromBase64UrlString(string base64Url)
    {
        string base64 = base64Url.Replace('-', '+').Replace('_', '/');
    
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }
    
        return Convert.FromBase64String(base64);
    }
}