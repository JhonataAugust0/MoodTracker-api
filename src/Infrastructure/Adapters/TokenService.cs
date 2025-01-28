using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Domain.Entities;
using Microsoft.IdentityModel.Tokens;
using MoodTracker_back.Application.Services;

namespace MoodTracker_back.Infrastructure.Adapters;

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;
    private static readonly byte[] _key = new byte[32];
    private static readonly byte[] _iv = new byte[16];

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
        var keyString = Environment.GetEnvironmentVariable("TOKEN_ENCRYPTION_KEY");
        var ivString = Environment.GetEnvironmentVariable("TOKEN_ENCRYPTION_KEY_IV");
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
        var tokenData = new
        {
            UserId = userId,
            Email = email,
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            Random = GenerateRandomString(16)
        };

        var json = JsonSerializer.Serialize(tokenData);
        var jsonBytes = Encoding.UTF8.GetBytes(json);

        using var aes = Aes.Create();
        aes.Key = _key;
        aes.IV = _iv;

        using var msEncrypt = new MemoryStream();
        using var cryptoStream = new CryptoStream(msEncrypt, aes.CreateEncryptor(), CryptoStreamMode.Write);
        
        cryptoStream.Write(jsonBytes, 0, jsonBytes.Length);
        cryptoStream.FlushFinalBlock();

        return ToBase64UrlString(msEncrypt.ToArray());
    }
    
    public (bool isValid, int userId, string email) ValidatePasswordResetToken(string token)
    {
        try
        {
            var encryptedBytes = FromBase64UrlString(token);

            using var aes = Aes.Create();
            aes.Key = _key;
            aes.IV = _iv;

            using var msDecrypt = new MemoryStream(encryptedBytes);
            using var cryptoStream = new CryptoStream(msDecrypt, aes.CreateDecryptor(), CryptoStreamMode.Read);
            using var reader = new StreamReader(cryptoStream);
            
            var json = reader.ReadToEnd();
            var tokenData = JsonSerializer.Deserialize<dynamic>(json);

            var timestamp = Convert.ToInt64(tokenData?.GetProperty("Timestamp").ToString());
            var tokenTime = DateTimeOffset.FromUnixTimeSeconds(timestamp);
            
            if (DateTimeOffset.UtcNow.Subtract(tokenTime).TotalHours > 24)
                return (false, 0, string.Empty);

            return (true, 
                   Convert.ToInt32(tokenData.GetProperty("UserId").ToString()),
                   tokenData.GetProperty("Email").ToString());
        }
        catch
        {
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
        var base64 = base64Url
            .Replace('-', '+')
            .Replace('_', '/');

        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }

        return Convert.FromBase64String(base64);
    }
}