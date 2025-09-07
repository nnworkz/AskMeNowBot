namespace AskMeNowBot.Database.Column;

public interface IBansColumn
{
    public string SenderId { get; }
    public string RecipientId { get; }
    public string Reason { get; }
    public string BannedAt { get; }
}
