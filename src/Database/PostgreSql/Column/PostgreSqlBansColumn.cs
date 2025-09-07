using AskMeNowBot.Database.Column;

namespace AskMeNowBot.Database.PostgreSql.Column;

public class PostgreSqlBansColumn : IBansColumn
{
    public string SenderId => "sender_id";
    public string RecipientId => "recipient_id";
    public string Reason => "reason";
    public string BannedAt => "banned_at";
}
