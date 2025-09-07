namespace AskMeNowBot.Configuration.Yaml.Prices;

public class PricesConfig
{
    public string Currency { get; init; } = string.Empty;
    public AmountsConfig Amounts { get; init; } = new();
}
