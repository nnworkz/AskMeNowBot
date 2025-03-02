using System.Globalization;
using System.Resources;
using AskMeNowBot.Database;
using AskMeNowBot.Exceptions;

namespace AskMeNowBot.Localization;

public class Locale(IProvider provider)
{
	private readonly Dictionary<string, (ResourceManager Resource, CultureInfo Culture)> _resourceCache = new();
	
	public async Task<string> Get(long id, string title, string? fallbackLanguage = null)
	{
		string language;

		try
		{
			language = await provider.GetLanguage(id);
		}
		catch (DataNotFoundException)
		{
			language = fallbackLanguage ?? LanguageNames.English;
		}

		if (!IsValidLanguage(language))
		{
			language = LanguageNames.English;
		}

		if (_resourceCache.TryGetValue(language, out var resourceEntry))
		{
			return resourceEntry.Resource.GetString(title, resourceEntry.Culture) ?? title;
		}

		resourceEntry = CreateResourceEntry(language);
		_resourceCache[language] = resourceEntry;

		return resourceEntry.Resource.GetString(title, resourceEntry.Culture) ?? title;
	}
	
	private static bool IsValidLanguage(string language)
	{
		var resourceEntry = CreateResourceEntry(language);
		
		return resourceEntry.Resource.GetResourceSet(CultureInfo.InvariantCulture, true, false) != null;
	}
	
	private static (ResourceManager Resource, CultureInfo Culture) CreateResourceEntry(string language)
	{
		var baseName = $"{typeof(Locale).Namespace!.Split('.')[0]}.resources.locale.{language}";
		var resourceManager = new ResourceManager(baseName, typeof(Locale).Assembly);
		var culture = new CultureInfo(language);
    
		return (resourceManager, culture);
	}
}
