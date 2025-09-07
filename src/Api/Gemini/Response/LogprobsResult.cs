namespace AskMeNowBot.Api.Gemini.Response;

public record LogprobsResult(object[] TopCandidates, Candidate[] ChosenCandidates);
