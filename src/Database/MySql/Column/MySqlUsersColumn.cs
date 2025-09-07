using AskMeNowBot.Database.Column;

namespace AskMeNowBot.Database.MySql.Column;

public class MySqlUsersColumn : IUsersColumn
{
    public string Id => "id";
    public string Language => "language";
    public string Role => "role";
    public string Messages => "messages";
    public string MessagesToday => "messages_today";
    public string LastResetAt => "last_reset_at";
    public string LastMessageAt => "last_message_at";
    public string RegisteredAt => "registered_at";
}
