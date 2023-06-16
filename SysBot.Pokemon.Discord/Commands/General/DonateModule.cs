using Discord.Commands;
using System.Threading.Tasks;

namespace SysBot.Pokemon.Discord
{
    public class DonateModule : ModuleBase<SocketCommandContext>
    {
        [Command("donate")]
        [Alias("donation")]
        [Summary("Returns the donation link.")]
        public async Task PingAsync()
        {
            var str = $"Here's the donation link! Thank you for your support {SysCordSettings.Settings.DonationLink}";
            await ReplyAsync(str).ConfigureAwait(false);
        }
    }
}