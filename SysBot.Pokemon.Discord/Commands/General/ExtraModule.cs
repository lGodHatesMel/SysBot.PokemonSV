using Discord.Commands;
using System.Threading.Tasks;

namespace SysBot.Pokemon.Discord
{
    public class ExtraModule : ModuleBase<SocketCommandContext>
    {
        [Command("donate")]
        [Alias("donation", "kofi")]
        [Summary("Returns the donation link.")]
        public async Task DonateLinkAsync()
        {
            var str = $"Here's the donation link! Thank you for your support {SysCordSettings.Settings.DonationLink}";
            await ReplyAsync(str).ConfigureAwait(false);
        }

        [Command("stream")]
        [Alias("streamlink")]
        [Summary("Returns the stream link.")]
        public async Task StreamLinkAsync()
        {
            var str = $"Here's the Stream link, enjoy {SysCordSettings.Settings.StreamLink}";
            await ReplyAsync(str).ConfigureAwait(false);
        }

        [Command("teambuilder")]
        [Summary("Returns the link to team builder.")]
        public async Task TeamBuilderLinkAsync()
        {
            var str = $"Here's the link to teambuilder {SysCordSettings.Settings.TeamBuilderLink}";
            await ReplyAsync(str).ConfigureAwait(false);
        }
    }
}