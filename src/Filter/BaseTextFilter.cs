using System.Diagnostics;
using System.Net;
using System.Reflection;

using AskMeNowBot.Api.Gemini;
using AskMeNowBot.Configuration;

using Refit;

namespace AskMeNowBot.Filter;

public class BaseTextFilter(IConfig config) : ITextFilter
{
    public async Task<bool> IsViolated(IFilter filter, string text)
    {
        var filters = $"[{string.Join(", ", filter
            .GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.PropertyType == typeof(bool) && (bool)p.GetValue(filter)!)
            .Select(p => p.Name))}]";

        var promptText =
            $"If the text contains any of the following filters: {filters}, respond with \"+\", Text: {text}";

        while (true)
        {
            try
            {
                var response = await new GeminiClient(config.Gemini.Key).GenerateContent(promptText);

                return response.Candidates.Any(
                    candidate => candidate.Content.Parts.Any(part => part.Text.Trim() == "+")
                );
            }
            catch (ApiException ex) when (ex.StatusCode is HttpStatusCode.Forbidden or HttpStatusCode.BadRequest)
            {
                RestartTor();
            }
        }
    }

    private static void RestartTor()
    {
        using var process = Process.Start(
            new ProcessStartInfo
            {
                FileName = "sudo",
                Arguments = "killall -HUP tor",
                UseShellExecute = false
            }
        );

        process?.WaitForExit();
    }
}
