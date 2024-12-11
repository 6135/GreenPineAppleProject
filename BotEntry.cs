﻿using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;

namespace GreenPineAppleProject
{
    public class BotEntry
    {
        public static IConfigurationRoot config;
        private static DiscordSocketClient _client;
        public static async Task Main()
        {
            //load config
            config = new ConfigurationBuilder()
               .AddJsonFile("appsettings.Development.json", optional: false, reloadOnChange: true)
               .Build();
            await Sentry();

            await BotInit();
        }

        private static async Task Sentry() => SentrySdk.Init(options =>
        {
            // A Sentry Data Source Name (DSN) is required.
            // See https://docs.sentry.io/product/sentry-basics/dsn-explainer/
            // You can set it in the SENTRY_DSN environment variable, or you can set it in code here.
            options.Dsn = config["SentrySettings:Dsn"];

            // When debug is enabled, the Sentry client will emit detailed debugging information to the console.
            // This might be helpful, or might interfere with the normal operation of your application.
            // We enable it here for demonstration purposes when first trying Sentry.
            // You shouldn't do this in your applications unless you're troubleshooting issues with Sentry.
            options.Debug = Boolean.Parse(config["SentrySettings:Debug"] ?? "false");

            // This option is recommended. It enables Sentry's "Release Health" feature.
            options.AutoSessionTracking = true;

            // Set TracesSampleRate to 1.0 to capture 100%
            // of transactions for tracing.
            // We recommend adjusting this value in production.
            options.TracesSampleRate = 1.0;

            // Sample rate for profiling, applied on top of othe TracesSampleRate,
            // e.g. 0.2 means we want to profile 20 % of the captured transactions.
            // We recommend adjusting this value in production.
            options.ProfilesSampleRate = 1.0;
            // Requires NuGet package: Sentry.Profiling
            // Note: By default, the profiler is initialized asynchronously. This can
            // be tuned by passing a desired initialization timeout to the constructor.
            options.AddIntegration(new ProfilingIntegration(
                // During startup, wait up to 500ms to profile the app startup code.
                // This could make launching the app a bit slower so comment it out if you
                // prefer profiling to start asynchronously
                TimeSpan.FromMilliseconds(500)
            ));
        });

        private static Task LogDiscordEvents(LogMessage log)
        {
            // Log all events to the console
            Console.WriteLine(log.ToString());

            //Convert Discord.NET log severity to Sentry log level
            SentryLevel sentryLevel = log.Severity switch
            {
                LogSeverity.Critical => SentryLevel.Fatal,
                LogSeverity.Error => SentryLevel.Error,
                LogSeverity.Warning => SentryLevel.Warning,
                LogSeverity.Info => SentryLevel.Info,
                LogSeverity.Verbose => SentryLevel.Debug,
                LogSeverity.Debug => SentryLevel.Debug,
                _ => SentryLevel.Debug
            };

            // Forward criticals/fatals,errors and warnings to sentry
            if (sentryLevel == SentryLevel.Fatal || sentryLevel == SentryLevel.Error || sentryLevel == SentryLevel.Warning)
            {
                SentrySdk.CaptureMessage(log.ToString(), sentryLevel);
            }
            return Task.CompletedTask;
        }



        public static async Task BotInit()
        {
            _client = new DiscordSocketClient();

            _client.Log += LogDiscordEvents;

            //  You can assign your bot token to a string, and pass that in to connect.
            //  This is, however, insecure, particularly if you plan to have your code hosted in a public repository.
            var token = config["AppSettings:AppToken"];

            // Some alternative options would be to keep your token in an Environment Variable or a standalone file.
            // var token = Environment.GetEnvironmentVariable("NameOfYourEnvironmentVariable");
            // var token = File.ReadAllText("token.txt");
            // var token = JsonConvert.DeserializeObject<AConfigurationClass>(File.ReadAllText("config.json")).Token;

            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            // Block this task until the program is closed.
            await Task.Delay(-1);
        }
    }
}