using System.Text.Json;
using System.Text.Json.Serialization;

using AskMeNowBot.Configuration;
using AskMeNowBot.Database;
using AskMeNowBot.Economy;
using AskMeNowBot.Localization;
using AskMeNowBot.Localization.Enum;
using AskMeNowBot.Subscription;
using AskMeNowBot.Transaction;

using CryptoPay;
using CryptoPay.Types;

using Microsoft.AspNetCore.Mvc;

using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace AskMeNowBot.Controller;

public class CryptoPayController(
    IConfig config,
    IProvider provider,
    ITelegramBotClient botClient,
    ILocale locale,
    IDatabase database
) : ControllerBase
{
    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString
    };

    [HttpPost]
    [ActionName(nameof(ControllerAction.PostAsync))]
    public async Task<IActionResult> PostAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var updateBodyBytes = new byte[HttpContext.Request.ContentLength!.Value];

            await HttpContext.Request.Body.ReadExactlyAsync(updateBodyBytes, cancellationToken);

            var update = JsonSerializer.Deserialize<Update>(updateBodyBytes, _jsonSerializerOptions);

            if (update is null ||
                update.UpdateType != UpdateTypes.invoice_paid ||
                !HttpContext.Request.Headers.TryGetValue("crypto-pay-api-signature", out var signature) ||
                !CryptoPayHelper.CheckSignature(signature, config.CryptoPay.Token, updateBodyBytes))
            {
                return BadRequest();
            }

            var payload = update.Payload.Payload.Split(":", StringSplitOptions.RemoveEmptyEntries);
            var userId = long.Parse(payload[0]);
            var user = await provider.GetUser(userId);

            await database.AddTransaction(
                new BaseTransaction(
                    -1,
                    userId,
                    TransactionType.TopUp,
                    update.Payload.Amount,
                    Enum.Parse<CurrencyName>(payload[1], true),
                    DateTime.UtcNow
                )
            );

            ISubscriber subscriber = new BaseSubscriber(
                userId,
                DateTime.UtcNow,
                DateTime.UtcNow.AddDays(int.Parse(payload[2]))
            );

            if (await provider.IsSubscriber(userId))
            {
                subscriber = await provider.GetSubscriber(userId);

                subscriber.EndsAt = subscriber.EndsAt.AddDays(int.Parse(payload[2]));
            }

            await database.AddSubscriber(subscriber);

            await database.AddTransaction(
                new BaseTransaction(
                    -1,
                    userId,
                    TransactionType.Purchase,
                    update.Payload.Amount,
                    Enum.Parse<CurrencyName>(payload[1], true),
                    DateTime.UtcNow
                )
            );

            await botClient.EditMessageText(
                userId,
                int.Parse(payload[3]),
                locale.Get(MessageKey.SuccessfulPayment, user.Language),
                parseMode: ParseMode.MarkdownV2,
                cancellationToken: cancellationToken
            );

            return Ok();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);

            throw;
        }
    }
}
