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
	class BoonPatches
	{
		[HarmonyPatch(typeof(GBC.CollectionUI), "Start")]
		public class AddBoonTab
		{
			public static void Prefix(ref GBC.CollectionUI __instance)
			{
				GameObject tab5 = GameObject.Instantiate(__instance.gameObject.transform.Find("MainPanel").Find("Tabs").Find("Tab_4").gameObject);
				tab5.name = "Tab_5";
				tab5.transform.parent = __instance.gameObject.transform.Find("MainPanel").Find("Tabs");
				tab5.transform.localPosition = new Vector3(-1.0905f, -0.305f, 0);
				tab5.transform.localRotation = Quaternion.Euler(0, 0, 90);
				__instance.tabButtons.Add(tab5.GetComponent<GBC.GenericUIButton>());
				tab5.GetComponent<GBC.GenericUIButton>().inputKey = KeyCode.Alpha5;
				tab5.GetComponent<GBC.GenericUIButton>().OnButtonDown = __instance.gameObject.transform.Find("MainPanel").Find("Tabs").Find("Tab_4").gameObject.GetComponent<GBC.GenericUIButton>().OnButtonDown;
				tab5.GetComponent<BoxCollider2D>().size = new Vector2(0.55f, 0.44f);
				tab5.gameObject.transform.Find("Icon").gameObject.GetComponent<SpriteRenderer>().sprite = Tools.getSprite("boontab.png");
			}
		}

		[HarmonyPatch(typeof(GBC.DeckBuildingUI), "Start")]
		public class AddBoonPanel
		{
			public static void Prefix(ref GBC.DeckBuildingUI __instance)
			{
				GameObject boonTab = GameObject.Instantiate(__instance.gameObject.transform.Find("DeckPanel").gameObject);
				boonTab.name = "BoonPanel";
				boonTab.transform.parent = __instance.gameObject.transform;
				boonTab.transform.localPosition = new Vector3(1.48f, -0.055f, 0);
				var instance = __instance;
				for (int i = 0; i < boonTab.transform.childCount; i++)
				{
					if (boonTab.transform.GetChild(i).gameObject.name.Contains("CardNamePanel"))
					{
						int j = i;
						boonTab.transform.GetChild(i).gameObject.SetActive(false);
						boonTab.transform.GetChild(i).gameObject.GetComponent<GBC.CardNamePanel>().OnButtonDown = (Action<GBC.GenericUIButton>)Delegate.Combine(boonTab.transform.GetChild(i).gameObject.GetComponent<GBC.CardNamePanel>().OnButtonDown, new Action<MainInputInteractable>(delegate (MainInputInteractable i)
						{
							instance.OnNamePanelPressed(boonTab.transform.GetChild(j).gameObject.GetComponent<GBC.CardNamePanel>());
						}));
					}
				}
				boonTab.transform.Find("ClearDeckButton").gameObject.GetComponent<GBC.GenericUIButton>().CursorSelectEnded = (Action<MainInputInteractable>)Delegate.Combine(boonTab.transform.Find("ClearDeckButton").gameObject.GetComponent<GBC.GenericUIButton>().CursorSelectEnded, new Action<MainInputInteractable>(delegate (MainInputInteractable i)
				{
					instance.OnClearDeckPressed();
				}));
				boonTab.transform.Find("ClearDeckButton").gameObject.SetActive(true);
				boonTab.transform.Find("AutoCompleteButton").gameObject.SetActive(false);
				boonTab.SetActive(false);
				updateBoonPanel();
			}
		}

            public static void updateBoonPanel()
		{
			GameObject boonTab = Singleton<GBC.DeckBuildingUI>.Instance.gameObject.transform.Find("BoonPanel").gameObject;
			for (int i = 0; i < 3; i++)
			{
				boonTab.transform.GetChild(i).gameObject.SetActive(false);
			}
			if (SaveData.boon1 is null)
			{
				SaveData.boon1 = "None";
				SaveData.boon2 = "None";
				SaveData.boon3 = "None";
			}
			if (SaveData.boon1 != "None" && SaveData.boon1 is not null) {
				boonTab.transform.GetChild(0).gameObject.GetComponent<GBC.CardNamePanel>().ShowCard(CardLoader.GetCardByName(SaveData.boon1), 0);
				boonTab.transform.GetChild(0).gameObject.SetActive(true);
			}
			if (SaveData.boon2 != "None" && SaveData.boon2 is not null)
			{
				boonTab.transform.GetChild(1).gameObject.GetComponent<GBC.CardNamePanel>().ShowCard(CardLoader.GetCardByName(SaveData.boon2), 0);
				boonTab.transform.GetChild(1).gameObject.SetActive(true);
			}
			if (SaveData.boon3 != "None" && SaveData.boon3 is not null)
			{
				boonTab.transform.GetChild(2).gameObject.GetComponent<GBC.CardNamePanel>().ShowCard(CardLoader.GetCardByName(SaveData.boon3), 0);
				boonTab.transform.GetChild(2).gameObject.SetActive(true);
			}
		}

		[HarmonyPatch(typeof(GBC.DeckBuildingUI), "OnClearDeckPressed")]
		public class RemoveBoons
		{
			public static bool Prefix(ref GBC.DeckBuildingUI __instance)
			{
				if (__instance.gameObject.transform.Find("BoonPanel").gameObject.activeSelf)
				{
					SaveData.boon1 = "None";
					SaveData.boon2 = "None";
					SaveData.boon3 = "None";
					SaveManager.SaveToFile();
					__instance.UpdatePanelContents();
					updateBoonPanel();
					return false;
				}
				return true;
			}
		}
		[HarmonyPatch(typeof(GBC.DeckBuildingUI), "OnNamePanelPressed")]
		public class RemoveBoon
		{
			public static bool Prefix(ref GBC.DeckBuildingUI __instance, GBC.CardNamePanel panel)
			{
				if (panel.gameObject.transform.parent.gameObject.name == "BoonPanel")
                {
					int num = 0;
					AudioController.Instance.PlaySound2D("chipBlip2", MixerGroup.None, 0.4f, 0f, new AudioParams.Pitch(Mathf.Min(0.9f + (float)num * 0.05f, 1.2f)), null, null, null, false);
					List<string> SelectedBoons = functionsnstuff.returnBoonsAsList();
					int dex = SelectedBoons.IndexOf(panel.Card.name);
					dex++;
					switch(dex)
                    {
						case 1:
							SaveData.boon1 = "None";
							break;
						case 2:
							SaveData.boon2 = "None";
							break;
						case 3:
							SaveData.boon3 = "None";
							break;
					}
					SaveManager.SaveToFile();
					updateBoonPanel();
					__instance.UpdatePanelContents();
				}
				return true;
			}
		}
	
        [HarmonyPatch(typeof(GBC.DeckBuildingUI), "OnCardPressed")]
		public class AddBoon
		{
			public static bool Prefix(ref GBC.DeckBuildingUI __instance, GBC.PixelSelectableCard card)
			{
				if (card.Info.HasCardMetaCategory(CustomCategory.BoonsPool))
                {
					List<string> SelectedBoons = functionsnstuff.returnBoonsAsList();
					bool duplicate = false;
					if (SelectedBoons.Contains(card.Info.name)) { duplicate = true; }
					int max = SaveData.isChallengeActive("Boon") ? 1 : 3;
					if (!duplicate && SelectedBoons.Count < max)
                    {
						AudioController.Instance.PlaySound2D("crushBlip2", MixerGroup.None, 0.4f, 0f, new AudioParams.Pitch(Mathf.Min(0.9f + (float)1f * 0.05f, 1.2f)), null, null, null, false);
						if (SaveData.boon1 == "None")
                        {
							SaveData.boon1 = card.Info.name;
                        } else if (SaveData.boon2 == "None")
						{
							SaveData.boon2 = card.Info.name;
						} else if (SaveData.boon3 == "None")
						{
							SaveData.boon3 = card.Info.name;
						}
						__instance.UpdatePanelContents();
						updateBoonPanel();
						SaveManager.SaveToFile();
						return false;
					}
					AudioController.Instance.PlaySound2D("toneless_negate", MixerGroup.None, 0.2f, 0f, null, new AudioParams.Repetition(0.1f, ""), null, null, false);
					return false;
				}
				return true;
			}
		}
		[HarmonyPatch(typeof(GBC.DeckBuildingUI), "OnAutoCompletePressed")]
		public class MakeAutoCompletNotSelectBoons
		{
			public static bool Prefix(ref GBC.DeckBuildingUI __instance)
			{
				List<CardInfo> list = new List<CardInfo>(SaveManager.saveFile.gbcData.collection.Cards);
				List<CardInfo> list2 = new List<CardInfo>(SaveManager.saveFile.gbcData.deck.Cards);
				list.RemoveAll((CardInfo x) => x.HasCardMetaCategory(CustomCategory.BoonsPool));
				using (List<CardInfo>.Enumerator enumerator = list2.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						CardInfo cardInDeck = enumerator.Current;
						CardInfo cardInfo = list.Find((CardInfo x) => x.name == cardInDeck.name);
						if (cardInfo != null)
						{
							list.Remove(cardInfo);
						}
					}
				}
				list2.ForEach(delegate (CardInfo x)
				{
					SaveManager.saveFile.gbcData.deck.RemoveCard(x);
				});
				List<CardInfo> completedDeck = GBC.AutoDeckBuilder.CompleteDeck(list2, list);
				__instance.StartCoroutine(__instance.AutoCompleteSequence(list2, completedDeck));
				return false;
			}
		}
		

        [HarmonyPatch(typeof(GBC.DeckBuildingUI), "UpdatePanelContents")]
		public class FixBoonCount
		{
			public static bool Prefix(ref GBC.DeckBuildingUI __instance)
			{
				if (Singleton<GBC.DeckBuildingUI>.Instance.gameObject.transform.childCount != 4)
				{
					return true;
				}
				__instance.UpdateDeckPanelEntries(SaveManager.saveFile.gbcData.deck);
				__instance.DisplayDeck(__instance.deckPanelEntries, __instance.currentScroll);
				__instance.cardCountText.SetText(SaveManager.saveFile.gbcData.deck.Cards.Count + "/" + 20, false);
				int max = SaveData.isChallengeActive("Boon") ? 1 : 3;
				__instance.gameObject.transform.Find("BoonPanel").Find("DeckCount").gameObject.GetComponentInChildren<UnityEngine.UI.Text>().text = functionsnstuff.returnBoonsAsList().Count + "/" + max;
				bool isMax = functionsnstuff.returnBoonsAsList().Count >= max;
				__instance.gameObject.transform.Find("BoonPanel").Find("DeckCount").gameObject.GetComponentInChildren<GBC.PixelText>().SetColor(isMax ? GameColors.Instance.darkRed : GameColors.Instance.nearBlack);
				__instance.UpdateScroll();
				__instance.clearDeckButton.gameObject.SetActive(SaveManager.saveFile.gbcData.deck.Cards.Count > 0);
				__instance.autoCompleteButton.SetEnabled(SaveManager.saveFile.gbcData.deck.Cards.Count < 20);
				__instance.collection.RefreshPage();
				return false;
			}
		}

		[HarmonyPatch(typeof(GBC.CollectionUI), "ShowPage")]
		public class ShowBoonPanel
		{
			public static bool Prefix(ref GBC.CollectionUI __instance, int pageIndex)
			{
				if (Singleton<GBC.DeckBuildingUI>.Instance.gameObject.transform.childCount != 4)
                {
					return true;
                }
				List<CardInfo> cards = SaveManager.saveFile.gbcData.collection.Cards;
				List<CardInfo> AllBoons = new List<CardInfo>();
				for (int i = 0; i < cards.Count; i++)
				{
					if (cards[i].HasCardMetaCategory(CustomCategory.BoonsPool))
					{
						AllBoons.Add(cards[i]);
					}
				}
				if (pageIndex >= __instance.tabPageIndices[4])
                {
					Singleton<GBC.DeckBuildingUI>.Instance.gameObject.transform.Find("BoonPanel").gameObject.SetActive(true);
					Singleton<GBC.DeckBuildingUI>.Instance.gameObject.transform.Find("DeckPanel").gameObject.SetActive(false);
				} else
                {
					Singleton<GBC.DeckBuildingUI>.Instance.gameObject.transform.Find("BoonPanel").gameObject.SetActive(false);
					Singleton<GBC.DeckBuildingUI>.Instance.gameObject.transform.Find("DeckPanel").gameObject.SetActive(true);
				}
				return true;
            }
        }

        [HarmonyPatch(typeof(GBC.CardPreviewPanel), "DisplayDescription")]
        public class AddBoonDescription
        {
			public static bool Prefix(ref GBC.CardPreviewPanel __instance, CardInfo cardInfo, List<Ability> abilities)
			{
				if (cardInfo.HasCardMetaCategory(CustomCategory.BoonsPool))
				{
					__instance.descriptionText.SetText("With this boon equipped, " + cardInfo.description);
					return false;
				}
				return true;
			}
		}

        [HarmonyPatch(typeof(GBC.CollectionUI), "CreatePages")]
		public class SortBoons
		{
			public static bool Prefix(ref GBC.CollectionUI __instance, ref List<List<CardInfo>> __result, ref List<CardInfo> cards)
			{
				cards.Sort(delegate (CardInfo a, CardInfo b)
				{
					int num2 = a.temple - b.temple;
					if (num2 != 0)
					{
						return num2;
					}
					int num3 = a.metaCategories.Contains(CardMetaCategory.Rare) ? 1 : 0;
					int num4 = (b.metaCategories.Contains(CardMetaCategory.Rare) ? 1 : 0) - num3;
					if (num4 != 0)
					{
						return num4;
					}
					int num5 = a.CostTier - b.CostTier;
					if (num5 != 0)
					{
						return num5;
					}
					int num6 = a.BonesCost - b.BonesCost;
					if (num6 != 0)
					{
						return num6;
					}
					int num7 = ((a.GemsCost.Count == 1) ? a.GemsCost[0] : GemType.Green) - ((b.GemsCost.Count == 1) ? b.GemsCost[0] : GemType.Green);
					if (num7 != 0)
					{
						return num7;
					}
					int num8 = a.DisplayedNameEnglish.CompareTo(b.DisplayedNameEnglish);
					if (num8 == 0)
					{
						return a.name.CompareTo(b.name);
					}
					return num8;
				});
				cards = new List<CardInfo>(cards);
				List<CardInfo> toRemove = new List<CardInfo>();
				for (int i = 1; i < cards.Count; i++)
				{
					if (cards[i].name == cards[i - 1].name)
					{
						toRemove.Add(cards[i]);
					}
				}
				cards.RemoveAll((CardInfo x) => toRemove.Contains(x));
				List<List<CardInfo>> boons = new List<List<CardInfo>> ();
				List<CardInfo> AllBoons = new List<CardInfo>();
				int pageId = 0;
				for (int i = 0; i < cards.Count; i++)
				{
					int page = pageId / 8;
					if (page >= boons.Count)
                    {
						boons.Add(new List<CardInfo>());
                    }
					if (cards[i].HasCardMetaCategory(CustomCategory.BoonsPool))
					{
						pageId += 1;
						AllBoons.Add(cards[i]);
						boons[page].Add(cards[i]);
					}
				}
				cards.RemoveAll((CardInfo x) => AllBoons.Contains(x));
				List<List<CardInfo>> list = new List<List<CardInfo>>();
				list.Add(new List<CardInfo>());
				__instance.tabPageIndices = new int[5];
				for (int i = 0; i < __instance.tabPageIndices.Length; i++)
				{
					__instance.tabPageIndices[i] = 0;
				}
				for (int j = 0; j < cards.Count; j++)
				{
					List<CardInfo> list2 = list[list.Count - 1];
					if (j == 0)
					{
						int temple = (int)cards[j].temple;
						for (int k = 0; k < temple; k++)
						{
							list.Add(new List<CardInfo>());
							__instance.tabPageIndices[k + 1] = list.IndexOf(list2) + 1;
							list2 = list[list.Count - 1];
						}
					}
					list2.Add(cards[j]);
					bool flag = j == cards.Count - 1;
					if (!flag)
					{
						int temple2 = (int)cards[j].temple;
						int temple3 = (int)cards[j + 1].temple;
						int num = temple3 - temple2 - 1;
						for (int l = 0; l < num; l++)
						{
							list.Add(new List<CardInfo>());
							__instance.tabPageIndices[temple2 + 1 + l] = list.IndexOf(list2) + 1;
							list2 = list[list.Count - 1];
						}
						bool flag2 = !flag && temple2 != temple3;
						if (list2.Count >= 8 || flag2)
						{
							list.Add(new List<CardInfo>());
							if (flag2)
							{
								__instance.tabPageIndices[temple3] = list.IndexOf(list2) + 1;
							}
						}
					}
				}

				for (int m = 0; m < __instance.tabPageIndices.Length; m++)
				{
					if (m > 0 && __instance.tabPageIndices[m] == 0)
					{
						list.Add(new List<CardInfo>());
						__instance.tabPageIndices[m] = list.Count - 1;
					}
				}
				list[list.Count - 1] = boons[0];
				int id = list.Count - 1;
				if (boons.Count > 0)
                {
					for (int i = 1; i < boons.Count; i++)
                    {
						list.Add(boons[i]);
					}
                }
				__instance.tabPageIndices[4] = id;
				__result = list;
				return false;
			}
		}
	}
}
