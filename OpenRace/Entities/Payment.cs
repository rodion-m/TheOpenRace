using NodaTime;

namespace OpenRace.Entities
{
    public record Payment
    {
#pragma warning disable CS8618 - for EF Core
        private Payment()
#pragma warning restore CS8618
        {
        }
        
        public Payment(string id, decimal amount, string hash)
        {
            Id = id;
            Amount = amount;
            Hash = hash;
        }

        public string Id { get; set; }
        public decimal Amount { get; set; }
        public string Hash { get; set; }
        public Instant? PaidAt { get; set; }
        public Member? Member { get; set; }
    }
}