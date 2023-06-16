using Discord.Commands;
using System.Threading.Tasks;

namespace SysBot.Pokemon.Discord
{
    public class TeamBuilderModule : ModuleBase<SocketCommandContext>
    {
        [Command("teambuilder")]
        [Summary("Returns the link to team builder.")]
        public async Task PingAsync()
        {
            var str = $"Here's the link to teambuilder {SysCordSettings.Settings.TeamBuilderLink}";
            await ReplyAsync(str).ConfigureAwait(false);
        }
    }
}