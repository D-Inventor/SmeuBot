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
                // create an embed
                string prefix = settings.CommandPrefix;
                EmbedBuilder eb = new EmbedBuilder
                {
                    Color = new Color(255, 255, 255),
                    Description = "Dit zijn de commando's die je kunt gebruiken",
                };

                // take all command modules
                foreach (var module in commands.Modules)
                {
                    string description = null;
                    foreach (var cmd in module.Commands)
                    {
                        // add all command signatures to the value
                        var result = await cmd.CheckPreconditionsAsync(Context);
                        if (result.IsSuccess) { description += $"{prefix}{cmd.Aliases.First()} {string.Join(" ", cmd.Parameters.Select(p => $"[{p.Name}]"))}\n"; }
                    }

                    if (!string.IsNullOrWhiteSpace(description))
                    {
                        // create field from module if it has commands
                        eb.AddField(x =>
                        {
                            x.Name = module.Name;
                            x.Value = description;
                            x.IsInline = false;
                        });
                    }
                }

                // send embed to discord
                await ReplyAsync(embed: eb.Build()).ConfigureAwait(false);
            }
        }

        [Command("help")]
        public async Task GetHelp([Remainder, Name("Commando")]string command)
        {
            using (var typing = Context.Channel.EnterTypingState())
            {
                // grab the requested command
                var result = commands.Search(Context, command);

                if (!result.IsSuccess)
                {
                    // tell user if requested command does not exist
                    await ReplyAsync($"Het spijt me, maar ik kan geen commando vinden die **{command}** heet.").ConfigureAwait(false);
                    return;
                }

                // create an embed
                string prefix = settings.CommandPrefix;
                EmbedBuilder eb = new EmbedBuilder
                {
                    Color = new Color(255, 255, 255),
                    Description = $"Deze commando's lijken op **{command}**"
                };

                foreach (var match in result.Commands)
                {
                    // create a field for each match with the parameters and the description
                    var cmd = match.Command;

                    eb.AddField(x =>
                    {
                        x.Name = string.Join(", ", cmd.Aliases);
                        x.Value = $"Parameters: {string.Join(", ", cmd.Parameters.Select(p => p.Name))}\n" +
                                  $"Beschrijving: {cmd.Summary}";
                        x.IsInline = false;
                    });
                }

                // send the embed to the user
                await ReplyAsync(embed: eb.Build()).ConfigureAwait(false);
            }
        }
    }
}
