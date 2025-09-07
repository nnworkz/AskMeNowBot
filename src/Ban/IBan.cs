namespace AskMeNowBot.Ban;

public interface IBan
{
    long SenderId { get; set; }
    long RecipientId { get; set; }
    string? Reason { get; set; }
    DateTime BannedAt { get; set; }
}
