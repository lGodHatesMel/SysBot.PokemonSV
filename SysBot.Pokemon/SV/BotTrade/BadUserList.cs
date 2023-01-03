using System.Collections.Generic;

namespace SysBot.Pokemon
{
    /// <summary>
    /// A list of users that have or are actively trying to get bots softbanned by leaving trades early (2+ attempts).
    /// There is currently no mitigation for being softbanned so we just leave the trade if we see one of these traders.
    /// There is probably a more elegant way to do this, but this is the best we have while the game is new.
    /// </summary>
    public static class BadUserList
    {
        public static IReadOnlyList<ulong> Users { get; } = new ulong[]
        {
            17966876822808517034,
            15519648569467555184,
            15108690203704459588,
            15108690203704459588,
            9785186464643638297,
        };
    }
}