using System.Collections.Concurrent;
using System.Security.Cryptography;

namespace CarDealerShip.Services
{
    public interface IOtpService
    {
        string Generate(string purpose, TimeSpan ttl);
        bool Validate(string purpose, string code);
    }

    public class OtpService : IOtpService
    {
        private readonly ConcurrentDictionary<string, (string Code, DateTimeOffset Exp)> _store = new();

        public string Generate(string purpose, TimeSpan ttl)
        {
            var code = RandomNumberGenerator.GetInt32(100000, 999999).ToString();
            _store[purpose] = (code, DateTimeOffset.UtcNow.Add(ttl));
            Console.WriteLine($"[OTP] {purpose} -> {code} (exp {_store[purpose].Exp:u})");
            return code;
        }

        public bool Validate(string purpose, string code)
        {
            return _store.TryGetValue(purpose, out var entry)
                && entry.Code == code
                && DateTimeOffset.UtcNow <= entry.Exp;
        }
    }
}
