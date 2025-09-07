using System.Globalization;

using AskMeNowBot.Localization.Enum;

namespace AskMeNowBot.Economy.Price;

public record BasePrice(double Amount, CurrencyName Currency) : IPrice
{
    public override string ToString()
    {
        var culture = Currency switch
        {
            CurrencyName.Usd => new CultureInfo(CultureName.En),
            CurrencyName.Rub => new CultureInfo(CultureName.Ru),
            _ => CultureInfo.InvariantCulture,
        };
        
        return string.Format(culture, "{0:C}", Amount);
    }
}
