using Discord.Commands;
using System.Threading.Tasks;

namespace SmeuArchief.Commands
{
    [Name("Smeu module")]
    public partial class SmeuModule : ModuleBase<SocketCommandContext>
    {
        [Command("ping"), Summary("Laat je weten of de bot nog leeft")]
        public async Task Ping()
        {
            using (var typing = Context.Channel.EnterTypingState())
            {
                await ReplyAsync($"Hallo {Context.User.Mention}!").ConfigureAwait(false);
            }
        }
    }
}
