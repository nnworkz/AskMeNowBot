using AskMeNowBot.Command;
using AskMeNowBot.Command.Standard;
using AskMeNowBot.Database;
using AskMeNowBot.User;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace AskMeNowBot.Handler.Sub;

public class WelcomeHandler(IProvider provider, CallbackQueryHandler callback, ChangeLanguageCommand language) : ISubUpdateHandler
{
	public async Task HandleUpdate(ITelegramBotClient botClient, Update update, string? args, CancellationToken cancellationToken)
	{
		var callbackQuery = update.CallbackQuery;
		var from = update.Message?.From ?? callbackQuery!.From;
		var sender = new Sender(from.Id, from.LanguageCode);
        
		if (!await provider.IsRegistered(sender.Id))
		{
			if (callbackQuery != null)
			{
				await callback.HandleUpdate(botClient, update, null, cancellationToken);
				return;
			}

			language.CanCancel = false;
			await language.ExecuteAsync(botClient, new BaseCommand(language.Name, args, sender), cancellationToken);
		}
	}
}
