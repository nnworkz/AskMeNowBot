using AskMeNowBot.Database;
using AskMeNowBot.Exceptions;
using AskMeNowBot.Localization;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace AskMeNowBot.Command.Standard;

public class SendCommand(Locale locale, IProvider provider, IDatabase database) : ICommand
{
	public bool IsResponse { get; set; }
	
	public string Name => "send";
	public string[] Aliases => [];

	public async Task ExecuteAsync(ITelegramBotClient botClient, BaseCommand command, CancellationToken cancellationToken)
	{
		var senderId = command.Sender.Id;

		if (!long.TryParse(command.Args, out var recipient))
		{
			throw new InvalidCommandArgsException();
		}
		
		if (senderId == recipient)
		{
			await botClient.SendMessage(
				
				senderId,
				await locale.Get(senderId, "CannotUseOwnLink"),
				ParseMode.MarkdownV2,
				cancellationToken: cancellationToken
			);
			return;
		}

		if (await provider.IsBanned(senderId, recipient))
		{
			await botClient.SendMessage(
				
				senderId,
				await locale.Get(senderId, "IsBanned"),
				ParseMode.MarkdownV2,
				cancellationToken: cancellationToken
			);
			return;
		}
		
		var keyboard = new InlineKeyboardMarkup(new []
		{
			InlineKeyboardButton.WithCallbackData(await locale.Get(senderId, "Cancel"), "cancel")
		});

		await database.AddExpectation(senderId, recipient, IsResponse);
		await botClient.SendMessage(
			
			senderId,
			await locale.Get(senderId, IsResponse ? "SubmitYourAnswer" : "SubmitYourQuestion"),
			ParseMode.MarkdownV2,
			replyMarkup: keyboard,
			cancellationToken: cancellationToken
		);
	}
}
