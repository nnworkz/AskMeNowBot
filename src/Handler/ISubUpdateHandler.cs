using Telegram.Bot;
using Telegram.Bot.Types;

namespace AskMeNowBot.Handler;

public interface ISubUpdateHandler
{
	Task HandleUpdate(ITelegramBotClient botClient, Update update, string? args, CancellationToken cancellationToken);
}
