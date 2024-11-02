namespace dotInstrukcijeBackend.Interfaces.Utility
{
    public interface IPasswordHasher
    {
        string HashPassword(string password);
        bool VerifyPassword(string hashedPasswordWithSalt, string passwordToCheck);
    }
}
