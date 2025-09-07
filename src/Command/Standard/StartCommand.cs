using AskMeNowBot.Configuration;
using AskMeNowBot.Localization;
using AskMeNowBot.Localization.Enum;
using AskMeNowBot.Utils.BotLinker;

using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace AskMeNowBot.Command.Standard;

public class StartCommand(SendCommand send, ILocale locale, IConfig config, IBotLinker botLinker) : ICommand
{
    public string Name => "start";
    public string[] Aliases => [];

    public async Task ExecuteAsync(
        ITelegramBotClient botClient,
        ICommandContext context,
        CancellationToken cancellationToken
    )
    {
        if (long.TryParse(context.Argument, out _) || !string.IsNullOrEmpty(context.Argument))
        {
            await send.ExecuteAsync(botClient, context, cancellationToken);

            return;
        }

        var sender = context.CommandSender;
        var language = sender.Language;
        var senderId = sender.Id;

        var text = locale.Get(MessageKey.YourLink, language, config.Bot.Name, senderId);

        var keyboard = new InlineKeyboardMarkup(
            new[]
            {
                InlineKeyboardButton.WithUrl(
                    locale.Get(MessageKey.ShareLink, language),
                    botLinker.GetStartLink(senderId.ToString())
                )
            }
        );

        if (context.ExtraData is { CanEdit: true, MessageId: not null })
        {
            await botClient.EditMessageText(
                senderId,
                context.ExtraData.MessageId.Value,
                text,
                ParseMode.MarkdownV2,
                replyMarkup: keyboard,
                cancellationToken: cancellationToken
            );

            return;
        }

        await botClient.SendMessage(
            senderId,
            text,
            ParseMode.MarkdownV2,
            replyMarkup: keyboard,
            cancellationToken: cancellationToken
        );
    }
}
