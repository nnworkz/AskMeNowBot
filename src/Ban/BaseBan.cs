namespace AskMeNowBot.Ban;

public record BaseBan(long SenderId, long RecipientId, string? Reason, DateTime BannedAt) : IBan
{
    public long SenderId { get; set; } = SenderId;
    public long RecipientId { get; set; } = RecipientId;
    public string? Reason { get; set; } = Reason;
    public DateTime BannedAt { get; set; } = BannedAt;
}
