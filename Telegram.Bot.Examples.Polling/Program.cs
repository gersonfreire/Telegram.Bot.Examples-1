using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;

using Microsoft.Extensions.Configuration;

namespace Telegram.Bot.Examples.Polling;

public static class Program
{
    private static TelegramBotClient? Bot;

    public static async Task Main(string[] args)
    {
        #region Get bot token from 3 posible sources

        // first, from configuration class
        string botToken = Configuration.BotToken;

        #region second, from appsettings.json
        string settingsFileName = "appsettings.json";
        var value = System.Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
        if(value?.ToString().ToLower() == "dev")
        {
            settingsFileName = "appsettings_dev.json";
        }

        IConfiguration config = new ConfigurationBuilder()
            .AddJsonFile(settingsFileName)
            .Build();

        botToken = config.GetSection("botToken").Value; 
        #endregion

        // third, from command line argument
        if(args.Length > 0)
        {
            botToken = args[0];
        } 
        #endregion

        Bot = new TelegramBotClient(botToken);

        User me = await Bot.GetMeAsync();
        Console.Title = me.Username ?? "My awesome Bot";

        using var cts = new CancellationTokenSource();

        // StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
        ReceiverOptions receiverOptions = new() { AllowedUpdates = { } };
        Bot.StartReceiving(Handlers.HandleUpdateAsync,
                           Handlers.HandleErrorAsync,
                           receiverOptions,
                           cts.Token);

        Console.WriteLine($"Start listening for @{me.Username}");
        Console.ReadLine();

        // Send cancellation request to stop bot
        cts.Cancel();
    }
}
