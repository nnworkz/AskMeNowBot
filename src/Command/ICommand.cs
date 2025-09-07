using Telegram.Bot;

namespace AskMeNowBot.Command;

public interface ICommand
{
	string Name { get; }
	string[] Aliases { get; }

	Task ExecuteAsync(ITelegramBotClient botClient, ICommandContext context, CancellationToken cancellationToken);
}
