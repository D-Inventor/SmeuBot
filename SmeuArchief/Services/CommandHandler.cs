using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace SmeuArchief.Services
{
    public class CommandHandler
    {
        private readonly IServiceProvider services;
        private readonly CommandService commands;
        private readonly DiscordSocketClient client;
        private readonly LogService logger;
        private readonly Settings settings;
        private readonly RestoreService restoreService;

        public CommandHandler(IServiceProvider services,
                                CommandService commands,
                                DiscordSocketClient client,
                                LogService logger,
                                Settings settings,
                                RestoreService restoreService)
        {
            this.services = services;
            this.commands = commands;
            this.client = client;
            this.logger = logger;
            this.settings = settings;
            this.restoreService = restoreService;
            client.MessageReceived += ReceiveMessageAsync;
        }

        private async Task ReceiveMessageAsync(SocketMessage arg)
        {
            // can only respond to user messages and they must not come from bots and they may not be in the smeu chat channel
            if (!(arg is SocketUserMessage msg)) { return; }
            if (msg.Author.IsBot) { return; }
            if (msg.Channel.Id == settings.SmeuChannelId) { return; }

            if (!restoreService.IsRestoreComplete)
            {
                await msg.Channel.SendMessageAsync("Ik ben nog niet klaar met mijn herstel procedure. Je kunt pas commando's uitvoeren als ik daar mee klaar ben.");
                return;
            }

            SocketCommandContext context = new SocketCommandContext(client, msg);

            int argPos = 0;
            if (msg.HasStringPrefix(settings.CommandPrefix, ref argPos))
            {
                // if the message was a command, try execute it
                IResult result = await commands.ExecuteAsync(context, argPos, services).ConfigureAwait(false);

                // if execution failed, write this to the log
                if (!result.IsSuccess) { await logger.LogAsync(new LogMessage(LogSeverity.Warning, "CommandHandler", $"Attempted to execute command, but failed: {result.ErrorReason}")).ConfigureAwait(false); }
            }
        }
    }
}
