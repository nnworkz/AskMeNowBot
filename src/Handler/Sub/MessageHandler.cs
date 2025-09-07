using AskMeNowBot.Ban;
using AskMeNowBot.Command;
using AskMeNowBot.Command.Registry;
using AskMeNowBot.Configuration;
using AskMeNowBot.Database;
using AskMeNowBot.Exceptions;
using AskMeNowBot.Localization;
using AskMeNowBot.Localization.Enum;
using AskMeNowBot.Moderation;
using AskMeNowBot.Utils.BotLinker;
using AskMeNowBot.Utils.Encryption;
using AskMeNowBot.Utils.TextFormat;
using AskMeNowBot.Wait;

using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace AskMeNowBot.Handler.Sub;

public class MessageHandler(
    IProvider provider,
    ILocale locale,
    IConfig config,
    IDatabase database,
    ITextFormat textFormat,
    IEncryption encryption,
    ICommandRegistry registry,
    IBotLinker botLinker,
    IModeration moderation
)
{
    public async Task HandleUpdateAsync(
        ITelegramBotClient botClient,
        Update update,
        CancellationToken cancellationToken
    )
    {
        var type = update.Type;
        var message = update.Message;

        if (type != UpdateType.Message || message == null)
        {
            throw new UnexpectedUpdateTypeException(type);
        }

        var from = message.From;

        if (from == null)
        {
            throw new NullMessageSenderException();
        }

        var text = message.Text;
        var userId = from.Id;
        var user = await provider.GetUser(userId);
        var language = user.Language;

        if (message.Type != MessageType.Text || text == null)
        {
            await botClient.SendMessage(
                userId,
                locale.Get(MessageKey.OnlyTextMessage, language),
                ParseMode.MarkdownV2,
                cancellationToken: cancellationToken
            );

            return;
        }

        if (text.StartsWith(config.Bot.CommandPrefix))
        {
            if (await provider.InWait(userId))
            {
                await database.RemoveWait(await provider.GetWait(userId));
            }

            var parts = text[1..].Split(" ", 2, StringSplitOptions.RemoveEmptyEntries);

            try
            {
                var command = registry.GetCommand(parts[0]);
                var context = new BaseCommandContext(user) { Argument = parts.Length > 1 ? parts[1] : null };

                await command.ExecuteAsync(botClient, context, cancellationToken);
            }
            catch (CommandNotFoundException)
            {
                await botClient.SendMessage(
                    userId,
                    locale.Get(MessageKey.CommandNotFound, language),
                    ParseMode.MarkdownV2,
                    cancellationToken: cancellationToken
                );
            }

            return;
        }

        if (!await provider.InWait(userId))
        {
            await botClient.SendMessage(
                userId,
                locale.Get(MessageKey.LinkToRecipient, language),
                ParseMode.MarkdownV2,
                cancellationToken: cancellationToken
            );

            return;
        }

        var wait = await provider.GetWait(userId);
        var maxMessageLength = config.Bot.MaxMessageLength;
        var maxReasonLength = config.Bot.MaxReasonLength;

        switch (wait.Type)
        {
            case WaitType.Message when text.Length > maxMessageLength:
            {
                await database.RemoveWait(await provider.GetWait(userId));

                await botClient.SendMessage(
                    userId,
                    locale.Get(MessageKey.MessageIsTooLong, language, maxMessageLength),
                    ParseMode.MarkdownV2,
                    cancellationToken: cancellationToken
                );

                break;
            }
            case WaitType.Message:
            {
                var recipientId = wait.RecipientId;

                if (recipientId == null)
                {
                    break;
                }

                var botMessage = await botClient.SendMessage(
                    userId,
                    locale.Get(MessageKey.Handling, language),
                    ParseMode.MarkdownV2,
                    cancellationToken: cancellationToken
                );

                var messageId = botMessage.Id;

                try
                {
                    if (await moderation.CheckAsync(await provider.GetFilter((long)recipientId), text))
                    {
                        await botClient.EditMessageText(
                            userId,
                            messageId,
                            locale.Get(MessageKey.FilteredOut, language),
                            ParseMode.MarkdownV2,
                            cancellationToken: cancellationToken
                        );

                        break;
                    }
                }
                catch
                {
                    await botClient.EditMessageText(
                        userId,
                        messageId,
                        locale.Get(MessageKey.Error, language),
                        ParseMode.MarkdownV2,
                        cancellationToken: cancellationToken
                    );

                    throw;
                }

                var recipient = await provider.GetUser(recipientId.Value);

                language = recipient.Language;
                var escapedText = textFormat.EscapeMarkdownV2(text);

                if (message.Entities != null)
                {
                    escapedText = message.Entities.ToList().GroupBy(n => n.Offset).Select(g => g.Last()).ToList()
                        .OrderByDescending(x => x.Offset).Aggregate(escapedText, textFormat.Format);
                }

                var encryptedSenderId = encryption.Encrypt(userId.ToString());

                var keyboard = new InlineKeyboardMarkup(
                    InlineKeyboardButton.WithCallbackData(
                        locale.Get(MessageKey.Respond, language),
                        $"{Action.Respond}:{encryptedSenderId}"
                    ),
                    InlineKeyboardButton.WithCallbackData(
                        locale.Get(MessageKey.Ban, language),
                        $"{Action.Ban}:{encryptedSenderId}"
                    )
                );

                try
                {
                    await botClient.SendMessage(
                        recipient.Id,
                        locale.Get(MessageKey.NewMessage, language) + escapedText,
                        ParseMode.MarkdownV2,
                        replyMarkup: keyboard,
                        cancellationToken: cancellationToken
                    );
                }
                catch (ApiRequestException)
                {
                    await botClient.EditMessageText(
                        userId,
                        messageId,
                        locale.Get(MessageKey.RecipientUnavailable, language),
                        ParseMode.MarkdownV2,
                        cancellationToken: cancellationToken
                    );

                    break;
                }

                await database.RemoveWait(wait);

                user.Messages += 1;
                user.MessagesToday += 1;
                user.LastMessageAt = DateTime.UtcNow;

                await database.AddUser(user);

                await botClient.EditMessageText(
                    userId,
                    messageId,
                    locale.Get(MessageKey.MessageSent, user.Language),
                    ParseMode.MarkdownV2,
                    cancellationToken: cancellationToken
                );

                break;
            }
            case WaitType.Comment when text.Length > maxReasonLength:
            {
                await database.RemoveWait(wait);

                await botClient.SendMessage(
                    userId,
                    locale.Get(MessageKey.MessageIsTooLong, language, maxReasonLength),
                    ParseMode.MarkdownV2,
                    cancellationToken: cancellationToken
                );

                break;
            }
            case WaitType.Comment:
            {
                var recipientId = wait.RecipientId;

                if (recipientId == null)
                {
                    break;
                }

                if (await provider.GetBan(recipientId.Value, userId) is BaseBan updatedBan)
                {
                    await database.AddBan(
                        updatedBan with
                        {
                            Reason = text,
                            BannedAt = updatedBan.BannedAt
                        }
                    );

                    await botClient.SendMessage(
                        userId,
                        locale.Get(MessageKey.ReasonAdded, language),
                        ParseMode.MarkdownV2,
                        cancellationToken: cancellationToken
                    );
                }

                break;
            }
            case WaitType.MaxUsagesLink when !long.TryParse(text, out _):
            {
                await botClient.SendMessage(
                    userId,
                    locale.Get(MessageKey.InvalidValue, language),
                    ParseMode.MarkdownV2,
                    cancellationToken: cancellationToken
                );

                break;
            }
            case WaitType.MaxUsagesLink:
            {
                if (wait.Link == null)
                {
                    throw new NullLinkException();
                }

                var link = await provider.GetLink(wait.Link);

                link.MaxUsages = long.Parse(text) < 0 ? null : long.Parse(text);

                await database.AddLink(link);

                var keyboard = new InlineKeyboardMarkup(
                    [
                        [
                            InlineKeyboardButton.WithUrl(
                                locale.Get(MessageKey.ShareLink, user.Language),
                                botLinker.GetStartLink(link.Link)
                            )
                        ],
                        [
                            InlineKeyboardButton.WithCallbackData(
                                "Редактировать",
                                $"{Action.EditLink}:{link.Link}"
                            )
                        ],
                        [
                            InlineKeyboardButton.WithCallbackData(
                                "Деактивировать",
                                $"{Action.DeactivateLink}:{link.Link}"
                            ),
                            InlineKeyboardButton.WithCallbackData(
                                "Удалить",
                                $"{Action.RemoveLink}:{link.Link}"
                            )
                        ],
                        [
                            InlineKeyboardButton.WithCallbackData(
                                "Отмена",
                                Action.Cancel
                            )
                        ]
                    ]
                );

                await botClient.SendMessage(
                    userId,
                    locale.Get(
                        MessageKey.LinkName,
                        user.Language,
                        config.Bot.Name,
                        link.Link,
                        link.Used,
                        link.MaxUsages != null ? link.MaxUsages : locale.Get(MessageKey.Infinity, user.Language),
                        link.ExpiresAt != null ? link.ExpiresAt : locale.Get(MessageKey.Infinity, user.Language),
                        link.IsDeactivated
                            ? locale.Get(MessageKey.Deactivated, user.Language)
                            : locale.Get(MessageKey.Activated, user.Language)
                    ),
                    ParseMode.MarkdownV2,
                    replyMarkup: keyboard,
                    cancellationToken: cancellationToken
                );

                break;
            }
            case WaitType.ExpiresAtLink when !int.TryParse(text, out _):
            {
                await botClient.SendMessage(
                    userId,
                    locale.Get(MessageKey.InvalidValue, language),
                    ParseMode.MarkdownV2,
                    cancellationToken: cancellationToken
                );

                break;
            }
            case WaitType.ExpiresAtLink:
            {
                if (wait.Link == null)
                {
                    throw new NullLinkException();
                }

                var link = await provider.GetLink(wait.Link);

                link.ExpiresAt = int.Parse(text) < 0 ? null : DateTime.UtcNow.AddDays(int.Parse(text));

                await database.AddLink(link);

                var keyboard = new InlineKeyboardMarkup(
                    [
                        [
                            InlineKeyboardButton.WithUrl(
                                locale.Get(MessageKey.ShareLink, user.Language),
                                botLinker.GetStartLink(link.Link)
                            )
                        ],
                        [
                            InlineKeyboardButton.WithCallbackData(
                                "Редактировать",
                                $"{Action.EditLink}:{link.Link}"
                            )
                        ],
                        [
                            InlineKeyboardButton.WithCallbackData(
                                "Деактивировать",
                                $"{Action.DeactivateLink}:{link.Link}"
                            ),
                            InlineKeyboardButton.WithCallbackData(
                                "Удалить",
                                $"{Action.RemoveLink}:{link.Link}"
                            )
                        ],
                        [
                            InlineKeyboardButton.WithCallbackData(
                                "Отмена",
                                Action.Cancel
                            )
                        ]
                    ]
                );

                await botClient.SendMessage(
                    userId,
                    locale.Get(
                        MessageKey.LinkName,
                        user.Language,
                        config.Bot.Name,
                        link.Link,
                        link.Used,
                        link.MaxUsages != null ? link.MaxUsages : locale.Get(MessageKey.Infinity, user.Language),
                        link.ExpiresAt != null ? link.ExpiresAt : locale.Get(MessageKey.Infinity, user.Language),
                        link.IsDeactivated
                            ? locale.Get(MessageKey.Deactivated, user.Language)
                            : locale.Get(MessageKey.Activated, user.Language)
                    ),
                    ParseMode.MarkdownV2,
                    replyMarkup: keyboard,
                    cancellationToken: cancellationToken
                );

                break;
            }
        }
    }
}
