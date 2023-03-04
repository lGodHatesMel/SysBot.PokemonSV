using System;
using System.Collections.Generic;
using System.Text;

namespace SysBot.Pokemon
{
    public static class PokeDataOffsetsSV
    {
        public const string ScarletID = "0100A3D008C5C000";
        public const string VioletID = "01008F6008C5E000";

        public static IReadOnlyList<long> BoxStartPokemonPointer { get; } = new long[] { 0x44CCA68, 0x110, 0x9B0, 0x00 };
        public static IReadOnlyList<long> MyStatusPointer { get; } = new long[] { 0x44A98C8, 0x100, 0x40 };

        public static IReadOnlyList<long> LinkTradePartnerPokemonPointer { get; } = new long[] { 0x44CCB50, 0x120, 0x1C8, 0xB8, 0x10, 0x30, 0x00 };
        public static IReadOnlyList<long> LinkTradePartnerNameSlot1Pointer { get; } = new long[] { 0x44CCBB0, 0x28, 0xB0, 0x00 };
        public static IReadOnlyList<long> LinkTradePartnerNameSlot2Pointer { get; } = new long[] { 0x44CCBB0, 0x28, 0xE0, 0x00 };
        public static IReadOnlyList<long> LinkTradePartnerNIDPointer { get; } = new long[] { 0x44C7730, 0xF8, 0x08 };

        public static IReadOnlyList<long> KeyboardBufferPointer { get; } = new long[] { 0x437F908, 0x30, 0x00 }; 

        public static IReadOnlyList<long> PokePortalPointer { get; } = new long[] { 0x43F3538, 0x00, 0x3C0, 0x3C0, 0x598 };

        public const int BoxFormatSlotSize = 0x158;
        public const int TradeFormatSlotSize = 0x148;
    }
}
