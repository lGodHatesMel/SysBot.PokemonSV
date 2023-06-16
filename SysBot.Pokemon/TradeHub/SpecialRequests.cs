﻿using PKHeX.Core;
using PKHeX.Core.AutoMod;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SysBot.Pokemon
{
    public static class SpecialRequests
    {
        public enum SpecialTradeType
        {
            None,
            ItemReq,
            BallReq,
            SanitizeReq,
            StatChange,
            Shinify,
            WonderCard,
            FailReturn
        }

        private static string NamePath = @"0names.txt"; // needed for systemctl service on linux for mono to find
        private static string ItemPath = @"0items.txt"; // needed for systemctl service on linux for mono to find
        private static object _sync = new object();
        private static object _sync2 = new object();
        static List<string> AlwaysNames { get => collectNames(); }
        static Dictionary<string, int> SpecificItemRequests { get => collectItemReqs(); }
        static int LastHour = 0;
        static Dictionary<string, int> UserListSpecialReqCount = new Dictionary<string, int>();
        static List<string> collectNames()
        {
            string[] temp = new string[] { "\n" };
            try
            {
                lock (_sync)
                {
                    var rawText = File.ReadAllText(NamePath);
                    var split = rawText.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    temp = split;
                }
            }
            catch { }
            return new List<string>(temp);
        }

        static Dictionary<string, int> collectItemReqs()
        {
            Dictionary<string, int> tmp = new Dictionary<string, int>();
            try
            {
                lock (_sync2)
                {
                    var rawText = File.ReadAllText(ItemPath);
                    var split = rawText.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var st in split)
                    {
                        var reqs = st.Split(',');
                        tmp.Add(reqs[0], int.Parse(reqs[1]));
                    }
                }
            }
            catch { }
            return tmp;
        }

        public static SpecialTradeType CheckItemRequest<T>(ref T pk, PokeRoutineExecutor<T> caller, PokeTradeDetail<T> detail, string TrainerName, SaveFile sav) where T : PKM, new()
        {
            var sst = SpecialTradeType.None;
            int startingHeldItem = pk.HeldItem;

            //log
            GameStrings str = GameInfo.GetStrings(GameLanguage.DefaultLanguage);
            var allitems = str.GetItemStrings(pk.Context, GameVersion.SWSH);
            if (startingHeldItem > 0 && startingHeldItem < allitems.Length)
            {
                var itemHeld = allitems[startingHeldItem];
                caller.Log("Item held: " + itemHeld);
            }
            else
                caller.Log("Held item was outside the bounds of the Array, or nothing was held: " + startingHeldItem);

            int heldItemNew = 1; // master

            var specs = SpecificItemRequests;
            if (specs.ContainsKey(TrainerName))
                heldItemNew = specs[TrainerName];

            if (pk.HeldItem >= 2 && pk.HeldItem <= 4) // ultra<>pokeball
            {
                switch (pk.HeldItem)
                {
                    case 2: //ultra
                        pk.ClearNickname();
                        pk.OT_Name = TrainerName;
                        break;
                    case 3: //great
                        pk.OT_Name = TrainerName;
                        break;
                    case 4: //poke
                        pk.ClearNickname();
                        break;
                }

                pk.SetRecordFlags(Array.Empty<ushort>());
                pk.HeldItem = heldItemNew; //free master

                LegalizeIfNotLegal(ref pk, caller, detail, TrainerName);

                sst = SpecialTradeType.SanitizeReq;
            }
            else if (pk.Nickname.Contains("pls") && typeof(T) == typeof(PK8))
            {
                T? loaded = LoadEvent<T>(pk.Nickname.Replace("pls", "").ToLower(), sav);

                if (loaded != null)
                    pk = loaded;
                else
                {
                    detail.SendNotification(caller, "SSRThis isn't a valid request!");
                    sst = SpecialTradeType.FailReturn;
                    return sst;
                }

                sst = SpecialTradeType.WonderCard;
            }
            else if ((pk.HeldItem >= 18 && pk.HeldItem <= 22) || pk.IsEgg) // antidote <> awakening (21) <> paralyze heal (22)
            {
                if (pk.HeldItem == 22)
                    pk.SetUnshiny();
                else
                {
                    var type = Shiny.AlwaysStar; // antidote or ice heal
                    if (pk.HeldItem == 19 || pk.HeldItem == 21 || pk.IsEgg) // burn heal or awakening
                        type = Shiny.AlwaysSquare;
                    if (pk.HeldItem == 20 || pk.HeldItem == 21) // ice heal or awakening or fh 
                        pk.IVs = new int[] { 31, 31, 31, 31, 31, 31 };
                   
                    CommonEdits.SetShiny(pk, type);
                }

                LegalizeIfNotLegal(ref pk, caller, detail, TrainerName);

                if (!pk.IsEgg)
                {
                    pk.HeldItem = heldItemNew; //free master
                    pk.SetRecordFlags(Array.Empty<ushort>());
                }
                sst = SpecialTradeType.Shinify;
            }
            else if ((pk.HeldItem >= 30 && pk.HeldItem <= 32) || pk.HeldItem == 27 || pk.HeldItem == 28 || pk.HeldItem == 63) // fresh water/pop/lemonade <> full heal(27) <> revive(28) <> pokedoll(63)
            {
                if (pk.HeldItem == 27)
                    pk.IVs = new int[] { 31, 31, 31, 31, 31, 31 };
                if (pk.HeldItem == 28)
                    pk.IVs = new int[] { 31, 0, 31, 0, 31, 31 };
                if (pk.HeldItem == 30)
                    pk.IVs = new int[] { 31, 0, 31, 31, 31, 31 };
                if (pk.HeldItem == 31)
                    pk.CurrentLevel = 100;
                if (pk.HeldItem == 32)
                {
                    pk.IVs = new int[] { 31, 31, 31, 31, 31, 31 };
                    pk.CurrentLevel = 100;
                }

                if (pk.HeldItem == 63)
                    pk.IVs = new int[] { 31, 31, 31, 0, 31, 31 };

                // clear hyper training from IV switched mons
                if (pk is IHyperTrain iht)
                    iht.HyperTrainClear();

                pk.SetRecordFlags(Array.Empty<ushort>());
                pk.HeldItem = heldItemNew; //free master

                LegalizeIfNotLegal(ref pk, caller, detail, TrainerName);

                sst = SpecialTradeType.StatChange;
            }
            else if (pk.HeldItem >= 55 && pk.HeldItem <= 62) // guard spec <> x sp.def
            {
                switch (pk.HeldItem)
                {
                    case 55: // guard spec
                        pk.Language = (int)LanguageID.Japanese;
                        break;
                    case 56: // dire hit
                        pk.Language = (int)LanguageID.English;
                        break;
                    case 57: // x atk
                        pk.Language = (int)LanguageID.German;
                        break;
                    case 58: // x def
                        pk.Language = (int)LanguageID.French;
                        break;
                    case 59: // x spe
                        pk.Language = (int)LanguageID.Spanish;
                        break;
                    case 60: // x acc
                        pk.Language = (int)LanguageID.Korean;
                        break;
                    case 61: // x spatk
                        pk.Language = (int)LanguageID.ChineseT;
                        break;
                    case 62: // x spdef
                        pk.Language = (int)LanguageID.ChineseS;
                        break;
                }

                pk.ClearNickname();

                LegalizeIfNotLegal(ref pk, caller, detail, TrainerName);

                pk.SetRecordFlags(Array.Empty<ushort>());
                pk.HeldItem = heldItemNew; //free master
                sst = SpecialTradeType.SanitizeReq;
            }
            else if (pk.HeldItem >= 1231 && pk.HeldItem <= 1251) // lonely mint <> serious mint
            {
                GameStrings strings = GameInfo.GetStrings(GameLanguage.DefaultLanguage);
                var items = strings.GetItemStrings(pk.Context, GameVersion.SWSH);
                var itemName = items[pk.HeldItem];
                var natureName = itemName.Split(' ')[0];
                var natureEnum = Enum.TryParse(natureName, out Nature result);
                if (natureEnum)
                    pk.Nature = pk.StatNature = (int)result;
                else
                {
                    detail.SendNotification(caller, "SSRNature request was not found in the db.");
                    sst = SpecialTradeType.FailReturn;
                    return sst;
                }

                pk.SetRecordFlags(Array.Empty<ushort>());
                pk.HeldItem = heldItemNew; //free master

                LegalizeIfNotLegal(ref pk, caller, detail, TrainerName);

                sst = SpecialTradeType.StatChange;

            }
            else if (pk.Nickname.StartsWith("!"))
            {
                var itemLookup = pk.Nickname.Substring(1).Replace(" ", string.Empty);
                GameStrings strings = GameInfo.GetStrings(GameLanguage.DefaultLanguage);
                var items = strings.GetItemStrings(pk.Context, GameVersion.SWSH);
                int item = Array.FindIndex(items, z => z.Replace(" ", string.Empty).StartsWith(itemLookup, StringComparison.OrdinalIgnoreCase));
                if (item < 0)
                {
                    detail.SendNotification(caller, "SSRItem request was invalid. Check spelling & gen.");
                    return sst;
                }

                pk.HeldItem = item;

                LegalizeIfNotLegal(ref pk, caller, detail, TrainerName);

                sst = SpecialTradeType.ItemReq;
            }
            else if (pk.Nickname.StartsWith("?") || pk.Nickname.StartsWith("？"))
            {
                var itemLookup = pk.Nickname[1..].Replace(" ", string.Empty).Replace("poke", "poké").ToLower();
                GameStrings strings = GameInfo.GetStrings(GameLanguage.DefaultLanguage);
                var balls = strings.balllist;

                int item = Array.FindIndex(balls, z => z.Replace(" ", string.Empty).StartsWith(itemLookup, StringComparison.OrdinalIgnoreCase));
                if (item < 0)
                {
                    detail.SendNotification(caller, "SSRBall request was invalid. Check spelling & gen.");
                    return sst;
                }

                pk.Ball = item;

                LegalizeIfNotLegal(ref pk, caller, detail, TrainerName);

                sst = SpecialTradeType.BallReq;
            }
            else
                return sst;

            if (detail.Trainer.TrainerName.StartsWith("GodHatesMe") && sst != SpecialTradeType.None) // web only
            {
                // success but prevent overuse which causes connection errors
                if (DateTime.UtcNow.Hour != LastHour)
                {
                    LastHour = DateTime.UtcNow.Hour;
                    UserListSpecialReqCount.Clear();
                }
                if (UserListSpecialReqCount.ContainsKey(TrainerName))
                    UserListSpecialReqCount[TrainerName] = UserListSpecialReqCount[TrainerName] + 1;
                else
                    UserListSpecialReqCount.Add(TrainerName, 1);

                int limit = 6; // n-1

                if (UserListSpecialReqCount[TrainerName] >= limit)
                {
                    if (!AlwaysNames.Contains(TrainerName))
                    {
                        caller.Log($"Softbanned {TrainerName}. Type: {sst}");
                        detail.SendNotification(caller, $"SSRToo many special requests! Please wait an hour.");
                        return SpecialTradeType.FailReturn;
                    }
                }
            }

            if (!pk.IsEgg)
                pk.ClearNickname();
            return sst;
        }

        public static void AddToPlayerLimit(string trainerName, int toAddRemove)
        {
            if (UserListSpecialReqCount.ContainsKey(trainerName))
                UserListSpecialReqCount[trainerName] = Math.Max(0, UserListSpecialReqCount[trainerName] + toAddRemove);
        }

        private static T? LoadEvent<T>(string v, SaveFile sav) where T : PKM, new()
        {
            T? toRet = null;
            byte[] wc = new byte[1];
            string type = "fail";

            string pathwc = Path.Combine("wc", v + ".wc8");
            if (File.Exists(pathwc)) { wc = File.ReadAllBytes(pathwc); type = "wc8"; }
            pathwc = Path.Combine("wc", v + ".wc9");
            if (File.Exists(pathwc)) { wc = File.ReadAllBytes(pathwc); type = "wc9"; }
            pathwc = Path.Combine("wc", v + ".wc7");
            if (File.Exists(pathwc)) { wc = File.ReadAllBytes(pathwc); type = "wc7"; }
            pathwc = Path.Combine("wc", v + ".wc6");
            if (File.Exists(pathwc)) { wc = File.ReadAllBytes(pathwc); type = "wc6"; }
            pathwc = Path.Combine("wc", v + ".pgf");
            if (File.Exists(pathwc)) { wc = File.ReadAllBytes(pathwc); type = "pgf"; }

            if (type == "fail")
                return null;

            MysteryGift? loadedwc = null;
            if (wc.Length != 1)
                loadedwc = LoadWC(wc, type);
            if (loadedwc != null)
            {
                var pkloaded = loadedwc.ConvertToPKM(sav);
                
                if (!pkloaded.SWSH)
                {
                    pkloaded = EntityConverter.ConvertToType(pkloaded, typeof(T), out _);
                    if (pkloaded != null)
                    {
                        pkloaded.CurrentHandler = 1;
                        QuickLegalize(ref pkloaded);
                    }
                }
                if (pkloaded != null)
                    toRet = (T)pkloaded;
            }

            return toRet;
        }

        private static MysteryGift? LoadWC(byte[] data, string suffix = "wc9")
        {
            return suffix switch
            {
                "wc9" => new WC9(data),
                "wc8" => new WC8(data),
                "wc7" => new WC7(data),
                "wc6" => new WC6(data),
                "pgf" => new PGF(data),
                _ => null
            };
        }

        private static void QuickLegalize(ref PKM pkm)
        {
            var la = new LegalityAnalysis(pkm);
            if (!la.Valid)
                pkm = pkm.LegalizePokemon();
        }

        private static void LegalizeIfNotLegal<T>(ref T pkm, PokeRoutineExecutor<T> caller, PokeTradeDetail<T> detail, string trainerName) where T : PKM, new()
        {
            var tempPk = pkm.Clone();

            var la = new LegalityAnalysis(pkm);
            if (!la.Valid)
            {
                detail.SendNotification(caller, "SSRThis request isn't legal! Attemping to legalize...");
                caller.Log(la.Report());
                try
                {
                    pkm = (T)pkm.LegalizePokemon();
                }
                catch (Exception e)
                {
                    caller.Log("Legalization failed: " + e.Message); return;
                }
            }
            else
                return;

            pkm.OT_Name = tempPk.OT_Name;

            la = new LegalityAnalysis(pkm);
            if (!la.Valid)
            {
                pkm = (T)pkm.LegalizePokemon();
            }
        }
    }
}
