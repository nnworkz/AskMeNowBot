using AskMeNowBot.Database.Column;

namespace AskMeNowBot.Database.MySql.Column;

public class MySqlWaitsColumn : IWaitsColumn
{
    public string SenderId => "sender_id";
    public string RecipientId => "recipient_id";
    public string Type => "type";
    public string IsResponse => "is_response";
    public string Link => "link";
}
