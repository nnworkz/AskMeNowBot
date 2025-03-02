using AskMeNowBot.Configuration;
using AskMeNowBot.Localization;
using AskMeNowBot.Utils;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace AskMeNowBot.Command.Standard;

public class StartCommand(SendCommand sendCommand, Locale locale, LinkGenerator linkGenerator, Config config) : ICommand
{
	public int? MessageId { get; set; }

	public string Name => "start";
	public string[] Aliases => [];

	public async Task ExecuteAsync(ITelegramBotClient botClient, BaseCommand command, CancellationToken cancellationToken)
	{
		var senderId = command.Sender.Id;
		
		if (!string.IsNullOrWhiteSpace(command.Args) || long.TryParse(command.Args, out _))
		{
			await sendCommand.ExecuteAsync(botClient, command, cancellationToken);
			return;
		}

		var link = (await locale.Get(senderId, "YourLink")).Replace("{username}", config.BotUsername)
														   .Replace("{id}", senderId.ToString());
		var keyboard = new InlineKeyboardMarkup(new []
		{
			InlineKeyboardButton.WithUrl(await locale.Get(senderId, "ShareLink"), linkGenerator.Get(senderId))
		});

		if (MessageId.HasValue)
		{
			await botClient.EditMessageText(
				
				senderId,
				MessageId.Value,
				link,
				ParseMode.MarkdownV2,
				replyMarkup: keyboard,
				cancellationToken: cancellationToken
			);
			return;
		}

		await botClient.SendMessage(
			
			senderId,
			link,
			ParseMode.MarkdownV2,
			replyMarkup: keyboard,
			cancellationToken: cancellationToken
		);
	}
}
