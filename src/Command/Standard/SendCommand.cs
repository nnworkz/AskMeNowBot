using AskMeNowBot.Configuration;
using AskMeNowBot.Database;
using AskMeNowBot.Exceptions;
using AskMeNowBot.Localization;
using AskMeNowBot.Localization.Enum;
using AskMeNowBot.Wait;

using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

using Action = AskMeNowBot.Handler.Sub.Action;

namespace AskMeNowBot.Command.Standard;

public class SendCommand(ILocale locale, IProvider provider, IDatabase database, IConfig config)
    : ICommand
{
    public string Name => "send";
    public string[] Aliases => [];

    public async Task ExecuteAsync(
        ITelegramBotClient botClient,
        ICommandContext context,
        CancellationToken cancellationToken
    )
    {
        var argument = context.Argument;

        if (string.IsNullOrEmpty(argument))
        {
            throw new InvalidCommandArgumentException();
        }

        var sender = context.CommandSender;
        var senderId = sender.Id;
        var language = sender.Language;

        try
        {
            if ((DateTime.UtcNow.Date - sender.LastResetAt.Date).TotalDays >= 1)
            {
                sender.MessagesToday = 0;
                sender.LastResetAt = DateTime.UtcNow;

                await database.AddUser(sender);
            }

            var isSubscriber = await provider.IsSubscriber(senderId);
            var messagesToday = sender.MessagesToday;
            var limits = config.Bot.Limits;
            var messagesPerDay = limits.MessagesPerDay;
            var cooldown = isSubscriber ? limits.Cooldown.Subscriber : limits.Cooldown.Default;
            var lastMessage = sender.LastMessageAt;

            if ((isSubscriber && messagesToday >= messagesPerDay.Subscriber) ||
                messagesToday >= messagesPerDay.Default)
            {
                await botClient.SendMessage(
                    senderId,
                    locale.Get(MessageKey.ExhaustedLimit, language),
                    ParseMode.MarkdownV2,
                    cancellationToken: cancellationToken
                );

                return;
            }

            if (lastMessage != null && (DateTime.UtcNow - lastMessage.Value).TotalSeconds <= cooldown)
            {
                await botClient.SendMessage(
                    senderId,
                    locale.Get(MessageKey.MessageCooldown, language, cooldown),
                    ParseMode.MarkdownV2,
                    cancellationToken: cancellationToken
                );

                return;
            }

            var link = await provider.GetLink(argument);
            var expiresAt = link.ExpiresAt;

            if (link.IsDeactivated ||
                (link.MaxUsages > 0 && link.Used >= link.MaxUsages) ||
                expiresAt != null && expiresAt <= DateTime.UtcNow)
            {
                throw new LinkNotFoundException();
            }

            if (long.TryParse(argument, out var recipientId))
            {
            }
            else if (!string.IsNullOrWhiteSpace(argument))
            {
                recipientId = link.UserId;
            }

            if (senderId == recipientId)
            {
                await botClient.SendMessage(
                    senderId,
                    locale.Get(MessageKey.CannotUseOwnLink, language),
                    ParseMode.MarkdownV2,
                    cancellationToken: cancellationToken
                );

                return;
            }

            if (await provider.IsBanned(senderId, recipientId))
            {
                var ban = await provider.GetBan(senderId, recipientId);

                await botClient.SendMessage(
                    senderId,
                    locale.Get(
                        MessageKey.IsBanned,
                        language,
                        await provider.IsSubscriber(senderId)
                            ? ban.Reason ?? locale.Get(MessageKey.None, language)
                            : locale.Get(MessageKey.OnlySubscriber, language)
                    ),
                    ParseMode.MarkdownV2,
                    cancellationToken: cancellationToken
                );

                return;
            }

            var isResponse = context.ExtraData is { IsResponse: true };

            await database.AddWait(new BaseWait(senderId, recipientId, WaitType.Message, isResponse, null));

            var keyboard = new InlineKeyboardMarkup(
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(
                        locale.Get(MessageKey.Cancel, language),
                        Action.Cancel
                    )
                }
            );

            await botClient.SendMessage(
                senderId,
                locale.Get(MessageKey.SubmitYourMessage, language),
                ParseMode.MarkdownV2,
                replyMarkup: keyboard,
                cancellationToken: cancellationToken
            );
        }
        catch (LinkNotFoundException)
        {
            await botClient.SendMessage(
                senderId,
                locale.Get(MessageKey.InvalidLink, language),
                ParseMode.MarkdownV2,
                cancellationToken: cancellationToken
            );
        }
    }
}
