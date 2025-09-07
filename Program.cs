using System.Diagnostics;

using AskMeNowBot.Command;
using AskMeNowBot.Command.Registry;
using AskMeNowBot.Configuration;
using AskMeNowBot.Controller;
using AskMeNowBot.Database;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Serilog;

using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;

namespace AskMeNowBot;

public class Program
{
    public static async Task Main(string[] args)
    {
        var stopwatch = Stopwatch.StartNew();
        ILogger<Program>? logger = null;
        
        try
        {
            var builder = WebApplication.CreateBuilder(args);
        
            await DependencyInjection.Configure(builder.Services);
        
            var app = builder.Build();
            var provider = app.Services;
            var lifetime = app.Lifetime;
        
            logger = provider.GetRequiredService<ILogger<Program>>();
        
            await provider.GetRequiredService<IDatabase>().InitAsync();
            logger.LogInformation("Database initialized");
        
            RegisterCommands(provider);
            logger.LogInformation("Commands registered");
        
            StartReceiving(provider, lifetime.ApplicationStopping);
            logger.LogInformation("Bot started");
        
            lifetime.ApplicationStarted.Register(
                () =>
                {
                    stopwatch.Stop();
                    logger.LogInformation("Enabled (§4{Seconds:F3} sec.§B)", stopwatch.Elapsed.TotalSeconds);
                }
            );
        
            app.MapControllerRoute(
                ControllerName.CryptoPay.ToString(),
                provider.GetRequiredService<IConfig>().CryptoPay.Token,
                new
                {
                    controller = ControllerName.CryptoPay.ToString(),
                    action = ControllerAction.PostAsync.ToString()
                }
            );
        
            await app.RunAsync();
        }
        catch (Exception exception)
        {
            logger?.LogError("§x{Message:l}§w\n{StackTrace:l}", exception.Message, exception.StackTrace);
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }
    }

    private static void RegisterCommands(IServiceProvider provider)
    {
        var commands = provider.GetServices<ICommand>();
        var registry = provider.GetRequiredService<ICommandRegistry>();

        foreach (var command in commands)
        {
            registry.RegisterCommand(command);
        }
    }

    private static void StartReceiving(IServiceProvider provider, CancellationToken cancellationToken)
    {
        var botClient = provider.GetRequiredService<ITelegramBotClient>();
        var updateHandler = provider.GetRequiredService<IUpdateHandler>();

        botClient.StartReceiving(
            updateHandler.HandleUpdateAsync,
            updateHandler.HandleErrorAsync,
            new ReceiverOptions { AllowedUpdates = [UpdateType.Message, UpdateType.CallbackQuery] },
            cancellationToken
        );
    }
}
