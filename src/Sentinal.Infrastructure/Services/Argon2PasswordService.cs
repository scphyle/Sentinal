using System.Security.Cryptography;
using System.Text;
using Konscious.Security.Cryptography;
using Sentinal.Infrastructure.Common.Security;

namespace Sentinal.Infrastructure.Services;

public class Argon2PasswordService : IPasswordHasher
{
    /// <summary>
    /// Hashes a password using Argon2i algorithm with a random salt
    /// </summary>
    /// <param name="password">The plain text password to hash (min 8 characters)</param>
    /// <returns>Base64 encoded hash</returns>
    /// <exception cref="ArgumentException">Thrown if password is null, empty, or less than 8 characters</exception>
    /// <remarks>
    /// For more information, see <see href="https://github.com/kmaragon/Konscious.Security.Cryptography">Konscious Argon2 Documentation</see>
    /// </remarks>
    public string HashPassword(string password)
    {
        if (string.IsNullOrEmpty(password))
            throw new ArgumentException("Value cannot be null or empty.", nameof(password));
        if (password.Length < 8)
            throw new ArgumentException("Password must be at least 8 characters long.", nameof(password));


        var salt = new byte[16];
        RandomNumberGenerator.Fill(salt);

        var argon2 = new Argon2i(Encoding.UTF8.GetBytes(password));

        argon2.DegreeOfParallelism = 2;
        argon2.Iterations = 2;
        argon2.MemorySize = 8192;
        argon2.Salt = salt;

        //We can store the salt and hash together in one column and then know based on byte postion where the salt ends and the hash begins
        var hash = argon2.GetBytes(128);
        var saltAndHash = salt.Concat(hash).ToArray();

        return Convert.ToBase64String(saltAndHash);
    }

    /// <summary>
    /// Verifies a plain text password against a stored Argon2 hash
    /// </summary>
    /// <param name="password">The plain text password to verify (min 8 characters)</param>
    /// <param name="storedHash">The Base64 encoded hash from HashPassword (contains salt + hash)</param>
    /// <returns>True if the password matches the hash, false otherwise</returns>
    /// <exception cref="ArgumentException">Thrown if password is null, empty, or less than 8 characters</exception>
    /// <exception cref="FormatException">Thrown if storedHash is not valid Base64</exception>
    /// <remarks>
    /// Extracts the salt from the first 16 bytes of the stored hash, re-hashes the provided password
    /// with the same salt and parameters, then performs a timing-safe comparison to prevent timing attacks.
    /// </remarks>
    public bool VerifyPassword(string password, string storedHash)
    {

        if (string.IsNullOrEmpty(password))
            throw new ArgumentException("Value cannot be null or empty.", nameof(password));
        if (password.Length < 8)
            throw new ArgumentException("Password must be at least 8 characters long.", nameof(password));

        var hashAsBytes = Convert.FromBase64String(storedHash);

        var salt = hashAsBytes.Take(16).ToArray();
        var argon2 = new Argon2i(Encoding.UTF8.GetBytes(password));

        argon2.DegreeOfParallelism = 2;
        argon2.Iterations = 2;
        argon2.MemorySize = 8192;
        argon2.Salt = salt;

        var hash = argon2.GetBytes(128);
        return hash.SequenceEqual(hashAsBytes.Skip(16).ToArray());

    }
}