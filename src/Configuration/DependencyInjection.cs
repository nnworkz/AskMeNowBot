using AskMeNowBot.Command;
using AskMeNowBot.Command.Standard;
using AskMeNowBot.Database;
using AskMeNowBot.Database.MySql;
using AskMeNowBot.Database.Npgsql;
using AskMeNowBot.Exceptions;
using AskMeNowBot.Handler;
using AskMeNowBot.Handler.Sub;
using AskMeNowBot.Localization;
using AskMeNowBot.Utils;
using Microsoft.Extensions.DependencyInjection;
using MySqlConnector;
using Npgsql;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using Telegram.Bot;

namespace AskMeNowBot.Configuration;

public static class DependencyInjection
{
	public static async Task<ServiceProvider> Configure()
	{
		var writeTo = new LoggerConfiguration().WriteTo;
		var logger = writeTo.Console(LogEventLevel.Information, theme: AnsiConsoleTheme.Sixteen).CreateLogger();

		try
		{
			var collection = new ServiceCollection();
			var config = await Config.Read();
			
			config.IsValid();
			
			collection.AddLogging(builder => builder.AddSerilog(logger, true));
			collection.AddSingleton<CancellationTokenSource>();
		
			ConfigureDatabase(collection, config);

			collection.AddSingleton<CommandRegistry>();
		
			collection.AddSingleton<ICommand, StartCommand>();
			collection.AddSingleton<ICommand, SendCommand>();
			collection.AddSingleton<ICommand, ChangeLanguageCommand>();
		
			collection.AddSingleton<StartCommand>();
			collection.AddSingleton<SendCommand>();
			collection.AddSingleton<ChangeLanguageCommand>();
		
			collection.AddSingleton<ITelegramBotClient>(new TelegramBotClient(config.Token));

			collection.AddTransient<UpdateHandler>();
			collection.AddTransient<PollingErrorHandler>();
			collection.AddTransient<CommandHandler>();
			collection.AddTransient<WelcomeHandler>();
			collection.AddTransient<MessageHandler>();
			collection.AddTransient<CallbackQueryHandler>();
		
			collection.AddSingleton(config);
		
			collection.AddSingleton<LinkGenerator>();
			collection.AddTransient<Locale>();
			collection.AddTransient<Encryption>();

			return collection.BuildServiceProvider();
		}
		catch (Exception exception)
		{
			logger.Error("Unhandled exception\n{exception}", exception);
			throw;
		}
	}
	
	private static void ConfigureDatabase(IServiceCollection collection, Config config)
	{
		switch (config.Database.ToLowerInvariant())
		{
			case var db when db.Equals(DatabaseTypes.MySql, StringComparison.OrdinalIgnoreCase):
			{
				var dataSource = new MySqlDataSource(config.ConnectionStringMySql);
				collection.AddSingleton(dataSource);
				
				collection.AddSingleton<IDatabase, MySqlDatabase>();
				collection.AddSingleton<IProvider, MySqlProvider>();
				collection.AddSingleton<IQueries, MySqlQueries>();
				
				break;
			}
			
			case var db when db.Equals(DatabaseTypes.Npgsql, StringComparison.OrdinalIgnoreCase):
			{
				var dataSource = NpgsqlDataSource.Create(config.ConnectionStringNpgsql);
				collection.AddSingleton(dataSource);
				
				collection.AddSingleton<IDatabase, NpgsqlDatabase>();
				collection.AddSingleton<IProvider, NpgsqlProvider>();
				collection.AddSingleton<IQueries, NpgsqlQueries>();
				
				break;
			}

			default:
			{
				throw new InvalidDatabaseException(config.Database);
			}
		}
	}
}
