using Microsoft.Extensions.Logging;
using Telegram.Bot;

namespace AskMeNowBot.Handler;

public class PollingErrorHandler(ILogger<PollingErrorHandler> logger) : IPollingErrorHandler
{
	public Task HandlePollingError(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
	{
		logger.LogError("Unhandled exception\n{exception}", exception.Message);

		return Task.CompletedTask;
	}
}
