using NodaTime;

namespace OpenRace.Entities
{
    public record Payment
    {
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
    }
}