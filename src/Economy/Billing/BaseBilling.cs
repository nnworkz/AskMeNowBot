using AskMeNowBot.Configuration;
using AskMeNowBot.Economy.Price;
using AskMeNowBot.Exceptions;
using AskMeNowBot.Subscription;

namespace AskMeNowBot.Economy.Billing;

public class BaseBilling(IConfig config) : IBilling
{
    public IPrice GetPriceByPeriod(SubscriptionPeriod period)
    {
        var amounts = config.Prices.Amounts;

        return new BasePrice(
            period switch
            {
                SubscriptionPeriod.Day => amounts.Day,
                SubscriptionPeriod.Week => amounts.Week,
                SubscriptionPeriod.Month => amounts.Month,
                SubscriptionPeriod.HalfYear => amounts.HalfYear,
                SubscriptionPeriod.Year => amounts.Year,
                _ => throw new PeriodNotFoundException(period.ToString())
            },
            Enum.Parse<CurrencyName>(config.Prices.Currency, true)
        );
    }
}
