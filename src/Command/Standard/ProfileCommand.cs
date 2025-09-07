using AskMeNowBot.Configuration;
using AskMeNowBot.Database;
using AskMeNowBot.Localization;
using AskMeNowBot.Localization.Enum;

using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

using Action = AskMeNowBot.Handler.Sub.Action;

namespace AskMeNowBot.Command.Standard;

public class ProfileCommand(IProvider provider, ILocale locale, IConfig config) : ICommand
{
    public string Name => "profile";
    public string[] Aliases => [];

    public async Task ExecuteAsync(
        ITelegramBotClient botClient,
        ICommandContext context,
        CancellationToken cancellationToken
    )
    {
        var sender = context.CommandSender;
        var senderId = sender.Id;

        var subscriber = await provider.IsSubscriber(senderId) ? await provider.GetSubscriber(senderId) : null;

        var subscriptionText = subscriber != null
            ? subscriber.EndsAt.ToString(locale.GetCultureByLanguage(sender.Language))
            : locale.Get(MessageKey.None, context.CommandSender.Language);

        var messagesLimit = subscriber != null
            ? config.Bot.Limits.MessagesPerDay.Subscriber
            : config.Bot.Limits.MessagesPerDay.Default;

        var keyboard = new InlineKeyboardMarkup(
            [
                [
                    subscriber != null
                        ? InlineKeyboardButton.WithCallbackData(
                            locale.Get(MessageKey.ExtendSubscription, sender.Language),
                            Action.Subscription
                        )
                        : new InlineKeyboardButton(
                            locale.Get(MessageKey.PurchaseSubscription, sender.Language),
                            Action.Subscription
                        )
                ]
            ]
        );

        if (context.ExtraData?.MessageId != null)
        {
            await botClient.EditMessageText(
                senderId,
                (int)context.ExtraData.MessageId,
                locale.Get(
                    MessageKey.ProfileInfo,
                    sender.Language,
                    senderId,
                    sender.RegisteredAt,
                    subscriptionText,
                    sender.MessagesToday,
                    messagesLimit
                ),
                ParseMode.MarkdownV2,
                replyMarkup: keyboard,
                cancellationToken: cancellationToken
            );
        }
        else
        {
            await botClient.SendMessage(
                senderId,
                locale.Get(
                    MessageKey.ProfileInfo,
                    sender.Language,
                    senderId,
                    sender.RegisteredAt,
                    subscriptionText,
                    sender.MessagesToday,
                    messagesLimit
                ),
                ParseMode.MarkdownV2,
                replyMarkup: keyboard,
                cancellationToken: cancellationToken
            );
        }
    }
}
