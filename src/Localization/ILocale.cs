using System.Globalization;

using AskMeNowBot.Economy;
using AskMeNowBot.Localization.Enum;

namespace AskMeNowBot.Localization;

public interface ILocale
{
	string Get(MessageKey key, LanguageName language, params object[] args);
	CurrencyName GetCurrencyByLanguage(LanguageName language);
	CultureInfo GetCultureByLanguage(LanguageName language);
}
