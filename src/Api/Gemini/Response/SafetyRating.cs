namespace AskMeNowBot.Api.Gemini.Response;

public record SafetyRating(object Category, object Probability, bool Blocked);
