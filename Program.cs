using System.Diagnostics;
using AskMeNowBot.Command;
using AskMeNowBot.Configuration;
using AskMeNowBot.Database;
using AskMeNowBot.Handler;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;

namespace AskMeNowBot;

public class Program
{
	private static int _isCancelled;
	
	public static async Task Main()
	{
		var stopwatch = Stopwatch.StartNew();

		ILogger<Program>? logger = null;
		CancellationTokenSource? cts = null;
		
		try
		{
			await using var provider = await DependencyInjection.Configure();
			
			logger = provider.GetRequiredService<ILogger<Program>>();
			cts = provider.GetRequiredService<CancellationTokenSource>();
			
			var database = provider.GetRequiredService<IDatabase>();
			await database.InitAsync();
			logger.LogInformation("Database initialized");

			RegisterCommands(provider);
			logger.LogInformation("Commands registered");
			
			StartReceiving(provider, cts.Token);
			logger.LogInformation("Bot started");

			stopwatch.Stop();
			logger.LogInformation("Enabled ({Seconds:F3} sec.)", stopwatch.Elapsed.TotalSeconds);
		}
		catch (Exception exception)
		{
			logger?.LogError("Unhandled exception\n{exception}", exception);
		}
		finally
		{
			await Shutdown(cts ?? new CancellationTokenSource());
			await Log.CloseAndFlushAsync();
		}
	}
	
	private static void RegisterCommands(IServiceProvider provider)
	{
		var commands = provider.GetServices<ICommand>();
		var commandRegistry = provider.GetRequiredService<CommandRegistry>();

		foreach (var command in commands)
		{
			commandRegistry.RegisterCommand(command);
		}
	}
	
	private static void StartReceiving(IServiceProvider provider, CancellationToken cancellationToken)
	{
		var botClient = provider.GetRequiredService<ITelegramBotClient>();
		var updateHandler = provider.GetRequiredService<UpdateHandler>();
		var errorHandler = provider.GetRequiredService<PollingErrorHandler>();

		botClient.StartReceiving(
			
			updateHandler.HandleUpdate,
			errorHandler.HandlePollingError,
			new ReceiverOptions
			{
				AllowedUpdates = [UpdateType.Message, UpdateType.CallbackQuery]
			},
			cancellationToken
		);
	}
	
	private static async Task Shutdown(CancellationTokenSource cts)
	{
		if (Interlocked.Exchange(ref _isCancelled, 1) == 1)
		{
			return;
		}
		
		Console.CancelKeyPress += (_, eventArgs) =>
		{
			eventArgs.Cancel = true;
			cts.Cancel();
		};

		try
		{
			await Task.Delay(Timeout.Infinite, cts.Token);
		}
		catch (TaskCanceledException)
		{
			Log.Information("Shutdown completed");
		}
	}
}
