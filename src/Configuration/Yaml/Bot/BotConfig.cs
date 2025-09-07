namespace AskMeNowBot.Configuration.Yaml.Bot;

public class BotConfig
{
    public string Name { get; init; } = string.Empty;
    public string CommandPrefix { get; init; } = string.Empty;
    public int MaxMessageLength { get; init; }
    public int MaxReasonLength { get; init; }
    public CredentialsConfig Credentials { get; init; } = new();
    public LimitsConfig Limits { get; init; } = new();
}
