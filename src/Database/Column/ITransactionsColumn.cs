namespace AskMeNowBot.Database.Column;

public interface ITransactionsColumn
{
    public string Id { get; }
    public string UserId { get; }
    public string Type { get; }
    public string Amount { get; }
    public string Currency { get; }
    public string CreatedAt { get; }
}
