namespace AskMeNowBot.Api.Gemini.Response;

public record GeminiResponse(Candidate[] Candidates, object PromptFeedback, object UsageMetadata, string ModelVersion);
