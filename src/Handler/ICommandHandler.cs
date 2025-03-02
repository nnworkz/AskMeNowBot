using AskMeNowBot.Command;
using Telegram.Bot;

namespace AskMeNowBot.Handler;

public interface ICommandHandler
{
	Task HandleCommand(ITelegramBotClient botClient, BaseCommand command, CancellationToken cancellationToken);
}
