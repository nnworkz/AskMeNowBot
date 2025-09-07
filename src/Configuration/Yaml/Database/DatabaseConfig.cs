namespace AskMeNowBot.Configuration.Yaml.Database;

public class DatabaseConfig
{
    public string Type { get; init; } = string.Empty;
    public ConnectionStringsConfig ConnectionStrings { get; init; } = new();
}
