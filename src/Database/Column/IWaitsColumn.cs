namespace AskMeNowBot.Database.Column;

public interface IWaitsColumn
{
    public string SenderId { get; }
    public string RecipientId { get; }
    public string Type { get; }
    public string IsResponse { get; }
    public string Link { get; }
}
