﻿using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace SmeuArchief.Services
{
    public class StartupService
    {
        private readonly IServiceProvider services;
        private readonly DiscordSocketClient client;
        private readonly CommandService commands;
        private readonly Settings settings;

        public StartupService(IServiceProvider services, DiscordSocketClient client, CommandService commands, Settings settings)
        {
            this.services = services;
            this.client = client;
            this.commands = commands;
            this.settings = settings;
        }

        public async Task StartAsync()
        {
            if (string.IsNullOrWhiteSpace(settings.Token))
            {
                await Console.Out.WriteLineAsync("The given token is invalid. Please insert a valid token in the settings file and restart.");
                Environment.Exit(-1);
            }

            await client.LoginAsync(TokenType.Bot, settings.Token);
            await client.StartAsync();

            await commands.AddModulesAsync(Assembly.GetEntryAssembly(), services);
        }
    }
}
