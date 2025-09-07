namespace AskMeNowBot.Filter;

public interface ITextFilter
{
    Task<bool> IsViolated(IFilter filter, string text);
}
