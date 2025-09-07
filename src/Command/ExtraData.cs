namespace AskMeNowBot.Command;

public class ExtraData
{
    public bool CanEdit { get; init; }
    public int? MessageId { get; init; }
    
    public bool IsResponse { get; init; }
}
