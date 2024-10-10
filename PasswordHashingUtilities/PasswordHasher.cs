using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System;
using System.Security.Cryptography;

namespace dotInstrukcijeBackend.PasswordHashingUtilities
{
    public static class PasswordHasher
    {
        public static string HashPassword(string password)
        {
            // Generiranje random salt-a
            byte[] salt = new byte[128 / 8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            // Hashiranje lozinke s PBKDF2 algoritmom
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));

            // Vraćanje salt-a i hashirane lozinke (potrebno ih je pohraniti zajedno)
            return $"{Convert.ToBase64String(salt)}.{hashed}";
        }

        public static bool VerifyPassword(string hashedPasswordWithSalt, string passwordToCheck)
        {
            var parts = hashedPasswordWithSalt.Split('.', 2);

            if (parts.Length != 2)
            {
                return false; // Format nije ispravan
            }

            var salt = Convert.FromBase64String(parts[0]);
            var hashedPassword = parts[1];

            // Ponovno hashiranje unesene lozinke s istim salt-om
            var hashOfInput = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: passwordToCheck,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));

            // Usporedba hashiranih lozinki
            return hashedPassword == hashOfInput;
        }

        internal static bool VerifyPassword(object password1, string password2)
        {
            throw new NotImplementedException();
        }
    }
}
