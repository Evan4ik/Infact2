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
namespace infact2
{
    public class MakeBluep
	{
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
					costed += card.BloodCost * 4;
					costed += card.gemsCost.Count * 3;
					foreach(Ability ability in card.abilities)
                    {
						int powerLevel = AbilitiesUtil.GetInfo(ability).powerLevel;
						if (powerLevel < 0) { powerLevel *= -1; }
						costed += powerLevel;
					}
					break;
				case "blood":
					costed += card.BloodCost;
					costed += card.energyCost * 0.25f;
					costed += card.bonesCost * 0.25f;
					costed += card.gemsCost.Count * 0.75f;
					if (costed > 4)
                    {
						costed = 4;
                    }
					foreach (Ability ability in card.abilities)
					{
						float powerLevel = AbilitiesUtil.GetInfo(ability).powerLevel;
						if (powerLevel < 0) { powerLevel *= -1f; }
						costed += powerLevel * 0.25f;
					}
					break;
				case "energy":
					costed += card.energyCost;
					costed += card.BloodCost * 4;
					costed += card.bonesCost;
					costed += card.gemsCost.Count * 3;
					if (costed > 6)
                    {
						costed = 6;
                    }
					foreach (Ability ability in card.abilities)
					{
						int powerLevel = AbilitiesUtil.GetInfo(ability).powerLevel;
						if (powerLevel < 0) { powerLevel *= -1; }
						costed += powerLevel;
					}
					break;
			}
			return costed;
        }
		private static readonly string[] BountyNames = new string[]
{
			"Barry",
			"Bolt",
			"Gear",
			"Zap",
			"Rust",
			"Clain",
			"Clank",
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
			BountyHunter.mods = new List<CardModificationInfo>();
			CardModificationInfo mod = new CardModificationInfo();
			int stars = Convert.ToInt32(Math.Floor(SaveData.bountyStars));
			mod.attackAdjustment = 1 + UnityEngine.Random.RandomRangeInt(-1 + stars, Convert.ToInt32(Math.Floor(1 + stars * 1.5f)));
			mod.healthAdjustment = 1 + UnityEngine.Random.RandomRangeInt(stars * 2, 3 + stars * 2);
			List<List<Ability>> sigils = new List<List<Ability>> { new List<Ability> { Ability.Flying, Ability.Submerge, Ability.Strafe, Ability.WhackAMole, Ability.GuardDog, Ability.ExplodeOnDeath }, new List<Ability> { Ability.Flying, Ability.Sentry, Ability.Strafe, Ability.BuffGems, Ability.ExplodeOnDeath, Ability.Deathtouch }, new List<Ability> { Ability.Flying, Ability.Sentry, Ability.SplitStrike, Ability.Sniper, Ability.BuffNeighbours, Ability.DebuffEnemy, Ability.Deathtouch, Ability.Sharp } };
			stars -= 1;
			if (stars < 0) { stars = 0; }
			mod.AddAbilities(sigils[stars][UnityEngine.Random.RandomRangeInt(0, sigils[stars].Count)]);
			if (stars > 0 && UnityEngine.Random.RandomRangeInt(0, 100) > 65 - SaveData.floor * 3)
			{
				mod.AddAbilities(sigils[stars - 1][UnityEngine.Random.RandomRangeInt(0, sigils[stars - 1].Count)]);
			}
			mod.bountyHunterInfo = new BountyHunterInfo();
			mod.bountyHunterInfo.dialogueIndex = UnityEngine.Random.RandomRangeInt(1, 6);
			BountyHunter.mods.Add(mod);
			return BountyHunter;
		}
		
		public class baseblueprint : EncounterBlueprintData//ty ara
		{
			public baseblueprint()
			{
				List<CardInfo> pixelCards = CardLoader.GetPixelCards();
				pixelCards.RemoveAll((CardInfo x) => !x.metaCategories.Contains(CardMetaCategory.GBCPack) || x.GetExtendedProperty("InfAct2ExcludeFromBattle") != null);
				 
				// i know i could do this in a list but this is more easily readable
				CardInfo earlygameCard;
				CardInfo earlygameCard2;
				CardInfo midgameCard;
				CardInfo midgameCard2;
				CardInfo lategameCard;
				CardInfo lategameCard2;

				base.name = "baseblueprint";
				this.turns = new List<List<EncounterBlueprintData.CardBlueprint>>();
				bool conditions = SaveData.floor > 3 && UnityEngine.Random.RandomRangeInt(0, 100) < 20;

				CardInfo BountyHunter = GenerateBountyHunter();
				string[] bountyData = SaveData.bountyHunters.Split(';');
				bool bountyEligble = SaveData.bountyStars >= 1 && bountyData[0] =="0" && UnityEngine.Random.RandomRangeInt(0, 100) > 70 - (SaveData.floor * 3 + SaveData.bountyStars * 5f);
				int bountyTurn = UnityEngine.Random.RandomRangeInt(3, 7);
				if (infact2.Plugin.functionsnstuff.getTemple(SaveData.roomId) != CardTemple.Wizard) { pixelCards.RemoveAll((CardInfo x) => x.HasAbility(Ability.GemDependant)); } else
                {
					List<CardInfo> pixelCards2 = CardLoader.GetPixelCards();
					pixelCards2.RemoveAll((CardInfo x) => !x.metaCategories.Contains(CardMetaCategory.GBCPack) && !x.HasAbility(Ability.GemDependant));
					foreach(CardInfo card in pixelCards2)
                    {
						pixelCards.Add(card);
                    }	
				}
				switch (infact2.Plugin.functionsnstuff.getTemple(SaveData.roomId))
				{
					case CardTemple.Undead:
						pixelCards.RemoveAll((CardInfo x) => x.name == "SkeletonMage" || x.bonesCost < 0 || x.temple != CardTemple.Undead);
						// there are a few free bone cards we want to play here like skeleton, thats why it checks for the temple here, pre floor 5 we dont want anything big
						if (SaveData.floor < 5) earlygameCard = pixelCards.FindAll((CardInfo x) => !x.metaCategories.Contains(CardMetaCategory.Rare) && returnBank("bones", x) < 3 && x.temple == CardTemple.Undead && x.Attack > 0).GetRandomItem();
						else earlygameCard = pixelCards.FindAll((CardInfo x) => returnBank("bones", x) > (SaveData.floor / 5) && returnBank("bones", x) < (4 + SaveData.floor / 5)).GetRandomItem();

						if (SaveData.floor < 5) earlygameCard2 = pixelCards.FindAll((CardInfo x) => !x.metaCategories.Contains(CardMetaCategory.Rare) && returnBank("bones", x) < 3 && x.temple == CardTemple.Undead && x.Attack > 0).GetRandomItem();
						else earlygameCard2 = pixelCards.FindAll((CardInfo x) => returnBank("bones", x) > (SaveData.floor / 5) && returnBank("bones", x) < (4 + SaveData.floor / 5)).GetRandomItem();


						if (SaveData.floor < 10) midgameCard = pixelCards.FindAll((CardInfo x) => !x.metaCategories.Contains(CardMetaCategory.Rare) && returnBank("bones", x) > (3 + SaveData.floor / 4) && returnBank("bones", x) < (4 + SaveData.floor / 3.5) && x.temple == CardTemple.Undead).GetRandomItem();
						else midgameCard = pixelCards.FindAll((CardInfo x) => returnBank("bones", x) > (3 + SaveData.floor / 4) && returnBank("bones", x) < (4 + SaveData.floor / 3.5) && x.temple == CardTemple.Undead).GetRandomItem();

						if (SaveData.floor < 10) midgameCard2 = pixelCards.FindAll((CardInfo x) => !x.metaCategories.Contains(CardMetaCategory.Rare) && returnBank("bones", x) > (3 + SaveData.floor / 4) && returnBank("bones", x) < (4 + SaveData.floor / 3.5) && x.temple == CardTemple.Undead).GetRandomItem();
						else midgameCard2 = pixelCards.FindAll((CardInfo x) => returnBank("bones", x) > (3 + SaveData.floor / 4) && returnBank("bones", x) < (4 + SaveData.floor / 3.5) && x.temple == CardTemple.Undead).GetRandomItem();


						if (SaveData.floor < 10) lategameCard = pixelCards.FindAll((CardInfo x) => !x.metaCategories.Contains(CardMetaCategory.Rare) && returnBank("bones", x) > (3 + SaveData.floor / 3) && returnBank("bones", x) < (4 + SaveData.floor / 2) && x.temple == CardTemple.Undead).GetRandomItem();
						else lategameCard = pixelCards.FindAll((CardInfo x) => returnBank("bones", x) > (3 + SaveData.floor / 4) && returnBank("bones", x) < (4 + SaveData.floor / 3.5) && x.temple == CardTemple.Undead).GetRandomItem();

						if (SaveData.floor < 10) lategameCard2 = pixelCards.FindAll((CardInfo x) => !x.metaCategories.Contains(CardMetaCategory.Rare) && returnBank("bones", x) > (3 + SaveData.floor / 3) && returnBank("bones", x) < (4 + SaveData.floor / 2) && x.temple == CardTemple.Undead).GetRandomItem();
						else lategameCard2 = pixelCards.FindAll((CardInfo x) => returnBank("bones", x) > (3 + SaveData.floor / 4) && returnBank("bones", x) < (4 + SaveData.floor / 3.5) && x.temple == CardTemple.Undead).GetRandomItem();

						//bone cost will eventually scale ad infinitum, if floor is more than 40 i will just set them all to the max possible where it will still find a card
						if (SaveData.floor > 39)
						{

							earlygameCard = pixelCards.FindAll((CardInfo x) => returnBank("bones", x) > 5 && returnBank("bones", x) < 10).GetRandomItem();
							earlygameCard2 = pixelCards.FindAll((CardInfo x) => returnBank("bones", x) > 5 && returnBank("bones", x) < 10).GetRandomItem();

							midgameCard = pixelCards.FindAll((CardInfo x) => returnBank("bones", x) > 10 && returnBank("bones", x) < 20).GetRandomItem();
							midgameCard2 = pixelCards.FindAll((CardInfo x) => returnBank("bones", x) > 10 && returnBank("bones", x) < 20).GetRandomItem();

							lategameCard = pixelCards.FindAll((CardInfo x) => returnBank("bones", x) > 12).GetRandomItem();
							lategameCard2 = pixelCards.FindAll((CardInfo x) => returnBank("bones", x) > 12).GetRandomItem();

						}


						break;

					default:
					case CardTemple.Nature:
						pixelCards.RemoveAll((CardInfo x) => x.BloodCost < 0 || x.temple != CardTemple.Nature);
						//no free blood cards exist that we want played, with blood we cant dynamically choose a cost as with bones so i just have some preset values where it will change
						if (SaveData.floor < 10) earlygameCard = pixelCards.FindAll((CardInfo x) => !x.metaCategories.Contains(CardMetaCategory.Rare) && returnBank("blood", x) == 1 && x.abilities.Count < 2 && x.Attack > 0).GetRandomItem();
						else if (SaveData.floor < 40) earlygameCard = pixelCards.FindAll((CardInfo x) => returnBank("blood", x) > 0 && returnBank("blood", x) < 2 && x.Attack > 0).GetRandomItem();
						else earlygameCard = pixelCards.FindAll((CardInfo x) => returnBank("blood", x) >= 2 && returnBank("blood", x) <= 4).GetRandomItem();

						if (SaveData.floor < 10) earlygameCard2 = pixelCards.FindAll((CardInfo x) => !x.metaCategories.Contains(CardMetaCategory.Rare) && returnBank("blood", x) == 1 && x.abilities.Count < 2 && x.Attack > 0).GetRandomItem();
						else if (SaveData.floor < 40) earlygameCard2 = pixelCards.FindAll((CardInfo x) => returnBank("blood", x) > 0 && returnBank("blood", x) < 2).GetRandomItem();
						else earlygameCard2 = pixelCards.FindAll((CardInfo x) => returnBank("blood", x) >= 2 && returnBank("blood", x) <= 4).GetRandomItem();


						if (SaveData.floor < 10) midgameCard = pixelCards.FindAll((CardInfo x) => !x.metaCategories.Contains(CardMetaCategory.Rare) && returnBank("blood", x) == 2).GetRandomItem();
						else if (SaveData.floor < 40) midgameCard = pixelCards.FindAll((CardInfo x) => returnBank("blood", x) > 1 && returnBank("blood", x) < 4).GetRandomItem();
						else midgameCard = pixelCards.FindAll((CardInfo x) => !x.metaCategories.Contains(CardMetaCategory.Rare) && returnBank("blood", x) >= 2).GetRandomItem();

						if (SaveData.floor < 10) midgameCard2 = pixelCards.FindAll((CardInfo x) => !x.metaCategories.Contains(CardMetaCategory.Rare) && returnBank("blood", x) == 2).GetRandomItem();
						else if (SaveData.floor < 40) midgameCard2 = pixelCards.FindAll((CardInfo x) => returnBank("blood", x) > 1 && returnBank("blood", x) < 4).GetRandomItem();
						else midgameCard2 = pixelCards.FindAll((CardInfo x) => !x.metaCategories.Contains(CardMetaCategory.Rare) && returnBank("blood", x) >= 2).GetRandomItem();


						if (SaveData.floor < 10) lategameCard = pixelCards.FindAll((CardInfo x) => !x.metaCategories.Contains(CardMetaCategory.Rare) && returnBank("blood", x) == 2).GetRandomItem();
						else if (SaveData.floor < 40) lategameCard = pixelCards.FindAll((CardInfo x) => returnBank("blood", x) > 2).GetRandomItem();
						else lategameCard = pixelCards.FindAll((CardInfo x) => !x.metaCategories.Contains(CardMetaCategory.Rare) && returnBank("blood", x) == 4).GetRandomItem();

						if (SaveData.floor < 10) lategameCard2 = pixelCards.FindAll((CardInfo x) => !x.metaCategories.Contains(CardMetaCategory.Rare) && returnBank("blood", x) == 2).GetRandomItem();
						else if (SaveData.floor < 40) lategameCard2 = pixelCards.FindAll((CardInfo x) => returnBank("blood", x) > 2).GetRandomItem();
						else lategameCard2 = pixelCards.FindAll((CardInfo x) => !x.metaCategories.Contains(CardMetaCategory.Rare) && returnBank("blood", x) == 4).GetRandomItem();

						break;

					case CardTemple.Tech:
						pixelCards.RemoveAll((CardInfo x) => x.energyCost < 0 || x.temple != CardTemple.Tech);
						if (SaveData.floor < 10) earlygameCard = pixelCards.FindAll((CardInfo x) => !x.metaCategories.Contains(CardMetaCategory.Rare) && returnBank("energy", x) > 0 && returnBank("energy", x) < 3 && x.Attack > 0).GetRandomItem();
						else if (SaveData.floor < 40) earlygameCard = pixelCards.FindAll((CardInfo x) => returnBank("energy", x) > 1 && returnBank("energy", x) < 5 && x.Attack > 0).GetRandomItem();
						else earlygameCard = pixelCards.FindAll((CardInfo x) => returnBank("energy", x) > 3).GetRandomItem();

						if (SaveData.floor < 10) earlygameCard2 = pixelCards.FindAll((CardInfo x) => !x.metaCategories.Contains(CardMetaCategory.Rare) && returnBank("energy", x) > 0 && returnBank("energy", x) < 3 && x.Attack > 0).GetRandomItem();
						else if (SaveData.floor < 40) earlygameCard2 = pixelCards.FindAll((CardInfo x) => returnBank("energy", x) > 1 && returnBank("energy", x) < 5).GetRandomItem();
						else earlygameCard2 = pixelCards.FindAll((CardInfo x) => returnBank("energy", x) > 3).GetRandomItem();


						if (SaveData.floor < 7) midgameCard = pixelCards.FindAll((CardInfo x) => !x.metaCategories.Contains(CardMetaCategory.Rare) && x.Attack > 0 && returnBank("energy", x) > 2 && returnBank("energy", x) < 6).GetRandomItem();
						else if (SaveData.floor < 40) midgameCard = pixelCards.FindAll((CardInfo x) => returnBank("energy", x) > 3 && x.Attack > 0).GetRandomItem();
						else midgameCard = pixelCards.FindAll((CardInfo x) => returnBank("energy", x) == 6).GetRandomItem();

						if (SaveData.floor < 7) midgameCard2 = pixelCards.FindAll((CardInfo x) => !x.metaCategories.Contains(CardMetaCategory.Rare) && x.Attack > 0 && returnBank("energy", x) > 2 && returnBank("energy", x) < 6).GetRandomItem();
						else if (SaveData.floor < 40) midgameCard2 = pixelCards.FindAll((CardInfo x) => returnBank("energy", x) > 3 && x.Attack > 0).GetRandomItem();
						else midgameCard2 = pixelCards.FindAll((CardInfo x) => returnBank("energy", x) == 6).GetRandomItem();


						if (SaveData.floor < 7) lategameCard = pixelCards.FindAll((CardInfo x) => !x.metaCategories.Contains(CardMetaCategory.Rare) && returnBank("energy", x) > 4).GetRandomItem();
						else lategameCard = pixelCards.FindAll((CardInfo x) => returnBank("energy", x) == 6).GetRandomItem();

						if (SaveData.floor < 7) lategameCard2 = pixelCards.FindAll((CardInfo x) => !x.metaCategories.Contains(CardMetaCategory.Rare) && returnBank("energy", x) > 4).GetRandomItem();
						else lategameCard2 = pixelCards.FindAll((CardInfo x) => returnBank("energy", x) == 6).GetRandomItem();

						break;

					case CardTemple.Wizard:
						if (SaveData.floor < 7) earlygameCard2 = pixelCards.FindAll((CardInfo x) => !x.metaCategories.Contains(CardMetaCategory.Rare) && x.HasTrait(Trait.Gem)).GetRandomItem();
						else earlygameCard2 = pixelCards.FindAll((CardInfo x) => x.HasTrait(Trait.Gem)).GetRandomItem();

						if (SaveData.floor < 7) earlygameCard = pixelCards.FindAll((CardInfo x) => !x.metaCategories.Contains(CardMetaCategory.Rare) && x.gemsCost.Count == 1).GetRandomItem();
						else earlygameCard = pixelCards.FindAll((CardInfo x) => !x.metaCategories.Contains(CardMetaCategory.Rare) && x.gemsCost.Count > 0).GetRandomItem();


						if (SaveData.floor < 7) midgameCard = pixelCards.FindAll((CardInfo x) => !x.metaCategories.Contains(CardMetaCategory.Rare) && x.gemsCost.Count > 0 && !x.HasTrait(Trait.Gem)).GetRandomItem();
						else midgameCard = pixelCards.FindAll((CardInfo x) => x.gemsCost.Count > 0 && x.gemsCost.Count < 3 && !x.HasTrait(Trait.Gem)).GetRandomItem();


						if (SaveData.floor < 7) midgameCard2 = pixelCards.FindAll((CardInfo x) => !x.metaCategories.Contains(CardMetaCategory.Rare) && x.temple == CardTemple.Wizard && x.HasTrait(Trait.Gem)).GetRandomItem();
						else midgameCard2 = pixelCards.FindAll((CardInfo x) => x.HasTrait(Trait.Gem)).GetRandomItem();

						//will finish this scaling when i come up with a better idea
						if (SaveData.floor < 7) lategameCard = pixelCards.FindAll((CardInfo x) => x.gemsCost.Count > 0 && !x.HasTrait(Trait.Gem)).GetRandomItem();
						else lategameCard = pixelCards.FindAll((CardInfo x) => x.gemsCost.Count > 1 && !x.HasTrait(Trait.Gem)).GetRandomItem();


						if (SaveData.floor < 7) lategameCard2 = pixelCards.FindAll((CardInfo x) => x.HasTrait(Trait.Gem)).GetRandomItem();
						else lategameCard2 = pixelCards.FindAll((CardInfo x) => x.HasTrait(Trait.Gem) && x.HasCardMetaCategory(CardMetaCategory.Rare)).GetRandomItem();



						break;
				}


				


				if (infact2.Plugin.functionsnstuff.getTemple(SaveData.roomId) != CardTemple.Wizard)
				{
					for (int i = 1; i < 20; i++)
					{
						if (i == bountyTurn && bountyEligble)
                        {
							this.turns.Add(new List<EncounterBlueprintData.CardBlueprint> { BountyHunter.CreateBlueprint() });
						}
						if (i < 4 - SaveData.floor / 5 && SaveData.floor < 20)
						{
							bool rand = UnityEngine.Random.RandomRangeInt(0, 100) < 95 - SaveData.floor * 3;
							if (i % 2 == 0 && rand)
							{
								this.turns.Add(new List<EncounterBlueprintData.CardBlueprint>
							{
								new EncounterBlueprintData.CardBlueprint
								{

								}

							});
							}
							else
							{
								List<CardInfo> cards = new List<CardInfo> { earlygameCard, earlygameCard2 };
								List<CardInfo> laterCards = new List<CardInfo> { midgameCard, midgameCard2 };
								List<EncounterBlueprintData.CardBlueprint> bp = new List<EncounterBlueprintData.CardBlueprint> { cards.GetRandomItem().CreateBlueprint() };

								if (UnityEngine.Random.RandomRangeInt(0, 100) > 99 - SaveData.floor * 4)
								{
									bp = new List<EncounterBlueprintData.CardBlueprint> { laterCards.GetRandomItem().CreateBlueprint() };
									this.turns.Add(bp);
									this.turns.Add(new List<EncounterBlueprintData.CardBlueprint> {  });
									if (i % 2 != 0)
									{
										i++;
									}
									continue;
								}
								else if (UnityEngine.Random.RandomRangeInt(0, 100) > 95 - SaveData.floor * 3)
								{
									bp.Add(cards.GetRandomItem().CreateBlueprint());
								}

								this.turns.Add(bp);
								
							}
						}
						else if (SaveData.floor >= 20)
						{
							this.turns.Add(new List<EncounterBlueprintData.CardBlueprint> { midgameCard.CreateBlueprint(), earlygameCard2.CreateBlueprint() });
						}
						
						if (i > 7 && UnityEngine.Random.RandomRangeInt(0, 100) > 70)
                        {
							List<EncounterBlueprintData.CardBlueprint> bp = new List<EncounterBlueprintData.CardBlueprint> { pixelCards.FindAll((CardInfo x) => !x.metaCategories.Contains(CardMetaCategory.Rare) && x.temple == infact2.Plugin.functionsnstuff.getTemple(SaveData.roomId)).GetRandomItem().CreateBlueprint() };
						}
						if (i < 10 && i > 4 - SaveData.floor / 5)
						{
							if (i % 2 == 0 && UnityEngine.Random.RandomRangeInt(0, 100) < 65 - SaveData.floor * 2)
							{
								this.turns.Add(new List<EncounterBlueprintData.CardBlueprint> { });
							}
							else
							{
								List<CardInfo> cards = new List<CardInfo> { midgameCard, midgameCard2, earlygameCard, earlygameCard2 };
								List<EncounterBlueprintData.CardBlueprint> bp = new List<EncounterBlueprintData.CardBlueprint> { cards.GetRandomItem().CreateBlueprint() };
								if (UnityEngine.Random.RandomRangeInt(0, 100) > 65 - SaveData.floor * 2)
								{
									bp.Add(cards.GetRandomItem().CreateBlueprint());
								}
								this.turns.Add(bp);
							}

						} else if (i > 10)
						{
							if (i % 2 == 0 || UnityEngine.Random.RandomRangeInt(0, 100) > 55 - SaveData.floor * 2)
							{
								this.turns.Add(new List<EncounterBlueprintData.CardBlueprint> { });
							}
							else
							{
								List<CardInfo> cards = new List<CardInfo> { lategameCard, lategameCard2, midgameCard, midgameCard2 };
								List<EncounterBlueprintData.CardBlueprint> bp = new List<EncounterBlueprintData.CardBlueprint> { cards.GetRandomItem().CreateBlueprint() };
								if (UnityEngine.Random.RandomRangeInt(0, 100) > 65 - SaveData.floor * 2)
								{
									bp.Add(cards.GetRandomItem().CreateBlueprint());
								}
								this.turns.Add(bp);
							}

						}
					}
				}
				else
				{
					for (int i = 1; i < 20; i++)
					{
						if (i == bountyTurn && bountyEligble)
						{
							this.turns.Add(new List<EncounterBlueprintData.CardBlueprint> { BountyHunter.CreateBlueprint() });
						}
						if (i < 4 - SaveData.floor / 5 && SaveData.floor < 20)
						{
							if (i % 2 == 0 && UnityEngine.Random.RandomRangeInt(0, 100) < 95 - SaveData.floor * 3)
							{
								this.turns.Add(new List<EncounterBlueprintData.CardBlueprint>
							{
								new EncounterBlueprintData.CardBlueprint
								{

								}

							});
							}
							else
							{
								List<CardInfo> cards = new List<CardInfo> { earlygameCard };
								List<CardInfo> laterCards = new List<CardInfo> { midgameCard };
								List<EncounterBlueprintData.CardBlueprint> bp = new List<EncounterBlueprintData.CardBlueprint> { cards.GetRandomItem().CreateBlueprint() };

								if (UnityEngine.Random.RandomRangeInt(0, 100) > 99 - SaveData.floor * 4)
								{
									bp = new List<EncounterBlueprintData.CardBlueprint> { laterCards.GetRandomItem().CreateBlueprint() };
									this.turns.Add(bp);
									this.turns.Add(new List<EncounterBlueprintData.CardBlueprint> { });
									if (i % 2 != 0)
									{
										i++;
									}
									continue;
								}
								else if (UnityEngine.Random.RandomRangeInt(0, 100) > 95 - SaveData.floor * 3)
								{
									bp.Add(cards.GetRandomItem().CreateBlueprint());
								}

								if (UnityEngine.Random.RandomRangeInt(0, 100) > 55)
                                {
									bp.Add(earlygameCard2.CreateBlueprint());
                                }

								this.turns.Add(bp);
							}
						}
						else if (SaveData.floor >= 20)
						{
							this.turns.Add(new List<EncounterBlueprintData.CardBlueprint> { midgameCard.CreateBlueprint(), earlygameCard2.CreateBlueprint() });
						}
						if (i > 7 && UnityEngine.Random.RandomRangeInt(0, 100) > 70)
						{
							List<EncounterBlueprintData.CardBlueprint> bp = new List<EncounterBlueprintData.CardBlueprint> { pixelCards.FindAll((CardInfo x) => !x.metaCategories.Contains(CardMetaCategory.Rare) && x.temple == infact2.Plugin.functionsnstuff.getTemple(SaveData.roomId)).GetRandomItem().CreateBlueprint() };
						}
						if (i < 10 && i > 4 - SaveData.floor / 5)
						{
							if (i % 2 == 0 && UnityEngine.Random.RandomRangeInt(0, 100) < 55 - SaveData.floor * 2)
							{
								this.turns.Add(new List<EncounterBlueprintData.CardBlueprint> { });
							}
							else
							{
								List<CardInfo> cards = new List<CardInfo> { midgameCard, lategameCard };
								List<CardInfo> cards2 = new List<CardInfo> { midgameCard2, lategameCard2 };
								List<EncounterBlueprintData.CardBlueprint> bp = new List<EncounterBlueprintData.CardBlueprint> { cards.GetRandomItem().CreateBlueprint() };
								if (UnityEngine.Random.RandomRangeInt(0, 100) > 65 - SaveData.floor * 2)
								{
									bp.Add(cards.GetRandomItem().CreateBlueprint());
								}
								if (UnityEngine.Random.RandomRangeInt(0, 100) > 80)
								{
									bp.Add(cards2.GetRandomItem().CreateBlueprint());
								}
								this.turns.Add(bp);
							}

						}
						else if (i > 10)
						{
							if (i % 2 == 0 && UnityEngine.Random.RandomRangeInt(0, 100) < 55 - SaveData.floor * 2)
							{
								this.turns.Add(new List<EncounterBlueprintData.CardBlueprint> { });
							}
							else
							{
								List<CardInfo> cards = new List<CardInfo> { lategameCard, lategameCard2 };
								List<EncounterBlueprintData.CardBlueprint> bp = new List<EncounterBlueprintData.CardBlueprint> { cards.GetRandomItem().CreateBlueprint() };
								if (UnityEngine.Random.RandomRangeInt(0, 100) > 65 - SaveData.floor * 2)
								{
									bp.Add(cards.GetRandomItem().CreateBlueprint());
								}
								this.turns.Add(bp);
							}

						}
					}
				}
			}
		}
		public class bossblueprint : EncounterBlueprintData
		{
			public bossblueprint()
			{
				List<CardInfo> pixelCards = CardLoader.GetPixelCards();
				pixelCards.RemoveAll((CardInfo x) => !x.metaCategories.Contains(CardMetaCategory.GBCPack) || x.name == "MasterBleene" || x.name == "Bonepile" || x.name == "TombRobber" || x.GetExtendedProperty("InfAct2ExcludeFromBattle") != null);
				

				// i know i could do this in a list but this is more easily readable
				CardInfo earlygameCard;
				CardInfo earlygameCard2;
				CardInfo midgameCard;
				CardInfo midgameCard2;
				CardInfo lategameCard;
				CardInfo lategameCard2;

				CardInfo BountyHunter = GenerateBountyHunter();
				string[] bountyData = SaveData.bountyHunters.Split(';');
				bool bountyEligble = SaveData.bountyStars >= 1 && bountyData[0] == "0" && UnityEngine.Random.RandomRangeInt(0, 100) > 70 - (SaveData.floor * 3 + SaveData.bountyStars * 5f);
				int bountyTurn = UnityEngine.Random.RandomRangeInt(3, 7);

				base.name = "bossblueprint";
				this.turns = new List<List<EncounterBlueprintData.CardBlueprint>>();
				bool conditions = SaveData.floor > 3 && UnityEngine.Random.RandomRangeInt(0, 100) < 20;

				if (infact2.Plugin.functionsnstuff.getTemple(SaveData.roomId) != CardTemple.Wizard) { pixelCards.RemoveAll((CardInfo x) => x.HasAbility(Ability.GemDependant)); }
				else
				{
					List<CardInfo> pixelCards2 = CardLoader.GetPixelCards();
					pixelCards2.RemoveAll((CardInfo x) => !x.metaCategories.Contains(CardMetaCategory.GBCPack) && !x.HasAbility(Ability.GemDependant));
					foreach (CardInfo card in pixelCards2)
					{
						pixelCards.Add(card);
					}
				}

				switch (infact2.Plugin.functionsnstuff.getTemple(SaveData.roomId))
				{
					case CardTemple.Undead:
						pixelCards.RemoveAll((CardInfo x) => x.bonesCost < 0 || x.temple != CardTemple.Undead ||x.name == "SkeletonMage");
						// there are a few free bone cards we want to play here like skeleton, thats why it checks for the temple here, pre floor 5 we dont want anything big
						if (SaveData.floor < 10) earlygameCard = pixelCards.FindAll((CardInfo x) => !x.metaCategories.Contains(CardMetaCategory.Rare) && returnBank("bones", x) < 5 && x.temple == CardTemple.Undead && x.Attack > 0).GetRandomItem();
						else earlygameCard = pixelCards.FindAll((CardInfo x) => returnBank("bones", x) > (SaveData.floor / 3) && returnBank("bones", x) < (6 + SaveData.floor / 3)).GetRandomItem();

						if (SaveData.floor < 10) earlygameCard2 = pixelCards.FindAll((CardInfo x) => returnBank("bones", x) < 5 && x.temple == CardTemple.Undead && x.Attack > 0).GetRandomItem();
						else earlygameCard2 = pixelCards.FindAll((CardInfo x) => returnBank("bones", x) > (SaveData.floor / 3) && returnBank("bones", x) < (6 + SaveData.floor / 3)).GetRandomItem();


						if (SaveData.floor < 15) midgameCard = pixelCards.FindAll((CardInfo x) => returnBank("bones", x) < 10 &&  x.temple == CardTemple.Undead).GetRandomItem();
						else midgameCard = pixelCards.FindAll((CardInfo x) => x.metaCategories.Contains(CardMetaCategory.Rare) && x.temple == CardTemple.Undead).GetRandomItem();

						if (SaveData.floor < 15) midgameCard2 = pixelCards.FindAll((CardInfo x) => returnBank("bones", x) < 10 && x.temple == CardTemple.Undead).GetRandomItem();
						else midgameCard2 = pixelCards.FindAll((CardInfo x) => x.metaCategories.Contains(CardMetaCategory.Rare) && x.temple == CardTemple.Undead).GetRandomItem();



						lategameCard = pixelCards.FindAll((CardInfo x) => x.metaCategories.Contains(CardMetaCategory.Rare) && x.temple == CardTemple.Undead).GetRandomItem();

						lategameCard2 = pixelCards.FindAll((CardInfo x) => x.metaCategories.Contains(CardMetaCategory.Rare) && x.temple == CardTemple.Undead).GetRandomItem();

						//bone cost will eventually scale ad infinitum, if floor is more than 40 i will just set them all to the max possible where it will still find a card
						if (SaveData.floor > 39)
						{

							earlygameCard = pixelCards.FindAll((CardInfo x) => returnBank("bones", x) < 7 && x.temple == CardTemple.Undead).GetRandomItem();
							earlygameCard2 = pixelCards.FindAll((CardInfo x) => returnBank("bones", x) < 7 && x.temple == CardTemple.Undead).GetRandomItem();
							

							midgameCard = pixelCards.FindAll((CardInfo x) => x.metaCategories.Contains(CardMetaCategory.Rare) && returnBank("bones", x) < 10 && x.temple == CardTemple.Undead).GetRandomItem();
							midgameCard2 = pixelCards.FindAll((CardInfo x) => x.metaCategories.Contains(CardMetaCategory.Rare) && x.temple == CardTemple.Undead).GetRandomItem();

						}


						break;

					default:
					case CardTemple.Nature:
						pixelCards.RemoveAll((CardInfo x) => x.BloodCost < 0 || x.temple != CardTemple.Nature);
						//no free blood cards exist that we want played, with blood we cant dynamically choose a cost as with bones so i just have some preset values where it will change
						if (SaveData.floor < 10) earlygameCard = pixelCards.FindAll((CardInfo x) => !x.metaCategories.Contains(CardMetaCategory.Rare) && returnBank("blood", x) >= 1 && returnBank("blood", x) < 3 && x.Attack > 0).GetRandomItem();
						else if (SaveData.floor < 40) earlygameCard = pixelCards.FindAll((CardInfo x) => x.metaCategories.Contains(CardMetaCategory.Rare) && returnBank("blood", x) > 0 && returnBank("blood", x) < 3).GetRandomItem();
						else earlygameCard = pixelCards.FindAll((CardInfo x) => x.metaCategories.Contains(CardMetaCategory.Rare) && returnBank("blood", x) > 2 && returnBank("blood", x) < 4).GetRandomItem();

						if (SaveData.floor < 10) earlygameCard2 = pixelCards.FindAll((CardInfo x) => x.metaCategories.Contains(CardMetaCategory.Rare) && returnBank("blood", x) == 1 && x.Attack > 0).GetRandomItem();
						else if (SaveData.floor < 40) earlygameCard2 = pixelCards.FindAll((CardInfo x) => returnBank("blood", x) > 0 && returnBank("blood", x) < 3 || x.metaCategories.Contains(CardMetaCategory.Rare) && returnBank("blood", x) < 3).GetRandomItem();
						else earlygameCard2 = pixelCards.FindAll((CardInfo x) => returnBank("blood", x) > 2 && returnBank("blood", x) <= 4 && x.metaCategories.Contains(CardMetaCategory.Rare)).GetRandomItem();


						if (SaveData.floor < 10) midgameCard = pixelCards.FindAll((CardInfo x) => x.metaCategories.Contains(CardMetaCategory.Rare) && returnBank("blood", x) < 4 && x.temple == CardTemple.Nature || returnBank("blood", x) >= 2).GetRandomItem();
						else midgameCard = pixelCards.FindAll((CardInfo x) => x.metaCategories.Contains(CardMetaCategory.Rare) && x.temple == CardTemple.Nature || returnBank("blood", x) >= 2).GetRandomItem();

						if (SaveData.floor < 10) midgameCard2 = pixelCards.FindAll((CardInfo x) => x.metaCategories.Contains(CardMetaCategory.Rare) && returnBank("blood", x) < 4 && x.temple == CardTemple.Nature || returnBank("blood", x) >= 2).GetRandomItem();
						else midgameCard2 = pixelCards.FindAll((CardInfo x) => x.metaCategories.Contains(CardMetaCategory.Rare) && x.temple == CardTemple.Nature || returnBank("blood", x) >= 2).GetRandomItem();


						lategameCard = pixelCards.FindAll((CardInfo x) => x.metaCategories.Contains(CardMetaCategory.Rare) && x.temple == CardTemple.Nature).GetRandomItem();
						lategameCard2 = pixelCards.FindAll((CardInfo x) => x.metaCategories.Contains(CardMetaCategory.Rare) && x.temple == CardTemple.Nature).GetRandomItem();

						break;

					case CardTemple.Tech:
						pixelCards.RemoveAll((CardInfo x) => x.energyCost < 0 || x.temple != CardTemple.Tech);
						if (SaveData.floor < 10) earlygameCard = pixelCards.FindAll((CardInfo x) => !x.metaCategories.Contains(CardMetaCategory.Rare) && returnBank("energy", x) >= 2 && returnBank("energy", x) <= 5 && x.Attack > 0).GetRandomItem();
						else if (SaveData.floor < 40) earlygameCard = pixelCards.FindAll((CardInfo x) => returnBank("energy", x) > 2 && returnBank("energy", x) <= 6 && x.Attack > 0).GetRandomItem();
						else earlygameCard = pixelCards.FindAll((CardInfo x) => returnBank("energy", x) > 3).GetRandomItem();

						if (SaveData.floor < 10) earlygameCard2 = pixelCards.FindAll((CardInfo x) => !x.metaCategories.Contains(CardMetaCategory.Rare) && returnBank("energy", x) >= 2 && returnBank("energy", x) <= 5).GetRandomItem();
						else if (SaveData.floor < 40) earlygameCard2 = pixelCards.FindAll((CardInfo x) => returnBank("energy", x) > 2 && returnBank("energy", x) <= 6).GetRandomItem();
						else earlygameCard2 = pixelCards.FindAll((CardInfo x) => returnBank("energy", x) > 3).GetRandomItem();


						if (SaveData.floor < 7) midgameCard = pixelCards.FindAll((CardInfo x) => returnBank("energy", x) > 1 && returnBank("energy", x) <= 6 && x.Attack > 0).GetRandomItem();
						else if (SaveData.floor < 40) midgameCard = pixelCards.FindAll((CardInfo x) => returnBank("energy", x) > 2 && x.Attack > 0).GetRandomItem();
						else midgameCard = pixelCards.FindAll((CardInfo x) => returnBank("energy", x) >= 4).GetRandomItem();

						if (SaveData.floor < 7) midgameCard2 = pixelCards.FindAll((CardInfo x) => returnBank("energy", x) > 1 && returnBank("energy", x) <= 6).GetRandomItem();
						else if (SaveData.floor < 40) midgameCard2 = pixelCards.FindAll((CardInfo x) => returnBank("energy", x) > 2).GetRandomItem();
						else midgameCard2 = pixelCards.FindAll((CardInfo x) => returnBank("energy", x) >= 4).GetRandomItem();


						if (SaveData.floor < 7) lategameCard = pixelCards.FindAll((CardInfo x) => x.metaCategories.Contains(CardMetaCategory.Rare) && returnBank("energy", x) > 0).GetRandomItem();
						else lategameCard = pixelCards.FindAll((CardInfo x) => x.metaCategories.Contains(CardMetaCategory.Rare) && returnBank("energy", x) >= 2).GetRandomItem();

						if (SaveData.floor < 7) lategameCard2 = pixelCards.FindAll((CardInfo x) => x.metaCategories.Contains(CardMetaCategory.Rare) && returnBank("energy", x) > 0).GetRandomItem();
						else lategameCard2 = pixelCards.FindAll((CardInfo x) => x.metaCategories.Contains(CardMetaCategory.Rare) && returnBank("energy", x) >= 2).GetRandomItem();

						break;

					case CardTemple.Wizard:
						earlygameCard2 = pixelCards.FindAll((CardInfo x) => x.metaCategories.Contains(CardMetaCategory.Rare) && x.HasTrait(Trait.Gem)).GetRandomItem();

						if (SaveData.floor < 7) earlygameCard = pixelCards.FindAll((CardInfo x) => x.gemsCost.Count == 1).GetRandomItem();
						else earlygameCard = pixelCards.FindAll((CardInfo x) => x.gemsCost.Count > 0 && !x.HasCardMetaCategory(CardMetaCategory.Rare)).GetRandomItem();


						midgameCard = pixelCards.FindAll((CardInfo x) => x.gemsCost.Count > 0 && !x.HasTrait(Trait.Gem)).GetRandomItem();

						midgameCard2 = pixelCards.FindAll((CardInfo x) => x.HasTrait(Trait.Gem)).GetRandomItem();

						lategameCard = pixelCards.FindAll((CardInfo x) => x.gemsCost.Count > 0 && x.HasCardMetaCategory(CardMetaCategory.Rare)).GetRandomItem();

						lategameCard2 = pixelCards.FindAll((CardInfo x) => x.HasTrait(Trait.Gem) && x.HasCardMetaCategory(CardMetaCategory.Rare)).GetRandomItem();



						break;
				}


				if (infact2.Plugin.functionsnstuff.getTemple(SaveData.roomId) != CardTemple.Wizard)
				{
					for (int i = 1; i < 20; i++)
					{
						if (i == bountyTurn && bountyEligble)
						{
							this.turns.Add(new List<EncounterBlueprintData.CardBlueprint> { BountyHunter.CreateBlueprint() });
						}
						if (i < 4 - SaveData.floor / 5 && SaveData.floor < 20)
						{
							if (i % 2 == 0 && UnityEngine.Random.RandomRangeInt(0, 100) < 85 - SaveData.floor * 2)
							{
								this.turns.Add(new List<EncounterBlueprintData.CardBlueprint>
							{
								new EncounterBlueprintData.CardBlueprint
								{

								}

							});
							}
							else
							{
								List<CardInfo> cards = new List<CardInfo> { earlygameCard, earlygameCard2 };
								List<CardInfo> laterCards = new List<CardInfo> { midgameCard, midgameCard2 };
								List<EncounterBlueprintData.CardBlueprint> bp = new List<EncounterBlueprintData.CardBlueprint> { cards.GetRandomItem().CreateBlueprint() };

								if (UnityEngine.Random.RandomRangeInt(0, 100) > 102 - SaveData.floor * 4)
								{
									bp = new List<EncounterBlueprintData.CardBlueprint> { laterCards.GetRandomItem().CreateBlueprint() };
									this.turns.Add(bp);
								}
								else if (UnityEngine.Random.RandomRangeInt(0, 100) > 95 - SaveData.floor * 3)
								{
									bp.Add(cards.GetRandomItem().CreateBlueprint());
								}

								this.turns.Add(bp);

							}
						}
						else if (SaveData.floor >= 20)
						{
							this.turns.Add(new List<EncounterBlueprintData.CardBlueprint> { midgameCard.CreateBlueprint(), earlygameCard2.CreateBlueprint() });
						}

						if (i > 7 && UnityEngine.Random.RandomRangeInt(0, 100) > 80)
						{
							List<EncounterBlueprintData.CardBlueprint> bp = new List<EncounterBlueprintData.CardBlueprint> { pixelCards.FindAll((CardInfo x) => x.temple == infact2.Plugin.functionsnstuff.getTemple(SaveData.roomId)).GetRandomItem().CreateBlueprint() };
						}
						if (i > 4 - SaveData.floor / 5 && i <= 7)
						{
							if (i % 2 == 0 && UnityEngine.Random.RandomRangeInt(0, 100) < 75 - SaveData.floor * 2)
							{
								this.turns.Add(new List<EncounterBlueprintData.CardBlueprint> { });
							}
							else
							{
								List<CardInfo> cards = new List<CardInfo> { midgameCard, midgameCard2 };
								List<CardInfo> laterCards = new List<CardInfo> { lategameCard, lategameCard2 };
								if (UnityEngine.Random.RandomRangeInt(0, 100) > 65 - SaveData.floor * 2)
								{
									this.turns.Add(new List<EncounterBlueprintData.CardBlueprint> { cards.GetRandomItem().CreateBlueprint(), cards.GetRandomItem().CreateBlueprint() });
								}
								else
								{
									this.turns.Add(new List<EncounterBlueprintData.CardBlueprint> { cards.GetRandomItem().CreateBlueprint() });
								}
							}

						} else if (i > 7)
                        {
							if (i % 2 == 0 && UnityEngine.Random.RandomRangeInt(0, 100) < 75 - SaveData.floor * 2)
							{
								this.turns.Add(new List<EncounterBlueprintData.CardBlueprint> { });
							}
							else
							{
								List<CardInfo> cards = new List<CardInfo> { midgameCard, midgameCard2 };
								if (UnityEngine.Random.RandomRangeInt(0, 100) > 65 - SaveData.floor * 2)
								{
									this.turns.Add(new List<EncounterBlueprintData.CardBlueprint> { cards.GetRandomItem().CreateBlueprint(), cards.GetRandomItem().CreateBlueprint() });
								}
								else
								{
									this.turns.Add(new List<EncounterBlueprintData.CardBlueprint> { cards.GetRandomItem().CreateBlueprint() });
								}
							}
						}
					}
				} else
                {
					for (int i = 1; i < 20; i++)
					{
						if (i == bountyTurn && bountyEligble)
						{
							this.turns.Add(new List<EncounterBlueprintData.CardBlueprint> { BountyHunter.CreateBlueprint() });
						}
						if (i < 4 - SaveData.floor / 5 && SaveData.floor < 20)
						{
							if (i % 2 == 0 && UnityEngine.Random.RandomRangeInt(0, 100) < 85 - SaveData.floor * 2)
							{
								this.turns.Add(new List<EncounterBlueprintData.CardBlueprint>
							{
								new EncounterBlueprintData.CardBlueprint
								{

								}

							});
							}
							else
							{
								List<EncounterBlueprintData.CardBlueprint> bp = new List<EncounterBlueprintData.CardBlueprint> { earlygameCard.CreateBlueprint() };
								if (UnityEngine.Random.RandomRangeInt(0, 100) > 45)
								{
									bp.Add(earlygameCard2.CreateBlueprint());
								} else if (UnityEngine.Random.RandomRangeInt(0, 100) > 85 - SaveData.floor * 3)
								{
									bp.Add(midgameCard.CreateBlueprint());
								}
								this.turns.Add(bp);
							}
						}
						else if (SaveData.floor >= 20)
						{
							this.turns.Add(new List<EncounterBlueprintData.CardBlueprint> { midgameCard.CreateBlueprint(), earlygameCard2.CreateBlueprint() });
						}
						if (i > 7 && UnityEngine.Random.RandomRangeInt(0, 100) > 70)
						{
							List<EncounterBlueprintData.CardBlueprint> bp = new List<EncounterBlueprintData.CardBlueprint> { pixelCards.FindAll((CardInfo x) => x.temple == infact2.Plugin.functionsnstuff.getTemple(SaveData.roomId)).GetRandomItem().CreateBlueprint() };
						}
						if (i > 4 - SaveData.floor / 5)
						{
							if (i % 2 == 0 && UnityEngine.Random.RandomRangeInt(0, 100) < 75 - SaveData.floor * 2)
							{
								this.turns.Add(new List<EncounterBlueprintData.CardBlueprint> { });
							}
							else
							{
								List<CardInfo> cards = new List<CardInfo> { midgameCard, lategameCard };
								List<CardInfo> cards2 = new List<CardInfo> { midgameCard2, lategameCard2 };
								List<EncounterBlueprintData.CardBlueprint> bp = new List<EncounterBlueprintData.CardBlueprint> { cards.GetRandomItem().CreateBlueprint() };
								if (UnityEngine.Random.RandomRangeInt(0, 100) > 75 - SaveData.floor * 2)
								{
									bp.Add(cards.GetRandomItem().CreateBlueprint());
								}
								if (UnityEngine.Random.RandomRangeInt(0, 100) > 80)
                                {
									bp.Add(cards2.GetRandomItem().CreateBlueprint());
                                }
								this.turns.Add(bp);
							}

						}
					}
				}


			}
		}
		public class doubleTempleBlueprint : EncounterBlueprintData
		{
			public doubleTempleBlueprint()
			{
				List<CardInfo> pixelCards = CardLoader.GetPixelCards();
				pixelCards.RemoveAll((CardInfo x) => !x.metaCategories.Contains(CardMetaCategory.GBCPack) || x.name == "MasterBleene" || x.name == "Bonepile" || x.name == "TombRobber" || x.GetExtendedProperty("InfAct2ExcludeFromBattle") != null);


				CardInfo earlygameCard;
				CardInfo earlygameCard2;
				CardInfo midgameCard;
				CardInfo midgameCard2;
				CardInfo lategameCard;
				CardInfo lategameCard2;

				CardInfo BountyHunter = GenerateBountyHunter();
				string[] bountyData = SaveData.bountyHunters.Split(';');
				bool bountyEligble = SaveData.bountyStars >= 1 && bountyData[0] == "0" && UnityEngine.Random.RandomRangeInt(0, 100) > 70 - (SaveData.floor * 3 + SaveData.bountyStars * 5f);
				int bountyTurn = UnityEngine.Random.RandomRangeInt(3, 7);

				base.name = "doubleTempleBlueprint";
				this.turns = new List<List<EncounterBlueprintData.CardBlueprint>>();
				bool conditions = SaveData.floor > 3 && UnityEngine.Random.RandomRangeInt(0, 100) < 20;
				List<CardTemple> baseTemples = new List<CardTemple> { CardTemple.Nature, CardTemple.Tech, CardTemple.Undead, CardTemple.Wizard };
				baseTemples.Remove(infact2.Plugin.functionsnstuff.getTemple(SaveData.roomId));
				CardTemple otherTemple = baseTemples.GetRandomItem();
				if (infact2.Plugin.functionsnstuff.getTemple(SaveData.roomId) != CardTemple.Wizard) { pixelCards.RemoveAll((CardInfo x) => x.HasAbility(Ability.GemDependant)); }
				else
				{
					List<CardInfo> pixelCards2 = CardLoader.GetPixelCards();
					pixelCards2.RemoveAll((CardInfo x) => !x.metaCategories.Contains(CardMetaCategory.GBCPack) && !x.HasAbility(Ability.GemDependant));
					foreach (CardInfo card in pixelCards2)
					{
						pixelCards.Add(card);
					}
				}
				switch (infact2.Plugin.functionsnstuff.getTemple(SaveData.roomId))
				{
					case CardTemple.Undead:
						pixelCards.RemoveAll((CardInfo x) => x.bonesCost < 0 || x.temple != CardTemple.Undead ||x.name == "SkeletonMage");
						// there are a few free bone cards we want to play here like skeleton, thats why it checks for the temple here, pre floor 5 we dont want anything big
						if (SaveData.floor < 10) earlygameCard = pixelCards.FindAll((CardInfo x) => returnBank("bones", x) < 5 && x.temple == CardTemple.Undead && x.Attack > 0).GetRandomItem();
						else earlygameCard = pixelCards.FindAll((CardInfo x) => x.metaCategories.Contains(CardMetaCategory.Rare) && returnBank("bones", x) > (SaveData.floor / 3) && returnBank("bones", x) < (6 + SaveData.floor / 3)).GetRandomItem();



						if (SaveData.floor < 15) midgameCard = pixelCards.FindAll((CardInfo x) => returnBank("bones", x) < 10 && x.temple == CardTemple.Undead).GetRandomItem();
						else midgameCard = pixelCards.FindAll((CardInfo x) => x.metaCategories.Contains(CardMetaCategory.Rare) && x.temple == CardTemple.Undead).GetRandomItem();




						lategameCard = pixelCards.FindAll((CardInfo x) => x.metaCategories.Contains(CardMetaCategory.Rare) && x.temple == CardTemple.Undead).GetRandomItem();


						//bone cost will eventually scale ad infinitum, if floor is more than 40 i will just set them all to the max possible where it will still find a card
						if (SaveData.floor > 39)
						{

							earlygameCard = pixelCards.FindAll((CardInfo x) => returnBank("bones", x) < 7 && x.temple == CardTemple.Undead).GetRandomItem();


							midgameCard = pixelCards.FindAll((CardInfo x) => x.metaCategories.Contains(CardMetaCategory.Rare) && returnBank("bones", x) < 10 && x.temple == CardTemple.Undead).GetRandomItem();

						}


						break;

					default:
					case CardTemple.Nature:
						pixelCards.RemoveAll((CardInfo x) => x.BloodCost < 0 || x.temple != CardTemple.Nature);
						//no free blood cards exist that we want played, with blood we cant dynamically choose a cost as with bones so i just have some preset values where it will change
						if (SaveData.floor < 10) earlygameCard = pixelCards.FindAll((CardInfo x) => !x.metaCategories.Contains(CardMetaCategory.Rare) && returnBank("blood", x) > 0 && returnBank("blood", x) < 3 && x.Attack > 0).GetRandomItem();
						else if (SaveData.floor < 40) earlygameCard = pixelCards.FindAll((CardInfo x) => x.metaCategories.Contains(CardMetaCategory.Rare) && returnBank("blood", x) >= 1 && returnBank("blood", x) < 3 && x.Attack > 0).GetRandomItem();
						else earlygameCard = pixelCards.FindAll((CardInfo x) => x.metaCategories.Contains(CardMetaCategory.Rare) && returnBank("blood", x) > 2 && returnBank("blood", x) < 4 && x.Attack > 0).GetRandomItem();




						if (SaveData.floor < 10) midgameCard = pixelCards.FindAll((CardInfo x) => x.metaCategories.Contains(CardMetaCategory.Rare) && returnBank("blood", x) < 4 && x.temple == CardTemple.Nature || returnBank("blood", x) >= 2 && returnBank("blood", x) < 4).GetRandomItem();
						else midgameCard = pixelCards.FindAll((CardInfo x) => x.metaCategories.Contains(CardMetaCategory.Rare) && x.temple == CardTemple.Nature && returnBank("blood", x) < 4 || returnBank("blood", x) >= 2 && returnBank("blood", x) < 4).GetRandomItem();




						lategameCard = pixelCards.FindAll((CardInfo x) => x.metaCategories.Contains(CardMetaCategory.Rare) && x.temple == CardTemple.Nature).GetRandomItem();

						break;

					case CardTemple.Tech:
						pixelCards.RemoveAll((CardInfo x) => x.energyCost < 0 || x.temple != CardTemple.Tech);
						if (SaveData.floor < 10) earlygameCard = pixelCards.FindAll((CardInfo x) => !x.metaCategories.Contains(CardMetaCategory.Rare) && returnBank("energy", x) >= 2 && returnBank("energy", x) <= 5 && x.Attack > 0).GetRandomItem();
						else if (SaveData.floor < 40) earlygameCard = pixelCards.FindAll((CardInfo x) => returnBank("energy", x) > 2 && returnBank("energy", x) <= 6 && x.Attack > 0).GetRandomItem();
						else earlygameCard = pixelCards.FindAll((CardInfo x) => returnBank("energy", x) > 3).GetRandomItem();



						if (SaveData.floor < 7) midgameCard = pixelCards.FindAll((CardInfo x) => returnBank("energy", x) > 1 && returnBank("energy", x) <= 6 && x.Attack > 0).GetRandomItem();
						else if (SaveData.floor < 40) midgameCard = pixelCards.FindAll((CardInfo x) => returnBank("energy", x) > 2 && x.Attack > 0).GetRandomItem();
						else midgameCard = pixelCards.FindAll((CardInfo x) => returnBank("energy", x) >= 4 && x.Attack > 0).GetRandomItem();



						if (SaveData.floor < 7) lategameCard = pixelCards.FindAll((CardInfo x) => x.metaCategories.Contains(CardMetaCategory.Rare) && returnBank("energy", x) > 0).GetRandomItem();
						else lategameCard = pixelCards.FindAll((CardInfo x) => x.metaCategories.Contains(CardMetaCategory.Rare) && returnBank("energy", x) >= 2).GetRandomItem();


						break;

					case CardTemple.Wizard:
						if (SaveData.floor < 7) earlygameCard = pixelCards.FindAll((CardInfo x) => x.gemsCost.Count == 1).GetRandomItem();
						else earlygameCard = pixelCards.FindAll((CardInfo x) => x.gemsCost.Count > 0).GetRandomItem();


						midgameCard = pixelCards.FindAll((CardInfo x) => x.gemsCost.Count > 0).GetRandomItem();

						lategameCard = pixelCards.FindAll((CardInfo x) => x.gemsCost.Count > 0 && x.HasCardMetaCategory(CardMetaCategory.Rare)).GetRandomItem();

						break;
				}
				pixelCards = CardLoader.GetPixelCards();
				pixelCards.RemoveAll((CardInfo x) => !x.metaCategories.Contains(CardMetaCategory.GBCPack) || x.name == "MasterBleene" || x.name == "Bonepile" || x.name == "TombRobber" || x.GetExtendedProperty("InfAct2ExcludeFromBattle") != null);
				if (otherTemple != CardTemple.Wizard) { pixelCards.RemoveAll((CardInfo x) => x.HasAbility(Ability.GemDependant)); }
				else
				{
					List<CardInfo> pixelCards2 = CardLoader.GetPixelCards();
					pixelCards2.RemoveAll((CardInfo x) => !x.metaCategories.Contains(CardMetaCategory.GBCPack) && !x.HasAbility(Ability.GemDependant));
					foreach (CardInfo card in pixelCards2)
					{
						pixelCards.Add(card);
					}
				}
				switch (otherTemple)
				{
					case CardTemple.Undead:
						pixelCards.RemoveAll((CardInfo x) => x.bonesCost < 0 || x.temple != CardTemple.Undead ||x.name == "SkeletonMage");
						// there are a few free bone cards we want to play here like skeleton, thats why it checks for the temple here, pre floor 5 we dont want anything big
						if (SaveData.floor < 10) earlygameCard2 = pixelCards.FindAll((CardInfo x) => returnBank("bones", x) < 5 && x.temple == CardTemple.Undead).GetRandomItem();
						else earlygameCard2 = pixelCards.FindAll((CardInfo x) => x.metaCategories.Contains(CardMetaCategory.Rare) && returnBank("bones", x) > (SaveData.floor / 3) && returnBank("bones", x) < (6 + SaveData.floor / 3)).GetRandomItem();
					


						if (SaveData.floor < 15) midgameCard2 = pixelCards.FindAll((CardInfo x) =>returnBank("bones", x) < 10 && x.temple == CardTemple.Undead).GetRandomItem();
						else midgameCard2 = pixelCards.FindAll((CardInfo x) => x.metaCategories.Contains(CardMetaCategory.Rare) && x.temple == CardTemple.Undead).GetRandomItem();




						lategameCard2 = pixelCards.FindAll((CardInfo x) => x.metaCategories.Contains(CardMetaCategory.Rare) && x.temple == CardTemple.Undead).GetRandomItem();


						//bone cost will eventually scale ad infinitum, if floor is more than 40 i will just set them all to the max possible where it will still find a card
						if (SaveData.floor > 39)
						{

							earlygameCard2 = pixelCards.FindAll((CardInfo x) => returnBank("bones", x) < 7 && x.temple == CardTemple.Undead).GetRandomItem();


							midgameCard2 = pixelCards.FindAll((CardInfo x) => x.metaCategories.Contains(CardMetaCategory.Rare) && returnBank("bones", x) < 10 && x.temple == CardTemple.Undead).GetRandomItem();

						}


						break;

					default:
					case CardTemple.Nature:
						pixelCards.RemoveAll((CardInfo x) => x.BloodCost < 0 || x.temple != CardTemple.Nature);
						//no free blood cards exist that we want played, with blood we cant dynamically choose a cost as with bones so i just have some preset values where it will change
						if (SaveData.floor < 10) earlygameCard2 = pixelCards.FindAll((CardInfo x) => !x.metaCategories.Contains(CardMetaCategory.Rare) && returnBank("blood", x) >= 1 && returnBank("blood", x) < 3).GetRandomItem();
						else if (SaveData.floor < 40) earlygameCard2 = pixelCards.FindAll((CardInfo x) => x.metaCategories.Contains(CardMetaCategory.Rare) && returnBank("blood", x) > 0 && returnBank("blood", x) <= 3).GetRandomItem();
						else earlygameCard2 = pixelCards.FindAll((CardInfo x) => x.metaCategories.Contains(CardMetaCategory.Rare) && returnBank("blood", x) > 2 && returnBank("blood", x) < 4).GetRandomItem();




						if (SaveData.floor < 10) midgameCard2 = pixelCards.FindAll((CardInfo x) => x.metaCategories.Contains(CardMetaCategory.Rare) && returnBank("blood", x) < 4 && x.temple == CardTemple.Nature || returnBank("blood", x) >= 2 && returnBank("blood", x) < 4).GetRandomItem();
						else midgameCard2 = pixelCards.FindAll((CardInfo x) => x.metaCategories.Contains(CardMetaCategory.Rare) && x.temple == CardTemple.Nature && returnBank("blood", x) < 4 || returnBank("blood", x) >= 2 && returnBank("blood", x) < 4).GetRandomItem();



						lategameCard2 = pixelCards.FindAll((CardInfo x) => x.metaCategories.Contains(CardMetaCategory.Rare) && x.temple == CardTemple.Nature).GetRandomItem();

						break;

					case CardTemple.Tech:
						pixelCards.RemoveAll((CardInfo x) => x.energyCost < 0 || x.temple != CardTemple.Tech);
						if (SaveData.floor < 10) earlygameCard2 = pixelCards.FindAll((CardInfo x) => !x.metaCategories.Contains(CardMetaCategory.Rare) && returnBank("energy", x) >= 2 && returnBank("energy", x) <= 5 && x.Attack > 0).GetRandomItem();
						else if (SaveData.floor < 40) earlygameCard2 = pixelCards.FindAll((CardInfo x) => returnBank("energy", x) > 2 && returnBank("energy", x) <= 6).GetRandomItem();
						else earlygameCard2 = pixelCards.FindAll((CardInfo x) => returnBank("energy", x) > 3).GetRandomItem();



						if (SaveData.floor < 7) midgameCard2 = pixelCards.FindAll((CardInfo x) => returnBank("energy", x) > 1 && returnBank("energy", x) <= 6 && x.Attack > 0).GetRandomItem();
						else if (SaveData.floor < 40) midgameCard2 = pixelCards.FindAll((CardInfo x) => returnBank("energy", x) > 2 && x.Attack > 0).GetRandomItem();
						else midgameCard2 = pixelCards.FindAll((CardInfo x) => returnBank("energy", x) >= 4).GetRandomItem();



						if (SaveData.floor < 7) lategameCard2 = pixelCards.FindAll((CardInfo x) => x.metaCategories.Contains(CardMetaCategory.Rare) && returnBank("energy", x) > 0).GetRandomItem();
						else lategameCard2 = pixelCards.FindAll((CardInfo x) => x.metaCategories.Contains(CardMetaCategory.Rare) && returnBank("energy", x) >= 2).GetRandomItem();


						break;

					case CardTemple.Wizard:
						if (SaveData.floor < 7) earlygameCard2 = pixelCards.FindAll((CardInfo x) => x.gemsCost.Count == 1).GetRandomItem();
						else earlygameCard2 = pixelCards.FindAll((CardInfo x) => x.gemsCost.Count > 0).GetRandomItem();


						midgameCard2 = pixelCards.FindAll((CardInfo x) => x.gemsCost.Count > 0).GetRandomItem();

						lategameCard2 = pixelCards.FindAll((CardInfo x) => x.gemsCost.Count > 0 && x.HasCardMetaCategory(CardMetaCategory.Rare)).GetRandomItem();

						break;
				}

				if (otherTemple == CardTemple.Wizard && infact2.Plugin.functionsnstuff.getTemple(SaveData.roomId) != CardTemple.Tech) { this.turns.Add(new List<EncounterBlueprintData.CardBlueprint> { CardLoader.GetCardByName("MoxTriple").CreateBlueprint() }); }
				else if (otherTemple == CardTemple.Wizard && infact2.Plugin.functionsnstuff.getTemple(SaveData.roomId) == CardTemple.Tech) { this.turns.Add(new List<EncounterBlueprintData.CardBlueprint> { CardLoader.GetCardByName("TechMoxTriple").CreateBlueprint() }); }

				for (int i = 1; i < 50; i++)
				{
					if (i == bountyTurn && bountyEligble)
					{
						this.turns.Add(new List<EncounterBlueprintData.CardBlueprint> { BountyHunter.CreateBlueprint() });
					}
					if (i < 4 - SaveData.floor / 5 && SaveData.floor < 20)
					{
						if (i % 2 == 0 && UnityEngine.Random.RandomRangeInt(0, 100) < 75 - SaveData.floor * 2)
						{
							this.turns.Add(new List<EncounterBlueprintData.CardBlueprint>
							{
								new EncounterBlueprintData.CardBlueprint
								{

								}

							});
						}
						else
						{
							List<CardInfo> cards = new List<CardInfo> { earlygameCard, earlygameCard2 };
							List<CardInfo> laterCards = new List<CardInfo> { midgameCard, midgameCard2 };
							List<EncounterBlueprintData.CardBlueprint> bp = new List<EncounterBlueprintData.CardBlueprint> { cards.GetRandomItem().CreateBlueprint() };

							if (UnityEngine.Random.RandomRangeInt(0, 100) > 102 - SaveData.floor * 4)
							{
								bp = new List<EncounterBlueprintData.CardBlueprint> { laterCards.GetRandomItem().CreateBlueprint() };
								this.turns.Add(bp);
							}
							else if (UnityEngine.Random.RandomRangeInt(0, 100) > 75 - SaveData.floor * 3)
							{
								bp.Add(cards.GetRandomItem().CreateBlueprint());
							}

							this.turns.Add(bp);
						}
					}
					else if (SaveData.floor >= 20)
					{
						this.turns.Add(new List<EncounterBlueprintData.CardBlueprint> { midgameCard.CreateBlueprint(), earlygameCard2.CreateBlueprint() });
					}
					if (i > 7 && UnityEngine.Random.RandomRangeInt(0, 100) > 80)
					{
						List<EncounterBlueprintData.CardBlueprint> bp = new List<EncounterBlueprintData.CardBlueprint> { pixelCards.FindAll((CardInfo x) => x.temple == infact2.Plugin.functionsnstuff.getTemple(SaveData.roomId) || x.temple == otherTemple).GetRandomItem().CreateBlueprint() };
					}
					if (i > 4 - SaveData.floor / 5 && i <= 7)
					{
						if (i % 2 == 0 && UnityEngine.Random.RandomRangeInt(0, 100) < 75 - SaveData.floor * 2)
						{
							this.turns.Add(new List<EncounterBlueprintData.CardBlueprint> { });
						}
						else
						{
							List<CardInfo> cards = new List<CardInfo> { midgameCard, midgameCard2 };
							if (UnityEngine.Random.RandomRangeInt(0, 100) > 65 - SaveData.floor * 2)
							{
								this.turns.Add(new List<EncounterBlueprintData.CardBlueprint> { cards.GetRandomItem().CreateBlueprint(), cards.GetRandomItem().CreateBlueprint() });
							}
							else
							{
								this.turns.Add(new List<EncounterBlueprintData.CardBlueprint> { cards.GetRandomItem().CreateBlueprint() });
							}
						}

					}
					else if (i > 7)
					{
						if (i % 2 == 0 && UnityEngine.Random.RandomRangeInt(0, 100) < 75 - SaveData.floor * 2)
						{
							this.turns.Add(new List<EncounterBlueprintData.CardBlueprint> { });
						}
						else
						{
							List<CardInfo> cards = new List<CardInfo> { lategameCard, lategameCard2 };
							if (UnityEngine.Random.RandomRangeInt(0, 100) > 65 - SaveData.floor * 2)
							{
								this.turns.Add(new List<EncounterBlueprintData.CardBlueprint> { cards.GetRandomItem().CreateBlueprint(), cards.GetRandomItem().CreateBlueprint() });
							}
							else
							{
								this.turns.Add(new List<EncounterBlueprintData.CardBlueprint> { cards.GetRandomItem().CreateBlueprint() });
							}
						}
					}
				}
			}
		}
	}
}

