namespace AskMeNowBot.Subscription;

public record BaseSubscriber(long UserId, DateTime StartedAt, DateTime EndsAt) : ISubscriber
{
    public long UserId { get; set; } = UserId;
    public DateTime StartedAt { get; set; } = StartedAt;
    public DateTime EndsAt { get; set; } = EndsAt;
}
