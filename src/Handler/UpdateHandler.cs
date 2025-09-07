using AskMeNowBot.Database;
using AskMeNowBot.Exceptions;
using AskMeNowBot.Filter;
using AskMeNowBot.Handler.Sub;
using AskMeNowBot.Link;
using AskMeNowBot.Localization.Enum;
using AskMeNowBot.User;

using Microsoft.Extensions.Logging;

using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace AskMeNowBot.Handler;

public class BaseUpdateHandler(
    IProvider provider,
    IDatabase database,
    MessageHandler messageHandler,
    CallbackQueryHandler callbackQueryHandler,
    ILogger<BaseUpdateHandler> logger
) : IUpdateHandler
{
    public async Task HandleUpdateAsync(
        ITelegramBotClient botClient,
        Update update,
        CancellationToken cancellationToken
    )
    {
        var type = update.Type;

        var from = type switch
        {
            UpdateType.Message when update.Message?.From != null => update.Message.From,
            UpdateType.CallbackQuery when update.CallbackQuery != null => update.CallbackQuery.From,
            _ => throw new UnexpectedUpdateTypeException(type)
        };

        var fromId = from.Id;

        if (!await provider.IsRegistered(fromId))
        {
            var language = Enum.Parse<LanguageName>(from.LanguageCode ?? LanguageName.En.ToString(), true);

            await database.AddUser(new BaseUser(fromId, language, UserRole.User, 0, 0, DateTime.UtcNow, null, DateTime.UtcNow));
            await database.AddLink(new BaseLink(fromId.ToString(), fromId, 0, null, false, null));
            await database.AddFilter(new BaseFilter(fromId, false, true, true, true, true));
        }

        switch (type)
        {
            case UpdateType.Message when update.Message?.From != null:
            {
                await messageHandler.HandleUpdateAsync(botClient, update, cancellationToken);
                
                break;
            }
            case UpdateType.CallbackQuery when update.CallbackQuery != null:
            {
                await callbackQueryHandler.HandleUpdateAsync(botClient, update, cancellationToken);

                break;
            }
        }
    }

    public Task HandleErrorAsync(
        ITelegramBotClient botClient,
        Exception exception,
        HandleErrorSource source,
        CancellationToken cancellationToken
    )
    {
        switch (source)
        {
            case HandleErrorSource.PollingError:
            case HandleErrorSource.HandleUpdateError:
            {
                logger.LogError("§x{Message:l}§w\n{StackTrace:l}", exception.Message, exception.StackTrace);

                break;
            }
            case HandleErrorSource.FatalError:
            {
                logger.LogCritical("§x{Message:l}§w\n{StackTrace:l}", exception.Message, exception.StackTrace);

                break;
            }
            default:
            {
                throw new InvalidErrorTypeException(source.ToString());
            }
        }

        return Task.CompletedTask;
    }
}
