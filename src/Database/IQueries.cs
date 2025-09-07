namespace AskMeNowBot.Database;

public interface IQueries
{
    string this[QueryName name] { get; }
}
