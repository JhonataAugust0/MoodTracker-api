namespace MoodTracker_back.Application.Interfaces;

public interface ICryptographService
{
    public string Encrypt(string plaintext);
    public string Decrypt(string plaintext);
}