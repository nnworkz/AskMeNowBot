using System.Data.Common;

using AskMeNowBot.Command;
using AskMeNowBot.Command.Registry;
using AskMeNowBot.Command.Standard;
using AskMeNowBot.Configuration;
using AskMeNowBot.Database;
using AskMeNowBot.Database.Column;
using AskMeNowBot.Database.MySql;
using AskMeNowBot.Database.MySql.Column;
using AskMeNowBot.Database.PostgreSql;
using AskMeNowBot.Database.PostgreSql.Column;
using AskMeNowBot.Economy.Billing;
using AskMeNowBot.Economy.Converter;
using AskMeNowBot.Exceptions;
using AskMeNowBot.Filter;
using AskMeNowBot.Formatter;
using AskMeNowBot.Handler;
using AskMeNowBot.Handler.Sub;
using AskMeNowBot.Localization;
using AskMeNowBot.Moderation;
using AskMeNowBot.Utils.BotLinker;
using AskMeNowBot.Utils.Encryption;
using AskMeNowBot.Utils.TextFormat;

using CryptoPay;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using MySqlConnector;

using Npgsql;

using Serilog;

using Telegram.Bot;
using Telegram.Bot.Polling;

namespace AskMeNowBot;

public static class DependencyInjection
{
    public static async Task Configure(IServiceCollection services)
    {
        var sink = new LoggerConfiguration().MinimumLevel.Information().WriteTo;
        var logger = sink.Console(new ColorFormatter(new Color())).CreateLogger();

        try
        {
            var config = await new YamlConfig().LoadAsync();

            if (!config.IsValid())
            {
                throw new ConfigNotInitializedException(config.GetPath());
            }

            services.AddControllers();
            services.AddLogging(
                builder =>
                {
                    builder.ClearProviders();
                    builder.AddSerilog(logger, true);
                }
            );

            ConfigureDatabase(services, config);

            services.AddSingleton(config);

            services.AddSingleton<ICommand, StartCommand>();
            services.AddSingleton<ICommand, LanguageCommand>();
            services.AddSingleton<ICommand, SendCommand>();
            services.AddSingleton<ICommand, SubscriptionCommand>();
            services.AddSingleton<ICommand, BanCommand>();
            services.AddSingleton<ICommand, UnbanCommand>();
            services.AddSingleton<ICommand, LinkCommand>();
            services.AddSingleton<ICommand, ProfileCommand>();
            services.AddSingleton<ICommand, FilterCommand>();

            services.AddSingleton<ICommandRegistry, BaseCommandRegistry>();

            services.AddSingleton<StartCommand>();
            services.AddSingleton<LanguageCommand>();
            services.AddSingleton<SendCommand>();
            services.AddSingleton<SubscriptionCommand>();
            services.AddSingleton<BanCommand>();
            services.AddSingleton<UnbanCommand>();
            services.AddSingleton<LinkCommand>();
            services.AddSingleton<ProfileCommand>();
            services.AddSingleton<FilterCommand>();

            services.AddSingleton<ITelegramBotClient>(new TelegramBotClient(config.Bot.Credentials.Token));

            services.AddTransient<IUpdateHandler, BaseUpdateHandler>();

            services.AddSingleton<ICommandContext, BaseCommandContext>();

            services.AddSingleton<MessageHandler>();
            services.AddSingleton<CallbackQueryHandler>();

            services.AddTransient<ILocale, BaseLocale>();

            services.AddSingleton<IBotLinker, BaseBotLinker>();
            services.AddSingleton<ITextFormat, BaseTextFormat>();
            services.AddSingleton<IEncryption, BaseEncryption>();
            services.AddSingleton<IConverter, ExchangeRateConverter>();

            services.AddSingleton<IBilling, BaseBilling>();

            services.AddSingleton<IModeration, BaseModeration>();
            services.AddSingleton<ITextFilter, BaseTextFilter>();

            services.AddSingleton<ICryptoPayClient>(
                _ =>
                {
                    var httpClient = new HttpClient { BaseAddress = new Uri("https://testnet-pay.crypt.bot/") };
                    httpClient.DefaultRequestHeaders.Add("Crypto-Pay-API-Token", config.CryptoPay.Token);

                    return new CryptoPayClient(httpClient);
                }
            );
        }
        catch (Exception exception)
        {
            logger.Fatal("§x{Message:l}§w\n{StackTrace:l}", exception.Message, exception.StackTrace);

            throw;
        }
    }

    private static void ConfigureDatabase(IServiceCollection services, IConfig config)
    {
        var type = Enum.Parse<DatabaseType>(config.Database.Type, true);
        DbDataSource dataSource;

        switch (type)
        {
            case DatabaseType.MySql:
            {
                dataSource = new MySqlDataSource(config.Database.ConnectionStrings.MySql);

                services.AddSingleton<IDatabase, MySqlDatabase>();
                services.AddSingleton<IProvider, MySqlProvider>();
                services.AddSingleton<IUsersColumn, MySqlUsersColumn>();
                services.AddSingleton<ISubscriptionsColumn, MySqlSubscriptionsColumn>();
                services.AddSingleton<IWaitsColumn, MySqlWaitsColumn>();
                services.AddSingleton<IBansColumn, MySqlBansColumn>();
                services.AddSingleton<ILinksColumn, MySqlLinksColumn>();
                services.AddSingleton<ITransactionsColumn, MySqlTransactionsColumn>();
                services.AddSingleton<IFiltersColumn, MySqlFiltersColumn>();

                break;
            }
            case DatabaseType.PostgreSql:
            {
                dataSource = NpgsqlDataSource.Create(config.Database.ConnectionStrings.PostgreSql);

                services.AddSingleton<IDatabase, PostgreSqlDatabase>();
                services.AddSingleton<IProvider, PostgreSqlProvider>();
                services.AddSingleton<IUsersColumn, PostgreSqlUsersColumn>();
                services.AddSingleton<ISubscriptionsColumn, PostgreSqlSubscriptionColumn>();
                services.AddSingleton<IWaitsColumn, PostgreSqlWaitsColumn>();
                services.AddSingleton<IBansColumn, PostgreSqlBansColumn>();
                services.AddSingleton<ILinksColumn, PostgreSqlLinksColumn>();
                services.AddSingleton<ITransactionsColumn, PostgreSqlTransactionsColumn>();
                services.AddSingleton<IFiltersColumn, PostgreSqlFiltersColumn>();

                break;
            }
            default:
            {
                throw new InvalidDatabaseTypeException(config.Database.Type.ToLower());
            }
        }

        services.AddSingleton<IQueries>(new Queries(type));
        services.AddSingleton(dataSource);
    }
}
