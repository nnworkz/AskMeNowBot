using AskMeNowBot.Exceptions;
using MySqlConnector;

namespace AskMeNowBot.Database.MySql;

public class MySqlProvider(IQueries queries, CancellationTokenSource cts, MySqlDataSource dataSource) : IProvider
{
	private CancellationToken CancellationToken { get; } = cts.Token;
	
	public async Task<bool> IsRegistered(long id)
	{
		await using var connection = await Connection();
		
		await using var command = new MySqlCommand(queries.IsRegistered, connection);
		command.Parameters.AddWithValue("@id", id);
		
		var result = await command.ExecuteScalarAsync(CancellationToken);
		return result != null && (bool) result;
	}

	public async Task<bool> InExpectation(long sender)
	{
		await using var connection = await Connection();
		
		await using var command = new MySqlCommand(queries.InExpectation, connection);
		command.Parameters.AddWithValue("@sender", sender);

		var result = await command.ExecuteScalarAsync(CancellationToken);
		return result != null && (bool) result;
	}

	public async Task<bool> IsResponse(long sender)
	{
		await using var connection = await Connection();
		
		await using var command = new MySqlCommand(queries.IsResponse, connection);
		command.Parameters.AddWithValue("@sender", sender);

		var result = await command.ExecuteScalarAsync(CancellationToken);
		return result != null && (bool) result;
	}

	public async Task<bool> IsBanned(long sender, long recipient)
	{
		await using var connection = await Connection();
		
		await using var command = new MySqlCommand(queries.IsBanned, connection);
		var parameters = command.Parameters;

		parameters.AddWithValue("@sender", sender);
		parameters.AddWithValue("@recipient", recipient);

		var result = await command.ExecuteScalarAsync(CancellationToken);
		return result != null && (bool) result;
	}
	
	public async Task<string> GetLanguage(long id)
	{
		await using var connection = await Connection();
		
		await using var command = new MySqlCommand(queries.GetLanguage, connection);
		command.Parameters.AddWithValue("@id", id);

		await using var reader = await command.ExecuteReaderAsync(CancellationToken);
		
		if (await reader.ReadAsync(CancellationToken))
		{
			return reader.GetString(reader.GetOrdinal("language"));
		}

		throw new DataNotFoundException();
	}

	public async Task<long> GetRecipient(long sender)
	{
		await using var connection = await Connection();
		
		await using var command = new MySqlCommand(queries.GetRecipient, connection);
		command.Parameters.AddWithValue("@sender", sender);

		await using var reader = await command.ExecuteReaderAsync(CancellationToken);
		
		if (await reader.ReadAsync(CancellationToken))
		{
			return reader.GetInt64(reader.GetOrdinal("recipient"));
		}
		
		throw new DataNotFoundException();
	}

	private async Task<MySqlConnection> Connection() => await dataSource.OpenConnectionAsync(CancellationToken);
}
