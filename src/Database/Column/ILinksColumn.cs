namespace AskMeNowBot.Database.Column;

public interface ILinksColumn
{
    public string Link { get; }
    public string UserId { get; }
    public string Used { get; }
    public string MaxUsages { get; }
    public string IsDeactivated { get; }
    public string ExpiresAt { get; }
}
