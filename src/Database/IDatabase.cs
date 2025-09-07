using AskMeNowBot.Ban;
using AskMeNowBot.Filter;
using AskMeNowBot.Link;
using AskMeNowBot.Subscription;
using AskMeNowBot.Transaction;
using AskMeNowBot.User;
using AskMeNowBot.Wait;

namespace AskMeNowBot.Database;

public interface IDatabase
{
    Task InitAsync();

    Task AddUser(IUser user);
    Task RemoveUser(IUser user);

    Task AddSubscriber(ISubscriber subscriber);
    Task RemoveSubscriber(ISubscriber subscriber);

    Task AddWait(IWait wait);
    Task RemoveWait(IWait wait);

    Task AddBan(IBan ban);
    Task RemoveBan(IBan ban);

    Task AddLink(ILink link);
    Task RemoveLink(ILink link);

    Task AddTransaction(ITransaction transaction);

    Task AddFilter(IFilter filter);
    Task RemoveFilter(IFilter filter);
}
