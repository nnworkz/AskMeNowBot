namespace AskMeNowBot.Database.Column;

public interface IUsersColumn
{
    public string Id { get; }
    public string Language { get; }
    public string Role { get; }
    public string Messages { get; }
    public string MessagesToday { get; }
    public string LastResetAt { get; }
    public string LastMessageAt { get; }
    public string RegisteredAt { get; }
}
