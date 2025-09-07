namespace AskMeNowBot.Api.Gemini.Response;

public record Candidate(
    Content Content,
    FinishReason FinishReason,
    SafetyRating[] SafetyRatings,
    CitationMetadata CitationMetadata,
    int TokenCount,
    GroundingAttribution[] GroundingAttributions,
    decimal AvgLogprobs,
    LogprobsResult LogprobsResult,
    UrlRetrievalMetadata UrlRetrievalMetadata,
    int Index
);
