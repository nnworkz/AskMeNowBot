using AskMeNowBot.Configuration.Yaml.Bot;
using AskMeNowBot.Configuration.Yaml.CryptoPay;
using AskMeNowBot.Configuration.Yaml.Database;
using AskMeNowBot.Configuration.Yaml.ExchangeRate;
using AskMeNowBot.Configuration.Yaml.Gemini;
using AskMeNowBot.Configuration.Yaml.Prices;
using AskMeNowBot.Configuration.Yaml.Security;

namespace AskMeNowBot.Configuration;

public interface IConfig
{
    DatabaseConfig Database { get; }
    BotConfig Bot { get; }
    GeminiConfig Gemini { get; }
    CryptoPayConfig CryptoPay { get; }
    ExchangeRateConfig ExchangeRate { get; }
    SecurityConfig Security { get; }
    PricesConfig Prices { get; }

    string GetPath();
    bool IsValid();
}
