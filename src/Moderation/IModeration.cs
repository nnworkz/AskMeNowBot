using AskMeNowBot.Filter;

namespace AskMeNowBot.Moderation;

public interface IModeration
{
    public Task<bool> CheckAsync(IFilter filter, string text);
}
