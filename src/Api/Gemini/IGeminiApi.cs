using AskMeNowBot.Api.Gemini.Response;

using Refit;

namespace AskMeNowBot.Api.Gemini;

public interface IGeminiApi
{
    [Post($"/{GeminiEndpoints.GenerateContent}")]
    Task<GeminiResponse> GenerateContent(
        [Query(GeminiParams.Key)] string key,
        [Body] GeminiBody body
    );
}
