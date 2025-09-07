using AskMeNowBot.Economy;

namespace AskMeNowBot.Transaction;

public record BaseTransaction(
    long Id,
    long UserId,
    TransactionType Type,
    double Amount,
    CurrencyName Currency,
    DateTime CreatedAt
) : ITransaction
{
    public long Id { get; set; } = Id;
    public long UserId { get; set; } = UserId;
    public TransactionType Type { get; set; } = Type;
    public double Amount { get; set; } = Amount;
    public CurrencyName Currency { get; set; } = Currency;
    public DateTime CreatedAt { get; set; } = CreatedAt;
}
