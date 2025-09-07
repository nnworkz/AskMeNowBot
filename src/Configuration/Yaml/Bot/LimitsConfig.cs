namespace AskMeNowBot.Configuration.Yaml.Bot;

public class LimitsConfig
{
    public MessagesPerDayConfig MessagesPerDay { get; init; } = new();
    public CooldownConfig Cooldown { get; init; } = new();
}
