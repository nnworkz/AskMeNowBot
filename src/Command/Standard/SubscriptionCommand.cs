using AskMeNowBot.Configuration;
using AskMeNowBot.Economy;
using AskMeNowBot.Economy.Billing;
using AskMeNowBot.Economy.Converter;
using AskMeNowBot.Localization;
using AskMeNowBot.Localization.Enum;
using AskMeNowBot.Subscription;

using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

using Action = AskMeNowBot.Handler.Sub.Action;

namespace AskMeNowBot.Command.Standard;

public class SubscriptionCommand(ILocale locale, IBilling billing, IConverter converter, IConfig config) : ICommand
{
    public string Name => "subscription";
    public string[] Aliases => ["sub"];

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
                        locale.Get(MessageKey.Year, language).Replace(
                            "{price}",
                            await GetPrice(SubscriptionPeriod.Year)
                        ),
                        $"{Action.Subscription}:{(int)SubscriptionPeriod.Year}"
                    )
                ],
                [
                    InlineKeyboardButton.WithCallbackData(
                        locale.Get(MessageKey.HalfYear, language).Replace(
                            "{price}",
                            await GetPrice(SubscriptionPeriod.HalfYear)
                        ),
                        $"{Action.Subscription}:{(int)SubscriptionPeriod.HalfYear}"
                    ),
                    InlineKeyboardButton.WithCallbackData(
                        locale.Get(MessageKey.Month, language).Replace(
                            "{price}",
                            await GetPrice(SubscriptionPeriod.Month)
                        ),
                        $"{Action.Subscription}:{(int)SubscriptionPeriod.Month}"
                    )
                ],
                [
                    InlineKeyboardButton.WithCallbackData(
                        locale.Get(MessageKey.Week, language).Replace(
                            "{price}",
                            await GetPrice(SubscriptionPeriod.Week)
                        ),
                        $"{Action.Subscription}:{(int)SubscriptionPeriod.Week}"
                    ),
                    InlineKeyboardButton.WithCallbackData(
                        locale.Get(MessageKey.Day, language).Replace("{price}", await GetPrice(SubscriptionPeriod.Day)),
                        $"{Action.Subscription}:{(int)SubscriptionPeriod.Day}"
                    )
                ],
                [InlineKeyboardButton.WithCallbackData(locale.Get(MessageKey.Cancel, language), "cancel")]
            ]
        );

        if (context.ExtraData?.MessageId != null)
        {
            await botClient.EditMessageText(
                sender.Id,
                (int)context.ExtraData.MessageId,
                locale.Get(MessageKey.Subscription, language),
                parseMode: ParseMode.MarkdownV2,
                replyMarkup: keyboard,
                cancellationToken: cancellationToken
            );
        }
        else
        {
            await botClient.SendMessage(
                sender.Id,
                locale.Get(MessageKey.Subscription, language),
                parseMode: ParseMode.MarkdownV2,
                replyMarkup: keyboard,
                cancellationToken: cancellationToken
            );
        }

        return;

        async Task<string> GetPrice(SubscriptionPeriod period)
        {
            var convertedCurrency = await converter.ConvertCurrency(
                billing.GetPriceByPeriod(period).Amount,
                Enum.Parse<CurrencyName>(config.Prices.Currency, true),
                locale.GetCurrencyByLanguage(language)
            );

            return convertedCurrency.ToString();
        }
    }
}
