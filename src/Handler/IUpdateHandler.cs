using Telegram.Bot;
using Telegram.Bot.Types;

namespace AskMeNowBot.Handler;

public interface IUpdateHandler
{
	Task HandleUpdate(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken);
}
