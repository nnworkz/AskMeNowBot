using AskMeNowBot.Database;
using AskMeNowBot.Filter;
using AskMeNowBot.Localization;
using AskMeNowBot.Localization.Enum;

using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

using Action = AskMeNowBot.Handler.Sub.Action;

namespace AskMeNowBot.Command.Standard;

public class FilterCommand(IProvider provider, ILocale locale) : ICommand
{
    public string Name => "filter";
    public string[] Aliases => [];

    public async Task ExecuteAsync(
        ITelegramBotClient botClient,
        ICommandContext context,
        CancellationToken cancellationToken
    )
    {
        var user = context.CommandSender;
        var userId = user.Id;
        var filter = await provider.GetFilter(userId);

        var keyboard = new InlineKeyboardMarkup(
            [
                [
                    InlineKeyboardButton.WithCallbackData(
                        await provider.IsSubscriber(userId) ? filter.Spam ? "Спам ✅" : "Спам ❌" : "Спам ⭐",
                        await provider.IsSubscriber(userId) ? $"{Action.Filter}:{FilterName.Spam}" : Action.Subscription
                    )
                ],
                [
                    InlineKeyboardButton.WithCallbackData(
                        filter.Terrorism ? "Терроризм ✅" : "Терроризм ❌",
                        $"{Action.Filter}:{FilterName.Terrorism}"
                    ),
                    InlineKeyboardButton.WithCallbackData(
                        filter.Drugs ? "Наркотики ✅" : "Наркотики ❌",
                        $"{Action.Filter}:{FilterName.Drugs}"
                    )
                ],
                [
                    InlineKeyboardButton.WithCallbackData(
                        filter.Violence ? "Насилие ✅" : "Насилие ❌",
                        $"{Action.Filter}:{FilterName.Violence}"
                    ),
                    InlineKeyboardButton.WithCallbackData(
                        filter.Pornography ? "Порнография ✅" : "Порнография ❌",
                        $"{Action.Filter}:{FilterName.Pornography}"
                    )
                ],
                [InlineKeyboardButton.WithCallbackData(locale.Get(MessageKey.Cancel, user.Language))]
            ]
        );

        if (context.ExtraData?.MessageId != null)
        {
            await botClient.EditMessageText(
                userId,
                (int)context.ExtraData.MessageId,
                "*Ваши фильтры*",
                ParseMode.MarkdownV2,
                replyMarkup: keyboard,
                cancellationToken: cancellationToken
            );
        }
        else
        {
            await botClient.SendMessage(
                userId,
                "*Ваши фильтры*",
                ParseMode.MarkdownV2,
                replyMarkup: keyboard,
                cancellationToken: cancellationToken
            );
        }
    }
}
