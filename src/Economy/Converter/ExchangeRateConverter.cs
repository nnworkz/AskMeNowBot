using AskMeNowBot.Api.ExchangeRate;
using AskMeNowBot.Configuration;
using AskMeNowBot.Economy.Price;
using AskMeNowBot.Exceptions;

namespace AskMeNowBot.Economy.Converter;

public class ExchangeRateConverter(IConfig config) : IConverter
{
    private readonly Dictionary<(CurrencyName from, CurrencyName to), double> _cachedRates = new();
    private DateTime _lastUpdate;
    
    public async Task<IPrice> ConvertCurrency(double amount, CurrencyName from, CurrencyName to)
    {
        if (amount < 0)
        {
            throw new AmountCannotNegativeException();
        }
        
        if (from == to)
        {
            return new BasePrice(amount, to);
        }

        if (_cachedRates.TryGetValue((from, to), out var rate) &&
            DateTime.UtcNow - _lastUpdate < TimeSpan.FromSeconds(config.ExchangeRate.CachePeriod))
        {
            return new BasePrice(amount * rate, to);
        }

        var client = new ExchangeRateClient(config.ExchangeRate.AccessKey);
        var response = await client.ConvertAsync(from.ToString(), to.ToString(), amount);
        rate = response.Info.Quote;
        
        _cachedRates[(from, to)] = rate;
        _lastUpdate = DateTime.UtcNow;

        return new BasePrice(amount * rate, to);
    }
}
