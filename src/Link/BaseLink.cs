namespace AskMeNowBot.Link;

public record BaseLink(string Link, long UserId, long Used, long? MaxUsages, bool IsDeactivated, DateTime? ExpiresAt)
    : ILink
{
    public string Link { get; set; } = Link;
    public long UserId { get; set; } = UserId;
    public long Used { get; set; } = Used;
    public long? MaxUsages { get; set; } = MaxUsages;
    public bool IsDeactivated { get; set; } = IsDeactivated;
    public DateTime? ExpiresAt { get; set; } = ExpiresAt;
}
