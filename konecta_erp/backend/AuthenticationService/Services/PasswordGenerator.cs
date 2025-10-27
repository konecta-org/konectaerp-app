using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace AuthenticationService.Services
{
    public static class PasswordGenerator
    {
        private const string Uppercase = "ABCDEFGHJKLMNPQRSTUVWXYZ";
        private const string Lowercase = "abcdefghijkmnopqrstuvwxyz";
        private const string Digits = "23456789";
        private const string Special = "!@$?_-";

        public static string Generate(int length = 12)
        {
            if (length < 6)
            {
                throw new ArgumentOutOfRangeException(nameof(length), "Password length must be at least 6 characters.");
            }

            var categories = new[] { Uppercase, Lowercase, Digits, Special };
            var passwordChars = new List<char>(length);

            foreach (var category in categories)
            {
                passwordChars.Add(GetRandomChar(category));
            }

            var allChars = string.Concat(categories);
            while (passwordChars.Count < length)
            {
                passwordChars.Add(GetRandomChar(allChars));
            }

            return Shuffle(passwordChars);
        }

        private static char GetRandomChar(string source)
        {
            var data = new byte[4];
            RandomNumberGenerator.Fill(data);
            var value = BitConverter.ToUInt32(data, 0);
            return source[(int)(value % (uint)source.Length)];
        }

        private static string Shuffle(IList<char> chars)
        {
            for (var i = chars.Count - 1; i > 0; i--)
            {
                var data = new byte[4];
                RandomNumberGenerator.Fill(data);
                var j = (int)(BitConverter.ToUInt32(data, 0) % (uint)(i + 1));
                (chars[i], chars[j]) = (chars[j], chars[i]);
            }

            var builder = new StringBuilder(chars.Count);
            foreach (var c in chars)
            {
                builder.Append(c);
            }
            return builder.ToString();
        }
    }
}
