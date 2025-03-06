using AskMeNowBot.User;

namespace AskMeNowBot.Database;

public interface IDatabase
{
	Task InitAsync();

	Task AddUser(BaseUser user);

	Task AddExpectation(long sender, long recipient, bool isResponse);
	Task RemoveExpectation(long sender);

	Task AddBan(long sender, long recipient);
	Task RemoveBan(long sender, long recipient);
}
