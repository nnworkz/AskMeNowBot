namespace AskMeNowBot.Database.Column;

public interface IFiltersColumn
{
    public string UserId { get; }
    public string Spam { get; }
    public string Terrorism { get; }
    public string Drugs { get; }
    public string Violence { get; }
    public string Pornography { get; }
}
