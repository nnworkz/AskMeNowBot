namespace AskMeNowBot.Subscription;

public interface ISubscriber
{
    long UserId { get; set; }
    DateTime StartedAt { get; set; }
    DateTime EndsAt { get; set; }

    string ToString();
}
