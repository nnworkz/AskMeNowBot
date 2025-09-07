namespace AskMeNowBot.Wait;

public record BaseWait(long SenderId, long? RecipientId, WaitType Type, bool? IsResponse, string? Link) : IWait
{
    public long SenderId { get; set; } = SenderId;
    public long? RecipientId { get; set; } = RecipientId;
    public WaitType Type { get; set; } = Type;
    public bool? IsResponse { get; set; } = IsResponse;
    public string? Link { get; set; } = Link;
}
