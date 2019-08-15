using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace SmeuArchief.Services
{
    public class LogService
    {
        private readonly DiscordSocketClient client;
        private readonly CommandService commands;

        public LogService(DiscordSocketClient client, CommandService commands)
        {
            this.client = client;
            this.commands = commands;

            this.client.Log += LogAsync;
            this.commands.Log += LogAsync;
        }

        public async Task LogAsync(LogMessage arg)
        {
            await Console.Out.WriteLineAsync($"[{arg.Severity.ToString().PadLeft(8)}] {DateTime.UtcNow} [{arg.Source.PadRight(15)}] {arg.Message}").ConfigureAwait(false);
            if (arg.Exception != null) { await LogExceptionAsync(arg.Exception).ConfigureAwait(false); }
        }

        private async Task LogExceptionAsync(Exception exception)
        {
            await Console.Out.WriteLineAsync().ConfigureAwait(false);
            await Console.Out.WriteLineAsync($"{exception.Message}\n{exception.StackTrace}").ConfigureAwait(false);

            if(exception.InnerException != null) { await LogExceptionAsync(exception.InnerException).ConfigureAwait(false); }
        }
    }
}
