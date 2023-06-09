using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using APIPlugin;
using InscryptionAPI.Card;
using InscryptionCommunityPatch.Card;
using BepInEx;
using DiskCardGame;
using HarmonyLib;
using Pixelplacement;
using UnityEngine;
using UnityEngine.UIElements;
using CustomCategory = infact2.Plugin;
using functionsnstuff = infact2.Plugin.functionsnstuff;

namespace infact2
{
	public class PickUpCardVolumeFixes
	{

		[HarmonyPatch(typeof(GBC.PickupCardPileVolume), "OnTextBoxButtonPressed")]
		public class chooseDeckType
		{
			public static bool Prefix(ref GBC.PickupCardPileVolume __instance, int buttonIndex)
			{
				if (__instance.name.Contains("CardcostEvent"))
				{
					string Type = __instance.gameObject.GetComponent<infact2.GainCardEvent>().options[buttonIndex];
					__instance.gameObject.GetComponent<infact2.GainCardEvent>().PackChoice = functionsnstuff.getTemple(Type);
					__instance.StartCoroutine(__instance.PickupCardsSequence());
					return false;
				} else if (__instance.name.Contains("MycoEvent"))
				{
					string Type = __instance.gameObject.GetComponent<infact2.GainCardEvent>().options[buttonIndex];
					if (Type == "None")
                    {
						Singleton<GBC.PlayerMovementController>.Instance.SetEnabled(true);
						SaveManager.SaveToFile(true);
						SaveData.nodesCompleted += 1;
						functionsnstuff.PlaceBarriers(true);
						__instance.gameObject.transform.Find("CardPile").gameObject.SetActive(false);
						return false;
					}
					__instance.gameObject.GetComponent<infact2.GainCardEvent>().selected = Type;
					__instance.StartCoroutine(__instance.PickupCardsSequence());
					return false;
				} else if (__instance.name == "ChallengeHitbox")
                {
					__instance.StartCoroutine(functionsnstuff.EnableChallengeUI());
					return false;
                }
				if (buttonIndex == 0)
				{
					__instance.StartCoroutine(__instance.PickupCardsSequence());
				}
				return false;
			}
		}
		[HarmonyPatch(typeof(GBC.PickupCardPileVolume), "ShowSingleTextLine")]
		public class FixCardText
		{
			public static void Prefix(ref GBC.PickupCardPileVolume __instance, out GBC.PickupCardPileVolume __state)
			{
				__state = __instance;
			}

			public static IEnumerator Postfix(IEnumerator enumerator, GBC.PickupCardPileVolume __state, string message, bool lastLine)
			{
				if (__state.gameObject.name.Contains("CardpackEvent") || __state.gameObject.name.Contains("BoonEvent") || __state.gameObject.name.Contains("Rarecardevent") || __state.gameObject.name.Contains("CardBattleEvent") || __state.gameObject.name.Contains("EliteBattleEvent") || __state.gameObject.name.Contains("CardBossEvent") || __state.gameObject.name.Contains("ShopEvent") || __state.gameObject.name.Contains("FishingEvent") || __state.gameObject.name.Contains("VesselEvent") || __state.gameObject.name.Contains("BoneLordEvent") || __state.gameObject.name.Contains("MoxEvent") || __state.gameObject.name == "ChallengeHitbox")
				{
					__state.OnTextBoxButtonPressed(0);
					yield break;
				}
				else if (__state.gameObject.name.Contains("CardcostEvent"))
				{
					List<string> options = new List<string> { "Beasts", "Dead", "Magick", "Technology" };
					string option1 = options[UnityEngine.Random.RandomRangeInt(0, options.Count)];
					options.Remove(option1);
					string option2 = options[UnityEngine.Random.RandomRangeInt(0, options.Count)];
					__state.gameObject.GetComponent<infact2.GainCardEvent>().options = new List<string> { option1, option2 };
					yield return Singleton<GBC.TextBox>.Instance.ShowUntilInput("Choose a card type.", __state.style, null, __state.screenPosition, 0f, true, false, new GBC.TextBox.Prompt(option1, option2, new Action<int>(__state.OnTextBoxButtonPressed)), true, Emotion.Neutral);
					yield break;
				}
				else if (__state.gameObject.name.Contains("Challenge_"))
				{
					string challenge = __state.gameObject.name.Split('_')[1];
					List<string> challengeData = new List<string> { "Bounty", "Every 3 succesful battles, you gain a bounty star. With bounty stars, random bounty hunters will start to spawn in battles.", "Boon", "You have only one boon slot.", "Elite", "Every regular card battle is replaced with an elite battle." };
					__state.gameObject.GetComponent<infact2.GainCardEvent>().options = new List<string> { challenge, "buh" };
					yield return Singleton<GBC.TextBox>.Instance.ShowUntilInput(challengeData[challengeData.IndexOf(challenge) + 1], __state.style, null, __state.screenPosition, 0f, true, false, new GBC.TextBox.Prompt("Enable", "Disable", new Action<int>(__state.OnTextBoxButtonPressed)), true, Emotion.Neutral);
					yield break;
				}
				else if (__state.gameObject.name.Contains("MycoEvent"))
				{
					List<int> counts = new List<int>();
					counts.Add(SaveManager.saveFile.gbcData.collection.cardIds.Count(x => x == "FieldMouse"));
					counts.Add(SaveManager.saveFile.gbcData.collection.cardIds.Count(x => x == "SentryBot"));
					counts.Add(SaveManager.saveFile.gbcData.collection.cardIds.Count(x => x == "BlueMage"));
					counts.Add(SaveManager.saveFile.gbcData.collection.cardIds.Count(x => x == "Gravedigger"));
					List<string> names = new List<string> { "FieldMouse", "SentryBot", "BlueMage", "Gravedigger" };
					List<string> options = new List<string>();
					for (int i = 0; i < counts.Count; i++)
					{
						if (counts[i] >= 2) { options.Add(names[i]); };
					}
					string option1 = "";
					string option2 = "";
					if (options.Count < 2) { option1 = "Merge " + CardLoader.GetCardByName(options[0]).displayedName; option2 = "Don't merge."; __state.gameObject.GetComponent<infact2.GainCardEvent>().options = new List<string> { options[0], "None" }; }
					else
					{
						string rand = options.GetRandomItem();
						option1 = "Merge " + CardLoader.GetCardByName(rand).displayedName;

						options.Remove(rand);
						string rand2 = options.GetRandomItem();
						option2 = "Merge " + CardLoader.GetCardByName(rand2).displayedName;
						__state.gameObject.GetComponent<infact2.GainCardEvent>().options = new List<string> { rand, rand2 };
					}

					yield return Singleton<GBC.TextBox>.Instance.ShowUntilInput("You have a choice of merging two cards here..", __state.style, null, __state.screenPosition, 0f, true, false, new GBC.TextBox.Prompt(option1, option2, new Action<int>(__state.OnTextBoxButtonPressed)), true, Emotion.Neutral);
					yield break;
				}
				if (SaveData.roomId == "DeckChooseRoom")
				{
					var selectMsg = "Select the {} deck?";
					switch (__state.starterDeckType)
					{
						case CardTemple.Wizard:
							selectMsg = "Select the Deck of Magick?";
							break;
						case CardTemple.Nature:
							selectMsg = "Select the Deck of Beasts?";
							break;
						case CardTemple.Undead:
							selectMsg = "Select the Deck of the Dead?";
							break;
						case CardTemple.Tech:
							selectMsg = "Select the Deck of Technology?";
							break;
					}
					yield return Singleton<GBC.TextBox>.Instance.ShowUntilInput(selectMsg, __state.style, null, __state.screenPosition, 0f, true, false, new GBC.TextBox.Prompt("Take the deck.", "Leave it.", new Action<int>(__state.OnTextBoxButtonPressed)), true, Emotion.Neutral);
					yield break;
				}
				yield return Singleton<GBC.TextBox>.Instance.ShowUntilInput(message, __state.style, null, __state.screenPosition, 0f, true, false, new GBC.TextBox.Prompt("Take the deck.", "Leave it.", new Action<int>(__state.OnTextBoxButtonPressed)), true, Emotion.Neutral);
				yield break;
			}
		}

		[HarmonyPatch(typeof(GBC.PickupCardPileVolume), "PickupCardsSequence")]
		public class InfCardReset
		{
			public static void Prefix(ref GBC.PickupCardPileVolume __instance)
			{
				if (SaveData.roomId == "DeckChooseRoom")
				{
					SaveManager.saveFile.gbcCardsCollected = new List<string>();
					for (int i = 0; i <= SaveManager.saveFile.gbcData.collection.Cards.Count; i = i)
					{
						if (SaveManager.saveFile.gbcData.collection.Cards.Count != 0)
						{
							SaveManager.saveFile.gbcData.collection.RemoveCard(SaveManager.saveFile.gbcData.collection.Cards[i]);
						}
						else
						{
							i = 1;
							continue;
						}

					}
					for (int i = 0; i <= SaveManager.saveFile.gbcData.deck.Cards.Count; i = i)
					{
						if (SaveManager.saveFile.gbcData.deck.Cards.Count != 0)
						{
							SaveManager.saveFile.gbcData.deck.RemoveCard(SaveManager.saveFile.gbcData.deck.Cards[i]);
						}
						else
						{
							i = 1;
							continue;
						}

					}
					SaveManager.SaveToFile(true);
				}

			}

		}
		
		public static IEnumerator AddBoonToDeck(CardInfo boon, GBC.PixelSelectableCard card)
        {
			GameObject boonUI = GameObject.Find("BoonChoiceUI");
			SaveManager.saveFile.gbcData.collection.AddCard(boon);
			card.SetFaceDown(true);
			AudioController.Instance.PlaySound2D("chipDelay_3", MixerGroup.None, 0.5f, 0f, null, null, null, null, false);
			Tween.LocalPosition(card.transform, new Vector3(card.gameObject.transform.localPosition.x, 10f, card.gameObject.transform.localPosition.z), 1f, 0.9f);
			yield return new WaitForSeconds(1.5f);
			AudioController.Instance.PlaySound2D("crunch_blip", MixerGroup.None, 0.6f, 0f, new AudioParams.Pitch(0.8f), new AudioParams.Repetition(0.1f, "menucrunch"), null, null, false);
			boonUI.transform.Find("MainPanel").gameObject.SetActive(false);
			boonUI.transform.Find("CardPreviewPanel").gameObject.SetActive(false);
			//boonUI.SetActive(false);
			Singleton<GBC.PlayerMovementController>.Instance.SetEnabled(true);
			SaveManager.SaveToFile(true);
			SaveData.nodesCompleted += 1;
			functionsnstuff.PlaceBarriers(true);
			yield break;
		}

		[HarmonyPatch(typeof(GBC.PickupCardPileVolume), "PickupCardsSequence")]
		public class InfCardPickup
		{
			public static void Prefix(ref GBC.PickupCardPileVolume __instance, ref GBC.PickupCardPileVolume __state)
			{
				__state = __instance;
			}

			public static IEnumerator Postfix(IEnumerator emuemuear, GBC.PickupCardPileVolume __state)
			{
				if (!Singleton<GBC.PlayerMovementController>.Instance.gameObject.activeSelf || GameObject.Find("UI").transform.Find("BoonChoiceUI").transform.Find("MainPanel").gameObject.activeSelf || __state.gameObject.name.Contains("ChallengeHitbox"))
				{
					yield break;
				}

				__state.GetComponent<Collider2D>().enabled = false;
				yield return new WaitForEndOfFrame();
				Singleton<GBC.PlayerMovementController>.Instance.SetEnabled(false);
				yield return new WaitForSeconds(0.2f);
				__state.pileObject.SetActive(false);
				yield return new WaitForSeconds(0.2f);

				if (__state.gameObject.name.Contains("CardpackEvent") || __state.gameObject.name.Contains("CardcostEvent") || __state.gameObject.name.Contains("Rarecardevent"))
				{
					List<CardTemple> packTypes = new List<CardTemple> { functionsnstuff.getTemple(SaveData.roomId) };
					if (__state.gameObject.name.Contains("CardcostEvent"))
					{
						packTypes = new List<CardTemple> { __state.gameObject.GetComponent<infact2.GainCardEvent>().PackChoice };
					}
					Singleton<GBC.PlayerMovementController>.Instance.SetEnabled(false);
					foreach (CardTemple packType in packTypes)
					{
						yield return GBC.PackOpeningUI.instance.OpenPack(packType);
					}
					List<CardTemple>.Enumerator enumerator = default(List<CardTemple>.Enumerator);
					Singleton<GBC.PlayerMovementController>.Instance.SetEnabled(true);
					SaveManager.SaveToFile(true);
					SaveData.nodesCompleted += 1;
					functionsnstuff.PlaceBarriers(true);
					yield break;
				}
				else if (__state.gameObject.name.Contains("CardBattleEvent") || __state.gameObject.name.Contains("CardBossEvent") || __state.gameObject.name.Contains("EliteBattleEvent"))
				{
					bool couldNotBattleInvalidDeck = false;
					Singleton<GBC.PlayerMovementController>.Instance.SetEnabled(false);
					__state.SetCollisionEnabled(false);
					AudioController.Instance.FadeOutLoop(1f, Array.Empty<int>());

					if (SaveManager.saveFile.gbcData.deck.IsValidGBCDeck())
					{
						couldNotBattleInvalidDeck = false;
						EncounterData encounterData = new EncounterData();
						encounterData.opponentTurnPlan = EncounterBuilder.BuildOpponentTurnPlan(ScriptableObject.CreateInstance<MakeBluep.baseblueprint>(), 1, false);
						if (__state.gameObject.name.Contains("CardBossEvent"))
						{
							encounterData.opponentTurnPlan = EncounterBuilder.BuildOpponentTurnPlan(ScriptableObject.CreateInstance<MakeBluep.bossblueprint>(), 1, false);
						}
						else if (__state.gameObject.name.Contains("EliteBattleEvent"))
						{
							encounterData.opponentTurnPlan = EncounterBuilder.BuildOpponentTurnPlan(ScriptableObject.CreateInstance<MakeBluep.doubleTempleBlueprint>(), 1, false);
						}
						encounterData.opponentType = Opponent.Type.Default;
						encounterData.aiId = "AI";
						string type = "INFBATTLE";
						if (__state.gameObject.name.Contains("CardBossEvent"))
						{
							type = "INFBATTLEBOSS";
							SaveData.lives = 1;
							GameObject.Find("FloorText").GetComponent<UnityEngine.UI.Text>().text = "Floor " + SaveData.floor + "\nLives: " + SaveData.lives;
							if (SaveData.floor > 3 && SaveData.roomId == "NatureDungeon")
							{
								EncounterData.StartCondition startCondition = new EncounterData.StartCondition();
								startCondition.cardsInOpponentSlots[0] = CardLoader.GetCardByName("BurrowingTrap");
								encounterData.startConditions.Add(startCondition);
							}
							else if (SaveData.floor > 3 && SaveData.roomId == "TechDungeon")
							{
								EncounterData.StartCondition startCondition = new EncounterData.StartCondition();
								int limit = 3;
								if (Harmony.HasAnyPatches("julianperge.inscryption.act2.increaseCardSlots"))
								{
									limit = 4;
									startCondition.cardsInOpponentQueue = new CardInfo[5];
									startCondition.cardsInPlayerSlots = new CardInfo[5];
									startCondition.cardsInOpponentSlots = new CardInfo[5];
								}
								List<CardInfo> conduits = ScriptableObjectLoader<CardInfo>.AllData.FindAll((CardInfo x) => x.GetExtendedProperty("InfAct2RandomConduit") != null || x.HasAbility(Ability.ConduitNull) || x.HasAbility(Ability.ConduitFactory) || x.HasAbility(Ability.ConduitEnergy) || x.HasAbility(Ability.ConduitBuffAttack));
								conduits.RemoveAll((CardInfo x) => x.temple != CardTemple.Tech || !x.HasCardMetaCategory(CardMetaCategory.GBCPlayable));
								startCondition.cardsInOpponentSlots[0] = CardLoader.GetCardByName(conduits.GetRandomItem().name);
								string otherConduit = "NullConduit";
								if (UnityEngine.Random.RandomRangeInt(0, 100) > 105 - SaveData.floor * 4) { otherConduit = conduits.GetRandomItem().name; }
								startCondition.cardsInOpponentSlots[limit] = CardLoader.GetCardByName(otherConduit);
								encounterData.startConditions.Add(startCondition);
							}
							else if (SaveData.floor > 3 && SaveData.roomId == "UndeadDungeon")
							{
								EncounterData.StartCondition startCondition = new EncounterData.StartCondition();
								startCondition.cardsInOpponentQueue[0] = CardLoader.GetCardByName("GhostShip");
								encounterData.startConditions.Add(startCondition);
							}
							else if (SaveData.roomId == "WizardDungeon")
							{
								EncounterData.StartCondition startCondition = new EncounterData.StartCondition();
								startCondition.cardsInOpponentSlots[0] = CardLoader.GetCardByName("MoxTriple");
								encounterData.startConditions.Add(startCondition);
							}
						}
						else if (!__state.gameObject.name.Contains("CardBossEvent") && SaveData.roomId == "WizardDungeon")
						{
							EncounterData.StartCondition startCondition = new EncounterData.StartCondition();
							List<string> moxes = new List<string> { "MoxEmerald", "MoxSapphire", "MoxRuby" };
							startCondition.cardsInOpponentSlots[UnityEngine.Random.RandomRangeInt(0, startCondition.cardsInOpponentSlots.Length)] = CardLoader.GetCardByName(moxes[UnityEngine.Random.RandomRangeInt(0, 2)]);
							encounterData.startConditions.Add(startCondition);
						}
						else if (__state.gameObject.name.Contains("EliteBattleEvent"))
						{
							if (SaveData.floor > 3 && SaveData.roomId == "NatureDungeon")
							{
								EncounterData.StartCondition startCondition = new EncounterData.StartCondition();
								startCondition.cardsInOpponentSlots[0] = CardLoader.GetCardByName("BurrowingTrap");
								encounterData.startConditions.Add(startCondition);
							}
							else if (SaveData.floor > 3 && SaveData.roomId == "TechDungeon")
							{
								EncounterData.StartCondition startCondition = new EncounterData.StartCondition();
								int limit = 3;
								if (Harmony.HasAnyPatches("julianperge.inscryption.act2.increaseCardSlots"))
								{
									limit = 4;
									startCondition.cardsInOpponentQueue = new CardInfo[5];
									startCondition.cardsInPlayerSlots = new CardInfo[5];
									startCondition.cardsInOpponentSlots = new CardInfo[5];
								}
								List<CardInfo> conduits = ScriptableObjectLoader<CardInfo>.AllData.FindAll((CardInfo x) => x.GetExtendedProperty("InfAct2RandomConduit") != null || x.HasAbility(Ability.ConduitNull) || x.HasAbility(Ability.ConduitFactory) || x.HasAbility(Ability.ConduitEnergy) || x.HasAbility(Ability.ConduitBuffAttack));
								conduits.RemoveAll((CardInfo x) => x.temple != CardTemple.Tech && !x.HasCardMetaCategory(CardMetaCategory.GBCPlayable));
								startCondition.cardsInOpponentSlots[0] = CardLoader.GetCardByName(conduits.GetRandomItem().name);
								string otherConduit = "NullConduit";
								if (UnityEngine.Random.RandomRangeInt(0, 100) > 105 - SaveData.floor * 4) { otherConduit = conduits.GetRandomItem().name; }
								startCondition.cardsInOpponentSlots[limit] = CardLoader.GetCardByName(otherConduit);
							}
							else if (SaveData.roomId == "WizardDungeon")
							{
								EncounterData.StartCondition startCondition = new EncounterData.StartCondition();
								startCondition.cardsInOpponentQueue[0] = CardLoader.GetCardByName("MoxTriple");
								encounterData.startConditions.Add(startCondition);
							}
							else if (SaveData.roomId == "UndeadDungeon" && SaveData.floor > 3)
							{
								EncounterData.StartCondition startCondition = new EncounterData.StartCondition();
								startCondition.cardsInOpponentQueue[0] = CardLoader.GetCardByName("GhostShip");
								encounterData.startConditions.Add(startCondition);
							}
						}
						if (__state.gameObject.name.Contains("EliteBattleEvent"))
						{
							type = "INFBATTLEDOUBLE";
						}
						List<string> boons = functionsnstuff.returnBoonsAsList();
						if (boons.Contains("infact2_boon_goat"))
						{
							EncounterData.StartCondition startCondition = new EncounterData.StartCondition();
							startCondition.cardsInPlayerSlots[UnityEngine.Random.RandomRangeInt(0, startCondition.cardsInOpponentSlots.Length)] = CardLoader.GetCardByName("Goat");
							encounterData.startConditions.Add(startCondition);
						}
						List<string> terrain = new List<string>();
						switch (SaveData.roomId)
						{
							case "NatureDungeon":
								terrain.Add("infact2_terrain_boulder");
								terrain.Add("infact2_terrain_fir");
								terrain.Add("infact2_terrain_snowfir");
								terrain.Add("infact2_terrain_frozenopossum");
								terrain.Add("infact2_terrain_stump");
								break;
							case "UndeadDungeon":
								terrain.Add("infact2_terrain_kennel");
								terrain.Add("infact2_terrain_obelisk");
								terrain.Add("infact2_terrain_tombstone");
								terrain.Add("infact2_terrain_disturbedgrave");
								break;
							case "TechDungeon":
								terrain.Add("infact2_terrain_annoyfm");
								terrain.Add("infact2_terrain_conduittower");
								terrain.Add("infact2_terrain_brokenbot");
								terrain.Add("infact2_terrain_railing");
								break;
							case "WizardDungeon":
								terrain.Add("infact2_terrain_pillar");
								terrain.Add("infact2_terrain_arch");
								terrain.Add("infact2_terrain_ruinedarch");
								terrain.Add("infact2_terrain_boulderemerald");
								terrain.Add("infact2_terrain_boulderruby");
								terrain.Add("infact2_terrain_bouldersapphire");
								break;
						}
						string pcName = terrain[UnityEngine.Random.RandomRangeInt(0, terrain.Count)];
						string ocName = terrain[UnityEngine.Random.RandomRangeInt(0, terrain.Count)];
						int randlimit = Harmony.HasAnyPatches("julianperge.inscryption.act2.increaseCardSlots") ? 5 : 4;


						if (UnityEngine.Random.RandomRangeInt(0, 100) > 50)
						{
							int dex = UnityEngine.Random.RandomRangeInt(0, randlimit);
							EncounterData.StartCondition startCondition = new EncounterData.StartCondition();
							if (Harmony.HasAnyPatches("julianperge.inscryption.act2.increaseCardSlots"))
							{
								startCondition.cardsInOpponentQueue = new CardInfo[5];
								startCondition.cardsInPlayerSlots = new CardInfo[5];
								startCondition.cardsInOpponentSlots = new CardInfo[5];
							}
							if (ocName == "infact2_terrain_railing")
							{
								ocName = pcName;
								pcName = "infact2_terrain_railing";
							}
							startCondition.cardsInOpponentSlots[dex] = CardLoader.GetCardByName(ocName);
							encounterData.startConditions.Add(startCondition);
						}
						bool random = UnityEngine.Random.RandomRangeInt(0, 100) > 50;
						if (SaveData.isChallengeActive("bridge")) { random = true; pcName = "infact2_terrain_railing"; }
						if (random)
						{
							int dex = UnityEngine.Random.RandomRangeInt(0, randlimit);
							EncounterData.StartCondition startCondition = new EncounterData.StartCondition();
							int limit = 3;
							if (Harmony.HasAnyPatches("julianperge.inscryption.act2.increaseCardSlots"))
                            {
								limit = 4;
								startCondition.cardsInOpponentQueue = new CardInfo[5];
								startCondition.cardsInPlayerSlots = new CardInfo[5];
								startCondition.cardsInOpponentSlots = new CardInfo[5];
                            }
							if (pcName != "infact2_terrain_railing")
							{
								startCondition.cardsInPlayerSlots[dex] = CardLoader.GetCardByName(pcName);
							}
							else
							{
								startCondition.cardsInPlayerSlots[0] = CardLoader.GetCardByName(pcName);
								startCondition.cardsInPlayerSlots[limit] = CardLoader.GetCardByName(pcName);
								startCondition.cardsInOpponentSlots[0] = CardLoader.GetCardByName(pcName);
								startCondition.cardsInOpponentSlots[limit] = CardLoader.GetCardByName(pcName);
								startCondition.cardsInOpponentQueue[0] = CardLoader.GetCardByName(pcName);
								startCondition.cardsInOpponentQueue[limit] = CardLoader.GetCardByName(pcName);
							}
							encounterData.startConditions.Add(startCondition);
						}
						GBC.GBCEncounterManager.Instance.StartEncounter(encounterData, type, null);
					}
					else
					{
						couldNotBattleInvalidDeck = true;
						yield return new WaitForSeconds(0.5f);
						yield return Singleton<GBC.TextBox>.Instance.ShowUntilInput("You need at least 20 cards in your deck!", __state.style, null, __state.screenPosition, 0f, true, false, null, true, Emotion.Neutral); AudioController.Instance.FadeInLoop(1f, 0.55f, Array.Empty<int>());
						if (!ProgressionData.LearnedMechanic(MechanicsConcept.GBCModifyDeck))
						{
							Singleton<GBC.GBCUIManager>.Instance.SetPauseMenuHintShown(true);
							Singleton<InteractionCursor>.Instance.SetHidden(true);
							yield return new WaitUntil(() => InputButtons.GetButton(Button.AltMenu) || InputButtons.GetButton(Button.Menu));
							Singleton<GBC.GBCUIManager>.Instance.SetPauseMenuHintShown(false);
							Singleton<InteractionCursor>.Instance.SetHidden(false);
						}
						__state.gameObject.transform.Find("CardPile").gameObject.SetActive(true);
						Singleton<GBC.PlayerMovementController>.Instance.SetEnabled(true);
						__state.SetCollisionEnabled(true);
						yield break;
					}
					yield break;
				}
				else if (__state.gameObject.name.Contains("ShopEvent"))
				{
					yield return new WaitForSeconds(0.2f);
					Singleton<GBC.ShopUI>.Instance.gameObject.transform.Find("MainPanel").Find("SelectablePack").gameObject.SetActive(false);
					GameObject boonCard = GameObject.Instantiate(Singleton<GBC.ShopUI>.Instance.gameObject.transform.Find("MainPanel").Find("PixelSelectableCard_1").gameObject);
					boonCard.transform.parent = Singleton<GBC.ShopUI>.Instance.gameObject.transform.Find("MainPanel");
					boonCard.transform.localPosition = new Vector3(-0.465f, 0f, 0);
					List<CardInfo> boons = new List<CardInfo>();
					for (int i = 0; i < CardLoader.allData.Count; i++)
					{
						if (CardLoader.allData[i].HasCardMetaCategory(CustomCategory.BoonsPool))
						{
							if (!dupeBoon(CardLoader.allData[i].name))
							{
								boons.Add(CardLoader.allData[i]);
							}
						}
					}
					boonCard.GetComponent<GBC.PixelSelectableCard>().SetInfo(boons[UnityEngine.Random.RandomRangeInt(0, boons.Count)]);
					boonCard.GetComponentInChildren<GBC.ShopUIPricetag>().SetPrice(8);
					Singleton<GBC.ShopUI>.Instance.UpdateInventory(Singleton<GBC.ShopNPC>.Instance, functionsnstuff.getTemple(SaveData.roomId), Singleton<GBC.ShopNPC>.Instance.GetInventory());
					Singleton<GBC.ShopUI>.Instance.Show();
					boonCard.GetComponent<GBC.PixelSelectableCard>().CursorTypeOverride = CursorType.Pickup;
					boonCard.GetComponent<GBC.PixelSelectableCard>().CursorEntered = Singleton<GBC.ShopUI>.Instance.gameObject.transform.Find("MainPanel").GetChild(1).gameObject.GetComponent<GBC.PixelSelectableCard>().CursorEntered;
					boonCard.GetComponent<GBC.PixelSelectableCard>().CursorSelectStarted = Singleton<GBC.ShopUI>.Instance.gameObject.transform.Find("MainPanel").GetChild(1).gameObject.GetComponent<GBC.PixelSelectableCard>().CursorSelectStarted;
					yield return new WaitWhile(() => Singleton<GBC.ShopUI>.Instance.Active);
					yield break;
				}
				else if (__state.gameObject.name.Contains("BoonEvent"))
				{
					yield return new WaitForSeconds(0.2f);
					GameObject boonUI = GameObject.Find("BoonChoiceUI");
					boonUI.transform.Find("MainPanel").gameObject.SetActive(true);
					boonUI.transform.Find("CardPreviewPanel").gameObject.SetActive(true);
					List<CardInfo> boons = new List<CardInfo>();
					for (int i = 0; i < CardLoader.allData.Count; i++)
					{
						if (CardLoader.allData[i].HasCardMetaCategory(CustomCategory.BoonsPool))
						{
							if (!dupeBoon(CardLoader.allData[i].name))
							{
								boons.Add(CardLoader.allData[i]);
							}
						}
					}
					for (int i = 0; i < 2; i++)
					{
						CardInfo boon = boons[UnityEngine.Random.RandomRangeInt(0, boons.Count)];
						float x = -0.25f;
						if (i > 0)
						{
							x = 0.25f;
						}
						boonUI.transform.Find("MainPanel").Find("Cards").GetChild(i).gameObject.transform.localPosition = new Vector3(x, 0, 0);
						boonUI.transform.Find("MainPanel").Find("Cards").GetChild(i).gameObject.GetComponent<GBC.PixelSelectableCard>().SetInfo(boon);
						int j = i;
						boonUI.transform.Find("MainPanel").Find("Cards").GetChild(i).gameObject.GetComponent<GBC.PixelSelectableCard>().CursorTypeOverride = CursorType.Pickup;
						boonUI.transform.Find("MainPanel").Find("Cards").GetChild(i).gameObject.GetComponent<GBC.PixelSelectableCard>().CursorEntered = (Action<MainInputInteractable>)Delegate.Combine(boonUI.transform.Find("MainPanel").Find("Cards").GetChild(i).gameObject.GetComponent<GBC.PixelSelectableCard>().CursorEntered, new Action<MainInputInteractable>(delegate (MainInputInteractable i)
						{
							boonUI.transform.Find("CardPreviewPanel").gameObject.GetComponent<GBC.CardPreviewPanel>().DisplayCard(boonUI.transform.Find("MainPanel").Find("Cards").GetChild(j).gameObject.GetComponent<GBC.PixelSelectableCard>().RenderInfo, null);
						}));
						var state = __state;
						boonUI.transform.Find("MainPanel").Find("Cards").GetChild(i).gameObject.GetComponent<GBC.PixelSelectableCard>().CursorSelectStarted = (Action<MainInputInteractable>)Delegate.Combine(boonUI.transform.Find("MainPanel").Find("Cards").GetChild(i).gameObject.GetComponent<GBC.PixelSelectableCard>().CursorSelectStarted, new Action<MainInputInteractable>(delegate (MainInputInteractable i)
						{
							state.StartCoroutine(AddBoonToDeck(boonUI.transform.Find("MainPanel").Find("Cards").GetChild(j).gameObject.GetComponent<GBC.PixelSelectableCard>().Info, boonUI.transform.Find("MainPanel").Find("Cards").GetChild(j).gameObject.GetComponent<GBC.PixelSelectableCard>()));
						}));
						boons.Remove(boon);
					}

					yield break;
				}
				else if (__state.gameObject.name.Contains("MycoEvent"))
				{
					Singleton<GBC.PlayerMovementController>.Instance.SetEnabled(false);
					string cardType = __state.gameObject.GetComponent<infact2.GainCardEvent>().selected;
					for (int i = 0; i < 2; i++)
					{
						SaveManager.saveFile.gbcData.collection.RemoveCardByName(cardType);
						SaveManager.saveFile.gbcData.deck.RemoveCardByName(cardType);
					}
					yield return new WaitForSeconds(0.2f);
					yield return GameObject.Find("SingleCardGainUI").GetComponent<GBC.SingleCardGainUI>().GainCard(CardLoader.GetCardByName(cardType + "_Fused"));
					SaveManager.SaveToFile();
					Singleton<GBC.PlayerMovementController>.Instance.SetEnabled(true);
					SaveManager.SaveToFile(true);
					SaveData.nodesCompleted += 1;
					functionsnstuff.PlaceBarriers(true);
					yield break;
				}
				else if (__state.gameObject.name.Contains("Challenge_"))
				{
					Singleton<GBC.PlayerMovementController>.Instance.SetEnabled(true);
					yield break;
				}
				else if (__state.gameObject.name.Contains("FishingEvent") && !SaveData.doneAreaSecret)
				{
					SaveData.doneAreaSecret = true;
					List<string> fishCards = new List<string> { "Salmon", "DrownedSoul", "Kraken", "SquidBell", "SquidCards", "SquidMirror", "Hrokkall", "GhostShip", "Kingfisher" };
					List<CardInfo> submerges = new List<CardInfo>();
					for (int i = 0; i < CardLoader.allData.Count; i++)
					{
						if (CardLoader.allData[i].HasAbility(Ability.Submerge) && fishCards.IndexOf(CardLoader.allData[i].name) < 0 && CardLoader.allData[i].HasCardMetaCategory(CardMetaCategory.GBCPlayable))
						{
							submerges.Add(CardLoader.allData[i]);
						}
					}
					foreach (string er in fishCards)
					{
						submerges.Add(CardLoader.GetCardByName(er));
					}
					yield return new WaitForSeconds(0.2f);
					yield return GameObject.Find("SingleCardGainUI").GetComponent<GBC.SingleCardGainUI>().GainCard(submerges[UnityEngine.Random.RandomRangeInt(0, submerges.Count)]);
					SaveManager.SaveToFile();
					yield break;
				}
				else if (__state.gameObject.name.Contains("BoneLordEvent") && !SaveData.doneAreaSecret)
				{
					SaveData.doneAreaSecret = true;
					bool hasLeft = false;
					bool hasRight = false;
					bool hasOffered = false;
					Singleton<GBC.PlayerMovementController>.Instance.SetEnabled(false);
					foreach (CardInfo card in SaveManager.saveFile.gbcData.deck.Cards)
					{
						if (card.name == "CoinLeft") { hasLeft = true; }
						if (card.name == "CoinRight") { hasRight = true; }
						if (card.name == "BonelordHorn") { hasOffered = true; }
					}
					yield return Singleton<GBC.TextBox>.Instance.ShowUntilInput("...", GBC.TextBox.Style.Undead, null, __state.screenPosition, 0f, true, false, null, true, Emotion.Neutral);
					if (hasLeft && hasRight && !hasOffered)
					{
						yield return Singleton<GBC.TextBox>.Instance.ShowUntilInput("Good offering.", GBC.TextBox.Style.Undead, null, __state.screenPosition, 0f, true, false, null, true, Emotion.Neutral);
						yield return Singleton<GBC.TextBox>.Instance.ShowUntilInput("Have this.", GBC.TextBox.Style.Undead, null, __state.screenPosition, 0f, true, false, null, true, Emotion.Neutral);
						yield return new WaitForSeconds(0.2f);
						SaveManager.saveFile.gbcData.collection.RemoveCardByName("CoinLeft");
						SaveManager.saveFile.gbcData.collection.RemoveCardByName("CoinRight");
						SaveManager.saveFile.gbcData.deck.RemoveCardByName("CoinLeft");
						SaveManager.saveFile.gbcData.deck.RemoveCardByName("CoinRight");
						yield return GameObject.Find("SingleCardGainUI").GetComponent<GBC.SingleCardGainUI>().GainCard(CardLoader.GetCardByName("BonelordHorn"));
					}
					else if (!hasOffered)
					{
						yield return Singleton<GBC.TextBox>.Instance.ShowUntilInput("No offering in your deck?", GBC.TextBox.Style.Undead, null, __state.screenPosition, 0f, true, false, null, true, Emotion.Neutral);
						yield return Singleton<GBC.TextBox>.Instance.ShowUntilInput("Fine. Take this instead.", GBC.TextBox.Style.Undead, null, __state.screenPosition, 0f, true, false, null, true, Emotion.Neutral);
						List<CardInfo> brittle = new List<CardInfo>();
						for (int i = 0; i < CardLoader.allData.Count; i++)
						{
							if (CardLoader.allData[i].HasAbility(Ability.Brittle) && CardLoader.allData[i].HasCardMetaCategory(CardMetaCategory.GBCPlayable))
							{
								brittle.Add(CardLoader.allData[i]);
							}
						}
						yield return new WaitForSeconds(0.2f);
						yield return GameObject.Find("SingleCardGainUI").GetComponent<GBC.SingleCardGainUI>().GainCard(brittle[UnityEngine.Random.RandomRangeInt(0, brittle.Count)]);
					}
					else
					{
						yield return Singleton<GBC.TextBox>.Instance.ShowUntilInput("What? You still crave more?", GBC.TextBox.Style.Undead, null, __state.screenPosition, 0f, true, false, null, true, Emotion.Neutral);
						yield return Singleton<GBC.TextBox>.Instance.ShowUntilInput("Fine. Take this.", GBC.TextBox.Style.Undead, null, __state.screenPosition, 0f, true, false, null, true, Emotion.Neutral);
						List<CardInfo> brittle = new List<CardInfo>();
						for (int i = 0; i < CardLoader.allData.Count; i++)
						{
							if (CardLoader.allData[i].HasAbility(Ability.Brittle) && CardLoader.allData[i].HasCardMetaCategory(CardMetaCategory.GBCPlayable))
							{
								brittle.Add(CardLoader.allData[i]);
							}
						}
						yield return new WaitForSeconds(0.2f);
						yield return GameObject.Find("SingleCardGainUI").GetComponent<GBC.SingleCardGainUI>().GainCard(brittle[UnityEngine.Random.RandomRangeInt(0, brittle.Count)]);
					}
					SaveManager.SaveToFile();
					yield break;
				}
				else if (__state.gameObject.name.Contains("VesselEvent") && !SaveData.doneAreaSecret)
				{
					yield return new WaitForSeconds(0.3f);
					if (GameObject.Find("UI").transform.Find("BoonChoiceUI").transform.Find("MainPanel").gameObject.activeSelf) { yield break; }
					SaveData.doneAreaSecret = true;
					List<string> vessels = new List<string> { "EmptyVessel", "EmptyVessel_BlueGem", "EmptyVessel_GreenGem", "EmptyVessel_OrangeGem" };
					foreach (CardInfo card in CardLoader.allData)
					{
						if (card.name.ToLower().Contains("vessel") && card.HasCardMetaCategory(CardMetaCategory.GBCPlayable))
						{
							vessels.Add(card.name);
						}
					}
					yield return GameObject.Find("SingleCardGainUI").GetComponent<GBC.SingleCardGainUI>().GainCard(CardLoader.GetCardByName(vessels[UnityEngine.Random.RandomRangeInt(0, vessels.Count)]));
					SaveManager.SaveToFile();
					yield break;
				}
				else if (__state.gameObject.name.Contains("MoxEvent") && !SaveData.doneAreaSecret)
				{
					SaveData.doneAreaSecret = true;
					yield return new WaitForSeconds(0.2f);
					List<string> moxen = new List<string>();
					Ability moxability = Ability.GainGemGreen;
					switch (GameObject.Find("PillarMox").GetComponent<SpriteRenderer>().sprite.texture.name)
					{
						case "mox_emerald.png":
							moxability = Ability.GainGemGreen;
							break;
						case "mox_ruby.png":
							moxability = Ability.GainGemOrange;
							break;
						case "mox_sapphire.png":
							moxability = Ability.GainGemBlue;
							break;
					}
					foreach (CardInfo card in CardLoader.allData)
					{
						if (card.HasAbility(moxability) && card.HasCardMetaCategory(CardMetaCategory.GBCPlayable))
						{
							moxen.Add(card.name);
						}
					}
					yield return GameObject.Find("SingleCardGainUI").GetComponent<GBC.SingleCardGainUI>().GainCard(CardLoader.GetCardByName(moxen.GetRandomItem()));
					SaveManager.SaveToFile();
					yield break;
				}
				bool skip = false;
				if (__state.isStarterDeck)
				{
					using (List<string>.Enumerator enumerator = GBC.StarterDecks.GetDeck(__state.starterDeckType).GetEnumerator())
					{
						if (!skip)
						{
							while (enumerator.MoveNext())
							{
								string name = enumerator.Current;
								SaveManager.SaveFile.CollectGBCCard(CardLoader.GetCardByName(name));
							}
						}
						skip = true;
					}
					if (SaveData.isChallengeActive("nuzlocke"))
                    {
						skip = false;
						using (List<string>.Enumerator enumerator = GBC.StarterDecks.GetDeck(__state.starterDeckType).GetEnumerator())
						{
							if (!skip)
							{
								while (enumerator.MoveNext())
								{
									string name = enumerator.Current;
									SaveManager.SaveFile.CollectGBCCard(CardLoader.GetCardByName(name));
								}
							}
							skip = true;
						}
					}
				}
				if (!skip)
				{
					foreach (CardInfo card in __state.cards)
					{
						SaveManager.SaveFile.CollectGBCCard(card);
					}
				}

				yield return Singleton<GBC.TextBox>.Instance.ShowUntilInput("Picked up your new deck.", __state.style, null, __state.screenPosition, 0f, true, false, null, true, Emotion.Neutral);
				if (__state.storyEventOnPickup)
				{
					StoryEventsData.SetEventCompleted(__state.onPickupStoryEvent, false, true);
				}

				if (SaveData.roomId == "DeckChooseRoom")
				{
					StoryEventsData.SetEventCompleted(StoryEvent.GBCIntroCompleted, true, true);
					GameObject Tablet2 = GameObject.Find("Tablet2s");
					for (int i = 0; i < Tablet2.transform.childCount; i++)
					{
						Tablet2.transform.GetChild(i).Find("DeckPileVolume").gameObject.SetActive(false);
					}
					SaveManager.SaveToFile(true);
					GameObject.Find("IntroSequence").transform.Find("BlockingTextBox").gameObject.SetActive(false);
					GameObject.Find("DeckChooseRoom").transform.Find("Overlay").gameObject.SetActive(false);
					Singleton<GBC.PlayerMovementController>.Instance.SetEnabled(false);
					Singleton<GBC.MultiDirectionalCharacterAnimator>.Instance.SetDirection(LookDirection.South);
					Tween.Position(GameObject.Find("FreeMovePlayer").transform, new Vector3(0.085f, 10f, 0), 1.5f, 0.5f);
					__state.gameObject.SetActive(true);
					__state.gameObject.transform.position = new Vector3(0, -550, 0);
					__state.StartCoroutine(functionsnstuff.TransitionToGame());
					yield break;
				}
				else
				{
					GameObject Tablets = GameObject.Find("Tablets");
					for (int i = 0; i < Tablets.transform.childCount; i++)
					{
						Tablets.transform.GetChild(i).Find("DeckPileVolume").gameObject.SetActive(false);
					}
					SaveManager.SaveToFile(true);
					GameObject.Find("IntroSequence").transform.Find("BlockingTextBox").gameObject.SetActive(false);
					Singleton<GBC.PlayerMovementController>.Instance.SetEnabled(true);
					StoryEventsData.SetEventCompleted(StoryEvent.GBCIntroCompleted, true, true);
					SaveManager.SaveToFile(true);
				}
				yield break;
			}
		}

		public static bool dupeBoon(string boonName)
        {
			List<string> allBoons = new List<string>();
			int boonCont = infact2.Plugin.BoonCount;
			foreach(CardInfo card in SaveManager.saveFile.gbcData.collection.Cards)
            {
				if (card.HasCardMetaCategory(infact2.Plugin.BoonsPool) && allBoons.IndexOf(card.name) < 0)
                {
					allBoons.Add(card.name);

				}
            }
			if (allBoons.Count >= boonCont - 1) { return false; } 
			if (allBoons.IndexOf(boonName) > -1) { return true; }
			return false;
        }

		[HarmonyPatch(typeof(GBC.ShopUI), "ShowCollectedThenRefresh")]
		public class DoNotRefreshTheBoon
		{
			public static void Prefix(ref GBC.ShopUI __instance, ref GBC.ShopUI __state)
			{
				__state = __instance;
			}

			public static IEnumerator Postfix(IEnumerator emuemuear, GBC.ShopUI __state, GBC.PixelSelectableCard card)
			{
				yield return Singleton<GBC.TextBox>.Instance.ShowUntilInput("The card was added to your collection.", (GBC.TextBox.Style)card.Info.temple, null, (__state.cards.IndexOf(card) > 1) ? GBC.TextBox.ScreenPosition.ForceTop : GBC.TextBox.ScreenPosition.ForceBottom, 0f, true, false, null, true, Emotion.Neutral);
				if (__state.Active && !card.Info.HasCardMetaCategory(CustomCategory.BoonsPool))
				{
					PauseMenu.pausingDisabled = true;
					__state.shopNPC.RefreshShopInventory();
					card.Anim.StrongNegationEffect();
				}
				yield break;
			}
		}

            [HarmonyPatch(typeof(GBC.GBCEncounterManager), "EncounterSequence")]//Bbattle fix is here too :)
		public class BattleFix
		{
			public static void Prefix(ref GBC.GBCEncounterManager __instance, ref GBC.GBCEncounterManager __state)
			{
				__state = __instance;
			}

			public static IEnumerator Postfix(IEnumerator emuemuear, GBC.GBCEncounterManager __state, EncounterData data, string specialBattleId, GBC.CardBattleNPC triggeringNPC)
			{
				bool boss = false;
				bool elite = false;
				if (specialBattleId == "INFBATTLEBOSS")
				{
					boss = true;
				} else if (specialBattleId == "INFBATTLEDOUBLE")
                {
					elite = true;

                }
				bool isdungeon = false;
				SaveManager.saveFile.gbcData.npcAttempts++;
				__state.Save();
				__state.EncounterOccurring = true;
				Singleton<GBC.PlayerMovementController>.Instance.SetEnabled(false);
				PauseMenu.pausingDisabled = true;
				Singleton<InteractionCursor>.Instance.SetHidden(true);
				GBC.CameraEffects.Transition transitionType;
				Sprite npcCardSprite = Tools.getSprite("battle transition.png");
				Vector3 CardTransitionPos = new Vector3(-0.589f, 9.4688f, 0f);
				if (specialBattleId != "INFBATTLE" && specialBattleId != "INFBATTLEBOSS" && specialBattleId != "INFBATTLEDOUBLE")
				{
					AudioController.Instance.SetLoopAndPlay(triggeringNPC.BattleMusicId, 0, true, true);
					AudioController.Instance.SetLoopVolumeImmediate(triggeringNPC.BattleMusicVolume, 0);
					transitionType = triggeringNPC.ToBattleTransition;
					npcCardSprite = triggeringNPC.CardTransitionSprite;
					if (transitionType == GBC.CameraEffects.Transition.NPCCard)
					{
						CardTransitionPos = triggeringNPC.CardTransitionPos;
					}
				}
				else
				{
					isdungeon = true;
					transitionType = GBC.CameraEffects.Transition.NPCCard;
					npcCardSprite = Tools.getSprite("battle transition.png");
					if (boss)
					{
						npcCardSprite = Tools.getSprite("bossbattle transition.png");
					} else if (elite)
                    {
						npcCardSprite = Tools.getSprite("doublebattle transition.png");
					}
					CardTransitionPos = new Vector3(-0.589f, 9.4688f, 0f);
				}
				switch (transitionType)
				{
					case GBC.CameraEffects.Transition.NPCCard:
						yield return Singleton<GBC.CameraEffects>.Instance.NPCTransitionOut(npcCardSprite, CardTransitionPos);
						break;
					case GBC.CameraEffects.Transition.Quill:
						yield return Singleton<GBC.CameraEffects>.Instance.QuillTransitionOut();
						break;
					case GBC.CameraEffects.Transition.Paintbrush:
						yield return Singleton<GBC.CameraEffects>.Instance.BrushTransitionOut();
						break;
					case GBC.CameraEffects.Transition.Camera:
						yield return Singleton<GBC.CameraEffects>.Instance.CameraTransitionOut();
						break;
					case GBC.CameraEffects.Transition.Scanner:
						yield return Singleton<GBC.CameraEffects>.Instance.ScannerTransitionOut();
						break;
				}
				string npcId = "";
				CardTemple bossTemple = CardTemple.Nature;
				bool isBossBattle = false;
			
				GBC.PixelBoardSpriteSetter.BoardTheme theme = GBC.PixelBoardSpriteSetter.BoardTheme.Nature;
				if (isdungeon)
				{
					switch (functionsnstuff.getTemple(SaveData.roomId))
					{
						case CardTemple.Nature:
							theme = GBC.PixelBoardSpriteSetter.BoardTheme.Nature;
							break;
						case CardTemple.Undead:
							theme = GBC.PixelBoardSpriteSetter.BoardTheme.Undead;
							break;
						case CardTemple.Wizard:
							theme = GBC.PixelBoardSpriteSetter.BoardTheme.Wizard;
							break;
						case CardTemple.Tech:
							theme = GBC.PixelBoardSpriteSetter.BoardTheme.Tech;
							break;
					}
				}
				GBC.DialogueSpeaker.Params speakerParams = null;
				if (triggeringNPC != null)
				{
					npcId = triggeringNPC.ID;
					bossTemple = triggeringNPC.BossTemple;
					isBossBattle = triggeringNPC.IsBoss;
					theme = triggeringNPC.BattleBackgroundTheme;
					if (triggeringNPC.DialogueSpeaker != null)
					{
						speakerParams = triggeringNPC.DialogueSpeaker.GetParams();
					}
				}
				__state.LoadBattleScene();
				yield return new WaitUntil(() => Singleton<TurnManager>.Instance != null);
				if (SaveData.isChallengeActive("nohammer"))
				{
					GameObject.Find("PixelHammerButton").SetActive(false);
				}
				var songName = "";
				if (isdungeon)
				{
					switch (SaveData.roomId)
					{
						case "NatureDungeon":
							songName = "gbc_battle_nature";
							break;
						case "UndeadDungeon":
							songName = "gbc_battle_undead";
							break;
						case "TechDungeon":
							songName = "gbc_battle_tech";
							break;
						case "WizardDungeon":
							songName = "gbc_battle_wizard";
							break;
					}
					if (boss)
                    {
						switch (SaveData.roomId)
						{
							case "NatureDungeon":
								songName = "gbc_battle_leshy";
								break;
							case "UndeadDungeon":
								songName = "gbc_battle_grimora";
								break;
							case "TechDungeon":
								songName = "gbc_battle_p03";
								break;
							case "WizardDungeon":
								songName = "gbc_battle_magnificus";
								break;
						}
					}
					AudioController.Instance.SetLoopAndPlay(songName, 0, true, true);
					AudioController.Instance.SetLoopVolumeImmediate(0.8f, 0);
				}
				PauseMenu.pausingDisabled = false;
				Singleton<InteractionCursor>.Instance.SetHidden(true);
				Singleton<TurnManager>.Instance.StartGame(data, specialBattleId);
				Singleton<GBC.PixelBoardSpriteSetter>.Instance.SetSpritesForTheme(theme);
				if (speakerParams != null)
				{
					(Singleton<TurnManager>.Instance.Opponent as GBC.PixelOpponent).InitializeDialogueSpeaker(speakerParams);
				}
				switch (transitionType)
				{
					case GBC.CameraEffects.Transition.NPCCard:
						yield return Singleton<GBC.CameraEffects>.Instance.NPCTransitionIn(npcCardSprite);
						break;
					case GBC.CameraEffects.Transition.Quill:
						yield return Singleton<GBC.CameraEffects>.Instance.QuillTransitionIn();
						break;
					case GBC.CameraEffects.Transition.Paintbrush:
						yield return Singleton<GBC.CameraEffects>.Instance.BrushTransitionIn();
						break;
					case GBC.CameraEffects.Transition.Camera:
						yield return Singleton<GBC.CameraEffects>.Instance.CameraTransitionIn();
						break;
					case GBC.CameraEffects.Transition.Scanner:
						yield return Singleton<GBC.CameraEffects>.Instance.ScannerTransitionIn();
						break;
				}
				yield return new WaitForSeconds(0.4f);
				Singleton<InteractionCursor>.Instance.SetHidden(false);

				GameObject boonCard = GameObject.Instantiate(Singleton<GBC.PixelPlayerHand>.Instance.gameObject.transform.GetChild(0).gameObject);
				boonCard.name = "BoonCard";
				boonCard.transform.parent = Singleton<GBC.PixelBoardManager>.Instance.gameObject.transform;
				boonCard.transform.localPosition = new Vector3(-1.445f, 10, 0);
				boonCard.SetActive(true);

				List<string> boons = functionsnstuff.returnBoonsAsList();


				if (boons.Contains("infact2_boon_ouroboros"))
                {
					if (SaveData.roomId == "NatureDungeon")
					{
						yield return Singleton<GBC.PixelResourcesManager>.Instance.AddBones(2);
					}
					else if (SaveData.roomId == "UndeadDungeon")
					{
						yield return Singleton<GBC.PixelResourcesManager>.Instance.AddMaxEnergy(1);
						yield return Singleton<GBC.PixelResourcesManager>.Instance.AddEnergy(1);
					}
				}

				if (boons.Contains("infact2_boon_bone"))
				{
					yield return Singleton<GBC.PixelResourcesManager>.Instance.AddBones(3);
					yield return functionsnstuff.PlayBoonAnim("infact2_boon_bone");
				} else if (boons.Contains("infact2_boon_goat"))
                {
					yield return functionsnstuff.PlayBoonAnim("infact2_boon_goat");
				}
				else if (boons.Contains("infact2_boon_forest"))
				{
					yield return functionsnstuff.PlayBoonAnim("infact2_boon_forest");
				}
				else if (boons.Contains("infact2_boon_clover"))
				{
					yield return functionsnstuff.PlayBoonAnim("infact2_boon_clover");
				}
				else if (boons.Contains("infact2_boon_ouroboros"))
				{
					yield return functionsnstuff.PlayBoonAnim("infact2_boon_ouroboros");
				}

				infact2.BoonAbilities.SavedGrace = false;
				infact2.BoonAbilities.deadcards = new List<CardInfo>();
				yield return new WaitUntil(() => Singleton<TurnManager>.Instance == null || Singleton<TurnManager>.Instance.GameEnded);
				bool playerDefeated = false;
				if (Singleton<TurnManager>.Instance != null)
				{
					playerDefeated = !Singleton<TurnManager>.Instance.PlayerWon;
					Singleton<InteractionCursor>.Instance.SetHidden(true);
					PauseMenu.pausingDisabled = true;
					Singleton<GBC.CameraEffects>.Instance.FadeOut();
					AudioController.Instance.FadeOutLoop(1f, Array.Empty<int>());
					yield return new WaitForSeconds(1f);
					if (!playerDefeated && isBossBattle)
					{
						yield return __state.ShowBossDefeatedSequence(bossTemple);
					}
					__state.LoadOverworldScene();
				}
				yield return new WaitUntil(() => Singleton<GBC.PlayerMovementController>.Instance != null);
				Singleton<InteractionCursor>.Instance.SetHidden(true);
				Singleton<GBC.PlayerMovementController>.Instance.SetEnabled(false);
				GBC.CardBattleNPC reloadedNPC = null;
				if (!string.IsNullOrEmpty(npcId))
				{
					yield return new WaitForEndOfFrame();
					reloadedNPC = new List<GBC.CardBattleNPC>(UnityEngine.Object.FindObjectsOfType<GBC.CardBattleNPC>()).Find((GBC.CardBattleNPC x) => x.ID == npcId);
					if (reloadedNPC != null)
					{
						reloadedNPC.SetCollisionEnabled(false);
						yield return new WaitForSeconds(0.4f);
						Singleton<InteractionCursor>.Instance.SetHidden(false);
						yield return reloadedNPC.PostCombatEncounterSequence(playerDefeated);
					}
				}
				else
				{
					yield return new WaitForSeconds(0.4f);
					Singleton<InteractionCursor>.Instance.SetHidden(false);
				}
				if (!playerDefeated && isBossBattle)
				{
					if (reloadedNPC != null)
					{
						reloadedNPC.SetCollisionEnabled(false);
					}
					yield return __state.GetComponent<GBC.GBCSpecialEventSequencer>().OnBossDefeated(bossTemple);
					if (reloadedNPC != null)
					{
						reloadedNPC.SetCollisionEnabled(true);
					}
				}
				if (playerDefeated && isdungeon)
				{
					SaveData.bountyStars = 0;
					Singleton<GBC.PlayerMovementController>.Instance.gameObject.transform.Find("BountyStars").gameObject.SetActive(false);
					SaveData.lives -= 1;
					GameObject.Find("FloorText").GetComponent<UnityEngine.UI.Text>().text = "Floor " + SaveData.floor + "\nLives: " + SaveData.lives;
					if (elite && !SaveData.isChallengeActive("elite"))
                    {
						for (int i = 0; i < GameObject.Find(SaveData.roomId).transform.childCount; i++)
						{
							if (GameObject.Find(SaveData.roomId).transform.GetChild(i).name.Contains("EliteBattleEvent"))
							{
								GameObject.Find(SaveData.roomId).transform.GetChild(i).gameObject.SetActive(false);

							}
						}
					} else if (SaveData.lives > 0)
                    {
						SaveData.nodesCompleted += 1;
						functionsnstuff.PlaceBarriers(true);
					}
					if (SaveData.lives < 1 || boss)
					{
						__state.EncounterOccurring = false;
						__state.Save();
						PauseMenu.pausingDisabled = false;
						if (SaveData.floor > SaveData.highscore || SaveData.highscore == 0)
						{
							SaveData.highscore = SaveData.floor;
						}
						SaveData.floor = 0;
						SaveData.lives = 2;
						SaveData.nodesCompleted = 0;
						GameObject Tablet2 = GameObject.Find("Tablet2s");
						for (int i = 0; i < Tablet2.transform.childCount; i++)
						{
							Tablet2.transform.GetChild(i).Find("Glow").gameObject.SetActive(false);
							Tablet2.transform.GetChild(i).Find("DeckPileVolume").gameObject.SetActive(true);
							yield return new WaitForSeconds(0.25f);
						}
						for (int i = 3; i < GameObject.Find(SaveData.roomId).transform.childCount; i++)
						{
							if (GameObject.Find(SaveData.roomId).transform.GetChild(i).gameObject.name.ToLower().Contains("event") || GameObject.Find(SaveData.roomId).transform.GetChild(i).gameObject.name == "Barriers")
							{
								GameObject.Destroy(GameObject.Find(SaveData.roomId).transform.GetChild(i).gameObject);
							}
						}
						Singleton<GBC.CameraEffects>.Instance.FadeIn();
						GameObject.Find("ScreenFade").GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1);
						Singleton<GBC.CameraController>.Instance.SetRoom(GameObject.Find("Room").transform.position, GameObject.Find("Room").name);
						Singleton<GBC.PlayerMovementController>.Instance.transform.position = GameObject.Find("BackToIsleVolume").transform.Find("PlayerPositionMarker").position;
						Singleton<GBC.PlayerMovementController>.Instance.SetEnabled(true);
						AudioController.Instance.SetLoopAndPlay("gbc_starting_island", 0, true, true);
						AudioController.Instance.SetLoopVolumeImmediate(0.8f, 0);
						GameObject.Find("CameraText").gameObject.SetActive(true);
						__state.Save();
						yield break;
					}
				}
				PauseMenu.pausingDisabled = false;
				Singleton<GBC.PlayerMovementController>.Instance.SetEnabled(true);
				__state.EncounterOccurring = false;
				__state.Save();
				if (isdungeon)
				{
					if (!playerDefeated && SaveData.isChallengeActive("bounty"))
                    {
						SaveData.bountyStars += 0.35f;
						if (SaveData.bountyStars > 3)
						{
							SaveData.bountyStars = 3;
						}
						if (SaveData.bountyStars >= 1)
						{
							Singleton<GBC.PlayerMovementController>.Instance.gameObject.transform.Find("BountyStars").gameObject.SetActive(true);
							Singleton<GBC.PlayerMovementController>.Instance.gameObject.transform.Find("BountyStars").gameObject.GetComponent<SpriteRenderer>().sprite = Tools.getSprite("star_" + Math.Floor(SaveData.bountyStars).ToString() + ".png");
						}
					
					}
					if (!boss && !elite && !playerDefeated)
					{
						SaveData.nodesCompleted += 1;
						functionsnstuff.PlaceBarriers(true);
						
					}
					else if (!playerDefeated && boss)
					{
						SaveData.nodesCompleted = 0;
						SaveData.lives = 2;
						SaveData.floor += 1;
						string bountyHunters = "0;";
						string[] bountyData = SaveData.bountyHunters.Split(';');
						for (int i = 1; i < bountyData.Length; i++)
						{
							string[] hunterData2 = bountyData[i].Split(',');
							for (int j = 0; j < hunterData2.Length; j++)
							{
								
								if (j > 0)
								{
									bountyHunters += "," + hunterData2[j];
								}
								else
								{
									bountyHunters += hunterData2[j];
								}
							}
							if (i < bountyData.Length - 1)
							{
								bountyHunters += ";";
							}
						}
						SaveData.bountyHunters = bountyHunters;
						List<string> rooms = new List<string> { "NatureDungeon", "UndeadDungeon", "TechDungeon", "WizardDungeon" };
						rooms.Remove(SaveData.roomId);
						string dungeon = rooms[UnityEngine.Random.RandomRangeInt(0, rooms.Count)];
						Singleton<GBC.CameraEffects>.Instance.FadeIn();
						GameObject.Find("ScreenFade").GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1);
						Singleton<GBC.CameraController>.Instance.SetRoom(GameObject.Find(dungeon).transform.position, GameObject.Find(dungeon).name);
						Singleton<GBC.PlayerMovementController>.Instance.transform.position = GameObject.Find(dungeon).transform.Find("PlayerPositionMarker").position;
						Singleton<GBC.PlayerMovementController>.Instance.SetEnabled(true);
						GameObject.Find("PixelCamera").transform.Find("CameraText").gameObject.SetActive(true);
						GameObject.Find("FloorText").GetComponent<UnityEngine.UI.Text>().text = "Floor " + SaveData.floor + "\nLives: " + SaveData.lives;
						functionsnstuff.PlayCorrectMusic();
						CustomCoroutine.WaitThenExecute(2f, delegate
						{
							AudioController.Instance.PlaySound2D("player_falling", MixerGroup.None, 0.3f, 0f, null, null, null, null, false);
						}, false);
						SaveData.doneAreaSecret = false;
						functionsnstuff.GenerateNodes();
						functionsnstuff.PlaceBarriers();
						SaveManager.SaveToFile(true);
						yield return Singleton<GBC.PlayerMovementController>.Instance.Anim.DropAnimation(5f);
					} else if (elite && !playerDefeated || elite && SaveData.isChallengeActive("elite"))
                    {
						SaveData.nodesCompleted += 1;
						functionsnstuff.PlaceBarriers(true);
					}
				}
				yield break;
			}
		}
	}
}
