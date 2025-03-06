using System.Text;
using AskMeNowBot.Command;
using AskMeNowBot.Database;
using AskMeNowBot.Localization;
using AskMeNowBot.User;
using AskMeNowBot.Utils;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace AskMeNowBot.Handler.Sub;

public class MessageHandler(Locale locale, IDatabase database, IProvider provider, CommandHandler command, Encryption encryption) : ISubUpdateHandler
{
	public async Task HandleUpdate(ITelegramBotClient botClient, Update update, string? args, CancellationToken cancellationToken)
	{
		var message = update.Message!;
		var from = message.From!;
		var sender = new Sender(from.Id, from.LanguageCode);
		var senderId = sender.Id;

		if (message.Type != MessageType.Text)
		{
			await botClient.SendMessage(
				
				senderId,
				await locale.Get(senderId, "OnlyTextMessage"),
				ParseMode.MarkdownV2,
				cancellationToken: cancellationToken
			);
			return;
		}

		var text = message.Text!;

		if (text.Length > 4000)
		{
			await database.RemoveExpectation(senderId);
			await botClient.SendMessage(
				
				senderId,
				await locale.Get(senderId, "MessageIsTooLong"),
				ParseMode.MarkdownV2,
				cancellationToken: cancellationToken
			);
			return;
		}

		var inExpectation = await provider.InExpectation(senderId);

		if (text.StartsWith('/'))
		{
			if (inExpectation)
			{
				await database.RemoveExpectation(senderId);
			}
			
			var labelSplit = text[1..].Split(" ", 2, StringSplitOptions.RemoveEmptyEntries);
			var name = labelSplit[0];
			args = labelSplit.Length > 1 ? labelSplit[1] : null;

			await command.HandleCommand(botClient, new BaseCommand(name, args, sender), cancellationToken);
			return;
		}

		if (!inExpectation)
		{
			await botClient.SendMessage(
				
				senderId,
				await locale.Get(senderId, "LinkToRecipient"),
				ParseMode.MarkdownV2,
				cancellationToken: cancellationToken
			);
			return;
		}

		var entities = message.Entities;
		var escapedText = TextFormat.EscapeMarkdownV2(new StringBuilder(text));

		if (entities != null)
		{
			var entity = entities.ToList().GroupBy(n => n.Offset).Select(g => g.Last()).ToList();
			escapedText = entity.OrderByDescending(x => x.Offset).Aggregate(escapedText, TextFormat.Format);
		}
		
		var recipient = await provider.GetRecipient(senderId);
		var statusText = await provider.IsResponse(senderId)
			? await locale.Get(recipient, "NewAnswer")
			: await locale.Get(recipient, "NewQuestion");
		
		var encrypted = encryption.Encrypt(senderId.ToString());
		var keyboard = new InlineKeyboardMarkup(
			
			InlineKeyboardButton.WithCallbackData(await locale.Get(recipient, "Respond"), $"respond:{encrypted}"),
			InlineKeyboardButton.WithCallbackData(await locale.Get(recipient, "Ban"), $"ban:{encrypted}")
		);
		
		await botClient.SendMessage(
			
			recipient,
			statusText + escapedText,
			ParseMode.MarkdownV2,
			replyMarkup: keyboard,
			cancellationToken: cancellationToken
		);
		
		statusText = await provider.IsResponse(senderId)
			? await locale.Get(senderId, "AnswerSent")
			: await locale.Get(senderId, "QuestionSent");

		await database.RemoveExpectation(senderId);
		await botClient.SendMessage(
			
			senderId,
			statusText,
			ParseMode.MarkdownV2,
			cancellationToken: cancellationToken
		);
	}
}
