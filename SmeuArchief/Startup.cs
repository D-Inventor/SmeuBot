﻿using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using SmeuArchief.Services;
using SmeuBase;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SmeuArchief
{
    public class Startup
    {
        public Settings Settings { get; private set; }

        private readonly string settingspath = Path.Combine(Directory.GetCurrentDirectory(), "settings.txt");

        public Startup(string[] args)
        {
            if (!File.Exists(settingspath))
            {
                // if the settings file doesn't exist, create a default one instead, report to user and stop the application
                SimpleSettings.Settings.ToFile<Settings>(null, settingspath);
                Console.WriteLine("No settings file was found, so a default one was created instead. please edit this file and restart the bot.");
                Environment.Exit(-1);
            }

            try
            {
                // try to read the settings from the file
                Settings = SimpleSettings.Settings.FromFile<Settings>(settingspath);
            }
            catch (Exception e)
            {
                // report failure and stop the application
                Console.WriteLine($"Attempted to read settings from settings file, but failed: {e.Message}\n{e.StackTrace}");
                Environment.Exit(-1);
            }
        }

        public static async Task RunAsync(string[] args)
        {
            // create a startup object and start
            Startup startup = new Startup(args);
            await startup.RunAsync().ConfigureAwait(false);
        }

        public async Task RunAsync()
        {
            // create services
            ServiceCollection services = new ServiceCollection();
            ConfigureServices(services);

            // initialise services that require initialisation
            IServiceProvider provider = services.BuildServiceProvider();
            provider.GetRequiredService<LogService>();
            provider.GetRequiredService<CommandHandler>();
            provider.GetRequiredService<SmeuService>();

            // restore state of the bot and start
            await provider.GetRequiredService<RestoreService>().RestoreAsync().ConfigureAwait(false);
            await provider.GetRequiredService<StartupService>().StartAsync().ConfigureAwait(false);
            await Task.Delay(-1).ConfigureAwait(false);
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // add all services
            services.AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = Settings.LogLevel,
            }))
            .AddSingleton(new CommandService(new CommandServiceConfig
            {
                LogLevel = Settings.LogLevel,
                DefaultRunMode = RunMode.Async,
                CaseSensitiveCommands = false,
            }))
            .AddSingleton(Settings)
            .AddSingleton<IContextSettingsProvider>(Settings)
            .AddSingleton<SmeuBaseFactory>()
            .AddSingleton<SmeuService>()
            .AddSingleton<CommandHandler>()
            .AddSingleton<LogService>()
            .AddSingleton<RestoreService>()
            .AddSingleton<StartupService>();
        }
    }
}
