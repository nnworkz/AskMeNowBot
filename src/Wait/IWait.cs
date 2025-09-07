namespace AskMeNowBot.Wait;

public interface IWait
{
    long SenderId { get; set; }
    long? RecipientId { get; set; }
    WaitType Type { get; set; }
    bool? IsResponse { get; set; }
    string? Link { get; set; }
}
