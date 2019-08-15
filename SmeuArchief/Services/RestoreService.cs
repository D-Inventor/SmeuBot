using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using SmeuBase;
using System;
using System.Threading.Tasks;

namespace SmeuArchief.Services
{
    public class RestoreService
    {
        private readonly DiscordSocketClient client;
        private readonly SmeuBaseFactory smeuBaseFactory;
        private readonly LogService logger;

        public RestoreService(DiscordSocketClient client, SmeuBaseFactory smeuBaseFactory, LogService logger)
        {
            this.client = client;
            this.smeuBaseFactory = smeuBaseFactory;
            this.logger = logger;

            client.Ready += SetStateMessageAsync;
            client.Ready += RestorePendingSmeuAsync;
        }

        private async Task SetStateMessageAsync()
        {
            // set the activity state to 'Watching all your activities'
            await client.SetGameAsync("all your activities", type: ActivityType.Watching).ConfigureAwait(false);
        }

        private async Task RestorePendingSmeuAsync()
        {
            foreach (var guild in client.Guilds)
            {
                // log all the guilds that are restored.
                await logger.LogAsync(new LogMessage(LogSeverity.Info, "RestoreService", $"Restoring {guild.Name}")).ConfigureAwait(false);

                // TODO: Actually add restore logic
            }
        }

        public async Task RestoreAsync()
        {
            await logger.LogAsync(new LogMessage(LogSeverity.Info, "RestoreService", "Start database migration")).ConfigureAwait(false);
            try
            {
                // try to migrate the database to the latest version
                using (SmeuContext context = smeuBaseFactory.GetSmeuBase())
                {
                    context.Database.Migrate();
                }
            }
            catch (Exception e)
            {
                // log failure and stop the application
                await logger.LogAsync(new LogMessage(LogSeverity.Critical, "RestoreService", "Attempted to migrate the database, but failed.", e)).ConfigureAwait(false);
                Environment.Exit(-1);
            }
            await logger.LogAsync(new LogMessage(LogSeverity.Info, "RestoreService", "Database migrated")).ConfigureAwait(false);
        }
    }
}
