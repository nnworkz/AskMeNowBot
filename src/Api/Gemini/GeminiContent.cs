namespace AskMeNowBot.Api.Gemini;

public class GeminiContent
{
    public string Role { get; set; } = "user";
    public List<GeminiPart> Parts { get; set; } = [];
}
