using AskMeNowBot.Database.Column;

namespace AskMeNowBot.Database.PostgreSql.Column;

public class PostgreSqlFiltersColumn : IFiltersColumn
{
    public string UserId => "user_id";
    public string Spam => "spam";
    public string Terrorism => "terrorism";
    public string Drugs => "drugs";
    public string Violence => "violence";
    public string Pornography =>  "pornography";
}
