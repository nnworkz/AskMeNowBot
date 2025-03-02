using System.Text.Json;
using AskMeNowBot.Exceptions;

namespace AskMeNowBot.Configuration;

public class Config
{
	public string Database                   { get; init; } = string.Empty;
	
	public string ConnectionStringMySql      { get; init; } = string.Empty;
	public string ConnectionStringNpgsql { get; init; } = string.Empty;
	
	public string EncryptionKey              { get; init; } = string.Empty;

	public string BotUsername                { get; init; } = string.Empty;
	public string Token                      { get; init; } = string.Empty;

	public bool IsValid()
	{
		string[] properties =
		[
			Database,
			ConnectionStringMySql,
			ConnectionStringNpgsql,
			EncryptionKey,
			BotUsername,
			Token
		];

		if (properties.Any(string.IsNullOrEmpty))
		{
			throw new ConfigNotInitializedException();
		}

		return true;
	}
	
	public static async Task<Config> Read()
	{
		await using var fileStream = File.OpenRead(@"..\..\..\resources\config.json");
		var config = await JsonSerializer.DeserializeAsync<Config>(fileStream);

		if (config == null)
		{
			throw new InvalidOperationException("Failed to deserialize config");
		}

		return config;
	}
}
