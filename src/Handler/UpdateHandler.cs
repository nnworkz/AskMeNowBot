using AskMeNowBot.Database;
using AskMeNowBot.Exceptions;
using AskMeNowBot.Handler.Sub;
using AskMeNowBot.User;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace AskMeNowBot.Handler;

public class UpdateHandler(IProvider provider, WelcomeHandler welcome, MessageHandler message, CallbackQueryHandler callback) : IUpdateHandler
{
	public async Task HandleUpdate(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
	{
		var callbackQuery = update.CallbackQuery;
		var from = update.Message?.From ?? callbackQuery!.From;
		var sender = new Sender(from.Id, from.LanguageCode);

		if (!await provider.IsRegistered(sender.Id))
		{
			var text = (update.Message ?? callbackQuery!.Message!).Text!;
			string? args = null;
			
			if (text.StartsWith("/start"))
			{
				var parts = text.Split(" ", 2, StringSplitOptions.RemoveEmptyEntries);
				
				if (parts.Length > 1)
				{
					args = parts[1];
				}
			}
			
			await welcome.HandleUpdate(botClient, update, args, cancellationToken);
			return;
		}

		switch (update.Type)
		{
			case UpdateType.Message:
			{
				await message.HandleUpdate(botClient, update, null, cancellationToken);
				break;
			}
			
			case UpdateType.CallbackQuery:
			{
				await callback.HandleUpdate(botClient, update, null, cancellationToken);
				break;
			}

			default:
			{
				throw new InvalidUpdateTypeException(update.Type.GetType().Name);
			}
		}
	}
}
