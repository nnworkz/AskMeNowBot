using AskMeNowBot.Localization;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace AskMeNowBot.Command.Standard;

public class ChangeLanguageCommand(Locale locale) : ICommand
{
	public bool CanCancel { get; set; } = true;
	
	public string Name => "changelanguage";
	public string[] Aliases => ["changelang"];

	public async Task ExecuteAsync(ITelegramBotClient botClient, BaseCommand command, CancellationToken cancellationToken)
	{
		var args = command.Args;
		var startArgs = string.IsNullOrEmpty(args) ? "" : $"@{args}";
		var (senderId, languageCode) = command.Sender;
		var keyboard = new InlineKeyboardMarkup([
			[
				InlineKeyboardButton.WithCallbackData("🇺🇸  English", $"language:en{startArgs}"),
				InlineKeyboardButton.WithCallbackData("🇷🇺  Русский", $"language:ru{startArgs}")
			],
			CanCancel ? [InlineKeyboardButton.WithCallbackData(await locale.Get(senderId, "Cancel"), "cancel")] : []
		]);

		await botClient.SendMessage(

			senderId,
			await locale.Get(senderId, "SelectLanguage", languageCode),
			ParseMode.MarkdownV2,
			replyMarkup: keyboard,
			cancellationToken: cancellationToken
		);
	}
}
