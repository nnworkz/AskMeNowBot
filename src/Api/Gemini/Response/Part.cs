namespace AskMeNowBot.Api.Gemini.Response;

public record Part(
    bool Thought,
    string Text,
    object InlineData,
    object FunctionCall,
    object FileData,
    object ExecutableCode,
    object CodeExecutionResult
);
