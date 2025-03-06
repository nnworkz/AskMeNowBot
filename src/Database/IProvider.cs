namespace AskMeNowBot.Database;

public interface IProvider
{
	Task<bool> IsRegistered(long id);
	Task<string> GetLanguage(long id);

	Task<bool> InExpectation(long sender);
	Task<long> GetRecipient(long sender);
	Task<bool> IsResponse(long sender);

	Task<bool> IsBanned(long sender, long recipient);
}
