using System.Globalization;
using System.Resources;
using System.Text.RegularExpressions;

using AskMeNowBot.Economy;
using AskMeNowBot.Exceptions;
using AskMeNowBot.Localization.Enum;
using AskMeNowBot.Utils.TextFormat;

namespace AskMeNowBot.Localization;

public partial class BaseLocale(ITextFormat textFormat) : ILocale
{
    private readonly Dictionary<LanguageName, (ResourceManager Resource, CultureInfo Culture)> _resourceCache = new();

    public string Get(MessageKey key, LanguageName language, params object[] args)
    {
        args = args.Select(arg => textFormat.EscapeMarkdownV2(arg.ToString() ?? string.Empty)).ToArray<object>();
        
        if (_resourceCache.TryGetValue(language, out var resourceEntry))
        {
            return ReplacePlaceholders(GetResource(resourceEntry, key), args);
        }

        var resourceManager = new ResourceManager(
            $"{typeof(BaseLocale).Assembly.GetName().Name}.resources.locale.{language}",
            typeof(BaseLocale).Assembly
        );

        resourceEntry = (resourceManager, new CultureInfo(language.ToString()));
        _resourceCache[language] = resourceEntry;

        return ReplacePlaceholders(GetResource(resourceEntry, key), args);
    }

    public CurrencyName GetCurrencyByLanguage(LanguageName language)
    {
        return language switch
        {
            LanguageName.En => CurrencyName.Usd,
            LanguageName.Ru => CurrencyName.Rub,
            _ => throw new UnsupportedLanguageException(language.ToString())
        };
    }

    public CultureInfo GetCultureByLanguage(LanguageName language)
    {
        return language switch
        {
            LanguageName.En => new CultureInfo(CultureName.En),
            LanguageName.Ru => new CultureInfo(CultureName.Ru),
            _ => CultureInfo.InvariantCulture
        };
    }

    private static string GetResource((ResourceManager Resource, CultureInfo Culture) resourceEntry, MessageKey key)
    {
        var name = key.ToString();
        var result = resourceEntry.Resource.GetString(name, resourceEntry.Culture);

        if (result != null)
        {
            return result;
        }

        throw new ResourceNotFoundException(name);
    }
    
    private static string ReplacePlaceholders(string template, object[] args)
    {
        var matches = Placeholder().Matches(template);

        for (var i = 0; i < matches.Count && i < args.Length; i++)
        {
            var placeholder = matches[i].Value;
            var value = args[i].ToString() ?? string.Empty;

            template = template.Replace(placeholder, value);
        }

        return template;
    }

    [GeneratedRegex(@"\{.*?\}")]
    private static partial Regex Placeholder();
}
