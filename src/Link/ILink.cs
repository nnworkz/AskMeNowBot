namespace AskMeNowBot.Link;

public interface ILink
{
    public string Link { get; set; }
    public long UserId { get; set; }
    public long Used { get; set; }
    public long? MaxUsages { get; set; }
    public bool IsDeactivated { get; set; }
    public DateTime? ExpiresAt { get; set; }
    
}
