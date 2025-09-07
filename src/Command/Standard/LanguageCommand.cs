using AskMeNowBot.Localization;
using AskMeNowBot.Localization.Enum;

using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

using Action = AskMeNowBot.Handler.Sub.Action;

namespace AskMeNowBot.Command.Standard;

public class LanguageCommand(ILocale locale) : ICommand
{
    public string Name => "language";
    public string[] Aliases => [];

    public async Task ExecuteAsync(
        ITelegramBotClient botClient,
        ICommandContext context,
        CancellationToken cancellationToken
    )
    {
        var sender = context.CommandSender;
        var language = sender.Language;

        var keyboard = new InlineKeyboardMarkup(
            [
                [
                    InlineKeyboardButton.WithCallbackData(
                        "🇺🇸  English",
                        $"{Action.Language}:{LanguageName.En.ToString()}"
                    ),
                    InlineKeyboardButton.WithCallbackData(
                        "🇷🇺  Русский",
                        $"{Action.Language}:{LanguageName.Ru.ToString()}"
                    )
                ],
                [
                    InlineKeyboardButton.WithCallbackData(
                        locale.Get(MessageKey.Cancel, language),
                        Action.Cancel
                    )
                ]
            ]
        );

        await botClient.SendMessage(
            sender.Id,
            locale.Get(MessageKey.SelectLanguage, language),
            ParseMode.MarkdownV2,
            replyMarkup: keyboard,
            cancellationToken: cancellationToken
        );
    }
}
