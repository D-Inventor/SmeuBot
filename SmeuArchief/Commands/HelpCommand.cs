using Discord;
using Discord.Commands;
using System.Linq;
using System.Threading.Tasks;

namespace SmeuArchief.Commands
{
    [Name("Help module")]
    public class HelpCommand : ModuleBase<SocketCommandContext>
    {
        private readonly CommandService commands;
        private readonly Settings settings;

        public HelpCommand(CommandService commands, Settings settings)
        {
            this.commands = commands;
            this.settings = settings;
        }

        [Command("help"), Summary("Dit commando laat zien wat je allemaal kunt met deze bot.")]
        public async Task GetHelp()
        {
            using (var typing = Context.Channel.EnterTypingState())
            {
                string prefix = settings.CommandPrefix;
                EmbedBuilder eb = new EmbedBuilder
                {
                    Color = new Color(255, 255, 255),
                    Description = "Dit zijn de commando's die je kunt gebruiken",
                };

                foreach (var module in commands.Modules)
                {
                    string description = null;
                    foreach (var cmd in module.Commands)
                    {
                        var result = await cmd.CheckPreconditionsAsync(Context);
                        if (result.IsSuccess) { description += $"{prefix}{cmd.Aliases.First()} {string.Join(" ", cmd.Parameters.Select(p => $"[{p.Name}]"))}\n"; }
                    }

                    if (!string.IsNullOrWhiteSpace(description))
                    {
                        eb.AddField(x =>
                        {
                            x.Name = module.Name;
                            x.Value = description;
                            x.IsInline = false;
                        });
                    }
                }

                await ReplyAsync(embed: eb.Build()).ConfigureAwait(false);
            }
        }

        [Command("help")]
        public async Task GetHelp([Remainder, Name("Commando")]string command)
        {
            using (var typing = Context.Channel.EnterTypingState())
            {
                var result = commands.Search(Context, command);

                if (!result.IsSuccess)
                {
                    await ReplyAsync($"Het spijt me, maar ik kan geen commando vinden die **{command}** heet.").ConfigureAwait(false);
                    return;
                }

                string prefix = settings.CommandPrefix;
                EmbedBuilder eb = new EmbedBuilder
                {
                    Color = new Color(255, 255, 255),
                    Description = $"Deze commando's lijken op **{command}**"
                };

                foreach (var match in result.Commands)
                {
                    var cmd = match.Command;

                    eb.AddField(x =>
                    {
                        x.Name = string.Join(", ", cmd.Aliases);
                        x.Value = $"Parameters: {string.Join(", ", cmd.Parameters.Select(p => p.Name))}\n" +
                                  $"Beschrijving: {cmd.Summary}";
                        x.IsInline = false;
                    });
                }

                await ReplyAsync(embed: eb.Build()).ConfigureAwait(false);
            }
        }
    }
}
