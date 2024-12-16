using Discord;
using GreenPineAppleProject.Bot;
using Microsoft.Extensions.Configuration;

namespace GreenPineAppleProject.Logging
{
    public static class SentryHandler
    {
        public static async Task Sentry(IConfigurationRoot config) => SentrySdk.Init(options =>
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

        public static Task LogDiscordEvents(LogMessage log)
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
    }
}