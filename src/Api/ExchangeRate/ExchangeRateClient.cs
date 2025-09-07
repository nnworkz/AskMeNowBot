using AskMeNowBot.Api.ExchangeRate.Response;
using AskMeNowBot.Exceptions;

using Refit;

namespace AskMeNowBot.Api.ExchangeRate;

public class ExchangeRateClient(string accessKey)
{
    private readonly IExchangeRateApi _api = RestService.For<IExchangeRateApi>("https://api.exchangerate.host");
    
    public async Task<ExchangeRateResponse> ConvertAsync(string from, string to, double amount)
    {
        var response = await _api.ConvertAsync(from, to, amount, accessKey);
        
        if (!response.Success)
        {
            throw new ExchangeRateFetchException();
        }

        return response;
    }
}
