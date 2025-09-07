using AskMeNowBot.Database.Column;

namespace AskMeNowBot.Database.PostgreSql.Column;

public class PostgreSqlTransactionsColumn : ITransactionsColumn
{
    public string Id => "id";
    public string UserId => "user_id";
    public string Type => "type";
    public string Amount => "amount";
    public string Currency => "currency";
    public string CreatedAt => "created_at";
}
