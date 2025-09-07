using AskMeNowBot.Ban;
using AskMeNowBot.Database;
using AskMeNowBot.Localization;
using AskMeNowBot.Localization.Enum;
using AskMeNowBot.Utils.Encryption;

using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

using Action = AskMeNowBot.Handler.Sub.Action;

namespace AskMeNowBot.Command.Standard;

public class BanCommand(IProvider provider, ILocale locale, IEncryption encryption, IDatabase database) : ICommand
{
    public string Name => "ban";
    public string[] Aliases => [];

    public async Task ExecuteAsync(
        ITelegramBotClient botClient,
        ICommandContext context,
        CancellationToken cancellationToken
    )
    {
        var recipient = context.CommandSender;
        var recipientId = recipient.Id;
        
        if (!long.TryParse(context.Argument, out var senderId))
        {
            await botClient.SendMessage(
                recipientId,
                locale.Get(MessageKey.EnterUserId, recipient.Language),
                ParseMode.MarkdownV2,
                cancellationToken: cancellationToken
            );

            return;
        }

        if (await provider.IsBanned(senderId, recipientId))
        {
            await botClient.SendMessage(
                recipientId,
                locale.Get(MessageKey.SenderAlreadyBanned, recipient.Language),
                ParseMode.MarkdownV2,
                cancellationToken: cancellationToken
            );

            return;
        }

        var language = recipient.Language;

        var keyboard = new InlineKeyboardMarkup(
            [
                [
                    InlineKeyboardButton.WithCallbackData(
                        locale.Get(MessageKey.Unban, language),
                        $"{Action.Unban}:{encryption.Encrypt(senderId.ToString())}"
                    )
                ],
                [
                    InlineKeyboardButton.WithCallbackData(
                        locale.Get(MessageKey.AddReason, language),
                        $"{Action.AddComment}:{encryption.Encrypt(senderId.ToString())}"
                    )
                ]
            ]
        );

        await database.AddBan(new BaseBan(senderId, recipientId, string.Empty, DateTime.UtcNow));

        await botClient.SendMessage(
            recipientId,
            locale.Get(MessageKey.SenderBanned, language),
            ParseMode.MarkdownV2,
            replyMarkup: keyboard,
            cancellationToken: cancellationToken
        );
    }
}
