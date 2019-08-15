using Discord.Commands;
using SmeuArchief.Services;
using SmeuBase;
using System.Linq;
using System.Threading.Tasks;

namespace SmeuArchief.Commands
{
    public partial class SmeuModule : ModuleBase<SocketCommandContext>
    {
        private readonly SmeuService smeuService;
        private readonly SmeuBaseFactory smeuBaseFactory;

        public SmeuModule(SmeuService smeuService, SmeuBaseFactory smeuBaseFactory)
        {
            this.smeuService = smeuService;
            this.smeuBaseFactory = smeuBaseFactory;
        }

        [Command("herstel"), Summary("Als je per ongeluk een woord verkeerd hebt geschreven, dan kun je hem hiermee weer weghalen.")]
        public async Task Recover([Remainder, Name("Smeu")]string input)
        {
            using (var typing = Context.Channel.EnterTypingState())
            {
                input = input.ToLower();

                // remove it from the database
                if(await smeuService.RemoveAsync(input, Context.User.Id).ConfigureAwait(false))
                {
                    await ReplyAsync($"{input} is hersteld!").ConfigureAwait(false);
                }
                else
                {
                    await ReplyAsync($"Ik kon geen inzending van jouw vinden voor {input}").ConfigureAwait(false);
                }
            }
        }
    }
}
