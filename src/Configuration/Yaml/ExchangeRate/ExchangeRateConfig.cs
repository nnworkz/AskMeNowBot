namespace AskMeNowBot.Configuration.Yaml.ExchangeRate;

public class ExchangeRateConfig
{
    public string AccessKey { get; init; } = string.Empty;
    public int CachePeriod { get; init; }
}
