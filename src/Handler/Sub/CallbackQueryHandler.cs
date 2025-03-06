using System.Security.Cryptography;
using AskMeNowBot.Command;
using AskMeNowBot.Command.Standard;
using AskMeNowBot.Database;
using AskMeNowBot.Localization;
using AskMeNowBot.User;
using AskMeNowBot.Utils;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace AskMeNowBot.Handler.Sub;

public class CallbackQueryHandler(StartCommand start, IDatabase database, SendCommand send, Encryption encryption, Locale locale) : ISubUpdateHandler
{
	public async Task HandleUpdate(ITelegramBotClient botClient, Update update, string? args, CancellationToken cancellationToken)
	{
		var callbackQuery = update.CallbackQuery!;

		await botClient.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: cancellationToken);
		
		var parts = callbackQuery.Data!.Split(":", 2, StringSplitOptions.RemoveEmptyEntries);
		var messageId = callbackQuery.Message!.MessageId;

		var from = callbackQuery.From;
		var sender = new Sender(from.Id, from.LanguageCode);
		var senderId = sender.Id;

		try
		{
			switch (parts[0])
			{
				case "language":
				{
					var startArgs = parts[1].Split("@", 2, StringSplitOptions.RemoveEmptyEntries);

					start.MessageId = messageId;

					await database.AddUser(new BaseUser(senderId, startArgs[0]));
					await StartCommand(startArgs.Length > 1 ? startArgs[1] : null);
					break;
				}
				
				case "cancel":
				{
					start.MessageId = messageId;

					await database.RemoveExpectation(senderId);
					await StartCommand();
					break;
				}
				
				case "respond":
				{
					var encrypted = parts[1];
					var recipient = long.Parse(encryption.Decrypt(encrypted));
					var command = new BaseCommand(send.Name, recipient.ToString(), sender);

					send.IsResponse = true;

					await send.ExecuteAsync(botClient, command, cancellationToken);
					await botClient.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: cancellationToken);
					break;
				}
				
				case "ban":
				{
					var encrypted = parts[1];
					var recipient = senderId;
					senderId = long.Parse(encryption.Decrypt(encrypted));
					var keyboard = new InlineKeyboardMarkup(new[]
					{
						InlineKeyboardButton.WithCallbackData(await locale.Get(recipient, "Unban"),
							$"unban:{encryption.Encrypt(senderId.ToString())}")
					});

					await database.AddBan(senderId, recipient);
					await botClient.EditMessageReplyMarkup(

						recipient,
						messageId,
						replyMarkup: keyboard,
						cancellationToken: cancellationToken
					);
					break;
				}
				
				case "unban":
				{
					var encrypted = parts[1];
					var recipient = senderId;
					senderId = long.Parse(encryption.Decrypt(encrypted));
					var keyboard = new InlineKeyboardMarkup(

						InlineKeyboardButton.WithCallbackData(

							await locale.Get(recipient, "Respond"),
							$"respond:{encryption.Encrypt(senderId.ToString())}"
						),
						InlineKeyboardButton.WithCallbackData(

							await locale.Get(recipient, "Ban"),
							$"ban:{encryption.Encrypt(senderId.ToString())}"
						)
					);

					await database.RemoveBan(senderId, recipient);
					await botClient.EditMessageReplyMarkup(

						recipient,
						messageId,
						replyMarkup: keyboard,
						cancellationToken: cancellationToken
					);
					break;
				}
			}
		}
		catch (CryptographicException)
		{
			await botClient.SendMessage(
						
				senderId,
				await locale.Get(senderId, "DecryptionFailed"),
				ParseMode.MarkdownV2,
				cancellationToken: cancellationToken
			);
			throw;
		}
		return;

		async Task StartCommand(string? startArgs = null)
		{
			await start.ExecuteAsync(botClient, new BaseCommand(start.Name, startArgs , sender), cancellationToken);
		}
	}
}
