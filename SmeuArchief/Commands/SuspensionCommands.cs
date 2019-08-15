using Discord.Commands;
using Discord.WebSocket;
using SmeuBase;
using System.Threading.Tasks;
using System.Linq;

namespace SmeuArchief.Commands
{
    public partial class SmeuModule : ModuleBase<SocketCommandContext>
    {
        [Command("tik"), Summary("Tik iemand aan zodat ie weer mee mag doen.")]
        public async Task Unsuspend([Name("Gebruiker")]SocketUser user)
        {
            using (var typing = Context.Channel.EnterTypingState())
            {
                if (user == Context.User)
                {
                    // users are not allowed to unsuspend themselves
                    await ReplyAsync("Het is niet toegestaan om jezelf af te tikken, stiekemerd!").ConfigureAwait(false);
                    return;
                }

                // try unsuspend given user and return if this was succesful or not
                if (await smeuService.UnsuspendAsync(user.Id, Context.User.Id).ConfigureAwait(false)) { await ReplyAsync($"{user.Mention} is niet meer af!").ConfigureAwait(false); }
                else { await ReplyAsync("Deze gebruiker kan niet afgetikt worden omdat deze niet af is!").ConfigureAwait(false); }
            }
        }

        [Command("af"), Summary("Gebruik deze als je denkt dat iemand af is!")]
        public async Task Suspend([Name("Gebruiker")]SocketUser user, [Name("Smeu"), Remainder]string smeu)
        {
            using(var typing = Context.Channel.EnterTypingState())
            {
                // find the given smeu in the duplicates list
                Duplicate duplicate;
                using(SmeuContext database = smeuBaseFactory.GetSmeuBase())
                {
                    duplicate = (from d in database.Duplicates
                                 where d.Author == user.Id && d.Suspension == null && d.Original.Smeu == smeu
                                 select d).FirstOrDefault();
                }

                if(duplicate == null)
                {
                    // if no such duplicate exists, return this to the user
                    await ReplyAsync("Nee, dat klopt niet.").ConfigureAwait(false);
                    return;
                }

                // try suspend the user on this ground
                if(await smeuService.SuspendAsync(user.Id, Context.User.Id, $"{smeu} is al eerder genoemd.", duplicate))
                {
                    // reply with success
                    await ReplyAsync($"{user.Mention} is nu **af**!").ConfigureAwait(false);
                }
                else
                {
                    // reply with failure
                    await ReplyAsync($"{user.Username} is al af!").ConfigureAwait(false);
                }
            }
        }
    }
}
