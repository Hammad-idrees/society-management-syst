using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace SocietyMS.Helpers
{
    /// <summary>Provides input validation and password hashing utilities.</summary>
    public static class ValidationHelper
    {
        // ─── Password Hashing ─────────────────────────────────────────────────────
        /// <summary>Returns SHA-256 hex hash of the given plain-text password.</summary>
        public static string HashPassword(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                throw new ArgumentNullException("plainText");

            using (SHA256 sha = SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(plainText));
                StringBuilder sb = new StringBuilder();
                foreach (byte b in bytes) sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }

        /// <summary>Verifies a plain-text password against a stored hash.</summary>
        public static bool VerifyPassword(string plainText, string hash)
        {
            return HashPassword(plainText) == hash;
        }

        // ─── Field Validation ─────────────────────────────────────────────────────
        /// <summary>Returns true if email format is valid.</summary>
        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            return Regex.IsMatch(email,
                @"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase);
        }

        /// <summary>Returns true if the string is non-null and non-empty.</summary>
        public static bool IsNotEmpty(string value)
        {
            return !string.IsNullOrWhiteSpace(value);
        }

        /// <summary>Returns true if password meets minimum requirements (8+ chars, digit, letter).</summary>
        public static bool IsStrongPassword(string password)
        {
            if (string.IsNullOrEmpty(password) || password.Length < 8) return false;
            bool hasLetter = Regex.IsMatch(password, @"[a-zA-Z]");
            bool hasDigit  = Regex.IsMatch(password, @"\d");
            return hasLetter && hasDigit;
        }

        /// <summary>Generates a unique event ticket number.</summary>
        public static string GenerateTicketNumber(int eventId, int userId)
        {
            return string.Format("TKT-{0:D4}-{1:D4}-{2:MMddHHmm}", eventId, userId, DateTime.Now);
        }
    }
}


