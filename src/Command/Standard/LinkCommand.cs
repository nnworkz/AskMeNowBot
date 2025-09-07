using AskMeNowBot.Database;
using AskMeNowBot.Exceptions;
using AskMeNowBot.Localization;
using AskMeNowBot.Localization.Enum;

using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

using Action = AskMeNowBot.Handler.Sub.Action;

namespace AskMeNowBot.Command.Standard;

public class LinkCommand(IProvider provider, ILocale locale) : ICommand
{
    public string Name => "link";
    public string[] Aliases => [];

    public async Task ExecuteAsync(
        ITelegramBotClient botClient,
        ICommandContext context,
        CancellationToken cancellationToken
    )
    {
        var isSubscriber = await provider.IsSubscriber(context.CommandSender.Id);
        var messageId = context.ExtraData?.MessageId;

        try
        {
            var links = await provider.GetLinks(context.CommandSender.Id);

            var linksButton = links.Select(
                link => InlineKeyboardButton.WithCallbackData(link.Link, $"{Action.Link}:{link.Link}")
            ).Select(
                button => new[] { button }
            );

            if (isSubscriber)
            {
                linksButton = linksButton.Concat(
                    [
                        [
                            InlineKeyboardButton.WithCallbackData(
                                locale.Get(MessageKey.CreateLink, context.CommandSender.Language),
                                Action.CreateLink
                            )
                        ]
                    ]
                );
            }

            if (messageId != null)
            {
                await botClient.EditMessageText(
                    context.CommandSender.Id,
                    (int)messageId,
                    locale.Get(MessageKey.YourLinks, context.CommandSender.Language),
                    ParseMode.MarkdownV2,
                    replyMarkup: new InlineKeyboardMarkup(linksButton),
                    cancellationToken: cancellationToken
                );
            }
            else
            {
                await botClient.SendMessage(
                    context.CommandSender.Id,
                    locale.Get(MessageKey.YourLinks, context.CommandSender.Language),
                    ParseMode.MarkdownV2,
                    replyMarkup: new InlineKeyboardMarkup(linksButton),
                    cancellationToken: cancellationToken
                );
            }
        }
        catch (LinksNotFoundException)
        {
            var keyboard = new InlineKeyboardMarkup(
                [
                    [
                        InlineKeyboardButton.WithCallbackData(
                            locale.Get(MessageKey.CreateLink, context.CommandSender.Language),
                            Action.CreateLink
                        )
                    ]
                ]
            );

            if (messageId != null)
            {
                await botClient.EditMessageText(
                    context.CommandSender.Id,
                    (int)messageId,
                    locale.Get(MessageKey.MissingLinks, context.CommandSender.Language),
                    ParseMode.MarkdownV2,
                    replyMarkup: keyboard,
                    cancellationToken: cancellationToken
                );
            }
            else
            {
                await botClient.SendMessage(
                    context.CommandSender.Id,
                    locale.Get(MessageKey.MissingLinks, context.CommandSender.Language),
                    ParseMode.MarkdownV2,
                    replyMarkup: keyboard,
                    cancellationToken: cancellationToken
                );
            }
        }
    }
}
