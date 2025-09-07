namespace AskMeNowBot.Economy.Price;

public interface IPrice
{
    double Amount { get; init; }
    CurrencyName Currency { get; init; }

    public string ToString();
}
