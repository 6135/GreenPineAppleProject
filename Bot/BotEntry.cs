using Discord;
using Discord.WebSocket;
using GreenPineAppleProject.Bot.Services;
using GreenPineAppleProject.Logging;
using Microsoft.Extensions.Configuration;
using System.Runtime.InteropServices;
using System.Threading;

namespace GreenPineAppleProject.Bot
{
    public static class BotEntry
    {
        public static readonly IConfigurationRoot config =
            new ConfigurationBuilder()
               .AddJsonFile(
                    "Config\\appsettings.Development.json",
                    optional: false,
                    reloadOnChange: true
               )
               .Build();

        private static DiscordSocketClient _client;

        public static async Task Main()
        {
            //load config

            await SentryHandler.Sentry(config);

            await BotInit();
        }

        public static async Task BotInit()
        {
            _client = new DiscordSocketClient();

            _client.Log += SentryHandler.LogDiscordEvents;

            var token = config["AppSettings:AppToken"];

            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            // Block this task until the program is closed.
            await Task.Delay(-1);
        }
    }
}