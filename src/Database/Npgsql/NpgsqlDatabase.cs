using AskMeNowBot.User;
using Npgsql;

namespace AskMeNowBot.Database.Npgsql;

public class NpgsqlDatabase(IQueries queries, CancellationTokenSource cts, NpgsqlDataSource dataSource) : IDatabase
{
	private CancellationToken CancellationToken { get; } = cts.Token;

	public async Task InitAsync()
	{
		string[] initQueries =
		[
			queries.SelectDatabase,
			queries.InitUsers,
			queries.InitExpectations,
			queries.InitBanList
		];

		await Task.WhenAll(initQueries.Select(async query =>
		{
			var connection = await Connection();
			
			await using var command = new NpgsqlCommand(query, connection);
			await command.ExecuteNonQueryAsync(CancellationToken);
		}));
	}

	public async Task AddUser(BaseUser user)
	{
		await using var connection = await Connection();
		await using var transaction = await connection.BeginTransactionAsync(CancellationToken);

		try
		{
			await using var command = new NpgsqlCommand(queries.AddUser, connection);
			var parameters = command.Parameters;

			parameters.AddWithValue("@id", user.Id);
			parameters.AddWithValue("@language", user.Language);

			await command.ExecuteNonQueryAsync(CancellationToken);
			await transaction.CommitAsync(CancellationToken);
		}
		catch
		{
			await transaction.RollbackAsync(CancellationToken);
			throw;
		}
	}

	public async Task AddExpectation(long sender, long recipient, bool isResponse)
	{
		await using var connection = await Connection();
		await using var transaction = await connection.BeginTransactionAsync(CancellationToken);

		try
		{
			await using var command = new NpgsqlCommand(queries.AddExpectation, connection);
			var parameters = command.Parameters;

			parameters.AddWithValue("@sender", sender);
			parameters.AddWithValue("@recipient", recipient);
			parameters.AddWithValue("@is_response", isResponse);

			await command.ExecuteNonQueryAsync(CancellationToken);
			await transaction.CommitAsync(CancellationToken);
		}
		catch
		{
			await transaction.RollbackAsync(CancellationToken);
			throw;
		}
	}
	
	public async Task AddBan(long sender, long recipient)
	{
		await using var connection = await Connection();
		await using var transaction = await connection.BeginTransactionAsync(CancellationToken);

		try
		{
			await using var command = new NpgsqlCommand(queries.AddBan, connection);
			var parameters = command.Parameters;

			parameters.AddWithValue("@sender", sender);
			parameters.AddWithValue("@recipient", recipient);

			await command.ExecuteNonQueryAsync(CancellationToken);
			await transaction.CommitAsync(CancellationToken);
		}
		catch
		{
			await transaction.RollbackAsync(CancellationToken);
			throw;
		}
	}

	public async Task RemoveExpectation(long sender)
	{
		await using var connection = await Connection();
		await using var transaction = await connection.BeginTransactionAsync(CancellationToken);

		try
		{
			await using var command = new NpgsqlCommand(queries.RemoveExpectation, connection);
			command.Parameters.AddWithValue("@sender", sender);

			await command.ExecuteNonQueryAsync(CancellationToken);
			await transaction.CommitAsync(CancellationToken);
		}
		catch
		{
			await transaction.RollbackAsync(CancellationToken);
			throw;
		}
	}

	public async Task RemoveBan(long sender, long recipient)
	{
		await using var connection = await Connection();
		await using var transaction = await connection.BeginTransactionAsync(CancellationToken);

		try
		{
			await using var command = new NpgsqlCommand(queries.RemoveBan, connection);
			var parameters = command.Parameters;

			parameters.AddWithValue("@sender", sender);
			parameters.AddWithValue("@recipient", recipient);

			await command.ExecuteNonQueryAsync(CancellationToken);
			await transaction.CommitAsync(CancellationToken);
		}
		catch
		{
			await transaction.RollbackAsync(CancellationToken);
			throw;
		}
	}

	private async Task<NpgsqlConnection> Connection() => await dataSource.OpenConnectionAsync(CancellationToken);
}
