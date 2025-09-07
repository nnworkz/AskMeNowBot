using AskMeNowBot.Economy.Price;

namespace AskMeNowBot.Economy.Converter;

public interface IConverter
{
    Task<IPrice> ConvertCurrency(double amount, CurrencyName from, CurrencyName to);
}
