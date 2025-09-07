using AskMeNowBot.Filter;

namespace AskMeNowBot.Moderation;

public class BaseModeration(ITextFilter textFilter) : IModeration
{
    public async Task<bool> CheckAsync(IFilter filter, string text)
    {
        return await textFilter.IsViolated(filter, text);
    }
}
