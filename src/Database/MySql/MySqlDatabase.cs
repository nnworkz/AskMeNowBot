using System.Data.Common;

using AskMeNowBot.Ban;
using AskMeNowBot.Database.Column;
using AskMeNowBot.Filter;
using AskMeNowBot.Link;
using AskMeNowBot.Subscription;
using AskMeNowBot.Transaction;
using AskMeNowBot.User;
using AskMeNowBot.Wait;

using Microsoft.Extensions.Hosting;

using MySqlConnector;

namespace AskMeNowBot.Database.MySql;

public class MySqlDatabase(
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
) : IDatabase
{
    private CancellationToken CancellationToken { get; } = lifetime.ApplicationStopping;

    public async Task InitAsync()
    {
        await using var connection = await Connection();
        await using var dbTransaction = await connection.BeginTransactionAsync(CancellationToken);

        try
        {
            QueryName[] initQueries =
            [
                QueryName.InitUsers,
                QueryName.InitSubscriptions,
                QueryName.InitWaits,
                QueryName.InitBans,
                QueryName.InitLinks,
                QueryName.InitTransactions,
                QueryName.InitFilters
            ];

            foreach (var query in initQueries)
            {
                await using var command = new MySqlCommand(queries[query], connection, dbTransaction);
                await command.ExecuteNonQueryAsync(CancellationToken);
            }

            await dbTransaction.CommitAsync(CancellationToken).ConfigureAwait(false);
        }
        catch
        {
            await dbTransaction.RollbackAsync(CancellationToken);

            throw;
        }
    }

    public async Task AddUser(IUser user)
    {
        await using var connection = await Connection();
        await using var dbTransaction = await connection.BeginTransactionAsync(CancellationToken);

        try
        {
            await using var command = new MySqlCommand(queries[QueryName.AddUser], connection, dbTransaction);
            var parameters = command.Parameters;

            parameters.AddWithValue($"@{usersColumn.Id}", user.Id);
            parameters.AddWithValue($"@{usersColumn.Language}", user.Language.ToString());
            parameters.AddWithValue($"@{usersColumn.Role}", user.Role.ToString());
            parameters.AddWithValue($"@{usersColumn.Messages}", user.Messages);
            parameters.AddWithValue($"@{usersColumn.MessagesToday}", user.MessagesToday);
            parameters.AddWithValue($"@{usersColumn.LastResetAt}", user.LastResetAt);
            parameters.AddWithValue($"@{usersColumn.LastMessageAt}", user.LastMessageAt ?? (object)DBNull.Value);
            parameters.AddWithValue($"@{usersColumn.RegisteredAt}", user.RegisteredAt);

            await command.ExecuteNonQueryAsync(CancellationToken);
            await dbTransaction.CommitAsync(CancellationToken).ConfigureAwait(false);
        }
        catch
        {
            await dbTransaction.RollbackAsync(CancellationToken);

            throw;
        }
    }

    public async Task RemoveUser(IUser user)
    {
        await using var connection = await Connection();
        await using var dbTransaction = await connection.BeginTransactionAsync(CancellationToken);

        try
        {
            await using var command = new MySqlCommand(queries[QueryName.RemoveUser], connection, dbTransaction);
            var parameters = command.Parameters;

            parameters.AddWithValue($"@{usersColumn.Id}", user.Id);

            await command.ExecuteNonQueryAsync(CancellationToken);
            await dbTransaction.CommitAsync(CancellationToken).ConfigureAwait(false);
        }
        catch
        {
            await dbTransaction.RollbackAsync(CancellationToken);

            throw;
        }
    }

    public async Task AddSubscriber(ISubscriber subscriber)
    {
        await using var connection = await Connection();
        await using var dbTransaction = await connection.BeginTransactionAsync(CancellationToken);

        try
        {
            await using var command = new MySqlCommand(queries[QueryName.AddSubscriber], connection, dbTransaction);
            var parameters = command.Parameters;

            parameters.AddWithValue($"@{subscriptionsColumn.UserId}", subscriber.UserId);
            parameters.AddWithValue($"@{subscriptionsColumn.StartedAt}", subscriber.StartedAt);
            parameters.AddWithValue($"@{subscriptionsColumn.EndsAt}", subscriber.EndsAt);

            await command.ExecuteNonQueryAsync(CancellationToken);
            await dbTransaction.CommitAsync(CancellationToken).ConfigureAwait(false);
        }
        catch
        {
            await dbTransaction.RollbackAsync(CancellationToken);

            throw;
        }
    }

    public async Task RemoveSubscriber(ISubscriber subscriber)
    {
        await using var connection = await Connection();
        await using var dbTransaction = await connection.BeginTransactionAsync(CancellationToken);

        try
        {
            await using var command = new MySqlCommand(queries[QueryName.RemoveSubscriber], connection, dbTransaction);
            var parameters = command.Parameters;

            parameters.AddWithValue($"@{subscriptionsColumn.UserId}", subscriber.UserId);

            await command.ExecuteNonQueryAsync(CancellationToken);
            await dbTransaction.CommitAsync(CancellationToken).ConfigureAwait(false);
        }
        catch
        {
            await dbTransaction.RollbackAsync(CancellationToken);

            throw;
        }
    }

    public async Task AddWait(IWait wait)
    {
        await using var connection = await Connection();
        await using var dbTransaction = await connection.BeginTransactionAsync(CancellationToken);

        try
        {
            await using var command = new MySqlCommand(queries[QueryName.AddWait], connection, dbTransaction);
            var parameters = command.Parameters;

            parameters.AddWithValue($"@{waitsColumn.SenderId}", wait.SenderId);
            parameters.AddWithValue($"@{waitsColumn.RecipientId}", wait.RecipientId ?? (object)DBNull.Value);
            parameters.AddWithValue($"@{waitsColumn.Type}", wait.Type.ToString());
            parameters.AddWithValue($"@{waitsColumn.IsResponse}", wait.IsResponse ?? (object)DBNull.Value);
            parameters.AddWithValue($"@{waitsColumn.Link}", wait.Link ?? (object)DBNull.Value);

            await command.ExecuteNonQueryAsync(CancellationToken);
            await dbTransaction.CommitAsync(CancellationToken);
        }
        catch
        {
            await dbTransaction.RollbackAsync(CancellationToken);

            throw;
        }
    }

    public async Task RemoveWait(IWait wait)
    {
        await using var connection = await Connection();
        await using var dbTransaction = await connection.BeginTransactionAsync(CancellationToken);

        try
        {
            await using var command = new MySqlCommand(queries[QueryName.RemoveWait], connection, dbTransaction);
            command.Parameters.AddWithValue($"@{waitsColumn.SenderId}", wait.SenderId);

            await command.ExecuteNonQueryAsync(CancellationToken);
            await dbTransaction.CommitAsync(CancellationToken);
        }
        catch
        {
            await dbTransaction.RollbackAsync(CancellationToken);

            throw;
        }
    }

    public async Task AddBan(IBan ban)
    {
        await using var connection = await Connection();
        await using var dbTransaction = await connection.BeginTransactionAsync(CancellationToken);

        try
        {
            await using var command = new MySqlCommand(queries[QueryName.AddBan], connection, dbTransaction);
            var parameters = command.Parameters;

            parameters.AddWithValue($"@{bansColumn.SenderId}", ban.SenderId);
            parameters.AddWithValue($"@{bansColumn.RecipientId}", ban.RecipientId);
            parameters.AddWithValue($"@{bansColumn.Reason}", ban.Reason ?? (object)DBNull.Value);
            parameters.AddWithValue($"@{bansColumn.BannedAt}", ban.BannedAt);

            await command.ExecuteNonQueryAsync(CancellationToken);
            await dbTransaction.CommitAsync(CancellationToken);
        }
        catch
        {
            await dbTransaction.RollbackAsync(CancellationToken);

            throw;
        }
    }

    public async Task RemoveBan(IBan ban)
    {
        await using var connection = await Connection();
        await using var dbTransaction = await connection.BeginTransactionAsync(CancellationToken);

        try
        {
            await using var command = new MySqlCommand(queries[QueryName.RemoveBan], connection, dbTransaction);
            var parameters = command.Parameters;

            parameters.AddWithValue($"@{bansColumn.SenderId}", ban.SenderId);
            parameters.AddWithValue($"@{bansColumn.RecipientId}", ban.RecipientId);

            await command.ExecuteNonQueryAsync(CancellationToken);
            await dbTransaction.CommitAsync(CancellationToken);
        }
        catch
        {
            await dbTransaction.RollbackAsync(CancellationToken);

            throw;
        }
    }

    public async Task AddLink(ILink link)
    {
        await using var connection = await Connection();
        await using var dbTransaction = await connection.BeginTransactionAsync(CancellationToken);

        try
        {
            await using var command = new MySqlCommand(queries[QueryName.AddLink], connection, dbTransaction);
            var parameters = command.Parameters;

            parameters.AddWithValue($"@{linksColumn.Link}", link.Link);
            parameters.AddWithValue($"@{linksColumn.UserId}", link.UserId);
            parameters.AddWithValue($"@{linksColumn.Used}", link.Used);
            parameters.AddWithValue($"@{linksColumn.MaxUsages}", link.MaxUsages ?? (object)DBNull.Value);
            parameters.AddWithValue($"@{linksColumn.IsDeactivated}", link.IsDeactivated);
            parameters.AddWithValue($"@{linksColumn.ExpiresAt}", link.ExpiresAt ?? (object)DBNull.Value);

            await command.ExecuteNonQueryAsync(CancellationToken);
            await dbTransaction.CommitAsync(CancellationToken);
        }
        catch
        {
            await dbTransaction.RollbackAsync(CancellationToken);

            throw;
        }
    }

    public async Task RemoveLink(ILink link)
    {
        await using var connection = await Connection();
        await using var dbTransaction = await connection.BeginTransactionAsync(CancellationToken);

        try
        {
            await using var command = new MySqlCommand(queries[QueryName.RemoveLink], connection, dbTransaction);
            command.Parameters.AddWithValue($"@{linksColumn.Link}", link.Link);

            await command.ExecuteNonQueryAsync(CancellationToken);
            await dbTransaction.CommitAsync(CancellationToken);
        }
        catch
        {
            await dbTransaction.RollbackAsync(CancellationToken);

            throw;
        }
    }

    public async Task AddTransaction(ITransaction transaction)
    {
        await using var connection = await Connection();
        await using var dbTransaction = await connection.BeginTransactionAsync(CancellationToken);

        try
        {
            await using var command = new MySqlCommand(queries[QueryName.AddTransaction], connection, dbTransaction);
            var parameters = command.Parameters;

            parameters.AddWithValue($"@{transactionsColumn.UserId}", transaction.UserId);
            parameters.AddWithValue($"@{transactionsColumn.Type}", transaction.Type.ToString());
            parameters.AddWithValue($"@{transactionsColumn.Amount}", transaction.Amount);
            parameters.AddWithValue($"@{transactionsColumn.Currency}", transaction.Currency.ToString());
            parameters.AddWithValue($"@{transactionsColumn.CreatedAt}", transaction.CreatedAt);

            await command.ExecuteNonQueryAsync(CancellationToken);
            await dbTransaction.CommitAsync(CancellationToken);
        }
        catch
        {
            await dbTransaction.RollbackAsync(CancellationToken);

            throw;
        }
    }
    
    public async Task AddFilter(IFilter filter)
    {
        await using var connection = await Connection();
        await using var dbTransaction = await connection.BeginTransactionAsync(CancellationToken);

        try
        {
            await using var command = new MySqlCommand(queries[QueryName.AddFilter], connection);
            var parameters = command.Parameters;

            parameters.AddWithValue($"@{filtersColumn.UserId}", filter.UserId);
            parameters.AddWithValue($"@{filtersColumn.Spam}", filter.Spam);
            parameters.AddWithValue($"@{filtersColumn.Terrorism}", filter.Terrorism);
            parameters.AddWithValue($"@{filtersColumn.Drugs}", filter.Drugs);
            parameters.AddWithValue($"@{filtersColumn.Violence}", filter.Violence);
            parameters.AddWithValue($"@{filtersColumn.Pornography}", filter.Pornography);

            await command.ExecuteNonQueryAsync(CancellationToken);
            await dbTransaction.CommitAsync(CancellationToken);
        }
        catch
        {
            await dbTransaction.RollbackAsync(CancellationToken);

            throw;
        }
    }

    public async Task RemoveFilter(IFilter filter)
    {
        await using var connection = await Connection();
        await using var dbTransaction = await connection.BeginTransactionAsync(CancellationToken);

        try
        {
            await using var command = new MySqlCommand(queries[QueryName.RemoveFilter], connection);

            command.Parameters.AddWithValue($"@{filtersColumn.UserId}", filter.UserId);

            await command.ExecuteNonQueryAsync(CancellationToken);
            await dbTransaction.CommitAsync(CancellationToken);
        }
        catch
        {
            await dbTransaction.RollbackAsync(CancellationToken);

            throw;
        }
    }

    private async Task<MySqlConnection> Connection()
    {
        return (MySqlConnection)await dataSource.OpenConnectionAsync(CancellationToken);
    }
}
