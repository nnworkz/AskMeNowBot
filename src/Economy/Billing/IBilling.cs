using AskMeNowBot.Economy.Price;
using AskMeNowBot.Subscription;

namespace AskMeNowBot.Economy.Billing;

public interface IBilling
{
    IPrice GetPriceByPeriod(SubscriptionPeriod period);
}
