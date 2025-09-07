using AskMeNowBot.Database.Column;

namespace AskMeNowBot.Database.PostgreSql.Column;

public class PostgreSqlSubscriptionColumn : ISubscriptionsColumn
{
    public string UserId => "user_id";
    public string StartedAt => "started_at";
    public string EndsAt => "ends_at";
}
