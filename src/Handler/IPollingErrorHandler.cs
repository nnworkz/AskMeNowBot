using Telegram.Bot;

namespace AskMeNowBot.Handler;

public interface IPollingErrorHandler
{
	Task HandlePollingError(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken);
}
