using System.Data.Common;

using AskMeNowBot.Ban;
using AskMeNowBot.Database.Column;
using AskMeNowBot.Economy;
using AskMeNowBot.Exceptions;
using AskMeNowBot.Filter;
using AskMeNowBot.Link;
using AskMeNowBot.Localization.Enum;
using AskMeNowBot.Subscription;
using AskMeNowBot.Transaction;
using AskMeNowBot.User;
using AskMeNowBot.Wait;

using Microsoft.Extensions.Hosting;

using MySqlConnector;

namespace AskMeNowBot.Database.MySql;

public class MySqlProvider(
    IHostApplicationLifetime lifetime,
    IQueries queries,
    IUsersColumn usersColumn,
    ISubscriptionsColumn subscriptionsColumn,
    IWaitsColumn waitsColumn,
    IBansColumn bansColumn,
    ILinksColumn linksColumn,
    ITransactionsColumn transactionsColumn,
    IFiltersColumn filtersColumn,
    DbDataSource dataSource
) : IProvider
{
    private CancellationToken CancellationToken { get; } = lifetime.ApplicationStopping;

    public async Task<bool> IsRegistered(long id)
    {
        await using var connection = await Connection();

        await using var command = new MySqlCommand(queries[QueryName.IsRegistered], connection);
        var parameters = command.Parameters;

        parameters.AddWithValue($"@{usersColumn.Id}", id);

        var result = await command.ExecuteScalarAsync(CancellationToken);

        return result != null && Convert.ToInt64(result) == 1;
    }

    public async Task<IUser> GetUser(long id)
    {
        await using var connection = await Connection();

        await using var command = new MySqlCommand(queries[QueryName.GetUser], connection);
        command.Parameters.AddWithValue($"@{usersColumn.Id}", id);

        await using var reader = await command.ExecuteReaderAsync(CancellationToken);

        if (!await reader.ReadAsync(CancellationToken))
        {
            throw new UserNotFoundException(id);
        }

        var language = Enum.Parse<LanguageName>(reader.GetString(reader.GetOrdinal(usersColumn.Language)), true);
        var role = Enum.Parse<UserRole>(reader.GetString(reader.GetOrdinal(usersColumn.Role)), true);
        var messages = reader.GetInt64(reader.GetOrdinal(usersColumn.Messages));
        var messagesToday = reader.GetInt32(reader.GetOrdinal(usersColumn.MessagesToday));
        var lastResetAt = reader.GetDateTime(reader.GetOrdinal(usersColumn.LastResetAt));

        var lastMessageAt = reader.IsDBNull(reader.GetOrdinal(usersColumn.LastMessageAt))
            ? (DateTime?)null
            : reader.GetDateTime(reader.GetOrdinal(usersColumn.LastMessageAt));

        var registeredAt = reader.GetDateTime(reader.GetOrdinal(usersColumn.RegisteredAt));

        return new BaseUser(id, language, role, messages, messagesToday, lastResetAt, lastMessageAt, registeredAt);
    }

    public async Task<bool> IsSubscriber(long userId)
    {
        await using var connection = await Connection();

        await using var command = new MySqlCommand(queries[QueryName.IsSubscriber], connection);
        var parameters = command.Parameters;

        parameters.AddWithValue($"@{subscriptionsColumn.UserId}", userId);

        var result = await command.ExecuteScalarAsync(CancellationToken);

        if (result == null || Convert.ToInt64(result) != 1)
        {
            return false;
        }

        var subscriber = await GetSubscriber(userId);

        return subscriber.EndsAt > DateTime.UtcNow;
    }

    public async Task<ISubscriber> GetSubscriber(long userId)
    {
        await using var connection = await Connection();

        await using var command = new MySqlCommand(queries[QueryName.GetSubscriber], connection);
        command.Parameters.AddWithValue($"@{subscriptionsColumn.UserId}", userId);

        await using var reader = await command.ExecuteReaderAsync(CancellationToken);

        if (!await reader.ReadAsync(CancellationToken))
        {
            throw new SubscriptionNotFoundException(userId);
        }

        var startedAt = reader.GetDateTime(reader.GetOrdinal(subscriptionsColumn.StartedAt));
        var endsAt = reader.GetDateTime(reader.GetOrdinal(subscriptionsColumn.EndsAt));

        return new BaseSubscriber(userId, startedAt, endsAt);
    }

    public async Task<bool> InWait(long senderId)
    {
        await using var connection = await Connection();

        await using var command = new MySqlCommand(queries[QueryName.InWait], connection);
        var parameters = command.Parameters;

        parameters.AddWithValue($"@{waitsColumn.SenderId}", senderId);

        var result = await command.ExecuteScalarAsync(CancellationToken);

        return result != null && Convert.ToInt64(result) == 1;
    }

    public async Task<IWait> GetWait(long senderId)
    {
        await using var connection = await Connection();

        await using var command = new MySqlCommand(queries[QueryName.GetWait], connection);
        command.Parameters.AddWithValue($"@{waitsColumn.SenderId}", senderId);

        await using var reader = await command.ExecuteReaderAsync(CancellationToken);

        if (!await reader.ReadAsync(CancellationToken))
        {
            throw new WaitNotFoundException(senderId);
        }

        var recipientId = reader.IsDBNull(reader.GetOrdinal(waitsColumn.RecipientId))
            ? (long?)null
            : reader.GetInt64(reader.GetOrdinal(waitsColumn.RecipientId));

        var type = Enum.Parse<WaitType>(reader.GetString(reader.GetOrdinal(waitsColumn.Type)), true);

        var isResponse = reader.IsDBNull(reader.GetOrdinal(waitsColumn.IsResponse))
            ? (bool?)null
            : reader.GetBoolean(reader.GetOrdinal(waitsColumn.IsResponse));

        var link = reader.IsDBNull(reader.GetOrdinal(waitsColumn.Link))
            ? null
            : reader.GetString(reader.GetOrdinal(waitsColumn.Link));

        return new BaseWait(senderId, recipientId, type, isResponse, link);
    }

    public async Task<bool> IsBanned(long senderId, long recipientId)
    {
        await using var connection = await Connection();

        await using var command = new MySqlCommand(queries[QueryName.IsBanned], connection);
        var parameters = command.Parameters;

        parameters.AddWithValue($"@{bansColumn.SenderId}", senderId);
        parameters.AddWithValue($"@{bansColumn.RecipientId}", recipientId);

        var result = await command.ExecuteScalarAsync(CancellationToken);

        return result != null && Convert.ToInt64(result) == 1;
    }

    public async Task<IBan> GetBan(long senderId, long recipientId)
    {
        await using var connection = await Connection();

        await using var command = new MySqlCommand(queries[QueryName.GetBan], connection);
        command.Parameters.AddWithValue($"@{bansColumn.SenderId}", senderId);
        command.Parameters.AddWithValue($"@{bansColumn.RecipientId}", recipientId);

        await using var reader = await command.ExecuteReaderAsync(CancellationToken);

        if (!await reader.ReadAsync(CancellationToken))
        {
            throw new BanNotFoundException(senderId, recipientId);
        }

        var reason = reader.GetString(reader.GetOrdinal(bansColumn.Reason));
        var bannedAt = reader.GetDateTime(reader.GetOrdinal(bansColumn.BannedAt));

        return new BaseBan(senderId, recipientId, reason, bannedAt);
    }

    public async Task<bool> LinkExist(string link)
    {
        await using var connection = await Connection();

        await using var command = new MySqlCommand(queries[QueryName.LinkExist], connection);
        command.Parameters.AddWithValue($"@{linksColumn.Link}", link);

        var result = await command.ExecuteScalarAsync(CancellationToken);

        return result != null && Convert.ToInt64(result) == 1;
    }

    public async Task<ILink> GetLink(string link)
    {
        await using var connection = await Connection();

        await using var command = new MySqlCommand(queries[QueryName.GetLink], connection);
        command.Parameters.AddWithValue($"@{linksColumn.Link}", link);

        await using var reader = await command.ExecuteReaderAsync(CancellationToken);

        if (!await reader.ReadAsync(CancellationToken))
        {
            throw new LinkNotFoundException();
        }

        var userId = reader.GetInt64(reader.GetOrdinal(linksColumn.UserId));
        var used = reader.GetInt64(reader.GetOrdinal(linksColumn.Used));

        var maxUsages = reader.IsDBNull(reader.GetOrdinal(linksColumn.MaxUsages))
            ? (long?)null
            : reader.GetInt64(reader.GetOrdinal(linksColumn.MaxUsages));

        var isDeactivated = reader.GetBoolean(reader.GetOrdinal(linksColumn.IsDeactivated));

        var expiresAt = reader.IsDBNull(reader.GetOrdinal(linksColumn.ExpiresAt))
            ? (DateTime?)null
            : reader.GetDateTime(reader.GetOrdinal(linksColumn.ExpiresAt));

        return new BaseLink(link, userId, used, maxUsages, isDeactivated, expiresAt);
    }

    public async Task<List<ILink>> GetLinks(long userId)
    {
        await using var connection = await Connection();

        await using var command = new MySqlCommand(queries[QueryName.GetLinks], connection);
        command.Parameters.AddWithValue($"@{linksColumn.UserId}", userId);

        await using var reader = await command.ExecuteReaderAsync(CancellationToken);
        var links = new List<ILink>();

        while (await reader.ReadAsync(CancellationToken))
        {
            var link = reader.GetString(reader.GetOrdinal(linksColumn.Link));
            var used = reader.GetInt64(reader.GetOrdinal(linksColumn.Used));

            var maxUsages = reader.IsDBNull(reader.GetOrdinal(linksColumn.MaxUsages))
                ? (long?)null
                : reader.GetInt64(reader.GetOrdinal(linksColumn.MaxUsages));

            var isDeactivated = reader.GetBoolean(reader.GetOrdinal(linksColumn.IsDeactivated));

            var expiresAt = reader.IsDBNull(reader.GetOrdinal(linksColumn.ExpiresAt))
                ? (DateTime?)null
                : reader.GetDateTime(reader.GetOrdinal(linksColumn.ExpiresAt));

            links.Add(new BaseLink(link, userId, used, maxUsages, isDeactivated, expiresAt));
        }

        if (links.Count == 0)
        {
            throw new LinksNotFoundException();
        }

        return links;
    }

    public async Task<List<ITransaction>> GetTransactions(long userId)
    {
        await using var connection = await Connection();

        await using var command = new MySqlCommand(queries[QueryName.GetTransactions]);
        command.Parameters.AddWithValue($"{transactionsColumn.UserId}", userId);

        await using var reader = await command.ExecuteReaderAsync(CancellationToken);
        var transactions = new List<ITransaction>();

        while (await reader.ReadAsync(CancellationToken))
        {
            var id = reader.GetInt64(reader.GetOrdinal(transactionsColumn.Id));
            var type = Enum.Parse<TransactionType>(reader.GetString(reader.GetOrdinal(transactionsColumn.Type)), true);
            var amount = reader.GetInt64(reader.GetOrdinal(transactionsColumn.Amount));

            var currency = Enum.Parse<CurrencyName>(
                reader.GetString(reader.GetOrdinal(transactionsColumn.Currency)),
                true
            );

            var createdAt = reader.GetDateTime(reader.GetOrdinal(transactionsColumn.CreatedAt));

            transactions.Add(new BaseTransaction(id, userId, type, amount, currency, createdAt));
        }

        if (transactions.Count == 0)
        {
            throw new TransactionsNotFoundException(userId);
        }

        return transactions;
    }
    
    public async Task<IFilter> GetFilter(long userId)
    {
        await using var connection = await Connection();

        await using var command = new MySqlCommand(queries[QueryName.GetFilter], connection);
        command.Parameters.AddWithValue($"@{filtersColumn.UserId}", userId);

        await using var reader = await command.ExecuteReaderAsync(CancellationToken);

        if (!await reader.ReadAsync(CancellationToken))
        {
            throw new FilterNotFoundException();
        }
        
        var spam = reader.GetBoolean(reader.GetOrdinal(filtersColumn.Spam));
        var terrorism = reader.GetBoolean(reader.GetOrdinal(filtersColumn.Terrorism));
        var drugs = reader.GetBoolean(reader.GetOrdinal(filtersColumn.Drugs));
        var violence = reader.GetBoolean(reader.GetOrdinal(filtersColumn.Violence));
        var pornography = reader.GetBoolean(reader.GetOrdinal(filtersColumn.Pornography));

        return new BaseFilter(userId, spam, terrorism, drugs, violence, pornography);
    }

    private async Task<MySqlConnection> Connection()
    {
        return (MySqlConnection)await dataSource.OpenConnectionAsync(CancellationToken);
    }
}
