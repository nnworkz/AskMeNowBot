using AskMeNowBot.Ban;
using AskMeNowBot.Filter;
using AskMeNowBot.Link;
using AskMeNowBot.Subscription;
using AskMeNowBot.Transaction;
using AskMeNowBot.User;
using AskMeNowBot.Wait;

namespace AskMeNowBot.Database;

public interface IProvider
{
    Task<bool> IsRegistered(long id);
    Task<IUser> GetUser(long id);

    Task<bool> IsSubscriber(long id);
    Task<ISubscriber> GetSubscriber(long id);

    Task<bool> InWait(long senderId);
    Task<IWait> GetWait(long senderId);

    Task<bool> IsBanned(long senderId, long recipientId);
    Task<IBan> GetBan(long senderId, long recipientId);

    Task<bool> LinkExist(string link);
    Task<ILink> GetLink(string link);
    Task<List<ILink>> GetLinks(long userId);

    Task<List<ITransaction>> GetTransactions(long userId);

    Task<IFilter> GetFilter(long userId);
}
