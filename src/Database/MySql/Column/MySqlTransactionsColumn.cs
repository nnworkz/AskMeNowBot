using AskMeNowBot.Database.Column;

namespace AskMeNowBot.Database.MySql.Column;

public class MySqlTransactionsColumn : ITransactionsColumn
{
    public string Id => "id";
    public string UserId => "user_id";
    public string Type => "type";
    public string Amount => "amount";
    public string Currency => "currency";
    public string CreatedAt => "created_at";
}
