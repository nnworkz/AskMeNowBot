using AskMeNowBot.Api.ExchangeRate.Response;

using Refit;

namespace AskMeNowBot.Api.ExchangeRate;

public interface IExchangeRateApi
{
    [Get($"/{ExchangeRateEndpoints.Convert}")]
    Task<ExchangeRateResponse> ConvertAsync(
        [AliasAs(ExchangeRateParams.From)] string from,
        [AliasAs(ExchangeRateParams.To)] string to,
        [AliasAs(ExchangeRateParams.Amount)] double amount,
        [AliasAs(ExchangeRateParams.AccessKey)] string accessKey
    );
}
