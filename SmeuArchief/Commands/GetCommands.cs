using Discord;
using Discord.Commands;
using Discord.WebSocket;
using SmeuArchief.Utilities;
using SmeuBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmeuArchief.Commands
{
    public partial class SmeuModule : ModuleBase<SocketCommandContext>
    {
        [Group("krijg"), Alias("bekom", "get"), Summary("Commando groep om informatie te krijgen."), Name("Info module")]
        public class GetCommands : ModuleBase<SocketCommandContext>
        {
            private readonly SmeuBaseFactory smeuBaseFactory;
            private readonly DiscordSocketClient client;

            public GetCommands(SmeuBaseFactory smeuBaseFactory, DiscordSocketClient client)
            {
                this.smeuBaseFactory = smeuBaseFactory;
                this.client = client;
            }

            [Command("schorsingen"), Alias("suspensions"), Summary("Krijg informatie over wie er nu af is.")]
            public async Task GetSuspensions()
            {
                using (var typing = Context.Channel.EnterTypingState())
                {
                    using (SmeuContext database = smeuBaseFactory.GetSmeuBase())
                    {
                        // get all suspensions
                        var dbresult = from s in database.Suspensions
                                       where s.Revoker == null
                                       select s;

                        // if nobody is suspended, notify the user about that
                        if (dbresult.Count() == 0) { await ReplyAsync("Er is op dit moment helemaal niemand af!").ConfigureAwait(false); }
                        else
                        {
                            // present the suspensions in an embed
                            EmbedBuilder eb = new EmbedBuilder()
                                .WithTitle("Deze mensen zijn **af**:")
                                .WithColor(Color.DarkRed);
                            foreach (Suspension suspension in dbresult)
                            {
                                eb.AddField(client.GetUser(suspension.User).Username, suspension);
                            }
                            await ReplyAsync(embed: eb.Build()).ConfigureAwait(false);
                        }
                    }
                }
            }

            [Command("smeu"), Summary("Krijg informatie over de gegeven smeu en smeu die er op lijken")]
            public async Task GetSmeu([Remainder, Name("Smeu")]string input)
            {
                input = input.ToLower();

                await GetSmeuReply(input).ConfigureAwait(false);
            }

            private async Task GetSmeuReply(string input)
            {
                using (var typing = Context.Channel.EnterTypingState())
                {
                    Embed embed = await GatherSmeuData(input).ConfigureAwait(false);
                    await ReplyAsync(embed: embed).ConfigureAwait(false);
                }
            }

            private async Task<Embed> GatherSmeuData(string input)
            {
                // create embed for given word
                EmbedBuilder eb = new EmbedBuilder()
                    .WithTitle($"__{input}__")
                    .WithColor(Color.LightOrange);

                IEnumerable<EmbedFieldBuilder> similarFields = new EmbedFieldBuilder[0];
                IEnumerable<EmbedFieldBuilder> SubmissionFields = new EmbedFieldBuilder[0];

                // Run a task that finds similar smeu
                Task similarSmeuTask = Task.Run(() =>
                {
                    // find similars
                    List<(Submission, float)> similars = GetSimilarSmeu(input);
                    if (similars.Any())
                    {
                        // if there are any similar smeu, create fields for the smeu and their similarity value
                        similarFields = new EmbedFieldBuilder[]
                        {
                            new EmbedFieldBuilder { IsInline = true, Name = "Vergelijkbaar met", Value = string.Join("\n", similars.Select(s => s.Item1.Smeu)) },
                            new EmbedFieldBuilder { IsInline = true, Name = "\u200B", Value = string.Join("\n", similars.Select(x => Math.Round(x.Item2 * 100).ToString() + "%")) }
                        };
                    }
                    else
                    {
                        // if there are no similar smeu, just create one field with indicating that there are no similar smeu
                        similarFields = new EmbedFieldBuilder[]
                        {
                            new EmbedFieldBuilder { IsInline = true, Name = "Vergelijkbaar met", Value = "*Geen vergelijkbare smeu*" }
                        };
                    }
                });

                Task submissionTask = Task.Run(() =>
                {
                    // find existing submission in database
                    Submission submission;
                    using (SmeuContext database = smeuBaseFactory.GetSmeuBase())
                    {
                        submission = (from s in database.Submissions
                                      where s.Smeu == input
                                      select s).FirstOrDefault();
                    }

                    // add potential existing submission to the embed
                    if (submission != null)
                    {
                        SocketUser user = client.GetUser(submission.Author);

                        eb.WithThumbnailUrl(user.GetAvatarUrl());

                        SubmissionFields = new EmbedFieldBuilder[]
                        {
                            new EmbedFieldBuilder { Name="Autheur", Value=user.Username},
                            new EmbedFieldBuilder {Name="Datum", Value=$"{submission.Date:d-MMMM-yyyy H:mm} UTC"},
                            new EmbedFieldBuilder {Name="\u200B", Value="\u200B"}
                        };
                    }
                });

                await Task.WhenAll(similarSmeuTask, submissionTask).ConfigureAwait(false);

                eb.WithFields(SubmissionFields.Concat(similarFields));

                // return the result
                return eb.Build();
            }

            private List<(Submission, float)> GetSimilarSmeu(string input)
            {
                using (SmeuContext database = smeuBaseFactory.GetSmeuBase())
                {
                    var dbresult = (from s in database.Submissions
                                    let d = Levenshtein.GetLevenshteinDistance(s.Smeu, input)
                                    where s.Smeu != input && d <= 4
                                    orderby d, s.Smeu
                                    select new { Submission = s, Similarity = Levenshtein.GetSimilarity(input, s.Smeu, d) }).Take(5);

                    List<(Submission, float)> similars = new List<(Submission, float)>(5);
                    foreach(var a in dbresult)
                    {
                        similars.Add((a.Submission, a.Similarity));
                    }

                    return similars;
                }
            }
        }
    }
}
