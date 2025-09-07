using AskMeNowBot.Database.Column;

namespace AskMeNowBot.Database.PostgreSql.Column;

public class PostgreSqlLinksColumn : ILinksColumn
{
    public string Link => "link";
    public string UserId => "user_id";
    public string Used => "used";
    public string MaxUsages => "max_usages";
    public string IsDeactivated => "is_deactivated";
    public string ExpiresAt => "expires_at";
}
