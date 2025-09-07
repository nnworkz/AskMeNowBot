namespace AskMeNowBot.Database.Column;

public interface ISubscriptionsColumn
{
    public string UserId { get; }
    public string StartedAt { get; }
    public string EndsAt { get; }
}
