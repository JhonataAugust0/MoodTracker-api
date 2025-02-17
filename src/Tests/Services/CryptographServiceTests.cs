using MoodTracker_back.Application.Interfaces;
using MoodTracker_back.Infrastructure.Adapters.Security;
using Moq;
using Xunit;

namespace MoodTracker_back.Tests.Services
{
    public class CryptographServiceTests
    {
        private readonly Mock<ILoggingService> _loggerMock = new();
        private readonly CryptographService _service;

        
        public CryptographServiceTests()
        {
            Environment.SetEnvironmentVariable("DATA_STORAGE_ENCRYPTION_KEY", "4jYYUxbPFggDQm4MPZHucUWQ8cy6f7EXw+hTpIPJ134=");
            Environment.SetEnvironmentVariable("DATA_STORAGE_ENCRYPTION_KEY_IV", "+ObmeFTp7EDn7kZAFxGz8w==");
            _service = new CryptographService(_loggerMock.Object);
        }
        
        [Fact]
        public void Encrypt_Decrypt_DeveRetornarTextoOriginal()
        {
            const string textoOriginal = "Teste123";

            var textoCriptografado = _service.Encrypt(textoOriginal);
            var textoDescriptografado = _service.Decrypt(textoCriptografado);

            Assert.Equal(textoOriginal, textoDescriptografado);
        }

        [Fact]
        public void Encrypt_QuandoTextoForNulo_DeveRetornarExcecao()
        {            
            Assert.Throws<ArgumentNullException>(() => _service.Encrypt(null!));
        }

        [Fact]
        public void Decrypt_QuandoTextoForInvalido_DeveRetornarExcecao()
        {
            Assert.Throws<ArgumentException>(() => _service.Decrypt("O texto para descriptografar é nulo ou vazio."));
        }
    }
}