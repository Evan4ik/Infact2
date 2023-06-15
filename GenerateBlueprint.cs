using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using APIPlugin;
using InscryptionAPI.Card;
using InscryptionCommunityPatch.Card;
using BepInEx;
using DiskCardGame;
using HarmonyLib;
using Pixelplacement;
using UnityEngine;
using UnityEngine.UIElements;
using static DiskCardGame.EncounterBlueprintData;
using InscryptionAPI.Guid;

namespace infact2
{
    public class MakeBluep
    {
        public static readonly AbilityMetaCategory Part2Modular = (AbilityMetaCategory)GuidManager.GetEnumValue<AbilityMetaCategory>("cyantist.inscryption.api", "Part2Modular");
        private static readonly string[] BountyNames = new string[]
{
            "Barry",
            "Bolt",
            "Gear",
            "Zap",
            "Rust",
            "Clain",
            "Clank",
            "Bonk",
            "Tank",
            "Gun",
            "Shoot",
            "Maksim",
            "Wilkin",
            "Kaycee",
            "Hobbes",
            "Grind",
            "Blast",
            "Crash",
            "Moon",
            "Zip",
            "Jerry",
            "Plasma",
            "Jimmy",
            "Silence",
            "Never",
            "Hunt",
            "Hunter",
            "Doom",
            "Const",
            "Boom",
            "West"
};
        private static readonly string[] BountySuffix = new string[]
        {
            "son",
            "stein",
            "dottir",
            "vic",
            "berg",
            "sky",
            "ski",
            "sin",
            "sim",
            "fellow",
            "ed",
            " II",
            " III"
        };
        private static readonly string[] BountyPrefix = new string[]
        {
            "Mac",
            "Mc",
            "Von ",
            "Van ",
            "Sir ",
            "Madame "
        };
        public enum Rarity
        {
            Unset,
            Common,
            Rare
        }
        public enum EncounterType
        {
            Basic,
            Rush,
            Spam
        }
        public static int returnCardPowerLevel(CardInfo card)
        {
            int attack = card.Attack;
            int health = card.Health / 2;
            int sigilLevels = 0;
            foreach (Ability ability in card.abilities)
            {
                sigilLevels += AbilitiesUtil.GetInfo(ability).powerLevel;
            }
            int power = attack + health + sigilLevels;
            if (card.HasCardMetaCategory(CardMetaCategory.Rare))
            {
                power += 5;
            }
            power += card.BloodCost;
            power += card.bonesCost / 3;
            power += card.energyCost / 2;
            power += card.gemsCost.Count;
            return power;

        }
        public static float returnBank(string cost, CardInfo card)
        {
            float costed = 0;
            switch (cost)
            {
                case "bones":
                    costed += card.bonesCost;
                    costed += card.energyCost;
                    costed += (float)card.BloodCost * 4f;
                    costed += (float)card.gemsCost.Count * 3f;
                    break;
                case "blood":
                    costed += card.BloodCost;
                    costed += (float)card.bonesCost / 4f;
                    costed += (float)card.energyCost / 4f;
                    costed += (float)card.gemsCost.Count * 0.75f;
                    break;
                case "energy":
                    costed += card.energyCost;
                    costed += (float)card.BloodCost * 4f;
                    costed += card.bonesCost;
                    costed += (float)card.gemsCost.Count * 3f;
                    break;
                case "mox":
                    costed += card.gemsCost.Count;
                    costed += (float)card.bonesCost / 3f;
                    costed += (float)card.energyCost / 3f;
                    costed += (float)card.BloodCost * 1.333333333333f;
                    break;
            }
            return costed;
        }
        public static T RandomElement<T>(List<T> list)
        {
            return list[UnityEngine.Random.Range(0, list.Count)];
        }
        public static void GenerateSaveHunters()
        {
            string bountyHunters = "0;";
            for (int i = 0; i < 5; i++)
            {
                bountyHunters += BountyPrefix[UnityEngine.Random.RandomRangeInt(0, BountyPrefix.Length)] + BountyNames[UnityEngine.Random.RandomRangeInt(0, BountyNames.Length)];
                if (UnityEngine.Random.RandomRangeInt(0, 100) > 30)
                {
                    bountyHunters += BountySuffix[UnityEngine.Random.RandomRangeInt(0, BountySuffix.Length)];
                }
                bountyHunters += ",0";
                for (int j = 0; j < 4; j++)
                {
                    bountyHunters += "," + UnityEngine.Random.RandomRangeInt(0, 4);
                }
                bountyHunters += ",-1";
                bountyHunters += ";";
            }
            Debug.Log(bountyHunters);
            SaveData.bountyHunters = bountyHunters;
        }

        public static CardInfo GenerateBountyHunter()
        {
            CardInfo BountyHunter = CardLoader.GetCardByName("infact2_BOUNTYHUNTER");

            int stars = Convert.ToInt32(Math.Floor(SaveData.bountyStars));
            int points = (stars * 4) + SaveData.floor;

            List<AbilityInfo> infos = ScriptableObjectLoader<AbilityInfo>.AllData.FindAll((AbilityInfo x) => x.metaCategories.Contains(Part2Modular) && x.opponentUsable);
            CardModificationInfo mod = CardInfoGenerator.CreateRandomizedAbilitiesStatsMod(infos, points, 1, 1);

            if (infact2.Plugin.functionsnstuff.getTemple(SaveData.roomId) == CardTemple.Wizard && UnityEngine.Random.value <= ((float)stars * 0.33f)) { mod.gemify = true; }

            mod.bountyHunterInfo = new BountyHunterInfo();
            mod.bountyHunterInfo.dialogueIndex = UnityEngine.Random.RandomRangeInt(1, 6);
            BountyHunter.mods.Add(mod);
            return BountyHunter;
        }

        //Returns a random card from the provided pool that meets the set parameters.
        public static CardInfo FindCardForTempleAndCost(List<CardInfo> pool, CardTemple temple, float minCostTier, float maxCostTier, int minAtkPower, int minHP, int maxAtkPower, int maxHP, Rarity rarity, Trait reqTrait = Trait.None, Trait excludedTrait = Trait.None)
        {
            List<CardInfo> revisedPool = new List<CardInfo>();
            revisedPool.AddRange(pool);

            string costType = "";
            switch (temple)
            {
                case CardTemple.Nature: costType = "blood"; break;
                case CardTemple.Undead: costType = "bones"; break;
                case CardTemple.Wizard: costType = "mox"; break;
                case CardTemple.Tech: costType = "energy"; break;
            }
            revisedPool.RemoveAll(x => x.temple != temple);
            foreach (CardInfo c in revisedPool)
            {
                //Debug.Log($"    {c.name}");

            }
            revisedPool.RemoveAll(x => returnBank(costType, x) >= maxCostTier || returnBank(costType, x) <= minCostTier);
            revisedPool.RemoveAll(x => (x.Attack < minAtkPower) || (x.Health < minHP));
            if (maxHP > 0) revisedPool.RemoveAll(x => x.Health > maxHP);
            if (maxAtkPower > -1) revisedPool.RemoveAll(x => x.Attack > maxAtkPower);
            if (reqTrait != Trait.None)
            {
                revisedPool.RemoveAll(x => !x.traits.Contains(reqTrait));
            }
            if (excludedTrait != Trait.None)
            {
                revisedPool.RemoveAll(x => x.traits.Contains(excludedTrait));
            }
            if (rarity == Rarity.Rare) { revisedPool.RemoveAll(x => !x.metaCategories.Contains(CardMetaCategory.Rare)); }
            else if (rarity == Rarity.Common) { revisedPool.RemoveAll(x => x.metaCategories.Contains(CardMetaCategory.Rare)); }

            if (revisedPool != null && revisedPool.Count > 0)
            {
                CardInfo chosen = revisedPool.GetRandomItem();
                //Debug.Log($"Chose: { chosen.name }");
                return chosen;
            }
            Debug.LogError($"(InfAct2) Could not find a card within the set parameters: {temple} {minCostTier} {maxCostTier} {minAtkPower} {rarity} {reqTrait} {excludedTrait}");
            return null;
        }

        //Takes lists for junk, early, mid, and lategame cards, and fills them with a set number of cards which match the scaling of the set region.
        public static void AssignCardsForFloorAndRegion(List<CardInfo> basePool, int floor, CardTemple temple, List<CardInfo> junkCards, int numJunk, List<CardInfo> earlyGameCards, int numEarly, List<CardInfo> midGameCards, int numMid, List<CardInfo> lateGameCards, int numLate, bool isBoss)
        {
            // Debug.Log("Internal");
            switch (temple)
            {
                case CardTemple.Undead:
                    for (int i = 0; i < numJunk; i++) { junkCards.Add(FindCardForTempleAndCost(basePool, CardTemple.Undead, -1, 2f, 0, -1, -1, 1, isBoss ? Rarity.Unset : Rarity.Common)); }
                    if (floor < 5)
                    {
                        for (int i = 0; i < numEarly; i++) { earlyGameCards.Add(FindCardForTempleAndCost(basePool, CardTemple.Undead, -1, 3, 1, -1, 3, 0, isBoss ? Rarity.Unset : Rarity.Common)); }
                    }
                    else
                    {
                        for (int i = 0; i < numEarly; i++) { earlyGameCards.Add(FindCardForTempleAndCost(basePool, CardTemple.Undead, (floor / 5), (4 + floor / 5), 0, -1, -1, 0, Rarity.Unset)); }
                    }

                    if (floor < 10)
                    {
                        for (int i = 0; i < numMid; i++) midGameCards.Add(FindCardForTempleAndCost(basePool, CardTemple.Undead, (3 + floor / 4), (6 + floor / 3.5f), 0, -1, -1, 0, isBoss ? Rarity.Unset : Rarity.Common));
                        for (int i = 0; i < numLate; i++) lateGameCards.Add(FindCardForTempleAndCost(basePool, CardTemple.Undead, ((8 + floor) / 2.2f), ((11 + floor) / 1.6f), 0, -1, -1, 0, isBoss ? Rarity.Unset : Rarity.Common));
                    }
                    else
                    {
                        for (int i = 0; i < numMid; i++) midGameCards.Add(FindCardForTempleAndCost(basePool, CardTemple.Undead, 4, 10, 0, -1, -1, 0, Rarity.Unset));
                        if (floor <= 34)
                        {
                            for (int i = 0; i < numLate; i++) lateGameCards.Add(FindCardForTempleAndCost(basePool, CardTemple.Undead, ((8 + (6 + floor / 2)) / 2.2f), ((11 + (6 + floor / 2)) / 1.6f), 0, -1, -1, 0, Rarity.Unset));
                        }
                        else
                        {
                            for (int i = 0; i < numLate; i++) lateGameCards.Add(FindCardForTempleAndCost(basePool, CardTemple.Undead, 11, 100, 0, -1, -1, 0, Rarity.Unset));
                        }
                    }

                    if (floor > 39)
                    {
                        for (int i = 0; i < numEarly; i++) earlyGameCards.Add(FindCardForTempleAndCost(basePool, CardTemple.Undead, 5, 10, 0, -1, -1, 0, Rarity.Unset));
                        for (int i = 0; i < numMid; i++) midGameCards.Add(FindCardForTempleAndCost(basePool, CardTemple.Undead, 10, 20, 0, -1, -1, 0, Rarity.Unset));
                        for (int i = 0; i < numLate; i++) lateGameCards.Add(FindCardForTempleAndCost(basePool, CardTemple.Undead, 11, 100, 0, -1, -1, 0, Rarity.Unset));
                    }
                    break;
                case CardTemple.Nature:
                    for (int i = 0; i < numJunk; i++) { junkCards.Add(FindCardForTempleAndCost(basePool, CardTemple.Nature, -1, 0.75f, 0, -1, -1, 1, isBoss ? Rarity.Unset : Rarity.Common)); }
                    if (floor < 10)
                    {
                        for (int i = 0; i < numEarly; i++) earlyGameCards.Add(FindCardForTempleAndCost(basePool, CardTemple.Nature, 0, 1.5f, 1, -1, -1, 0, isBoss ? Rarity.Unset : Rarity.Common));
                        for (int i = 0; i < numMid; i++) midGameCards.Add(FindCardForTempleAndCost(basePool, CardTemple.Nature, 1.75f, 2.5f, 0, -1, -1, 0, isBoss ? Rarity.Unset : Rarity.Common));
                        for (int i = 0; i < numLate; i++) lateGameCards.Add(FindCardForTempleAndCost(basePool, CardTemple.Nature, 1.75f, 2.5f, 0, -1, -1, 0, isBoss ? Rarity.Unset : Rarity.Common));
                    }
                    else if (floor < 40)
                    {
                        for (int i = 0; i < numEarly; i++) earlyGameCards.Add(FindCardForTempleAndCost(basePool, CardTemple.Nature, 0, 2, 1, -1, -1, 0, Rarity.Unset));
                        for (int i = 0; i < numMid; i++) midGameCards.Add(FindCardForTempleAndCost(basePool, CardTemple.Nature, 1, 4, 0, -1, -1, 0, Rarity.Unset));
                        for (int i = 0; i < numLate; i++) lateGameCards.Add(FindCardForTempleAndCost(basePool, CardTemple.Nature, 1.75f, 100, 0, -1, -1, 0, isBoss ? Rarity.Unset : Rarity.Common));
                    }
                    else
                    {
                        for (int i = 0; i < numEarly; i++) earlyGameCards.Add(FindCardForTempleAndCost(basePool, CardTemple.Nature, 1.75f, 5, 1, -1, -1, 0, Rarity.Unset));
                        for (int i = 0; i < numMid; i++) midGameCards.Add(FindCardForTempleAndCost(basePool, CardTemple.Nature, 1.75f, 100, 0, -1, -1, 0, Rarity.Unset));
                        for (int i = 0; i < numLate; i++) lateGameCards.Add(FindCardForTempleAndCost(basePool, CardTemple.Nature, 2.75f, 100, 0, -1, -1, 0, Rarity.Unset));
                    }
                    break;

                case CardTemple.Tech:
                    for (int i = 0; i < numJunk; i++) { junkCards.Add(FindCardForTempleAndCost(basePool, CardTemple.Tech, -1, 3f, 0, -1, -1, 2, isBoss ? Rarity.Unset : Rarity.Common)); }
                    if (floor < 10)
                    {
                        for (int i = 0; i < numEarly; i++) earlyGameCards.Add(FindCardForTempleAndCost(basePool, CardTemple.Tech, 0, 4, 1, -1, -1, 0, isBoss ? Rarity.Unset : Rarity.Common));
                    }
                    else if (floor < 40)
                    {
                        for (int i = 0; i < numEarly; i++) earlyGameCards.Add(FindCardForTempleAndCost(basePool, CardTemple.Tech, 1, 5, 1, -1, -1, 0, Rarity.Unset));
                    }
                    else
                    {
                        for (int i = 0; i < numEarly; i++) earlyGameCards.Add(FindCardForTempleAndCost(basePool, CardTemple.Tech, 3, 100, 0, -1, -1, 0, Rarity.Unset));
                    }

                    if (floor < 7)
                    {
                        for (int i = 0; i < numMid; i++) midGameCards.Add(FindCardForTempleAndCost(basePool, CardTemple.Tech, 2, 6, 1, -1, -1, 0, isBoss ? Rarity.Unset : Rarity.Common));
                    }
                    else if (floor < 40)
                    {
                        for (int i = 0; i < numMid; i++) midGameCards.Add(FindCardForTempleAndCost(basePool, CardTemple.Tech, 3, 100, 1, -1, -1, 0, Rarity.Unset));
                    }
                    else
                    {
                        for (int i = 0; i < numMid; i++) midGameCards.Add(FindCardForTempleAndCost(basePool, CardTemple.Tech, 5, 100, 0, -1, -1, 0, Rarity.Unset));
                    }

                    if (floor < 7)
                    {
                        for (int i = 0; i < numLate; i++) lateGameCards.Add(FindCardForTempleAndCost(basePool, CardTemple.Tech, 4, 100, 0, -1, -1, 0, isBoss ? Rarity.Unset : Rarity.Common));
                    }
                    else
                    {
                        for (int i = 0; i < numLate; i++) lateGameCards.Add(FindCardForTempleAndCost(basePool, CardTemple.Tech, 5, 100, 0, -1, -1, 0, Rarity.Unset));
                    }

                    break;

                case CardTemple.Wizard:
                    for (int i = 0; i < numJunk; i++) { junkCards.Add(FindCardForTempleAndCost(basePool, CardTemple.Wizard, -1, 1f, 0, -1, -1, 1, isBoss ? Rarity.Unset : Rarity.Common)); }
                    if (floor < 7)
                    {
                        for (int i = 0; i < numEarly; i++) earlyGameCards.Add(FindCardForTempleAndCost(basePool, CardTemple.Wizard, -1, 2, 0, -1, -1, 0, isBoss ? Rarity.Unset : Rarity.Common, Trait.None, Trait.Gem));
                        for (int i = 0; i < numMid; i++) midGameCards.Add(FindCardForTempleAndCost(basePool, CardTemple.Wizard, 0, 100, UnityEngine.Random.value < 0.5f ? 1 : 0, -1, -1, 0, isBoss ? Rarity.Unset : Rarity.Common, Trait.None, Trait.Gem));
                        for (int i = 0; i < numLate; i++) lateGameCards.Add(FindCardForTempleAndCost(basePool, CardTemple.Wizard, 0, 100, 1, -1, -1, 0, Rarity.Unset, Trait.None, Trait.Gem));
                    }
                    else
                    {
                        for (int i = 0; i < numEarly; i++) earlyGameCards.Add(FindCardForTempleAndCost(basePool, CardTemple.Wizard, 0, 100, 0, -1, -1, 0, isBoss ? Rarity.Unset : Rarity.Common, Trait.None, Trait.Gem));
                        for (int i = 0; i < numMid; i++) midGameCards.Add(FindCardForTempleAndCost(basePool, CardTemple.Wizard, 0, 100, 1, -1, -1, 0, Rarity.Unset, Trait.None, Trait.Gem));
                        for (int i = 0; i < numLate; i++) lateGameCards.Add(FindCardForTempleAndCost(basePool, CardTemple.Wizard, 0, 100, 2, -1, -1, 0, Rarity.Unset, Trait.None, Trait.Gem));
                    }
                    break;
            }
        }

        //Generates a single turns data using the turn number, floor, number of cards to be played, and lists of what cards can be played in the encounter
        public static List<CardBlueprint> GenerateTurnData(EncounterType type, int turn, int floor, int numberOfCards,
                List<CardInfo> junkCards, List<CardInfo> earlyGameCards, List<CardInfo> midGameCards, List<CardInfo> lateGameCards,
                List<CardInfo> earlyGameMox, List<CardInfo> midGameMox, List<CardInfo> lateGameMox, bool useMox)
        {
            List<EncounterBlueprintData.CardBlueprint> blueprint = new List<EncounterBlueprintData.CardBlueprint> { };
            int gameStage = GetGameStage(turn, floor, type);
            if (gameStage == 1) //Early Game
            {
                int spent = 0;
                while (spent < numberOfCards)
                {
                    switch (type)
                    {
                        case EncounterType.Basic:
                            if (UnityEngine.Random.value <= (floor < 5 ? 0.2f : 0.35f) && ((numberOfCards - spent) >= 2))
                            {
                                blueprint.Add(midGameCards.GetRandomItem().CreateBlueprint());
                                spent += 2;
                            }
                            else
                            {
                                blueprint.Add(earlyGameCards.GetRandomItem().CreateBlueprint());
                                spent += 1;
                            }
                            if (UnityEngine.Random.value < 0.1f && junkCards.Count > 0) { blueprint.Add(junkCards.GetRandomItem().CreateBlueprint()); }
                            break;
                        case EncounterType.Rush:
                            if (!(turn % 2 == 0) && !(turn % 3 == 0))
                            {
                                float upgradeTier = UnityEngine.Random.value;
                                if (upgradeTier <= (floor < 5 ? 0.1f : 0.17f) && ((numberOfCards - spent) >= 2)) //Upgrade two midgame cards into a lategame card
                                {
                                    blueprint.Add(lateGameCards.GetRandomItem().CreateBlueprint());
                                    spent += 2;
                                }
                                else if (upgradeTier <= (floor < 5 ? 0.2f : 0.34f)) //Downgrade a midgame card into two early game cards
                                {
                                    blueprint.Add(earlyGameCards.GetRandomItem().CreateBlueprint());
                                    blueprint.Add(earlyGameCards.GetRandomItem().CreateBlueprint());
                                    spent += 1;
                                }
                                else
                                {
                                    blueprint.Add(midGameCards.GetRandomItem().CreateBlueprint());
                                    spent += 1;
                                }
                            }
                            else { spent += 1; }
                            break;
                        case EncounterType.Spam:
                            blueprint.Add(earlyGameCards.GetRandomItem().CreateBlueprint());
                            if (UnityEngine.Random.value < (0.5f + (0.05f * floor))) { blueprint.Add(earlyGameCards.GetRandomItem().CreateBlueprint()); }
                            if (UnityEngine.Random.value < 0.5f && junkCards.Count > 0) { blueprint.Add(junkCards.GetRandomItem().CreateBlueprint()); }
                            spent += 1;
                            break;
                    }
                }

            }
            else if (gameStage == 2) //Midgame
            {
                int spent = 0;
                while (spent < numberOfCards)
                {
                    switch (type)
                    {
                        case EncounterType.Basic:
                            float upgradeTier = UnityEngine.Random.value;
                            if (upgradeTier <= (floor < 5 ? 0.1f : 0.17f) && ((numberOfCards - spent) >= 2)) //Upgrade two midgame cards into a lategame card
                            {
                                blueprint.Add(lateGameCards.GetRandomItem().CreateBlueprint());
                                spent += 2;
                            }
                            else if (upgradeTier <= (floor < 5 ? 0.2f : 0.34f)) //Downgrade a midgame card into two early game cards
                            {
                                blueprint.Add(earlyGameCards.GetRandomItem().CreateBlueprint());
                                blueprint.Add(earlyGameCards.GetRandomItem().CreateBlueprint());
                                spent += 1;
                            }
                            else
                            {
                                blueprint.Add(midGameCards.GetRandomItem().CreateBlueprint());
                                spent += 1;
                            }
                            if (UnityEngine.Random.value < 0.15f && junkCards.Count > 0) { blueprint.Add(junkCards.GetRandomItem().CreateBlueprint()); }
                            break;
                        case EncounterType.Rush:
                            if (UnityEngine.Random.value <= 0.1f)
                            {
                                blueprint.Add(earlyGameCards.GetRandomItem().CreateBlueprint());
                            }
                            spent += 1;
                            if (UnityEngine.Random.value < 0.15f && junkCards.Count > 0) { blueprint.Add(junkCards.GetRandomItem().CreateBlueprint()); }
                            break;
                        case EncounterType.Spam:
                            if (UnityEngine.Random.value < 0.2f) { blueprint.Add(midGameCards.GetRandomItem().CreateBlueprint()); }
                            else
                            {
                                blueprint.Add(earlyGameCards.GetRandomItem().CreateBlueprint());
                                blueprint.Add(earlyGameCards.GetRandomItem().CreateBlueprint());
                            }
                            if (UnityEngine.Random.value < 0.25f && junkCards.Count > 0) { blueprint.Add(junkCards.GetRandomItem().CreateBlueprint()); }
                            spent += 1;
                            break;
                    }

                }
                if (useMox && turn % 3 == 0)
                {
                    blueprint.Add(earlyGameMox.GetRandomItem().CreateBlueprint());
                }
            }
            else //Lategame
            {
                int spent = 0;
                while (spent < numberOfCards)
                {
                    switch (type)
                    {
                        case EncounterType.Basic:
                            if (UnityEngine.Random.value <= (floor < 5 ? 0.2f : 0.35f)) //Downgrade a lategame card into two midgame cards
                            {
                                blueprint.Add(midGameCards.GetRandomItem().CreateBlueprint());
                                blueprint.Add(midGameCards.GetRandomItem().CreateBlueprint());
                                spent += 1;
                            }
                            else
                            {
                                blueprint.Add(lateGameCards.GetRandomItem().CreateBlueprint());
                                spent += 1;
                            }
                            if (UnityEngine.Random.value < 0.05f && junkCards.Count > 0) { blueprint.Add(junkCards.GetRandomItem().CreateBlueprint()); }
                            break;
                        case EncounterType.Rush:
                            if (UnityEngine.Random.value <= 0.2)
                            {
                                blueprint.Add(midGameCards.GetRandomItem().CreateBlueprint());
                            }
                            spent += 1;
                            if (UnityEngine.Random.value < 0.15f && junkCards.Count > 0) { blueprint.Add(junkCards.GetRandomItem().CreateBlueprint()); }
                            break;
                        case EncounterType.Spam:
                            if (UnityEngine.Random.value < 0.2f) { blueprint.Add(lateGameCards.GetRandomItem().CreateBlueprint()); }
                            else
                            {
                                blueprint.Add(midGameCards.GetRandomItem().CreateBlueprint());
                                blueprint.Add(midGameCards.GetRandomItem().CreateBlueprint());
                            }
                            spent += 1;
                            if (UnityEngine.Random.value < 0.25f) { blueprint.Add(earlyGameCards.GetRandomItem().CreateBlueprint()); }
                            break;
                    }

                }
                if (useMox && turn % 3 == 0)
                {
                    blueprint.Add(midGameMox.GetRandomItem().CreateBlueprint());
                }
            }
            return blueprint;
        }

        //Returns 1 if the current battle should be considered early game, 2 if it should be considered midgame, and 3 if it should be considered lategame.
        public static int GetGameStage(int turn, int floor, EncounterType type)
        {
            int stage = 0;

            if (turn < (floor >= 6 ? 4 : 5)) { stage = 1; }
            else if (turn < (floor >= 6 ? 7 : 10)) { stage = 2; }
            else { stage = 3; }
            return stage;
        }

        //Adds a turn to a blueprint, first modifying the turn data to support gem dependancy, conduits, and custom dependancy
        public static void AddTurnToBlueprint(List<CardBlueprint> bp, EncounterBlueprintData data)
        {
            //Handle Gem Dependent 
            if (bp.Exists(x => x.card && x.card.HasAbility(Ability.GemDependant)) && !bp.Exists(x => x.card && x.card.traits.Contains(Trait.Gem)))
            {
                if (UnityEngine.Random.RandomRangeInt(0, 100) > 40 - SaveData.floor * 4)
                {
                    CardTemple relevantTemple = bp.Find(x => x.card && x.card.HasAbility(Ability.GemDependant)).card.temple;

                    List<CardInfo> pixelCards = CardLoader.GetPixelCards();
                    pixelCards.RemoveAll((CardInfo x) => !x.metaCategories.Contains(CardMetaCategory.GBCPack) || x.GetExtendedProperty("InfAct2ExcludeFromBattle") != null);
                    if (relevantTemple != CardTemple.Wizard && pixelCards.Exists(x => x.temple == relevantTemple && x.traits.Contains(Trait.Gem) && returnBank("bones", x) <= 3))
                    {
                        bp.Add(pixelCards.Find(x => x.temple == relevantTemple && x.traits.Contains(Trait.Gem) && returnBank("bones", x) <= 3).CreateBlueprint());
                    }
                    else
                    {
                        bp.Add(CardLoader.GetCardByName(RandomElement(new List<string>() { "MoxEmerald", "MoxRuby", "MoxSapphire" })).CreateBlueprint());
                    }
                }
            }
            //HandleConduits
            if (bp.Exists(x => x.card && x.card.abilities.Exists(y => AbilitiesUtil.GetInfo(y).conduitCell)) && !bp.Exists(x => x.card && x.card.abilities.Exists(y => AbilitiesUtil.GetInfo(y).conduit)))
            {
                if (UnityEngine.Random.RandomRangeInt(0, 100) > 40 - SaveData.floor * 2)
                {
                    CardTemple relevantTemple = bp.Find(x => x.card && x.card.abilities.Exists(y => AbilitiesUtil.GetInfo(y).conduitCell)).card.temple;

                    List<CardInfo> pixelCards = CardLoader.GetPixelCards();
                    pixelCards.RemoveAll((CardInfo x) => !x.metaCategories.Contains(CardMetaCategory.GBCPack) || x.GetExtendedProperty("InfAct2ExcludeFromBattle") != null);
                    if (pixelCards.Exists(x => x.temple == relevantTemple && x.abilities.Exists(y => AbilitiesUtil.GetInfo(y).conduit) && returnBank("bones", x) <= 2))
                    {
                        bp.Add(pixelCards.Find(x => x.temple == relevantTemple && x.abilities.Exists(y => AbilitiesUtil.GetInfo(y).conduit) && returnBank("bones", x) <= 2).CreateBlueprint());
                    }
                    else
                    {
                        bp.Add(CardLoader.GetCardByName("NullConduit").CreateBlueprint());
                    }
                }
            }
            foreach (CardBlueprint c in bp)
            {
                if (c.card != null && c.card.GetExtendedProperty("InfAct2OpponentDependantLogic") != null)
                {
                    if (UnityEngine.Random.RandomRangeInt(0, 100) > 40 - SaveData.floor * 4)
                    {
                        if (!bp.Exists(x => x.card != null && x.card.name == c.card.GetExtendedProperty("InfAct2OpponentDependantLogic")))
                        {
                            CardInfo inf = CardLoader.GetCardByName(c.card.GetExtendedProperty("InfAct2OpponentDependantLogic"));
                            if (inf != null) { bp.Add(inf.CreateBlueprint()); }
                        }
                    }
                }
            }

            data.turns.Add(bp);
        }
        public class baseblueprint : EncounterBlueprintData
        {
            public baseblueprint()
            {
                List<CardInfo> pixelCards = CardLoader.GetPixelCards();
                pixelCards.RemoveAll((CardInfo x) => !x.metaCategories.Contains(CardMetaCategory.GBCPack) || x.GetExtendedProperty("InfAct2ExcludeFromBattle") != null);

                List<CardInfo> junkCards = new List<CardInfo>();
                List<CardInfo> earlyGameCards = new List<CardInfo>();
                List<CardInfo> midGameCards = new List<CardInfo>();
                List<CardInfo> lateGameCards = new List<CardInfo>();

                List<CardInfo> earlyGameMox = new List<CardInfo>() { FindCardForTempleAndCost(pixelCards, CardTemple.Wizard, -1, 1.5f, 0, -1, -1, 0, Rarity.Common, Trait.Gem) };
                List<CardInfo> midGameMox = new List<CardInfo>() { FindCardForTempleAndCost(pixelCards, CardTemple.Wizard, -1, 1.5f, 0, -1, -1, 0, Rarity.Unset, Trait.Gem) };
                List<CardInfo> lateGameMox = new List<CardInfo>() { FindCardForTempleAndCost(pixelCards, CardTemple.Wizard, -1, 5, 0, -1, -1, 0, Rarity.Rare, Trait.Gem) };

                base.name = "baseblueprint";
                this.turns = new List<List<EncounterBlueprintData.CardBlueprint>>();

                CardInfo BountyHunter = GenerateBountyHunter();
                string[] bountyData = SaveData.bountyHunters.Split(';');
                bool bountyEligble = SaveData.bountyStars >= 1 && bountyData[0] == "0" && UnityEngine.Random.RandomRangeInt(0, 100) > 70 - (SaveData.floor * 3 + SaveData.bountyStars * 5f);
                int bountyTurn = UnityEngine.Random.RandomRangeInt(3, 7);

                int numberOfJunkCards = 1;
                if (SaveData.floor < 4) { numberOfJunkCards = 0; }
                else if (UnityEngine.Random.value <= 0.5f) { numberOfJunkCards++; }
                int numberOfEarlyCards = 2;
                if (UnityEngine.Random.value <= 0.2f) { numberOfEarlyCards++; }
                int numberOfMidCards = 2;
                int numberOfLateCards = 2;

                float ranType = UnityEngine.Random.value;
                EncounterType encounterType = EncounterType.Basic;
                if (ranType < 0.33f) { encounterType = EncounterType.Spam; }
                else if (ranType < 0.66f) { encounterType = EncounterType.Rush; }

                CardTemple temple = infact2.Plugin.functionsnstuff.getTemple(SaveData.roomId);

                int boonPunishment = 0;
                if (!string.IsNullOrEmpty(SaveData.boon1)) boonPunishment += 5;
                if (!string.IsNullOrEmpty(SaveData.boon2)) boonPunishment += 5;
                if (!string.IsNullOrEmpty(SaveData.boon3)) boonPunishment += 5;

                //Debug.Log($"Pre ({encounterType})");
                AssignCardsForFloorAndRegion(pixelCards, SaveData.floor + boonPunishment, temple,
                     junkCards, numberOfJunkCards,
                     earlyGameCards, numberOfEarlyCards,
                     midGameCards, numberOfMidCards,
                     lateGameCards, numberOfLateCards,
                    false);
                // Debug.Log("Post");

                int bias = 0;
                //Iterate through 30 turns
                for (int i = 1; i < 30; i++)
                {
                    //Debug.Log($"Turn {i}");
                    int numCardsToQueue = UnityEngine.Random.Range(i == 1 ? 1 : 0, 3);
                    if (SaveData.floor < 3 && numCardsToQueue == 2) { numCardsToQueue = 1; }
                    int reqBias = SaveData.floor > 7 ? 2 : 4;
                    List<CardBlueprint> turnData = null;
                    if (bias > reqBias && i != 1)
                    {
                        turnData = new List<EncounterBlueprintData.CardBlueprint> { new EncounterBlueprintData.CardBlueprint { } };
                        bias = 0;
                    }
                    else if (bias <= -1)
                    {
                        turnData = GenerateTurnData(encounterType, i, SaveData.floor, 2,
                           junkCards,
                           earlyGameCards,
                           midGameCards,
                           lateGameCards,
                           earlyGameMox,
                           midGameMox,
                           lateGameMox,
                           temple == CardTemple.Wizard
                           );
                        bias += numCardsToQueue;
                    }
                    else if (numCardsToQueue == 0)
                    {
                        turnData = new List<EncounterBlueprintData.CardBlueprint> { new EncounterBlueprintData.CardBlueprint { } };
                        bias -= 1;
                    }
                    else
                    {
                        // Debug.Log($"elsed");
                        turnData = GenerateTurnData(encounterType, i, SaveData.floor, numCardsToQueue,
                            junkCards,
                            earlyGameCards,
                            midGameCards,
                            lateGameCards,
                            earlyGameMox,
                            midGameMox,
                            lateGameMox,
                            temple == CardTemple.Wizard
                            );
                        bias += numCardsToQueue;
                    }
                    if (i == bountyTurn && bountyEligble)
                    {
                        //Debug.Log("Add hunter");
                        turnData.Add(BountyHunter.CreateBlueprint());
                    }
                    AddTurnToBlueprint(turnData, this);
                }
            }
        }
        public class bossblueprint : EncounterBlueprintData
        {
            public bossblueprint()
            {
                List<CardInfo> pixelCards = CardLoader.GetPixelCards();
                pixelCards.RemoveAll((CardInfo x) => !x.metaCategories.Contains(CardMetaCategory.GBCPack) || x.GetExtendedProperty("InfAct2ExcludeFromBattle") != null);

                List<CardInfo> junkCards = new List<CardInfo>();
                List<CardInfo> earlyGameCards = new List<CardInfo>();
                List<CardInfo> midGameCards = new List<CardInfo>();
                List<CardInfo> lateGameCards = new List<CardInfo>();

                List<CardInfo> earlyGameMox = new List<CardInfo>() { FindCardForTempleAndCost(pixelCards, CardTemple.Wizard, -1, 1.5f, 0, -1, -1, 0, Rarity.Common, Trait.Gem) };
                List<CardInfo> midGameMox = new List<CardInfo>() { FindCardForTempleAndCost(pixelCards, CardTemple.Wizard, -1, 1.5f, 0, -1, -1, 0, Rarity.Unset, Trait.Gem) };
                List<CardInfo> lateGameMox = new List<CardInfo>() { FindCardForTempleAndCost(pixelCards, CardTemple.Wizard, -1, 5, 0, -1, -1, 0, Rarity.Rare, Trait.Gem) };

                base.name = "baseblueprint";
                this.turns = new List<List<EncounterBlueprintData.CardBlueprint>>();

                CardInfo BountyHunter = GenerateBountyHunter();
                string[] bountyData = SaveData.bountyHunters.Split(';');
                bool bountyEligble = SaveData.bountyStars >= 1 && bountyData[0] == "0" && UnityEngine.Random.RandomRangeInt(0, 100) > 70 - (SaveData.floor * 3 + SaveData.bountyStars * 5f);
                int bountyTurn = UnityEngine.Random.RandomRangeInt(3, 7);

                int numberOfJunkCards = 0;
                int numberOfEarlyCards = 2;
                if (UnityEngine.Random.value <= 0.2f) { numberOfEarlyCards++; }
                int numberOfMidCards = 2;
                int numberOfLateCards = 2;

                float ranType = UnityEngine.Random.value;
                EncounterType encounterType = EncounterType.Basic;
                if (ranType < 0.33f) { encounterType = EncounterType.Spam; }
                else if (ranType < 0.66f) { encounterType = EncounterType.Rush; }

                CardTemple temple = infact2.Plugin.functionsnstuff.getTemple(SaveData.roomId);

                int boonPunishment = 0;
                if (!string.IsNullOrEmpty(SaveData.boon1)) boonPunishment += 5;
                if (!string.IsNullOrEmpty(SaveData.boon2)) boonPunishment += 5;
                if (!string.IsNullOrEmpty(SaveData.boon3)) boonPunishment += 5;

                AssignCardsForFloorAndRegion(pixelCards, SaveData.floor + 5 + boonPunishment, temple,
                     junkCards, numberOfJunkCards,
                     earlyGameCards, numberOfEarlyCards,
                     midGameCards, numberOfMidCards,
                     lateGameCards, numberOfLateCards,
                    true);

                int bias = 0;
                //Iterate through 30 turns
                for (int i = 1; i < 30; i++)
                {
                    int numCardsToQueue = UnityEngine.Random.Range(i == 1 ? 1 : 0, 3);
                    int reqBias = SaveData.floor > 7 ? 2 : 4;
                    List<CardBlueprint> turnData = null;
                    if (bias > reqBias && i != 1)
                    {
                        turnData = new List<EncounterBlueprintData.CardBlueprint> { new EncounterBlueprintData.CardBlueprint { } };
                        bias = 0;
                    }
                    else if (bias == -1)
                    {
                        turnData = GenerateTurnData(encounterType, i, SaveData.floor, 2,
                           junkCards,
                           earlyGameCards,
                           midGameCards,
                           lateGameCards,
                           earlyGameMox,
                           midGameMox,
                           lateGameMox,
                           temple == CardTemple.Wizard
                           );
                        bias += numCardsToQueue;
                    }
                    else if (numCardsToQueue == 0)
                    {
                        turnData = new List<EncounterBlueprintData.CardBlueprint> { new EncounterBlueprintData.CardBlueprint { } };
                        bias -= 1;
                    }
                    else
                    {
                        turnData = GenerateTurnData(encounterType, i, SaveData.floor, numCardsToQueue,
                            junkCards,
                            earlyGameCards,
                            midGameCards,
                            lateGameCards,
                            earlyGameMox,
                            midGameMox,
                            lateGameMox,
                            temple == CardTemple.Wizard
                            );
                        bias += numCardsToQueue;
                    }
                    if (i == bountyTurn && bountyEligble)
                    {
                        turnData.Add(BountyHunter.CreateBlueprint());
                    }
                    AddTurnToBlueprint(turnData, this);
                }
            }

        }
        public class doubleTempleBlueprint : EncounterBlueprintData
        {
            public doubleTempleBlueprint()
            {
                List<CardInfo> pixelCards = CardLoader.GetPixelCards();
                pixelCards.RemoveAll((CardInfo x) => !x.metaCategories.Contains(CardMetaCategory.GBCPack) || x.GetExtendedProperty("InfAct2ExcludeFromBattle") != null);

                List<CardInfo> junkCards = new List<CardInfo>();
                List<CardInfo> earlyGameCards = new List<CardInfo>();
                List<CardInfo> midGameCards = new List<CardInfo>();
                List<CardInfo> lateGameCards = new List<CardInfo>();

                List<CardInfo> earlyGameMox = new List<CardInfo>() { FindCardForTempleAndCost(pixelCards, CardTemple.Wizard, -1, 1.5f, 0, -1, -1, 0, Rarity.Common, Trait.Gem) };
                List<CardInfo> midGameMox = new List<CardInfo>() { FindCardForTempleAndCost(pixelCards, CardTemple.Wizard, -1, 1.5f, 0, -1, -1, 0, Rarity.Unset, Trait.Gem) };
                List<CardInfo> lateGameMox = new List<CardInfo>() { FindCardForTempleAndCost(pixelCards, CardTemple.Wizard, -1, 5, 0, -1, -1, 0, Rarity.Rare, Trait.Gem) };

                base.name = "baseblueprint";
                this.turns = new List<List<EncounterBlueprintData.CardBlueprint>>();

                CardInfo BountyHunter = GenerateBountyHunter();
                string[] bountyData = SaveData.bountyHunters.Split(';');
                bool bountyEligble = SaveData.bountyStars >= 1 && bountyData[0] == "0" && UnityEngine.Random.RandomRangeInt(0, 100) > 70 - (SaveData.floor * 3 + SaveData.bountyStars * 5f);
                int bountyTurn = UnityEngine.Random.RandomRangeInt(3, 7);

                int numberOfJunkCards = 1;
                if (SaveData.floor < 4) { numberOfJunkCards = 0; }
                else if (UnityEngine.Random.value <= 0.5f) { numberOfJunkCards++; }
                int numberOfEarlyCards = 1;
                if (UnityEngine.Random.value <= 0.2f) { numberOfEarlyCards++; }
                int numberOfMidCards = 1;
                int numberOfLateCards = 1;

                float ranType = UnityEngine.Random.value;
                EncounterType encounterType = EncounterType.Basic;
                if (ranType < 0.33f) { encounterType = EncounterType.Spam; }
                else if (ranType < 0.66f) { encounterType = EncounterType.Rush; }

                CardTemple temple = infact2.Plugin.functionsnstuff.getTemple(SaveData.roomId);

                int boonPunishment = 0;
                if (!string.IsNullOrEmpty(SaveData.boon1)) boonPunishment += 5;
                if (!string.IsNullOrEmpty(SaveData.boon2)) boonPunishment += 5;
                if (!string.IsNullOrEmpty(SaveData.boon3)) boonPunishment += 5;

                AssignCardsForFloorAndRegion(pixelCards, SaveData.floor + boonPunishment, temple,
                     junkCards, numberOfJunkCards,
                     earlyGameCards, numberOfEarlyCards,
                     midGameCards, numberOfMidCards,
                     lateGameCards, numberOfLateCards,
                    false);

                List<CardTemple> allTemples = new List<CardTemple>() { CardTemple.Undead, CardTemple.Tech, CardTemple.Nature, CardTemple.Wizard };
                allTemples.Remove(temple);
                CardTemple otherTemple = RandomElement<CardTemple>(allTemples);

                AssignCardsForFloorAndRegion(pixelCards, SaveData.floor + boonPunishment, otherTemple,
                     junkCards, numberOfJunkCards,
                     earlyGameCards, numberOfEarlyCards,
                     midGameCards, numberOfMidCards,
                     lateGameCards, numberOfLateCards,
                    false);

                int bias = 0;
                //Iterate through 30 turns
                for (int i = 1; i < 30; i++)
                {
                    int numCardsToQueue = UnityEngine.Random.Range(i == 1 ? 1 : 0, 3);
                    int reqBias = SaveData.floor > 7 ? 2 : 4;
                    List<CardBlueprint> turnData = null;
                    if (bias > reqBias && i != 1)
                    {
                        turnData = new List<EncounterBlueprintData.CardBlueprint> { new EncounterBlueprintData.CardBlueprint { } };
                        bias = 0;
                    }
                    else if (bias == -1)
                    {
                        turnData = GenerateTurnData(encounterType, i, SaveData.floor, 2,
                           junkCards,
                           earlyGameCards,
                           midGameCards,
                           lateGameCards,
                           earlyGameMox,
                           midGameMox,
                           lateGameMox,
                           temple == CardTemple.Wizard
                           );
                        bias += numCardsToQueue;
                    }
                    else if (numCardsToQueue == 0)
                    {
                        turnData = new List<EncounterBlueprintData.CardBlueprint> { new EncounterBlueprintData.CardBlueprint { } };
                        bias -= 1;
                    }
                    else
                    {
                        turnData = GenerateTurnData(encounterType, i, SaveData.floor, numCardsToQueue,
                            junkCards,
                            earlyGameCards,
                            midGameCards,
                            lateGameCards,
                            earlyGameMox,
                            midGameMox,
                            lateGameMox,
                            (temple == CardTemple.Wizard || otherTemple == CardTemple.Wizard)
                            );
                        bias += numCardsToQueue;
                    }
                    if (i == bountyTurn && bountyEligble)
                    {
                        turnData.Add(BountyHunter.CreateBlueprint());
                    }
                    AddTurnToBlueprint(turnData, this);
                }
            }
        }
    }
}