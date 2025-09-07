namespace AskMeNowBot.Api.ExchangeRate.Response;

public class QueryResponse
{
    public string From { get; set; } = string.Empty;
    public string To { get; set; } = string.Empty;
    public double Amount { get; set; }
}
