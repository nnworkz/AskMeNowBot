using AskMeNowBot.Economy;

namespace AskMeNowBot.Transaction;

public interface ITransaction
{
    long Id { get; set; }
    long UserId { get; set; }
    TransactionType Type { get; set; }
    double Amount { get; set; }
    CurrencyName Currency { get; set; }
    DateTime CreatedAt { get; set; }
}
