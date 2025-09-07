using AskMeNowBot.Database.Column;

namespace AskMeNowBot.Database.MySql.Column;

public class MySqlSubscriptionsColumn : ISubscriptionsColumn
{
    public string UserId => "user_id";
    public string StartedAt => "started_at";
    public string EndsAt => "ends_at";
}
