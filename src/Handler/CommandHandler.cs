using AskMeNowBot.Command;
using AskMeNowBot.Localization;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace AskMeNowBot.Handler;

public class CommandHandler(CommandRegistry registry, Locale locale) : ICommandHandler
{
	public async Task HandleCommand(ITelegramBotClient botClient, BaseCommand command, CancellationToken cancellationToken)
	{
		var cmd = registry.GetCommand(command.Name);
		var senderId = command.Sender.Id;

		if (cmd != null)
		{
			await cmd.ExecuteAsync(botClient, command, cancellationToken);
			return;
		}

		await botClient.SendMessage(
			
			senderId,
			await locale.Get(senderId, "CommandNotFound"),
			ParseMode.MarkdownV2,
			cancellationToken: cancellationToken
		);
	}
}
