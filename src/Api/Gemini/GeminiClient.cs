using System.Net;

using AskMeNowBot.Api.Gemini.Response;
using AskMeNowBot.Exceptions;

using Refit;

namespace AskMeNowBot.Api.Gemini;

public class GeminiClient(string key)
{
    private readonly IGeminiApi _api = RestService.For<IGeminiApi>(
        new HttpClient(new HttpClientHandler
        {
            Proxy = new WebProxy("socks5://127.0.0.1:9050"),
            UseProxy = true
        })
        {
            BaseAddress = new Uri("https://generativelanguage.googleapis.com")
        }
    );

    public async Task<GeminiResponse> GenerateContent(string text)
    {
        var response = await _api.GenerateContent(
            key,
            new GeminiBody
            {
                Contents =
                [
                    new GeminiContent
                    {
                        Parts = [new GeminiPart { Text = text }]
                    }
                ]
            }
        );

        if (response.Candidates == null || response.Candidates.Length == 0)
        {
            throw new GeminiFetchException();
        }

        return response;
    }
}
