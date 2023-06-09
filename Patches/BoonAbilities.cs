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
using functionsnstuff = infact2.Plugin.functionsnstuff;

namespace infact2
{
	public class BoonAbilities
	{
		public static bool SavedGrace = false;
		public static List<CardInfo> deadcards = new List<CardInfo>();


		//BOON OF FLESH

		[HarmonyPatch(typeof(BoardManager), "AvailableSacrificeValue", MethodType.Getter)]
		public class fixSacrificesHeHe
		{
			public static bool Prefix(ref BoardManager __instance, ref int __result)
			{
				List<string> boons = functionsnstuff.returnBoonsAsList();
				if (!boons.Contains("infact2_boon_flesh") && !boons.Contains("infact2_boon_bloodstone"))
				{
					return true;
				}
				__result = __instance.GetValueOfSacrifices(__instance.playerSlots.FindAll((CardSlot x) => x.Card != null && x.Card.CanBeSacrificed && x.Card.Info.HasTrait(Trait.Gem) && boons.Contains("infact2_boon_flesh") || x.Card != null && x.Card.CanBeSacrificed && x.Card.Info.HasTrait(Trait.Terrain) && boons.Contains("infact2_boon_bloodstone") || x.Card != null && x.Card.CanBeSacrificed));
				return false;
			}
		}

		[HarmonyPatch(typeof(PlayableCard), "CanBeSacrificed", MethodType.Getter)]
		public class fixSacrifice
		{
			public static bool Prefix(ref PlayableCard __instance, ref bool __result)
			{
				List<string> boons = functionsnstuff.returnBoonsAsList();
				if (!boons.Contains("infact2_boon_flesh") && !boons.Contains("infact2_boon_bloodstone"))
				{
					return true;
				}
				__result = __instance.Info.HasTrait(Trait.Gem) && boons.Contains("infact2_boon_flesh") || __instance.Info.HasTrait(Trait.Terrain) && boons.Contains("infact2_boon_bloodstone") && !__instance.Info.HasTrait(Trait.Gem) || !__instance.FaceDown && (__instance.Info.Sacrificable || __instance.HasAbility(Ability.TripleBlood));
				return false;
			}
		}


		//SAVING GRACE

		[HarmonyPatch(typeof(GBC.PixelScales), "AddDamage")]
		public class savingGraceBoon
		{
			public static void Prefix(ref GBC.PixelScales __instance, out GBC.PixelScales __state)
			{
				__state = __instance;
			}

			public static IEnumerator Postfix(IEnumerator neaio, GBC.PixelScales __state, int damage, int numWeights, bool toPlayer, GameObject alternatePrefab)
			{
				List<string> boons = functionsnstuff.returnBoonsAsList();
				int dmg = damage;
				if (toPlayer && Singleton<GBC.PixelLifeManager>.Instance.Balance - damage < -4 && boons.Contains("infact2_boon_savinggrace") && !SavedGrace)
				{
					dmg = 0;
				}
				for (int i = 0; i < dmg; i++)
				{
					if (toPlayer)
					{
						__state.playerWeight++;
					}
					else
					{
						__state.opponentWeight++;
					}
					AudioController.Instance.PlaySound2D("crunch_short#1", MixerGroup.None, 0.35f, 0f, new AudioParams.Pitch(1f + (float)Singleton<LifeManager>.Instance.Balance * 0.05f), null, null, null, false);
					__state.UpdateWeightSprites();
					__state.UpdateScaleRotation();
					yield return new WaitForSeconds(0.25f);
				}
				yield break;

			}
		}

		[HarmonyPatch(typeof(TurnManager), "LifeLossConditionsMet")]
		public class dontDIe
		{
			public static bool Prefix(ref TurnManager __instance, ref bool __result)
			{
				List<string> boons = functionsnstuff.returnBoonsAsList();
				if (boons.Contains("infact2_boon_savinggrace") && !SavedGrace && Singleton<GBC.PixelLifeManager>.Instance.Balance < -4)
				{
					SavedGrace = true;
					Singleton<GBC.PixelScales>.Instance.playerWeight = 4;
					Singleton<GBC.PixelScales>.Instance.opponentWeight = 0;
					Singleton<GBC.PixelLifeManager>.Instance.PlayerDamage = 4;
					Singleton<GBC.PixelLifeManager>.Instance.OpponentDamage = 0;
					Singleton<GBC.PixelScales>.Instance.UpdateScaleRotation();
					Singleton<GBC.PixelScales>.Instance.UpdateWeightSprites();
					AudioController.Instance.PlaySound2D("crunch_short#1", MixerGroup.None, 0.35f, 0f, new AudioParams.Pitch(1f + (float)Singleton<LifeManager>.Instance.Balance * 0.05f), null, null, null, false);
					__instance.StartCoroutine(functionsnstuff.PlayBoonAnim("infact2_boon_savinggrace"));
					__result = false;
					return false;
				}
				return true;
			}
		}

		//SCRAP

		[HarmonyPatch(typeof(GBC.PixelResourcesManager), "ShowAddEnergy")]
		public class giveBoneOnMax
		{
			public static void Prefix(ref GBC.PixelResourcesManager __instance, int amount)
			{
				List<string> boons = functionsnstuff.returnBoonsAsList();
				if (__instance.PlayerMaxEnergy >= 6 && boons.Contains("infact2_boon_scrap"))
				{
					__instance.StartCoroutine(__instance.AddBones(1));
					__instance.StartCoroutine(functionsnstuff.PlayBoonAnim("infact2_boon_scrap"));
				}
			}
		}

		//NECROMANCY

		[HarmonyPatch(typeof(Brittle), "OnAttackEnded")]
		public class dropMoxOnBrittle
		{
			public static void Prefix(ref Brittle __instance, out Brittle __state)
			{
				__state = __instance;
			}

			public static IEnumerator Postfix(IEnumerator neaio, Brittle __state)
			{
				yield return __state.PreSuccessfulTriggerSequence();
				if (__state.Card != null && !__state.Card.Dead)
				{
					if (!SaveManager.SaveFile.IsPart2)
					{
						Singleton<ViewManager>.Instance.SwitchToView(View.Board, false, false);
						yield return new WaitForSeconds(0.1f);
					}
					yield return __state.Card.Die(false, null, true);
					yield return __state.LearnAbility(0.25f);
					if (!SaveManager.SaveFile.IsPart2)
					{
						yield return new WaitForSeconds(0.1f);
					}
					List<string> boons = functionsnstuff.returnBoonsAsList();
					if (boons.Contains("infact2_boon_necromancy") && __state.Card.name == "Skeleton")
                    {
						List<string> moxen = new List<string> { "MoxRuby", "MoxEmerald", "MoxSapphire" };
						yield return new WaitForSeconds(0.1f);
						yield return Singleton<BoardManager>.Instance.CreateCardInSlot(CardLoader.GetCardByName(moxen[UnityEngine.Random.RandomRangeInt(0, moxen.Count)]), __state.Card.Slot, 0.1f, true);
						yield return new WaitForSeconds(0.2f);
						yield return functionsnstuff.PlayBoonAnim("infact2_boon_necromancy");
					}
				}
				yield break;
			}
		}

		//LUCK

		[HarmonyPatch(typeof(GBC.GBCCloverButton), "Start")]
		public class showClover
		{
			public static bool Prefix(ref GBC.GBCCloverButton __instance)
			{
				GBC.GenericUIButton genericUIButton = __instance.button;
				var instance = __instance;
				genericUIButton.OnButtonDown = (Action<GBC.GenericUIButton>)Delegate.Combine(genericUIButton.OnButtonDown, new Action<GBC.GenericUIButton>(delegate (GBC.GenericUIButton b)
				{
					instance.OnButtonPressed();
				}));
				List<string> boons = functionsnstuff.returnBoonsAsList();
				if (StoryEventsData.EventCompleted(StoryEvent.GBCCloverFound) && !SaveData.roomId.Contains("Dungeon") || boons.Contains("infact2_boon_clover"))
				{
					__instance.StartCoroutine(__instance.WaitThenEnableThenDisable());
					return false;
				}
				__instance.gameObject.SetActive(false);
				return false;
			}
		}

		//AMBIDEXTROUS 

		[HarmonyPatch(typeof(GBC.PixelCardDrawPiles), "ChooseDraw")]
		public class drawTwice
		{
			public static void Prefix(ref GBC.PixelCardDrawPiles __instance, out GBC.PixelCardDrawPiles __state)
			{
				__state = __instance;
			}
			
			public static IEnumerator Postfix(IEnumerator tea, GBC.PixelCardDrawPiles __state, int numDrawnThisPhase)
			{
				if (__state.Deck.CardsInDeck > 0)
				{
					yield return __state.DrawCardFromDeck(null, null);
					List<string> boons = functionsnstuff.returnBoonsAsList();
					if (boons.Contains("infact2_boon_ambidextrous"))
                    {
						if (__state.Deck.CardsInDeck > 0 && __state.Deck.CardsInDeck < 20)
						{
							yield return __state.DrawCardFromDeck(null, null);
						}
					}
				}
				yield break;
			}
		}

		//FOREST AND OURO

		[HarmonyPatch(typeof(GBC.PixelCardDrawPiles), "DrawOpeningHand")]
		public class add2firs
		{
			public static void Prefix(ref GBC.PixelCardDrawPiles __instance, out GBC.PixelCardDrawPiles __state)
			{
				__state = __instance;
			}

			public static IEnumerator Postfix(IEnumerator tea, GBC.PixelCardDrawPiles __state, List<CardInfo> fixedHand)
			{
				List<CardInfo> hand = null;
				if (fixedHand != null && fixedHand.Count > 0)
				{
					hand = fixedHand;
				}
				else
				{
					hand = __state.Deck.GetFairHand(4, true, null);
				}
				int num;
				for (int i = 0; i < 4; i = num + 1)
				{
					if (i < hand.Count)
					{
						yield return __state.DrawCardFromDeck(hand[i], null);
					}
					else
					{
						yield return __state.DrawCardFromDeck(null, null);
					}
					num = i;
				}
				List<string> boons = functionsnstuff.returnBoonsAsList();
				if (boons.Contains("infact2_boon_forest"))
				{
					yield return Singleton<CardSpawner>.Instance.SpawnCardToHand(CardLoader.GetCardByName("infact2_terrain_fir"));
					yield return Singleton<CardSpawner>.Instance.SpawnCardToHand(CardLoader.GetCardByName("infact2_terrain_fir"));
				}
				if (boons.Contains("infact2_boon_ouroboros"))
                {
					if (SaveData.roomId == "TechDungeon")
                    {
						List<string> moxes = new List<string> { "MoxRuby", "MoxEmerald", "MoxSapphire" };
						yield return Singleton<CardSpawner>.Instance.SpawnCardToHand(CardLoader.GetCardByName(moxes[UnityEngine.Random.RandomRangeInt(0, 2)]));
						yield return Singleton<CardSpawner>.Instance.SpawnCardToHand(CardLoader.GetCardByName(moxes[UnityEngine.Random.RandomRangeInt(0, 2)]));
					} else if (SaveData.roomId == "WizardDungeon")
					{
						yield return Singleton<CardSpawner>.Instance.SpawnCardToHand(CardLoader.GetCardByName("Squirrel"));
						yield return Singleton<CardSpawner>.Instance.SpawnCardToHand(CardLoader.GetCardByName("Squirrel"));
					}
				}
			
                yield break;
			}
		}

		//photographer
		/*
		public class gbcBattleSequencer : SpecialBattleSequencer
        {
			public override IEnumerator OnOtherCardDie(PlayableCard card, CardSlot deathSlot, bool fromCombat, PlayableCard killer)
			{
				List<string> boons = functionsnstuff.returnBoonsAsList();
				if (boons.Contains("infact2_boon_photographer"))
				{
				}
				yield break;
			}
		}
		*/

		//The Nuzlocke Challenge!
		/*
		[HarmonyPatch(typeof(BoardManager), "ResolveCardOnBoard")]
		public class oncardresolve
		{
			public static void Prefix(ref BoardManager __instance, PlayableCard card, CardSlot slot, float tweenLength = 0.1f, Action landOnBoardCallback = null, bool resolveTriggers = true)
			{
				if (SaveData.isChallengeActive("nuzlocke"))
				{
					Debug.Log("hello!");
					List<string> sideDeck = new List<string> { "Squirrel", "Skeleton", "LeapBot", "MoxRuby", "MoxSapphire", "MoxEmerald" };
					if (slot.IsPlayerSlot && sideDeck.IndexOf(card.Info.name) < 0)
                    {
						Debug.Log(card.Info.name);
						card.Info.AddSpecialAbilities(Plugin.Nuzlocke);
                    }
				}
			}
		}*/
	}
}
