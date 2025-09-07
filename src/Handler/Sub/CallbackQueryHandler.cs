using System.Security.Cryptography;

using AskMeNowBot.Ban;
using AskMeNowBot.Billing;
using AskMeNowBot.Command;
using AskMeNowBot.Command.Standard;
using AskMeNowBot.Configuration;
using AskMeNowBot.Database;
using AskMeNowBot.Economy;
using AskMeNowBot.Economy.Billing;
using AskMeNowBot.Economy.Converter;
using AskMeNowBot.Exceptions;
using AskMeNowBot.Filter;
using AskMeNowBot.Link;
using AskMeNowBot.Localization;
using AskMeNowBot.Localization.Enum;
using AskMeNowBot.Subscription;
using AskMeNowBot.Utils.BotLinker;
using AskMeNowBot.Utils.Encryption;
using AskMeNowBot.Wait;

using CryptoPay;
using CryptoPay.Types;

using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

using Update = Telegram.Bot.Types.Update;

namespace AskMeNowBot.Handler.Sub;

public class CallbackQueryHandler(
    IProvider provider,
    StartCommand start,
    IDatabase database,
    IEncryption encryption,
    SendCommand send,
    ILocale locale,
    IConfig config,
    IBilling billing,
    ICryptoPayClient cryptoPay,
    IConverter converter,
    LinkCommand linkCommand,
    IBotLinker botLinker,
    SubscriptionCommand subscriptionCommand,
    FilterCommand filterCommand
)
{
    public async Task HandleUpdateAsync(
        ITelegramBotClient botClient,
        Update update,
        CancellationToken cancellationToken
    )
    {
        var type = update.Type;
        var callbackQuery = update.CallbackQuery;

        if (type != UpdateType.CallbackQuery || callbackQuery == null)
        {
            throw new UnexpectedUpdateTypeException(type);
        }

        var data = callbackQuery.Data;

        if (data == null)
        {
            throw new NullCallbackDataException();
        }

        var message = callbackQuery.Message;

        if (update.Type is UpdateType.InlineQuery or UpdateType.ChosenInlineResult || message == null)
        {
            throw new InlineModeNotSupportedException();
        }

        await botClient.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: cancellationToken);

        var parts = data.Split(":", StringSplitOptions.RemoveEmptyEntries);
        var messageId = message.MessageId;
        var userId = callbackQuery.From.Id;
        var user = await provider.GetUser(userId);

        try
        {
            switch (parts[0])
            {
                case Action.Filter:
                {
                    var filter = await provider.GetFilter(userId);
                    
                    switch (Enum.Parse<FilterName>(parts[1], true))
                    {
                        case FilterName.Spam:
                        {
                            filter.Spam = !filter.Spam;

                            break;
                        }
                        case FilterName.Terrorism:
                        {
                            filter.Terrorism = !filter.Terrorism;

                            break;
                        }
                        case FilterName.Drugs:
                        {
                            filter.Drugs = !filter.Drugs;

                            break;
                        }
                        case FilterName.Violence:
                        {
                            filter.Violence = !filter.Violence;

                            break;
                        }
                        case FilterName.Pornography:
                        {
                            filter.Pornography = !filter.Pornography;

                            break;
                        }
                        default:
                        {
                            throw new NotSupportedFilterNameException(parts[1]);
                        }
                    }

                    await database.AddFilter(filter);

                    await filterCommand.ExecuteAsync(
                        botClient,
                        new BaseCommandContext(user) { ExtraData = new ExtraData { MessageId = messageId } },
                        cancellationToken
                    );

                    break;
                }
                case Action.Language:
                {
                    user.Language = Enum.Parse<LanguageName>(parts[1], true);

                    await database.AddUser(user);

                    await start.ExecuteAsync(
                        botClient,
                        new BaseCommandContext(user)
                        {
                            ExtraData = new ExtraData
                            {
                                CanEdit = true,
                                MessageId = messageId
                            }
                        },
                        cancellationToken
                    );

                    break;
                }
                case Action.Cancel:
                {
                    if (await provider.InWait(userId))
                    {
                        await database.RemoveWait(await provider.GetWait(userId));
                    }

                    await start.ExecuteAsync(
                        botClient,
                        new BaseCommandContext(user)
                        {
                            ExtraData = new ExtraData
                            {
                                CanEdit = true,
                                MessageId = messageId
                            }
                        },
                        cancellationToken
                    );

                    break;
                }
                case Action.Respond:
                {
                    var command = new BaseCommandContext(user)
                    {
                        Argument = Convert.ToString(long.Parse(encryption.Decrypt(parts[1]))),
                        ExtraData = new ExtraData { IsResponse = true }
                    };

                    await send.ExecuteAsync(botClient, command, cancellationToken);

                    break;
                }
                case Action.Subscription:
                {
                    if (parts.Length < 2)
                    {
                        await subscriptionCommand.ExecuteAsync(
                            botClient,
                            new BaseCommandContext(user) { ExtraData = new ExtraData { MessageId = messageId } },
                            cancellationToken
                        );

                        break;
                    }

                    var language = user.Language;

                    var keyboard = new InlineKeyboardMarkup(
                        [
                            [
                                InlineKeyboardButton.WithCallbackData(
                                    CurrencyName.Usd.ToString(),
                                    $"{Action.CurrencyType}:{CurrencyName.Usd}:{parts[1]}"
                                ),
                                InlineKeyboardButton.WithCallbackData(
                                    CurrencyName.Rub.ToString(),
                                    $"{Action.CurrencyType}:{CurrencyName.Rub}:{parts[1]}"
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

                    await botClient.EditMessageText(
                        userId,
                        messageId,
                        locale.Get(MessageKey.Payment, language),
                        ParseMode.MarkdownV2,
                        replyMarkup: keyboard,
                        cancellationToken: cancellationToken
                    );

                    break;
                }
                case Action.CurrencyType:
                {
                    var currency = Enum.Parse<CurrencyName>(parts[1], true);
                    var language = user.Language;

                    var keyboard = currency switch
                    {
                        CurrencyName.Usd or CurrencyName.Rub => new InlineKeyboardMarkup(
                            [
                                [
                                    InlineKeyboardButton.WithCallbackData(
                                        PaymentType.CryptoPay.ToString(),
                                        $"{Action.Payment}:{PaymentType.CryptoPay}:{currency}:{parts[2]}"
                                    )
                                ],
                                [
                                    InlineKeyboardButton.WithCallbackData(
                                        locale.Get(MessageKey.Cancel, language),
                                        Action.Cancel
                                    )
                                ]
                            ]
                        ),
                        _ => throw new CurrencyNotSupportedException()
                    };

                    await botClient.EditMessageText(
                        userId,
                        messageId,
                        locale.Get(MessageKey.Payment, language),
                        ParseMode.MarkdownV2,
                        replyMarkup: keyboard,
                        cancellationToken: cancellationToken
                    );

                    break;
                }
                case Action.Payment:
                {
                    var language = user.Language;

                    switch (Enum.Parse<PaymentType>(parts[1], true))
                    {
                        case PaymentType.CryptoPay:
                        {
                            var convertedCurrency = await converter.ConvertCurrency(
                                billing.GetPriceByPeriod(Enum.Parse<SubscriptionPeriod>(parts[3], true)).Amount,
                                Enum.Parse<CurrencyName>(config.Prices.Currency, true),
                                Enum.Parse<CurrencyName>(parts[2], true)
                            );

                            var invoice = await cryptoPay.CreateInvoiceAsync(
                                convertedCurrency.Amount,
                                CurrencyTypes.fiat,
                                fiats: Enum.Parse<CurrencyName>(parts[2], true).ToString(),
                                paidBtnName: PaidButtonNames.callback,
                                paidBtnUrl: $"https://t.me/{config.Bot.Name}",
                                payload: $"{userId.ToString()}:{parts[2]}:{parts[3]}:{messageId}",
                                cancellationToken: cancellationToken
                            );

                            var keyboard = new InlineKeyboardMarkup(
                                [
                                    [
                                        InlineKeyboardButton.WithUrl(
                                            locale.Get(MessageKey.Purchase, language),
                                            invoice.BotInvoiceUrl
                                        )
                                    ]
                                ]
                            );

                            await botClient.EditMessageText(
                                userId,
                                messageId,
                                locale.Get(
                                    MessageKey.BillInfo,
                                    language,
                                    parts[1],
                                    convertedCurrency.ToString(),
                                    DateTime.UtcNow.AddDays(int.Parse(parts[3]))
                                        .ToString(locale.GetCultureByLanguage(language))
                                ),
                                ParseMode.MarkdownV2,
                                replyMarkup: keyboard,
                                cancellationToken: cancellationToken
                            );

                            break;
                        }
                        default:
                        {
                            throw new InvalidPaymentTypeException(parts[1]);
                        }
                    }

                    break;
                }
                case Action.Ban:
                {
                    var recipient = await provider.GetUser(userId);
                    var senderId = long.Parse(encryption.Decrypt(parts[1]));

                    var keyboard = new InlineKeyboardMarkup(
                        [
                            [
                                InlineKeyboardButton.WithCallbackData(
                                    locale.Get(MessageKey.Unban, recipient.Language),
                                    $"{Action.Unban}:{encryption.Encrypt(senderId.ToString())}"
                                )
                            ],
                            [
                                InlineKeyboardButton.WithCallbackData(
                                    locale.Get(MessageKey.AddReason, recipient.Language),
                                    $"{Action.AddComment}:{encryption.Encrypt(senderId.ToString())}"
                                )
                            ]
                        ]
                    );

                    await database.AddBan(new BaseBan(senderId, recipient.Id, string.Empty, DateTime.UtcNow));

                    await botClient.EditMessageText(
                        recipient.Id,
                        messageId,
                        locale.Get(MessageKey.SenderBanned, user.Language),
                        ParseMode.MarkdownV2,
                        replyMarkup: keyboard,
                        cancellationToken: cancellationToken
                    );

                    break;
                }
                case Action.AddComment:
                {
                    var senderId = long.Parse(encryption.Decrypt(parts[1]));
                    var recipient = await provider.GetUser(userId);

                    await database.AddWait(
                        new BaseWait(userId, senderId, WaitType.Comment, false, null)
                    );

                    var keyboard = new InlineKeyboardMarkup(
                        [
                            [
                                InlineKeyboardButton.WithCallbackData(
                                    locale.Get(MessageKey.Unban, recipient.Language),
                                    $"{Action.Unban}:{encryption.Encrypt(senderId.ToString())}"
                                )
                            ],
                            [
                                InlineKeyboardButton.WithCallbackData(
                                    locale.Get(MessageKey.RemoveReason, recipient.Language),
                                    $"{Action.RemoveComment}:{encryption.Encrypt(senderId.ToString())}"
                                )
                            ]
                        ]
                    );

                    await botClient.EditMessageReplyMarkup(
                        userId,
                        messageId,
                        replyMarkup: keyboard,
                        cancellationToken: cancellationToken
                    );

                    await botClient.SendMessage(
                        userId,
                        locale.Get(MessageKey.SendReason, user.Language),
                        ParseMode.MarkdownV2,
                        cancellationToken: cancellationToken
                    );

                    break;
                }
                case Action.RemoveComment:
                {
                    var ban = await provider.GetBan(long.Parse(encryption.Decrypt(parts[1])), userId);

                    if (ban is BaseBan updatedBan)
                    {
                        await database.AddBan(
                            updatedBan with
                            {
                                Reason = string.Empty,
                                BannedAt = ban.BannedAt
                            }
                        );
                    }

                    break;
                }
                case Action.Unban:
                {
                    var recipient = await provider.GetUser(userId);
                    var language = recipient.Language;
                    var senderId = long.Parse(encryption.Decrypt(parts[1]));
                    var encrypted = encryption.Encrypt(senderId.ToString());

                    var keyboard = new InlineKeyboardMarkup(
                        InlineKeyboardButton.WithCallbackData(
                            locale.Get(MessageKey.Ban, language),
                            $"{Action.Ban}:{encrypted}"
                        )
                    );

                    await database.RemoveBan(await provider.GetBan(senderId, recipient.Id));

                    await botClient.EditMessageText(
                        recipient.Id,
                        messageId,
                        locale.Get(MessageKey.SenderUnbanned, language),
                        ParseMode.MarkdownV2,
                        replyMarkup: keyboard,
                        cancellationToken: cancellationToken
                    );

                    break;
                }
                case Action.CreateLink:
                {
                    List<ILink> links = [];

                    try
                    {
                        links = await provider.GetLinks(userId);
                    }
                    catch (LinksNotFoundException)
                    {
                    }

                    if (links.Count >= 10)
                    {
                        await botClient.EditMessageText(
                            userId,
                            messageId,
                            locale.Get(MessageKey.LinkLimit, user.Language),
                            ParseMode.MarkdownV2,
                            cancellationToken: cancellationToken
                        );

                        break;
                    }

                    var link = botLinker.GenerateLink();

                    if (!await provider.LinkExist(userId.ToString()))
                    {
                        link = userId.ToString();
                    }
                    else
                    {
                        while (await provider.LinkExist(link))
                        {
                            link = botLinker.GenerateLink();
                        }
                    }

                    await database.AddLink(new BaseLink(link, userId, 0, null, false, null));

                    await linkCommand.ExecuteAsync(
                        botClient,
                        new BaseCommandContext(user) { ExtraData = new ExtraData { MessageId = messageId } },
                        cancellationToken
                    );

                    break;
                }
                case Action.Link:
                {
                    var link = await provider.GetLink(parts[1]);

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
                                    locale.Get(MessageKey.Edit, user.Language),
                                    $"{Action.EditLink}:{parts[1]}"
                                )
                            ],
                            [
                                InlineKeyboardButton.WithCallbackData(
                                    locale.Get(MessageKey.Deactivate, user.Language),
                                    $"{Action.DeactivateLink}:{parts[1]}"
                                ),
                                InlineKeyboardButton.WithCallbackData(
                                    locale.Get(MessageKey.Remove, user.Language),
                                    $"{Action.RemoveLink}:{parts[1]}"
                                )
                            ],
                            [
                                InlineKeyboardButton.WithCallbackData(
                                    locale.Get(MessageKey.Cancel, user.Language),
                                    Action.Cancel
                                )
                            ]
                        ]
                    );

                    await botClient.EditMessageText(
                        userId,
                        messageId,
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
                case Action.EditLink:
                {
                    if (!await provider.IsSubscriber(userId))
                    {
                        await botClient.EditMessageText(
                            userId,
                            messageId,
                            locale.Get(MessageKey.OnlySubscriber, user.Language),
                            ParseMode.MarkdownV2,
                            cancellationToken: cancellationToken
                        );

                        break;
                    }

                    var keyboard = new InlineKeyboardMarkup(
                        [
                            [
                                InlineKeyboardButton.WithCallbackData(
                                    locale.Get(MessageKey.UsageCount, user.Language),
                                    $"{Action.EditLinkMaxUsages}:{parts[1]}"
                                )
                            ],
                            [
                                InlineKeyboardButton.WithCallbackData(
                                    locale.Get(MessageKey.ExpirationDate, user.Language),
                                    $"{Action.EditLinkExpiresAt}:{parts[1]}"
                                )
                            ]
                        ]
                    );

                    await botClient.EditMessageText(
                        userId,
                        messageId,
                        locale.Get(MessageKey.ChooseChange, user.Language),
                        ParseMode.MarkdownV2,
                        replyMarkup: keyboard,
                        cancellationToken: cancellationToken
                    );

                    break;
                }
                case Action.EditLinkMaxUsages:
                {
                    if (!await provider.IsSubscriber(userId))
                    {
                        await botClient.EditMessageText(
                            userId,
                            messageId,
                            locale.Get(MessageKey.OnlySubscriber, user.Language),
                            ParseMode.MarkdownV2,
                            cancellationToken: cancellationToken
                        );

                        break;
                    }

                    await database.AddWait(new BaseWait(userId, null, WaitType.MaxUsagesLink, false, parts[1]));

                    await botClient.EditMessageText(
                        userId,
                        messageId,
                        locale.Get(MessageKey.SendNewMaxUsages, user.Language),
                        ParseMode.MarkdownV2,
                        cancellationToken: cancellationToken
                    );

                    break;
                }
                case Action.EditLinkExpiresAt:
                {
                    if (!await provider.IsSubscriber(userId))
                    {
                        await botClient.EditMessageText(
                            userId,
                            messageId,
                            locale.Get(MessageKey.OnlySubscriber, user.Language),
                            ParseMode.MarkdownV2,
                            cancellationToken: cancellationToken
                        );

                        break;
                    }

                    await database.AddWait(new BaseWait(userId, null, WaitType.ExpiresAtLink, false, parts[1]));

                    await botClient.EditMessageText(
                        userId,
                        messageId,
                        locale.Get(MessageKey.SendNewExpiresAt, user.Language),
                        ParseMode.MarkdownV2,
                        cancellationToken: cancellationToken
                    );

                    break;
                }
                case Action.DeactivateLink:
                {
                    if (!await provider.IsSubscriber(userId))
                    {
                        await botClient.EditMessageText(
                            userId,
                            messageId,
                            locale.Get(MessageKey.OnlySubscriber, user.Language),
                            ParseMode.MarkdownV2,
                            cancellationToken: cancellationToken
                        );

                        break;
                    }

                    var link = await provider.GetLink(parts[1]);
                    link.IsDeactivated = true;

                    await database.AddLink(link);

                    var keyboard = new InlineKeyboardMarkup(
                        [
                            [
                                InlineKeyboardButton.WithCallbackData(
                                    locale.Get(MessageKey.Edit, user.Language),
                                    $"{Action.EditLink}:{parts[1]}"
                                )
                            ],
                            [
                                InlineKeyboardButton.WithCallbackData(
                                    locale.Get(MessageKey.Activate, user.Language),
                                    $"{Action.ActivateLink}:{parts[1]}"
                                ),
                                InlineKeyboardButton.WithCallbackData(
                                    locale.Get(MessageKey.Remove, user.Language),
                                    $"{Action.RemoveLink}:{parts[1]}"
                                )
                            ],
                            [
                                InlineKeyboardButton.WithCallbackData(
                                    locale.Get(MessageKey.Cancel, user.Language),
                                    Action.Cancel
                                )
                            ]
                        ]
                    );

                    await botClient.EditMessageText(
                        userId,
                        messageId,
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
                case Action.ActivateLink:
                {
                    if (!await provider.IsSubscriber(userId))
                    {
                        await botClient.EditMessageText(
                            userId,
                            messageId,
                            locale.Get(MessageKey.OnlySubscriber, user.Language),
                            ParseMode.MarkdownV2,
                            cancellationToken: cancellationToken
                        );

                        break;
                    }

                    var link = await provider.GetLink(parts[1]);
                    link.IsDeactivated = false;

                    await database.AddLink(link);

                    var keyboard = new InlineKeyboardMarkup(
                        [
                            [
                                InlineKeyboardButton.WithCallbackData(
                                    locale.Get(MessageKey.Edit, user.Language),
                                    $"{Action.EditLink}:{parts[1]}"
                                )
                            ],
                            [
                                InlineKeyboardButton.WithCallbackData(
                                    locale.Get(MessageKey.Edit, user.Language),
                                    $"{Action.DeactivateLink}:{parts[1]}"
                                ),
                                InlineKeyboardButton.WithCallbackData(
                                    locale.Get(MessageKey.Remove, user.Language),
                                    $"{Action.RemoveLink}:{parts[1]}"
                                )
                            ],
                            [
                                InlineKeyboardButton.WithCallbackData(
                                    locale.Get(MessageKey.Cancel, user.Language),
                                    Action.Cancel
                                )
                            ]
                        ]
                    );

                    await botClient.EditMessageText(
                        userId,
                        messageId,
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
                case Action.RemoveLink:
                {
                    await database.RemoveLink(await provider.GetLink(parts[1]));

                    await linkCommand.ExecuteAsync(
                        botClient,
                        new BaseCommandContext(user) { ExtraData = new ExtraData { MessageId = messageId } },
                        cancellationToken
                    );

                    break;
                }
            }
        }
        catch (CryptographicException)
        {
            await botClient.SendMessage(
                userId,
                locale.Get(MessageKey.DecryptionFailed, user.Language),
                ParseMode.MarkdownV2,
                cancellationToken: cancellationToken
            );

            throw;
        }
    }
}
