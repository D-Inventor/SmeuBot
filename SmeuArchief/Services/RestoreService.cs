using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using SmeuBase;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SmeuArchief.Services
{
    public class RestoreService
    {
        private readonly DiscordSocketClient client;
        private readonly SmeuBaseFactory smeuBaseFactory;
        private readonly Settings settings;
        private readonly LogService logger;

        public RestoreService(DiscordSocketClient client, SmeuBaseFactory smeuBaseFactory, Settings settings, LogService logger)
        {
            this.client = client;
            this.smeuBaseFactory = smeuBaseFactory;
            this.settings = settings;
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

            await AddPendingSmeuToChat();
        }

        private async Task AddPendingSmeuToChat()
        {
            // get the smeu channel and add all the un-assigned smeu to it
            if (!(client.GetChannel(settings.SmeuChannelId) is IMessageChannel smeuChannel))
            {
                await logger.LogAsync(new LogMessage(LogSeverity.Warning, "RestoreService", $"Could not restore pending smeu, because something went wrong while acquiring the smeu text channel")).ConfigureAwait(false);
                return;
            }

            using (SmeuContext database = smeuBaseFactory.GetSmeuBase())
            {
                // get all unassigned smeu from the chat
                var dbresult = from s in database.Submissions
                                where s.MessageId == 0
                                select s;

                await logger.LogAsync(new LogMessage(LogSeverity.Info, "RestoreService", $"Found {dbresult.Count()} smeu to restore.")).ConfigureAwait(false);
                foreach (Submission submission in dbresult)
                {
                    // send all unassigned smeu to the chat and update the database
                    IUserMessage msg = await smeuChannel.SendMessageAsync(submission.Smeu).ConfigureAwait(false);
                    submission.MessageId = msg.Id;
                    database.Submissions.Update(submission);
                    try
                    {
                        await database.SaveChangesAsync().ConfigureAwait(false);
                    }
                    catch(DbUpdateException e)
                    {
                        await logger.LogAsync(new LogMessage(LogSeverity.Error, "RestoreService", "Attempted to save restore to database, but failed.", e)).ConfigureAwait(false);
                    }
                }
                await database.SaveChangesAsync().ConfigureAwait(false);
                await logger.LogAsync(new LogMessage(LogSeverity.Info, "RestoreService", "All smeu are restored")).ConfigureAwait(false);
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
