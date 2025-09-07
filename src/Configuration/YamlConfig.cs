using System.Reflection;

using AskMeNowBot.Configuration.Converter;
using AskMeNowBot.Configuration.Yaml.Bot;
using AskMeNowBot.Configuration.Yaml.CryptoPay;
using AskMeNowBot.Configuration.Yaml.Database;
using AskMeNowBot.Configuration.Yaml.ExchangeRate;
using AskMeNowBot.Configuration.Yaml.Gemini;
using AskMeNowBot.Configuration.Yaml.Prices;
using AskMeNowBot.Configuration.Yaml.Security;
using AskMeNowBot.Exceptions;

using YamlDotNet.Serialization;

namespace AskMeNowBot.Configuration;

public class YamlConfig : IConfig
{
    public DatabaseConfig Database { get; init; } = new();
    public BotConfig Bot { get; init; } = new();
    public GeminiConfig Gemini { get; init; } = new();
    public CryptoPayConfig CryptoPay { get; init; } = new();
    public ExchangeRateConfig ExchangeRate { get; init; } = new();
    public SecurityConfig Security { get; init; } = new();
    public PricesConfig Prices { get; init; } = new();

    public string GetPath()
    {
        return Path.Combine(Directory.GetCurrentDirectory(), "resources", "config.yaml");
    }

    public bool IsValid()
    {
        return IsObjectValid();
    }

    public async Task<IConfig> LoadAsync()
    {
        var path = GetPath();

        if (!File.Exists(path))
        {
            throw new ConfigFileNotFoundException(path);
        }

        var deserializer = new DeserializerBuilder().WithTypeConverter(new StringToNumberConverter()).Build();
        var yaml = await File.ReadAllTextAsync(path);

        return deserializer.Deserialize<YamlConfig>(yaml);
    }
    
    private bool IsObjectValid(object? obj = null)
    {
        obj ??= this;

        var properties = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
        {
            var value = property.GetValue(obj);

            switch (value)
            {
                case null:
                case string str when string.IsNullOrWhiteSpace(str):
                case double and <= 0:
                {
                    return false;
                }
            }

            if (!property.PropertyType.IsClass || property.PropertyType == typeof(string))
            {
                continue;
            }

            if (!IsObjectValid(value))
            {
                return false;
            }
        }

        return true;
    }
}
