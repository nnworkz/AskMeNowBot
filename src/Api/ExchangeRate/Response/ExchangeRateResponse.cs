namespace AskMeNowBot.Api.ExchangeRate.Response;

public record ExchangeRateResponse(
    bool Success,
    string Terms,
    string Privacy,
    QueryResponse Query,
    InfoResponse Info,
    double Result
);
