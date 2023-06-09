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

namespace infact2
{
	[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
	[BepInDependency("cyantist.inscryption.api", BepInDependency.DependencyFlags.HardDependency)]
	public class Plugin : BaseUnityPlugin
	{
		private const string PluginGuid = "mrfantastik.inscryption.infact2";

		private const string PluginName = "InfiniteAct2";

		private const string PluginVersion = "1.1.0";

		private static Assembly _assembly;

		private static bool didNuzlocke = false;
		public static Assembly CurrentAssembly => _assembly ??= Assembly.GetExecutingAssembly();

		public readonly static SpecialTriggeredAbility Reincarnation = SpecialTriggeredAbilityManager.Add(PluginGuid, "Reincarnation", typeof(ReincarantionBoon)).Id;
		public readonly static SpecialTriggeredAbility Voltage = SpecialTriggeredAbilityManager.Add(PluginGuid, "Voltage", typeof(VoltageBoon)).Id;
		public readonly static SpecialTriggeredAbility Construct = SpecialTriggeredAbilityManager.Add(PluginGuid, "Construct", typeof(ConstructBoon)).Id;
		public readonly static SpecialTriggeredAbility Fossil = SpecialTriggeredAbilityManager.Add(PluginGuid, "Fossil", typeof(FossilBoon)).Id;
		public readonly static SpecialTriggeredAbility BountyHunter = SpecialTriggeredAbilityManager.Add(PluginGuid, "BountyHunter", typeof(Bounty)).Id;
		public readonly static SpecialTriggeredAbility Nuzlocke = SpecialTriggeredAbilityManager.Add(PluginGuid, "Nuzlocke", typeof(OverclockAct2)).Id;

		public static int BoonCount = 0;

		public static CardMetaCategory BoonsPool = (CardMetaCategory)InscryptionAPI.Guid.GuidManager.GetEnumValue<CardMetaCategory>("mrfantastik.inscryption.infact2", "ExcludeFromAct2Endless");
		public static CardMetaCategory ExcludeFromAct2Endless = (CardMetaCategory)InscryptionAPI.Guid.GuidManager.GetEnumValue<CardMetaCategory>("mrfantastik.inscryption.infact2", "ExcludeFromAct2Endless");
		private void Awake()
		{
			Harmony harmony = new Harmony("mrfantastik.inscryption.infact2");
			harmony.PatchAll();
			AddTerrain();
			AddBoons();
			foreach (CardInfo card in CardLoader.AllData)
            {
				if (card.HasCardMetaCategory(BoonsPool))
                {
					BoonCount++;
                }
            }
			//Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), PluginGuid);
		}


		[HarmonyPatch(typeof(GBC.PlayerMovementController), "Start")]
		public class transferTabletPosition
		{
			public static void Prefix(ref GBC.PlayerMovementController __instance)
			{
				if (SceneLoader.ActiveSceneName == "GBC_Starting_Island")
				{
					if (SaveData.roomId == "Room") {SaveManager.SaveToFile(true); }
					Plugin.functionsnstuff.FixTabletPos();
				}
			}
		}

		[HarmonyPatch(typeof(MenuController), "LoadGameFromMenu")]
		public class quickStart
		{
			public static bool Prefix(bool newGameGBC)
			{
				SaveManager.LoadFromFile();
				if (newGameGBC)
				{
					SaveManager.SaveFile.ResetGBCSaveData();
					StoryEventsData.SetEventCompleted(StoryEvent.StartScreenNewGameUsed, false, true);
					StoryEventsData.SetEventCompleted(StoryEvent.GBCIntroCompleted, false, true);
					SaveData.cameraX = 0;
					SaveData.cameraY = 0;
					SaveData.roomId = "Room";
					SaveManager.savingDisabled = false;
					SaveManager.SaveToFile(false);
					LoadingScreenManager.LoadScene("GBC_Starting_Island");
					return false;
				}
				return true;
			}
		}

		[HarmonyPatch(typeof(GBC.StartingIslandIntroSequencer), "IntroSequence")]
		public class fixBugs
		{
			public static void Prefix(ref GBC.StartingIslandIntroSequencer __instance)
			{
				SaveData.roomId = "Room";
				SaveData.cameraX = 0;
				SaveData.cameraY = 0;
				SaveManager.SaveToFile();
			}
		}

            public class functionsnstuff
		{
			public static void FixTabletPos()
			{
				if (!didNuzlocke)
				{
					foreach (CardInfo card in CardLoader.allData)
					{
						if (card.HasCardMetaCategory(CardMetaCategory.GBCPlayable))
						{
							CardInfo customCard2 = CardLoader.allData.CardByName(card.name);
							customCard2.AddSpecialAbilities(Nuzlocke);
						}
					}
					didNuzlocke = true;
				}
				GameObject.Find("Tablets").GetComponent<BoxCollider2D>().size = new Vector2(0.5f, 0.04f);
				GameObject.Find("Tablets").GetComponent<BoxCollider2D>().offset = new Vector2(-0.5f, 0.02f);
				for (int i = 0; i < GameObject.Find("Tablets").transform.childCount; i++)
				{
					var offset = -0.2f;
					if (i > 1)
					{
						offset = 0.2f;
					}
					GameObject.Find("Tablets").transform.GetChild(i).transform.position = new Vector2(GameObject.Find("Tablets").transform.GetChild(i).transform.position.x + offset, GameObject.Find("Tablets").transform.GetChild(i).transform.position.y);

				}
				BoxCollider2D comp = GameObject.Find("Tablets").AddComponent<BoxCollider2D>();
				comp.offset = new Vector2(0.5f, 0.02f);
				comp.size = new Vector2(0.5f, 0.04f);
				GameObject door = Instantiate(GameObject.Find("Tablet_Magic"));
				door.name = "Door";
				door.transform.localPosition = new Vector3(0, 0.2f, 0);
				door.transform.parent = GameObject.Find("Tablets").transform;
				door.GetComponent<SpriteRenderer>().sprite = Tools.getSprite("enterdoor.png");
				GameObject transition = Instantiate(GameObject.Find("ReturnToMapVolume"));
				transition.name = "DungeonVolume";
				transition.transform.localPosition = new Vector3(0, 0.2f, 0);
				transition.GetComponent<BoxCollider2D>().size = new Vector2(1, 0.25f);
				Destroy(transition.GetComponent<GBC.SceneTransitionVolume>());
				transition.AddComponent<GBC.RoomTransitionVolume>();

				//bounty stars!

				GameObject stars = Instantiate(new GameObject());
				stars.name = "BountyStars";
				stars.AddComponent<SpriteRenderer>().sprite = Tools.getSprite("star_1.png");
				stars.GetComponent<SpriteRenderer>().sortingLayerName = "GBC Character";
				stars.transform.parent = Singleton<GBC.PlayerMovementController>.Instance.gameObject.transform;
				stars.transform.localPosition = new Vector3(0, 0.25f, 0);
				stars.layer = GameObject.Find("Cliffs").layer;
				if (SaveData.bountyStars < 1)
				{
					stars.SetActive(false);
				} else
                {
					stars.GetComponent<SpriteRenderer>().sprite = Tools.getSprite("star_" + Math.Floor(SaveData.bountyStars).ToString() + ".png");
				}

				//room making time
				GameObject deckChooseRoom = Instantiate(new GameObject());
				deckChooseRoom.name = "DeckChooseRoom";
				deckChooseRoom.layer = GameObject.Find("Cliffs").layer;
				deckChooseRoom.transform.localPosition = new Vector3(0, 10, 0);

				GameObject floor = Instantiate(new GameObject());
				floor.name = "floor";
				floor.transform.parent = deckChooseRoom.transform;
				floor.AddComponent<SpriteRenderer>();
				floor.GetComponent<SpriteRenderer>().size = new Vector2(4.2f, 2.4f);
				floor.transform.localPosition = new Vector3(0, 0, 0);
				floor.transform.localScale = new Vector3(1.25f, 1.25f, 1f);
				floor.layer = GameObject.Find("Cliffs").layer;
				floor.GetComponent<SpriteRenderer>().sprite = Tools.getSprite("deckroomfloor.png");

				GameObject topThing = Instantiate(floor);
				topThing.name = "Overlay";
				topThing.transform.localPosition = new Vector3(0, 10, 0);
				topThing.transform.parent = deckChooseRoom.transform;
				topThing.GetComponent<SpriteRenderer>().sortingLayerName = "GBC Character";
				topThing.GetComponent<SpriteRenderer>().sprite = Tools.getSprite("deckroomborder.png");

				GameObject DCRMarker = Instantiate(new GameObject(), new Vector3(0.07f, 9.2f, 1.67f), Quaternion.Euler(0, 0, 0), deckChooseRoom.transform);
				DCRMarker.name = "PlayerPositionMarker";

				GameObject Colliders = Instantiate(new GameObject(), new Vector3(0, 10, 0), Quaternion.Euler(0, 0, 0), deckChooseRoom.transform);
				Colliders.name = "Colliders";

				GameObject Collider1 = Instantiate(new GameObject(), new Vector3(-1.36f, 9.78f, 0), Quaternion.Euler(0, 0, 0), Colliders.transform);
				Collider1.AddComponent<BoxCollider2D>();
				Collider1.GetComponent<BoxCollider2D>().size = new Vector2(0.2f, 50);

				GameObject Collider2 = Instantiate(new GameObject(), new Vector3(1.52f, 9.78f, 0), Quaternion.Euler(0, 0, 0), Colliders.transform);
				Collider2.AddComponent<BoxCollider2D>();
				Collider2.GetComponent<BoxCollider2D>().size = new Vector2(0.2f, 50);

				GameObject Collider3 = Instantiate(new GameObject(), new Vector3(0.1f, 10.9f, 0), Quaternion.Euler(0, 0, 0), Colliders.transform);
				Collider3.AddComponent<BoxCollider2D>();
				Collider3.GetComponent<BoxCollider2D>().size = new Vector2(100f, 0.25f);

				GameObject Collider4 = Instantiate(new GameObject(), new Vector3(0.1f, 8.4f, 0), Quaternion.Euler(0, 0, 0), Colliders.transform);
				Collider4.AddComponent<BoxCollider2D>();
				Collider4.GetComponent<BoxCollider2D>().size = new Vector2(100f, 0.25f);

				transition.GetComponent<GBC.RoomTransitionVolume>().destinationRoom = deckChooseRoom.transform;
				transition.GetComponent<GBC.RoomTransitionVolume>().exitMarker = DCRMarker.transform;

				GameObject transition2 = Instantiate(transition);
				transition2.name = "BackToIsleVolume";
				transition2.transform.localPosition = new Vector3(0, 8.5f, 0);

				transition2.GetComponent<GBC.RoomTransitionVolume>().destinationRoom = GameObject.Find("Room").transform;
				transition2.transform.GetChild(0).transform.position = new Vector3(0, -0.15f, 0);
				Destroy(transition2.GetComponent<GBC.SceneTransitionVolume>());
				transition2.GetComponent<GBC.RoomTransitionVolume>().exitMarker = transition2.transform.GetChild(0).transform;

				//place the deck things
				GameObject Tablet2 = Instantiate(new GameObject(), new Vector3(0f, 10, 0), Quaternion.Euler(0, 0, 0), deckChooseRoom.transform);
				Tablet2.name = "Tablet2s";

				GameObject Undead2 = Instantiate(GameObject.Find("Tablet_Undead"), new Vector3(-0.4f, 10, 0), Quaternion.Euler(0, 0, 0), Tablet2.transform);
				Undead2.transform.GetChild(0).gameObject.SetActive(false);
				GameObject Nature2 = Instantiate(GameObject.Find("Tablet_Nature"), new Vector3(0.075f, 10.5f, 0), Quaternion.Euler(0, 0, 0), Tablet2.transform);
				GameObject Tech2 = Instantiate(GameObject.Find("Tablet_Tech"), new Vector3(0.5f, 10, 0), Quaternion.Euler(0, 0, 0), Tablet2.transform);
				Tech2.transform.GetChild(0).gameObject.SetActive(false);
				Tech2.transform.GetChild(1).gameObject.SetActive(false);
				GameObject Magic2 = Instantiate(GameObject.Find("Tablet_Magic"), new Vector3(0.075f, 9.5f, 0), Quaternion.Euler(0, 0, 0), Tablet2.transform);

				for (int i = 0; i < Tablet2.transform.childCount; i++)
				{
					Tablet2.transform.GetChild(i).gameObject.AddComponent<BoxCollider2D>();
					Tablet2.transform.GetChild(i).gameObject.GetComponent<BoxCollider2D>().offset = new Vector2(0, 0);
					Tablet2.transform.GetChild(i).gameObject.GetComponent<BoxCollider2D>().size = new Vector2(0.26f, 0.02f);
				}

				//Nature Dungeon

				GameObject NatureDungeon = Instantiate(new GameObject());
				NatureDungeon.name = "NatureDungeon";
				NatureDungeon.layer = GameObject.Find("Cliffs").layer;
				NatureDungeon.transform.localPosition = new Vector3(0, 50, 0);

				GameObject Colliders2 = Instantiate(new GameObject(), new Vector3(0, 50, 0), Quaternion.Euler(0, 0, 0), NatureDungeon.transform);
				Colliders2.name = "Colliders";

				GameObject Collider12 = Instantiate(new GameObject(), new Vector3(-1.3f, 51.25f, 0), Quaternion.Euler(0, 0, 0), Colliders2.transform);
				Collider12.AddComponent<BoxCollider2D>();
				Collider12.GetComponent<BoxCollider2D>().size = new Vector2(1.5f, 2.7f);

				GameObject Collider12andahalf = Instantiate(new GameObject(), new Vector3(-1.3f, 48.2f, 0), Quaternion.Euler(0, 0, 0), Colliders2.transform);
				Collider12andahalf.AddComponent<BoxCollider2D>();
				Collider12andahalf.GetComponent<BoxCollider2D>().size = new Vector2(1.5f, 2.7f);

				GameObject Collider12andaquart = Instantiate(new GameObject(), new Vector3(-1.8f, 49f, 0), Quaternion.Euler(0, 0, 0), Colliders2.transform);
				Collider12andaquart.AddComponent<BoxCollider2D>();
				Collider12andaquart.GetComponent<BoxCollider2D>().size = new Vector2(0.1f, 2.7f);

				GameObject Collider23 = Instantiate(new GameObject(), new Vector3(1f, 50f, 0), Quaternion.Euler(0, 0, 0), Colliders2.transform);
				Collider23.AddComponent<BoxCollider2D>();
				Collider23.GetComponent<BoxCollider2D>().size = new Vector2(0.2f, 50);

				GameObject RockBlocker = Instantiate(new GameObject(), new Vector3(-0.02f, 50.4f, -1f), Quaternion.Euler(0, 0, 0), Colliders2.transform);
				RockBlocker.AddComponent<BoxCollider2D>();
				RockBlocker.GetComponent<BoxCollider2D>().size = new Vector2(0.23f, 0.45f);
				RockBlocker.AddComponent<SpriteRenderer>().sprite = Tools.getSprite("block_rocks.png");
				RockBlocker.layer = GameObject.Find("Cliffs").layer;

				GameObject creatureEyes = Instantiate(new GameObject(), new Vector3(-1.9f, 50.9f, 0), Quaternion.Euler(0, 0, 0), NatureDungeon.transform);
				creatureEyes.name = "Eyes";
				creatureEyes.AddComponent<AnimatingSprite>();
				creatureEyes.GetComponent<AnimatingSprite>().animSpeed = 0.3f;
				creatureEyes.GetComponent<AnimatingSprite>().blinkTimer = 24.84f;
				creatureEyes.GetComponent<AnimatingSprite>().animOffset = -3.88f;
				creatureEyes.GetComponent<AnimatingSprite>().frames = new List<Sprite> { null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null };
				createEyeFrames(creatureEyes, 0);
				creatureEyes.GetComponent<AnimatingSprite>().frameCount = 22;
				creatureEyes.GetComponent<AnimatingSprite>().frameIndex = 1;
				creatureEyes.GetComponent<AnimatingSprite>().Awake();
				creatureEyes.AddComponent<SpriteRenderer>().sprite = Tools.getSprite("eyes_1.png");
				creatureEyes.layer = GameObject.Find("Tablet_Nature").layer;
				creatureEyes.GetComponent<SpriteRenderer>().sortingLayerName = "GBC Pixel";

				GameObject eyes2 = Instantiate(creatureEyes, new Vector3(-1.5f, 50.4f, 0), Quaternion.Euler(0, 0, 0), NatureDungeon.transform);
				createEyeFrames(eyes2, UnityEngine.Random.RandomRangeInt(0, 10));
				eyes2.GetComponent<AnimatingSprite>().Awake();
				GameObject eyes3 = Instantiate(creatureEyes, new Vector3(1.5f, 50.5f, 0), Quaternion.Euler(0, 0, 0), NatureDungeon.transform);
				createEyeFrames(eyes3, UnityEngine.Random.RandomRangeInt(0, 10));
				eyes3.GetComponent<AnimatingSprite>().Awake();
				GameObject eyes4 = Instantiate(creatureEyes, new Vector3(1.29f, 48.8f, 0), Quaternion.Euler(0, 0, 0), NatureDungeon.transform);
				createEyeFrames(eyes4, UnityEngine.Random.RandomRangeInt(0, 10));
				eyes4.GetComponent<AnimatingSprite>().Awake();
				creatureEyes.GetComponent<AnimatingSprite>().Awake();

				GameObject floor2 = Instantiate(new GameObject());
				floor2.name = "floor";
				floor2.transform.parent = NatureDungeon.transform;
				floor2.AddComponent<SpriteRenderer>();
				floor2.GetComponent<SpriteRenderer>().size = new Vector2(4.2f, 2.4f);
				floor2.transform.localPosition = new Vector3(0, 0, 0);
				floor2.layer = GameObject.Find("Cliffs").layer;
				floor2.GetComponent<SpriteRenderer>().sprite = Tools.getSprite("naturedungeon.png");

				GameObject NDMarker = Instantiate(new GameObject(), new Vector3(0.07f, 48.95f, 1.67f), Quaternion.Euler(0, 0, 0), NatureDungeon.transform);
				NDMarker.name = "PlayerPositionMarker";

				//Undead Dungeon

				GameObject UndeadDungeon = Instantiate(new GameObject());
				UndeadDungeon.name = "UndeadDungeon";
				UndeadDungeon.layer = GameObject.Find("Cliffs").layer;
				UndeadDungeon.transform.localPosition = new Vector3(20, 50, 0);

				GameObject Colliders22 = Instantiate(new GameObject(), new Vector3(20, 50, 0), Quaternion.Euler(0, 0, 0), UndeadDungeon.transform);
				Colliders22.name = "Colliders";

				GameObject Collider22 = Instantiate(new GameObject(), new Vector3(19.45f, 50f, 0), Quaternion.Euler(0, 0, 0), Colliders22.transform);
				Collider22.AddComponent<BoxCollider2D>();
				Collider22.GetComponent<BoxCollider2D>().size = new Vector2(0.2f, 100);

				GameObject Collider33 = Instantiate(new GameObject(), new Vector3(20.56f, 50f, 0), Quaternion.Euler(0, 0, 0), Colliders22.transform);
				Collider33.AddComponent<BoxCollider2D>();
				Collider33.GetComponent<BoxCollider2D>().size = new Vector2(0.2f, 100);

				GameObject GraveBlocker = Instantiate(new GameObject(), new Vector3(19.98f, 50.4f, -1f), Quaternion.Euler(0, 0, 0), Colliders22.transform);
				GraveBlocker.AddComponent<BoxCollider2D>();
				GraveBlocker.GetComponent<BoxCollider2D>().size = new Vector2(0.2f, 0.25f);
				GraveBlocker.AddComponent<SpriteRenderer>().sprite = Tools.getSprite("block_grave.png");
				GraveBlocker.layer = GameObject.Find("Cliffs").layer;

				GameObject floor3 = Instantiate(new GameObject());
				floor3.name = "floor";
				floor3.transform.parent = UndeadDungeon.transform;
				floor3.AddComponent<SpriteRenderer>();
				floor3.GetComponent<SpriteRenderer>().size = new Vector2(4.2f, 2.4f);
				floor3.transform.localPosition = new Vector3(0, 0, 0);
				floor3.layer = GameObject.Find("Cliffs").layer;
				floor3.GetComponent<SpriteRenderer>().sprite = Tools.getSprite("undeaddungeon.png");

				GameObject boneEyes = Instantiate(creatureEyes, new Vector3(20.6f, 50.8f, 0), Quaternion.Euler(0, 0, 0), UndeadDungeon.transform);
				for (int i = 0; i < 22; i++)
				{
					if (i > 0 && i < 6)
					{
						Sprite spriteCranberry = Tools.getSprite("boneyes_0.png");
						if (i > 1 && i < 5)
						{
							spriteCranberry = Tools.getSprite("boneyes_1.png");
						}
						boneEyes.GetComponent<AnimatingSprite>().frames[i] = spriteCranberry;
					}

				}
				boneEyes.GetComponent<AnimatingSprite>().Awake();

				GameObject candelholder = Instantiate(Resources.Load("prefabs/gbcinterior/temples/CandleHolder") as GameObject, new Vector3(20.93f, 50.58f, 0), Quaternion.Euler(0, 0, 0), UndeadDungeon.transform);
				GameObject candelholder2 = Instantiate(Resources.Load("prefabs/gbcinterior/temples/CandleHolder") as GameObject, new Vector3(19.54f, 49.95f, 0), Quaternion.Euler(0, 0, 0), UndeadDungeon.transform);
				GameObject candelholder3 = Instantiate(Resources.Load("prefabs/gbcinterior/temples/CandleHolder") as GameObject, new Vector3(20.46f, 49.95f, 0), Quaternion.Euler(0, 0, 0), UndeadDungeon.transform);
				GameObject candelholder4 = Instantiate(Resources.Load("prefabs/gbcinterior/temples/CandleHolder") as GameObject, new Vector3(19.54f, 48.82f, 0), Quaternion.Euler(0, 0, 0), UndeadDungeon.transform);
				GameObject candelholder5 = Instantiate(Resources.Load("prefabs/gbcinterior/temples/CandleHolder") as GameObject, new Vector3(20.46f, 48.82f, 0), Quaternion.Euler(0, 0, 0), UndeadDungeon.transform);
				//19.54 49.95 0
				GameObject UDMarker = Instantiate(new GameObject(), new Vector3(20.07f, 48.95f, 1.67f), Quaternion.Euler(0, 0, 0), UndeadDungeon.transform);
				UDMarker.name = "PlayerPositionMarker";

				//Wizard Dungeon

				GameObject WizardDungeon = Instantiate(new GameObject());
				WizardDungeon.name = "WizardDungeon";
				WizardDungeon.layer = GameObject.Find("Cliffs").layer;
				WizardDungeon.transform.localPosition = new Vector3(40, 50, 0);

				GameObject Colliders43 = Instantiate(new GameObject(), new Vector3(40, 50, 0), Quaternion.Euler(0, 0, 0), WizardDungeon.transform);
				Colliders43.name = "Colliders";

				GameObject Collider62 = Instantiate(new GameObject(), new Vector3(39.6f, 49f, 0), Quaternion.Euler(0, 0, 0), Colliders43.transform);
				Collider62.AddComponent<BoxCollider2D>();
				Collider62.GetComponent<BoxCollider2D>().size = new Vector2(0.2f, 5.5f);

				GameObject Collider62andahalf = Instantiate(new GameObject(), new Vector3(40.9f, 50.82f, 0), Quaternion.Euler(0, 0, 0), Colliders43.transform);
				Collider62andahalf.AddComponent<BoxCollider2D>();
				Collider62andahalf.GetComponent<BoxCollider2D>().size = new Vector2(0.2f, 3.5f);


				GameObject Collider52 = Instantiate(new GameObject(), new Vector3(40.4f, 49f, 0), Quaternion.Euler(0, 0, 0), Colliders43.transform);
				Collider52.AddComponent<BoxCollider2D>();
				Collider52.GetComponent<BoxCollider2D>().size = new Vector2(0.2f, 3.5f);

				GameObject BookBlocker = Instantiate(new GameObject(), new Vector3(39.98f, 50.4f, -1f), Quaternion.Euler(0, 0, 0), Colliders43.transform);
				BookBlocker.AddComponent<BoxCollider2D>();
				BookBlocker.GetComponent<BoxCollider2D>().size = new Vector2(0.1f, 0.25f);
				BookBlocker.AddComponent<SpriteRenderer>().sprite = Tools.getSprite("block_books.png");
				BookBlocker.layer = GameObject.Find("Cliffs").layer;

				GameObject floor5 = Instantiate(new GameObject());
				floor5.name = "floor";
				floor5.transform.parent = WizardDungeon.transform;
				floor5.AddComponent<SpriteRenderer>();
				floor5.GetComponent<SpriteRenderer>().size = new Vector2(4.2f, 2.4f);
				floor5.transform.localPosition = new Vector3(0, 0, 0);
				floor5.layer = GameObject.Find("Tablet_Nature").layer;
				floor5.GetComponent<SpriteRenderer>().sprite = Tools.getSprite("wizarddungeon.png");

				GameObject WDMarker = Instantiate(new GameObject(), new Vector3(40.07f, 48.95f, 1.67f), Quaternion.Euler(0, 0, 0), WizardDungeon.transform);
				WDMarker.name = "PlayerPositionMarker";

				GameObject isles = Instantiate(floor5);
				isles.name = "Isles";
				isles.transform.parent = WizardDungeon.transform;
				isles.transform.localPosition = new Vector3(0f, -0.25f, -1f);
				isles.GetComponent<SpriteRenderer>().sprite = Tools.getSprite("wizarddungeon_isles.png");
				isles.AddComponent<SineWaveMovement>().speed = 0.5f;
				isles.GetComponent<SineWaveMovement>().originalPosition = new Vector3(40f, 49.85f, -1f);
				isles.GetComponent<SineWaveMovement>().yMagnitude = 0.1f;

				GameObject gems = Instantiate(floor5);
				gems.name = "Gems";
				gems.transform.parent = WizardDungeon.transform;
				gems.transform.localPosition = new Vector3(-0.025f, 0, -1f);
				gems.GetComponent<SpriteRenderer>().sprite = Tools.getSprite("wizarddungeon_gems.png");
				gems.GetComponent<SpriteRenderer>().sortingLayerName = "GBC Character";
				gems.AddComponent<SineWaveMovement>().speed = 0.5f;
				gems.GetComponent<SineWaveMovement>().originalPosition = new Vector3(39.975f, 50, -1f);
				gems.GetComponent<SineWaveMovement>().xMagnitude = 0.045f;

				GameObject mox = Instantiate(new GameObject(), new Vector3(40.97f, 51.325f, 0), Quaternion.Euler(0, 0, 0), WizardDungeon.transform);
				mox.name = "PillarMox";
				mox.AddComponent<SpriteRenderer>().sprite = Tools.getSprite("mox_ruby.png");
				mox.layer = GameObject.Find("Cliffs").layer;
				mox.GetComponent<SpriteRenderer>().sortingLayerName = "GBC Character";

				GameObject giantArch = Instantiate(floor5);
				giantArch.name = "Arch";
				giantArch.transform.parent = WizardDungeon.transform;
				giantArch.transform.localPosition = new Vector3(-0.035f, 0, -1f);
				giantArch.GetComponent<SpriteRenderer>().sortingLayerName = "GBC Character";
				giantArch.GetComponent<SpriteRenderer>().sprite = Tools.getSprite("wizarddungeon_arch.png");

				//tech dungeon

				GameObject TechDungeon = Instantiate(new GameObject());
				TechDungeon.name = "TechDungeon";
				TechDungeon.layer = GameObject.Find("Cliffs").layer;
				TechDungeon.transform.localPosition = new Vector3(30, 50, 0);

				GameObject Colliders33 = Instantiate(new GameObject(), new Vector3(30, 50, 0), Quaternion.Euler(0, 0, 0), TechDungeon.transform);
				Colliders22.name = "Colliders";

				GameObject Collider32 = Instantiate(new GameObject(), new Vector3(29.5f, 50f, 0), Quaternion.Euler(0, 0, 0), Colliders33.transform);
				Collider32.AddComponent<BoxCollider2D>();
				Collider32.GetComponent<BoxCollider2D>().size = new Vector2(0.2f, 100);

				GameObject Collider42 = Instantiate(new GameObject(), new Vector3(30.5f, 50f, 0), Quaternion.Euler(0, 0, 0), Colliders33.transform);
				Collider42.AddComponent<BoxCollider2D>();
				Collider42.GetComponent<BoxCollider2D>().size = new Vector2(0.2f, 100);

				GameObject TechBlocker = Instantiate(new GameObject(), new Vector3(29.98f, 50.4f, -1f), Quaternion.Euler(0, 0, 0), Colliders33.transform);;
				TechBlocker.AddComponent<BoxCollider2D>();
				TechBlocker.GetComponent<BoxCollider2D>().size = new Vector2(0.2f, 0.25f);
				TechBlocker.AddComponent<SpriteRenderer>().sprite = Tools.getSprite("block_tech.png");
				TechBlocker.layer = GameObject.Find("Cliffs").layer;

				GameObject floor4 = Instantiate(new GameObject());
				floor4.name = "floor";
				floor4.transform.parent = TechDungeon.transform;
				floor4.AddComponent<SpriteRenderer>();
				floor4.GetComponent<SpriteRenderer>().size = new Vector2(4.2f, 2.4f);
				floor4.transform.localPosition = new Vector3(-0.02f, 0, 0);
				floor4.layer = GameObject.Find("Cliffs").layer;
				floor4.GetComponent<SpriteRenderer>().sprite = Tools.getSprite("techdungeon.png");

				GameObject drones = Instantiate(floor4);
				drones.name = "Drones";
				drones.transform.parent = TechDungeon.transform;
				drones.transform.localPosition = new Vector3(0, 0, -1f);
				drones.GetComponent<SpriteRenderer>().sprite = Tools.getSprite("techdrones.png");
				drones.AddComponent<SineWaveMovement>().speed = 0.5f;
				drones.GetComponent<SineWaveMovement>().originalPosition = new Vector3(30, 50, -1f);
				drones.GetComponent<SineWaveMovement>().yMagnitude = 0.1f;

				GameObject TDMarker = Instantiate(new GameObject(), new Vector3(30.07f, 48.95f, 1.67f), Quaternion.Euler(0, 0, 0), TechDungeon.transform);
				TDMarker.name = "PlayerPositionMarker";

				//Card Events

				GameObject CardEvent = Instantiate(GameObject.Find("Tablet_Nature").transform.Find("DeckPileVolume").gameObject, new Vector3(0.07f, 60.7f, 1.67f), Quaternion.Euler(0, 0, 0), NatureDungeon.transform);
				CardEvent.name = "CardpackEvent";
				CardEvent.SetActive(true);
				CardEvent.transform.Find("CardPile").gameObject.GetComponent<SpriteRenderer>().sprite = Tools.getSprite("cardchoice.png");
				CardEvent.transform.Find("CardPile").gameObject.GetComponent<SpriteRenderer>().sortingLayerName = "GBC Pixel";
				CardEvent.transform.localScale = new Vector3(1f, 1f, 1f);
				CardEvent.transform.Find("CollisionVolumeButton").localScale = new Vector3(1f, 1f, 1f);
				CardEvent.transform.Find("CardPile").gameObject.GetComponent<BoxCollider2D>().enabled = false;
				
				GameObject CardTypeEvent = Instantiate(CardEvent);
				CardTypeEvent.name = "CardcostEvent";
				CardTypeEvent.transform.Find("CardPile").gameObject.GetComponent<SpriteRenderer>().sprite = Tools.getSprite("cardchoicecost.png");
				CardTypeEvent.AddComponent<infact2.GainCardEvent>();
				CardTypeEvent.transform.Find("CollisionVolumeButton").localScale = new Vector3(1, 1, 1);
				CardTypeEvent.transform.position = new Vector3(0.07f, 60f, 1.67f);

				GameObject RareCardEvent = Instantiate(CardEvent);
				RareCardEvent.name = "Rarecardevent";
				RareCardEvent.transform.Find("CardPile").gameObject.GetComponent<SpriteRenderer>().sprite = Tools.getSprite("rarecardchoice.png");
				RareCardEvent.AddComponent<infact2.GainCardEvent>();
				RareCardEvent.transform.position = new Vector3(0.07f, 74f, 1.67f);
				
				GameObject CardBattleEvent = Instantiate(CardEvent);
				CardBattleEvent.name = "CardBattleEvent";
				CardBattleEvent.transform.Find("CardPile").gameObject.GetComponent<SpriteRenderer>().sprite = Tools.getSprite("cardbattle.png");
				CardBattleEvent.transform.position = new Vector3(0.07f, 69.5f, 1.67f);
				CardBattleEvent.transform.Find("CollisionVolumeButton").GetChild(0).GetChild(0).GetChild(0).gameObject.GetComponent<SpriteRenderer>().sprite = Tools.getSprite("button_battle.png");

				GameObject ShopEvent = Instantiate(CardEvent);
				ShopEvent.name = "ShopEvent";
				ShopEvent.transform.Find("CardPile").gameObject.GetComponent<SpriteRenderer>().sprite = Tools.getSprite("shop.png");
				ShopEvent.transform.position = new Vector3(0.07f, 70.5f, 1.67f);
				ShopEvent.transform.Find("CollisionVolumeButton").GetChild(0).GetChild(0).GetChild(0).gameObject.GetComponent<SpriteRenderer>().sprite = Tools.getSprite("button_shop.png");

				GameObject BoonEvent = Instantiate(CardEvent);
				BoonEvent.name = "BoonEvent";
				BoonEvent.transform.Find("CardPile").gameObject.GetComponent<SpriteRenderer>().sprite = Tools.getSprite("boon.png");
				BoonEvent.transform.position = new Vector3(0.07f, 71.5f, 1.67f);
				BoonEvent.transform.Find("CollisionVolumeButton").localScale = new Vector3(1f, 1f, 1f);
				BoonEvent.transform.Find("CollisionVolumeButton").GetChild(0).GetChild(0).GetChild(0).gameObject.GetComponent<SpriteRenderer>().sprite = Tools.getSprite("button_boon.png");

				GameObject MycoEvent = Instantiate(CardEvent);
				MycoEvent.name = "MycoEvent";
				MycoEvent.transform.Find("CardPile").gameObject.GetComponent<SpriteRenderer>().sprite = Tools.getSprite("myco.png");
				MycoEvent.transform.position = new Vector3(1.07f, 71.5f, 1.67f);
				MycoEvent.AddComponent<infact2.GainCardEvent>();
				MycoEvent.transform.Find("CollisionVolumeButton").localScale = new Vector3(1f, 1f, 1f);
				MycoEvent.transform.Find("CollisionVolumeButton").GetChild(0).GetChild(0).GetChild(0).gameObject.GetComponent<SpriteRenderer>().sprite = Tools.getSprite("button_boon.png");

				GameObject BoonChoiceUI = Instantiate(GameObject.Find("SingleCardGainUI"));
				BoonChoiceUI.transform.localPosition = new Vector3(0, 0, 0);
				BoonChoiceUI.name = "BoonChoiceUI";
				BoonChoiceUI.transform.parent = GameObject.Find("UI").transform;
				BoonChoiceUI.transform.Find("MainPanel").Find("ExitButton").gameObject.SetActive(false);
				BoonChoiceUI.transform.Find("MainPanel").Find("Cards").GetChild(0).localPosition = new Vector3(-0.25f, 0, 0);
				BoonChoiceUI.GetComponent<GBC.SingleCardGainUI>().enabled = false;
				GameObject SelectableCard2 = Instantiate(BoonChoiceUI.transform.Find("MainPanel").Find("Cards").GetChild(0).gameObject);
				SelectableCard2.transform.parent = BoonChoiceUI.transform.Find("MainPanel").Find("Cards");
				SelectableCard2.transform.localPosition = new Vector3(0.25f, 0, 0);

				GameObject CardBossEvent = Instantiate(CardBattleEvent);
				CardBossEvent.name = "CardBossEvent";
				CardBossEvent.transform.Find("CardPile").gameObject.GetComponent<SpriteRenderer>().sprite = Tools.getSprite("cardboss.png");
				CardBossEvent.transform.position = new Vector3(0.07f, 69.5f, 1.9f);

				GameObject EliteBattleEvent = Instantiate(CardBattleEvent);
				EliteBattleEvent.name = "EliteBattleEvent";
				EliteBattleEvent.transform.Find("CardPile").gameObject.GetComponent<SpriteRenderer>().sprite = Tools.getSprite("doubletemplebattle.png");
				EliteBattleEvent.transform.position = new Vector3(0.37f, 69.5f, 1.9f);

				//secrets

				GameObject FishingEvent = Instantiate(CardEvent);
				FishingEvent.name = "FishingEvent";
				FishingEvent.transform.Find("CardPile").gameObject.SetActive(false);
				FishingEvent.GetComponent<BoxCollider2D>().size = new Vector2(0.5f, 0.5f);
				FishingEvent.transform.position = new Vector3(0.07f, 60f, 1.67f);
				FishingEvent.transform.Find("CollisionVolumeButton").GetChild(0).GetChild(0).GetChild(0).gameObject.GetComponent<SpriteRenderer>().sprite = Tools.getSprite("button_secret.png");

				GameObject BoneLordEvent = Instantiate(FishingEvent);
				BoneLordEvent.name = "BoneLordEvent";
				BoneLordEvent.transform.Find("CardPile").gameObject.SetActive(false);
				BoneLordEvent.GetComponent<BoxCollider2D>().size = new Vector2(0.5f, 0.25f);
				BoneLordEvent.transform.position = new Vector3(0.07f, 60f, 1.67f);

				GameObject VesselEvent = Instantiate(FishingEvent);
				VesselEvent.name = "VesselEvent";
				VesselEvent.transform.Find("CardPile").gameObject.SetActive(false);
				VesselEvent.GetComponent<BoxCollider2D>().size = new Vector2(0.5f, 0.25f);
				VesselEvent.transform.position = new Vector3(0.07f, 60f, 1.67f);

				GameObject MoxEvent = Instantiate(FishingEvent);
				MoxEvent.name = "MoxEvent";
				MoxEvent.transform.Find("CardPile").gameObject.SetActive(false);
				MoxEvent.GetComponent<BoxCollider2D>().size = new Vector2(0.5f, 0.25f);
				MoxEvent.transform.position = new Vector3(0.07f, 60f, 1.67f);

				//chalenges

				GameObject ChallengeUI = Instantiate(GameObject.Find("SingleCardGainUI"));
				ChallengeUI.transform.localPosition = new Vector3(0, 0, 0);
				ChallengeUI.name = "ChallengeUI";
				ChallengeUI.transform.parent = GameObject.Find("UI").transform;
				ChallengeUI.transform.Find("MainPanel").Find("Cards").parent = ChallengeUI.transform;
				ChallengeUI.transform.Find("MainPanel").Find("ExitButton").parent = ChallengeUI.transform;
				ChallengeUI.transform.Find("ExitButton").localPosition = new Vector3(1.4f, 0, 0);
				ChallengeUI.transform.Find("ExitButton").gameObject.SetActive(false);
				ChallengeUI.transform.Find("ExitButton").gameObject.GetComponent<SpriteRenderer>().sprite = Tools.getSprite("challengeexit.png");
				ChallengeUI.transform.Find("ExitButton").gameObject.GetComponent<GBC.GenericUIButton>().OnButtonDown = (Action<MainInputInteractable>)Delegate.Combine(ChallengeUI.transform.Find("ExitButton").gameObject.GetComponent<GBC.GenericUIButton>().OnButtonDown, new Action<MainInputInteractable>(delegate (MainInputInteractable j)
				{
					ChallengeUI.transform.Find("ExitButton").gameObject.GetComponent<GBC.GenericUIButton>().StartCoroutine(DisableChallengeUI());
				}));
				ChallengeUI.transform.Find("MainPanel").transform.localScale = new Vector3(2, 1, 1);
				ChallengeUI.GetComponent<GBC.SingleCardGainUI>().enabled = false;
				ChallengeUI.transform.Find("Cards").gameObject.SetActive(false);
				GameObject challenge = ChallengeUI.transform.Find("Cards").GetChild(0).gameObject;
				challenge.name = "example";
				challenge.transform.localPosition = new Vector3(-1.2f, 0.3f, 0);
				challenge.transform.GetChild(0).GetChild(0).Find("CardElements").Find("CardBack").gameObject.SetActive(true);
				challenge.transform.GetChild(0).GetChild(0).Find("CardElements").Find("PixelCardStats").gameObject.SetActive(false);
				challenge.transform.GetChild(0).GetChild(0).Find("CardElements").Find("CardBack").gameObject.GetComponent<SpriteRenderer>().sprite = Tools.getSprite("challenge_bounty.png");
				for (int i = 1; i < challenge.transform.GetChild(0).GetChild(0).Find("CardElements").childCount; i++)
                {
					challenge.transform.GetChild(0).GetChild(0).Find("CardElements").GetChild(i).gameObject.SetActive(false);
                }
				challenge.transform.GetChild(0).GetChild(0).gameObject.GetComponent<GBC.PixelCardDisplayer>().ReplaceBackgroundSprites(Tools.getSprite("challengebg.png"), Tools.getSprite("challengebg.png"));
				challenge.transform.GetChild(0).GetChild(0).gameObject.GetComponent<GBC.PixelCardDisplayer>().UpdateBackground(CardLoader.GetCardByName("Squirrel"));
				GameObject checkMark = Instantiate(challenge.transform.GetChild(0).GetChild(0).Find("CardElements").Find("CardBack").gameObject);
				checkMark.name = "CheckMark";
				checkMark.GetComponent<SpriteRenderer>().sprite = Tools.getSprite("challengecheckmark.png");
				checkMark.transform.parent = challenge.transform.GetChild(0).GetChild(0).Find("CardElements");
				checkMark.transform.localPosition = new Vector3(0f, 0f, 0);
				checkMark.SetActive(false);
				challenge.SetActive(false);

				List<string> challenges = new List<string> { "bounty", "boon", "elite", "bridge", "nuzlocke", "nohammer" };

				for(int i = 0; i < challenges.Count; i++)
                {
					GameObject challengeObj = Instantiate(challenge);
					challengeObj.name = challenges[i] + "challenge";
					challengeObj.transform.parent = ChallengeUI.transform.Find("Cards");
					challengeObj.transform.GetChild(0).GetChild(0).Find("CardElements").Find("CardBack").gameObject.GetComponent<SpriteRenderer>().sprite = Tools.getSprite("challenge_" + challenges[i] + ".png");
					float y = i % 2 == 0 ? 0.3f : -0.3f;
					float x = -1.4f;
					int offSet = i != 0 ? i / 2 : 0;
					challengeObj.transform.GetChild(0).GetChild(0).Find("CardElements").Find("CheckMark").gameObject.GetComponent<SpriteRenderer>().sprite = Tools.getSprite("challenge_" + challenges[i] + "A.png");
					x += (0.5f * offSet);
					challengeObj.transform.localPosition = new Vector3(x, y, -1f);
					challengeObj.SetActive(true);
					int dex = i;
					challengeObj.gameObject.GetComponent<GBC.PixelSelectableCard>().CursorEntered = (Action<MainInputInteractable>)Delegate.Combine(challengeObj.gameObject.GetComponent<GBC.PixelSelectableCard>().CursorSelectStarted, new Action<MainInputInteractable>(delegate (MainInputInteractable j)
					{
						challengeObj.gameObject.GetComponent<GBC.PixelSelectableCard>().StartCoroutine(DisplayChallengeDescription(challenges[dex]));
					}));
					challengeObj.gameObject.GetComponent<GBC.PixelSelectableCard>().CursorExited = (Action<MainInputInteractable>)Delegate.Combine(challengeObj.gameObject.GetComponent<GBC.PixelSelectableCard>().CursorExited, new Action<MainInputInteractable>(delegate (MainInputInteractable j)
					{
						RemoveDescription();
					}));
					challengeObj.gameObject.GetComponent<GBC.PixelSelectableCard>().CursorSelectStarted = (Action<MainInputInteractable>)Delegate.Combine(challengeObj.gameObject.GetComponent<GBC.PixelSelectableCard>().CursorSelectStarted, new Action<MainInputInteractable>(delegate (MainInputInteractable j)
					{
						string sfxName = !challengeObj.transform.GetChild(0).GetChild(0).Find("CardElements").Find("CheckMark").gameObject.activeSelf ? "crushBlip2" : "crunch_short#1";
						AudioController.Instance.PlaySound2D(sfxName, MixerGroup.None, 0.85f, 0f, null, null, null, null, false);
						SaveData.setChallengeActive(challenges[dex], !challengeObj.transform.GetChild(0).GetChild(0).Find("CardElements").Find("CheckMark").gameObject.activeSelf);
						challengeObj.GetComponent<GBC.PixelCardAnimationController>().PlayFaceDownAnimation(!challengeObj.transform.GetChild(0).GetChild(0).Find("CardElements").Find("CheckMark").gameObject.activeSelf, false);
						challengeObj.GetComponent<GBC.PixelCardAnimationController>().StartCoroutine(ShowEnabledChallenge(challengeObj));
					}));
				}

				GameObject bountyHitbox = Instantiate(FishingEvent);
				bountyHitbox.name = "ChallengeHitbox";
				bountyHitbox.transform.parent = deckChooseRoom.transform;
				bountyHitbox.transform.Find("CardPile").gameObject.SetActive(false);
				bountyHitbox.GetComponent<BoxCollider2D>().size = new Vector2(1f, 0.75f);
				bountyHitbox.transform.position = new Vector3(0.0445f, 10.98f, 0);
				bountyHitbox.AddComponent<infact2.GainCardEvent>();
				bountyHitbox.transform.Find("CollisionVolumeButton").GetChild(0).GetChild(0).GetChild(0).gameObject.GetComponent<SpriteRenderer>().sprite = Tools.getSprite("button_boon.png");

				//create the floor text

				GameObject Text = Instantiate(GameObject.Find("IntroSequence").transform.Find("BlockingTextBox").Find("Box").Find("PixelText").gameObject);
				Text.transform.parent = GameObject.Find("PixelCamera").transform;
				Text.name = "CameraText";
				Text.transform.Find("Text").gameObject.name = "FloorText";
				Text.transform.Find("FloorText").gameObject.GetComponent<UnityEngine.UI.Text>().text = "Floor " + SaveData.floor + "\nLives: " + SaveData.lives;
				Text.transform.Find("FloorText").gameObject.GetComponent<UnityEngine.UI.Text>().color = new Color(1, 1, 1, 1);
				Text.transform.localPosition = new Vector3(-0.8f, 1.25f, 10);
				Text.SetActive(false);
				if (SaveData.roomId.Contains("Dungeon"))
				{
					Text.SetActive(true);
				}
				functionsnstuff.setUiPos(Screen.currentResolution.width, Screen.currentResolution.height);
				//Destroy(CardEvent.GetComponent<GBC.PickupCardPileVolume>());
				//CardEvent.AddComponent<infact2.GainCardEvent>();
				functionsnstuff.PlayCorrectMusic();

			}

			public static IEnumerator DisplayChallengeDescription(string challengeName)
            {
				List<string> challengeData = new List<string> { "bounty", "Every 3 succesful battles, you gain a bounty star. With bounty stars, random bounty hunters will start to spawn in battles.", "boon", "You have only one boon slot.", "elite", "Every regular card battle is replaced with an elite battle.", "bridge", "Every battle now has bridge railing!", "nuzlocke", "Your starting deck has double the cards, but if a card dies in battle it is removed from your deck.\n(This does not apply to side deck cards.)", "nohammer", "You no longer have a hammer." };
				yield return Singleton<GBC.TextBox>.Instance.InitiateShowMessage(challengeData[challengeData.IndexOf(challengeName) + 1], GBC.TextBox.Style.Neutral, null, GBC.TextBox.ScreenPosition.ForceBottom);
				yield break;
			}

			public static void RemoveDescription()
			{
				Singleton<GBC.TextBox>.Instance.Hide();
				Singleton<GBC.TextBox>.Instance.PlaySound(false);
			}

			public static IEnumerator ShowEnabledChallenge(GameObject challengeObj)
            {
				yield return new WaitForSeconds(0.3f);
				challengeObj.transform.GetChild(0).GetChild(0).Find("CardElements").Find("CardBack").gameObject.SetActive(challengeObj.transform.GetChild(0).GetChild(0).Find("CardElements").Find("CheckMark").gameObject.activeSelf);
				challengeObj.transform.GetChild(0).GetChild(0).Find("CardElements").Find("CheckMark").gameObject.SetActive(!challengeObj.transform.GetChild(0).GetChild(0).Find("CardElements").Find("CheckMark").gameObject.activeSelf);
				yield break;
            }
			public static IEnumerator DisableChallengeUI()
			{
				GameObject challengeUI = GameObject.Find("ChallengeUI");
				AudioController.Instance.PlaySound2D("crunch_short#1", MixerGroup.None, 0.6f, 0f, null, null, null, null, false);
				Tween.LocalPosition(challengeUI.transform, new Vector3(0.5f, 3f, 0), 0.3f, 0f);
				yield return new WaitForSeconds(0.3f);
				GameObject.Find("IntroSequence").transform.Find("BlockingTextBox").gameObject.SetActive(true);
				Singleton<GBC.PlayerMovementController>.Instance.SetEnabled(true);
				PauseMenu.pausingDisabled = false;
				for (int i = 0; i < challengeUI.transform.childCount; i++)
                {
					challengeUI.transform.GetChild(i).gameObject.SetActive(false);
                }
				challengeUI.transform.Find("ExitButton").gameObject.GetComponent<BoxCollider2D>().enabled = true;
				challengeUI.transform.localPosition = new Vector3(0.5f, 0, 0);
				SaveManager.SaveToFile();
				yield break;
			}

			public static IEnumerator EnableChallengeUI()
			{

				GameObject challengeUI = GameObject.Find("ChallengeUI");
				GameObject.Find("IntroSequence").transform.Find("BlockingTextBox").gameObject.SetActive(false);
				challengeUI.transform.localPosition = new Vector3(0.5f, 3, 0);
				PauseMenu.pausingDisabled = true;
				Singleton<GBC.PlayerMovementController>.Instance.SetEnabled(false);
				for (int i = 1; i < challengeUI.transform.childCount; i++)
				{
					challengeUI.transform.GetChild(i).gameObject.SetActive(true);
				}
				Tween.LocalPosition(challengeUI.transform, new Vector3(0.5f, 0f, 0), 0.3f, 0f);
				yield return new WaitForSeconds(0.3f);
				Singleton<GBC.PlayerMovementController>.Instance.SetEnabled(false);
				PauseMenu.pausingDisabled = true;
				challengeUI.transform.Find("ExitButton").gameObject.GetComponent<BoxCollider2D>().enabled = true;
				challengeUI.transform.localPosition = new Vector3(0.5f, 0, 0);
				yield break;
			}

			public static void PlaceBarriers(bool update = false)
			{
				GameObject dungeon = GameObject.Find(SaveData.roomId);
				if (!update)
				{
					GameObject Barriers = Instantiate(new GameObject(), new Vector3(dungeon.transform.position.x, 50, dungeon.transform.position.z), Quaternion.Euler(0, 0, 0), dungeon.transform);
					Barriers.name = "Barriers";

					GameObject Barrier1 = Instantiate(new GameObject(), new Vector3(dungeon.transform.position.x, 49.2f + 0.35f * SaveData.nodesCompleted, dungeon.transform.position.z), Quaternion.Euler(0, 0, 0), Barriers.transform);
					Barrier1.AddComponent<BoxCollider2D>();
					Barrier1.GetComponent<BoxCollider2D>().size = new Vector2(100f, 0.01f);

					GameObject Barrier2 = Instantiate(new GameObject(), new Vector3(dungeon.transform.position.x, 48.9f + 0.35f * SaveData.nodesCompleted, dungeon.transform.position.z), Quaternion.Euler(0, 0, 0), Barriers.transform);
					Barrier2.AddComponent<BoxCollider2D>();
					Barrier2.GetComponent<BoxCollider2D>().size = new Vector2(100f, 0.01f);
				} else
				{
					SaveManager.saveFile.randomSeed -= UnityEngine.Random.RandomRangeInt(1, 15);
					Singleton<GBC.MultiDirectionalCharacterAnimator>.Instance.SetDirection(LookDirection.North);
					Singleton<GBC.MultiDirectionalCharacterAnimator>.Instance.enabled = false;
					Tween.Position(Singleton<GBC.PlayerMovementController>.Instance.gameObject.transform, new Vector3(Singleton<GBC.PlayerMovementController>.Instance.gameObject.transform.position.x, Singleton<GBC.PlayerMovementController>.Instance.gameObject.transform.position.y + 0.35f, Singleton<GBC.PlayerMovementController>.Instance.gameObject.transform.position.z), 0.6f, 0f);
					GameObject Barriers = GameObject.Find("Barriers");
					Barriers.transform.GetChild(0).transform.position = new Vector3(dungeon.transform.position.x, 49.2f + 0.35f * SaveData.nodesCompleted, dungeon.transform.position.z);
					Barriers.transform.GetChild(1).transform.position = new Vector3(dungeon.transform.position.x, 48.9f + 0.35f * SaveData.nodesCompleted, dungeon.transform.position.z);
					Singleton<GBC.MultiDirectionalCharacterAnimator>.Instance.enabled = true;
					SaveManager.saveFile.gbcData.overworldIndoorPosition = new Vector3(Singleton<GBC.PlayerMovementController>.Instance.gameObject.transform.position.x, Singleton<GBC.PlayerMovementController>.Instance.gameObject.transform.position.y + 0.35f, Singleton<GBC.PlayerMovementController>.Instance.gameObject.transform.position.z);
					SaveManager.SaveToFile();
				}

			}

			public static void createEyeFrames(GameObject creatureEyes, int offset = 0)
            {
				for (int i = 0; i < 22; i++)
				{
					if (i > 0 + offset && i < 6 + offset)
					{
						Sprite spriteCranberry = Tools.getSprite("eyes_0.png");
						if (i > 1 + offset && i < 5 + offset)
						{
							spriteCranberry = Tools.getSprite("eyes_1.png");
						}
						creatureEyes.GetComponent<AnimatingSprite>().frames[i] = spriteCranberry;
					}

				}
			} 
			public static void GenerateNodes()
			{
				Debug.Log(SaveData.isChallengeActive("elite"));
				GameObject dungeon = GameObject.Find(SaveData.roomId);
				bool boonOnLeft = true;
				if (UnityEngine.Random.RandomRangeInt(0, 100) > 50) { boonOnLeft = false; }
				string nodeLayout = "";
				if (GameObject.Find("CardpackEvent") is null)
                {
					GameObject CardEvent = Instantiate(GameObject.Find("Rarecardevent"));
					CardEvent.name = "CardpackEvent";
					CardEvent.transform.Find("CardPile").gameObject.GetComponent<SpriteRenderer>().sprite = Tools.getSprite("cardchoice.png");
					CardEvent.AddComponent<infact2.GainCardEvent>();
					CardEvent.transform.position = new Vector3(0.07f, 64f, 1.67f);
				}
				for (int i = 0; i < 7; i++)
				{
					float pos = 0.35f * i;
					if (i == 0 && SaveData.floor == 1)
					{
						GameObject cardChoice = Instantiate(GameObject.Find("CardpackEvent"));
						cardChoice.transform.position = new Vector3(dungeon.transform.position.x - 0.01f, 49f + pos, dungeon.transform.position.z);
						cardChoice.transform.parent = dungeon.transform;
						nodeLayout += "C/";
					}
					else if (i == 0 && SaveData.floor != 1)
					{
						GameObject cardChoice = Instantiate(GameObject.Find("Rarecardevent"));
						cardChoice.transform.position = new Vector3(dungeon.transform.position.x - 0.01f, 49f + pos, dungeon.transform.position.z);
						cardChoice.transform.parent = dungeon.transform;
						nodeLayout += "R/";
					} else if (i == 3)
                    {
						float offset = boonOnLeft ? 0.25f : -0.25f;
						if (SaveData.roomId == "WizardDungeon")
                        {
							offset = boonOnLeft ? 0.15f : -0.15f;
						}
						GameObject battle = Instantiate(GameObject.Find("EliteBattleEvent"));
						battle.transform.position = new Vector3(dungeon.transform.position.x - offset, 49f + pos, dungeon.transform.position.z);
						battle.transform.parent = dungeon.transform;
						GameObject cardChoice = GameObject.Find("CardpackEvent");
						string cardType = "C";
						if (UnityEngine.Random.RandomRangeInt(0, 100) > 60)
						{
							cardChoice = Instantiate(GameObject.Find("CardpackEvent"));
							cardType  = "C";
						}
						else
						{
							cardChoice = Instantiate(GameObject.Find("CardcostEvent"));
							cardType = "P";
						}
						nodeLayout += boonOnLeft ? "E" : cardType;
						nodeLayout += boonOnLeft ? "," + cardType + "/" : ",E/";
						cardChoice.transform.position = new Vector3(dungeon.transform.position.x + offset, 49f + pos, dungeon.transform.position.z);
						cardChoice.transform.parent = dungeon.transform;
					}
					else if (i == 4)
					{
						float offset = boonOnLeft ? 0.25f : -0.25f;
						if (SaveData.roomId == "WizardDungeon")
						{
							offset = boonOnLeft ? 0.15f : -0.15f;
						}
						List<int> counts = new List<int>();
						counts.Add(SaveManager.saveFile.gbcData.collection.cardIds.Count(x => x == "FieldMouse"));
						counts.Add(SaveManager.saveFile.gbcData.collection.cardIds.Count(x => x == "SentryBot"));
						counts.Add(SaveManager.saveFile.gbcData.collection.cardIds.Count(x => x == "BlueMage"));
						counts.Add(SaveManager.saveFile.gbcData.collection.cardIds.Count(x => x == "Gravedigger"));
						bool hasDupes = false;
						foreach(int count in counts)
                        {
							if (count >= 2)
                            {
								hasDupes = true;
                            }
                        }
						bool condition = hasDupes && SaveData.floor >= 5 && UnityEngine.Random.RandomRangeInt(0, 100) < 65;
						string battleT = SaveData.isChallengeActive("elite") ? "E" : "B";
						if (!condition)
						{
							GameObject battle = Instantiate(GameObject.Find("BoonEvent"));
							battle.transform.position = new Vector3(dungeon.transform.position.x - offset, 49f + pos, dungeon.transform.position.z);
							battle.transform.parent = dungeon.transform;
							nodeLayout += boonOnLeft ? "N" : battleT;
						} else
                        {
							GameObject myco = Instantiate(GameObject.Find("MycoEvent"));
							myco.transform.position = new Vector3(dungeon.transform.position.x - offset, 49f + pos, dungeon.transform.position.z);
							myco.transform.parent = dungeon.transform;
							nodeLayout += boonOnLeft ? "M" : battleT;
						}
						GameObject cardChoice = GameObject.Find("CardBattleEvent");
						if (!SaveData.isChallengeActive("elite"))
						{
							cardChoice = Instantiate(GameObject.Find("CardBattleEvent"));
						} else
                        {
							cardChoice = Instantiate(GameObject.Find("EliteBattleEvent"));
						}
						if (!condition)
						{
							nodeLayout += boonOnLeft ? "," + battleT + "/" : ",N/";
						} else { nodeLayout += boonOnLeft ? "," + battleT + "/" : ",M/"; }
						cardChoice.transform.position = new Vector3(dungeon.transform.position.x + offset, 49f + pos, dungeon.transform.position.z);
						cardChoice.transform.parent = dungeon.transform;
					}
					else if (i == 6)
					{
						GameObject cardChoice = Instantiate(GameObject.Find("CardBossEvent"));
						cardChoice.transform.position = new Vector3(dungeon.transform.position.x - 0.01f, 49f + pos, dungeon.transform.position.z);
						cardChoice.transform.parent = dungeon.transform;
						nodeLayout += "K/";
					} else if (i == 1)
					{
						if (SaveData.floor > 1)
						{
							GameObject cardChoice = Instantiate(GameObject.Find("ShopEvent"));
							cardChoice.transform.position = new Vector3(dungeon.transform.position.x - 0.01f, 49f + pos, dungeon.transform.position.z);
							cardChoice.transform.parent = dungeon.transform;
							nodeLayout += "S/";
							MakeShopPool(cardChoice);
						} else
                        {
							GameObject cardChoice = Instantiate(GameObject.Find("CardcostEvent"));
							nodeLayout += "P/";
							cardChoice.transform.position = new Vector3(dungeon.transform.position.x - 0.01f, 49f + pos, dungeon.transform.position.z);
							cardChoice.transform.parent = dungeon.transform;
						}
					}
					else if (i % 2 != 0)
					{
						GameObject cardChoice = GameObject.Find("CardpackEvent");
						if (UnityEngine.Random.RandomRangeInt(0, 100) > 60)
						{
							cardChoice = Instantiate(GameObject.Find("CardpackEvent"));
							nodeLayout += "C/";
						}
						else
						{
							cardChoice = Instantiate(GameObject.Find("CardcostEvent"));
							nodeLayout += "P/";
						}
						cardChoice.transform.position = new Vector3(dungeon.transform.position.x, 49f + pos, dungeon.transform.position.z);
						cardChoice.transform.parent = dungeon.transform;
					}
					else if (i % 2 == 0)
					{
						GameObject cardChoice = GameObject.Find("CardBattleEvent");
						if (!SaveData.isChallengeActive("elite"))
						{
							cardChoice = Instantiate(GameObject.Find("CardBattleEvent"));
						}
						else
						{
							cardChoice = Instantiate(GameObject.Find("EliteBattleEvent"));
						}
						cardChoice.transform.position = new Vector3(dungeon.transform.position.x, 49f + pos, dungeon.transform.position.z);
						cardChoice.transform.parent = dungeon.transform;
						string battlet = SaveData.isChallengeActive("elite") ? "E/" : "B/";
						nodeLayout += battlet;
					}
				}
				if (SaveData.roomId == "NatureDungeon")
                {
					GameObject cardChoice = Instantiate(GameObject.Find("FishingEvent"));
					cardChoice.transform.position = new Vector3(-1.9f, 49.7f, 0f);
					cardChoice.transform.parent = dungeon.transform;
				} else if (SaveData.roomId == "UndeadDungeon")
				{
					GameObject cardChoice = Instantiate(GameObject.Find("BoneLordEvent"));
					cardChoice.transform.position = new Vector3(20.608f, 50.77f, 0);
					cardChoice.transform.parent = dungeon.transform;
				}
				else if (SaveData.roomId == "TechDungeon")
				{
					GameObject cardChoice = Instantiate(GameObject.Find("VesselEvent"));
					cardChoice.transform.position = new Vector3(30.65f, 50.4f, 0);
					cardChoice.transform.parent = dungeon.transform;
				}
				else if (SaveData.roomId == "WizardDungeon")
				{
					List<string> spriteNames = new List<string> { "mox_emerald.png", "mox_ruby.png", "mox_sapphire.png" };
					GameObject.Find("PillarMox").GetComponent<SpriteRenderer>().sprite = Tools.getSprite(spriteNames.GetRandomItem());	
					GameObject cardChoice = Instantiate(GameObject.Find("MoxEvent"));
					cardChoice.transform.position = new Vector3(40.975f, 50.9f, 0);
					cardChoice.transform.parent = dungeon.transform;
				}
				SaveData.nodeLayout = nodeLayout;
			}

			public static void PlayCorrectMusic()
            {
				switch (getTemple(SaveData.roomId))
				{
					case CardTemple.Nature:
						AudioController.Instance.SetLoopAndPlay("gbc_temple_nature", 0, true, true);
						break;
					case CardTemple.Undead:
						AudioController.Instance.SetLoopAndPlay("gbc_temple_undead", 0, true, true);
						break;
					case CardTemple.Tech:
						AudioController.Instance.SetLoopAndPlay("gbc_temple_tech", 0, true, true);
						break;
					case CardTemple.Wizard:
						AudioController.Instance.SetLoopAndPlay("gbc_temple_wizard", 0, true, true);
						break;
				}
				AudioController.Instance.SetLoopVolumeImmediate(0.8f, 0);
			}

			public static void DecodeNodes()
			{
				GameObject dungeon = GameObject.Find(SaveData.roomId);
				string nodeLayout = SaveData.nodeLayout;
				string[] nodes = nodeLayout.Split('/');
				for (int i = 0; i < nodes.Length; i++)
				{
					float pos = 0.35f * i;
					if (nodes[i].Contains(','))
                    {
						string[] subNodes = nodes[i].Split(',');
						for (int j = 0; j < subNodes.Length; j++)
                        {
							float xOffset = 0.25f;
							if (SaveData.roomId == "WizardDungeon")
							{
								xOffset = 0.15f;
							}
							if (j < 1)
                            {
								xOffset = -xOffset;
                            }
							if (subNodes[j] == "E")
                            {
								GameObject battle = Instantiate(GameObject.Find("EliteBattleEvent"));
								battle.transform.position = new Vector3(dungeon.transform.position.x + xOffset, 49f + pos, dungeon.transform.position.z);
								battle.transform.parent = dungeon.transform;
							} else if (subNodes[j] == "C")
                            {
								GameObject cardChoice = Instantiate(GameObject.Find("CardpackEvent"));
								cardChoice.transform.position = new Vector3(dungeon.transform.position.x + xOffset, 49f + pos, dungeon.transform.position.z);
								cardChoice.transform.parent = dungeon.transform;
							}
							else if (subNodes[j] == "P")
							{
								GameObject cardChoice = Instantiate(GameObject.Find("CardcostEvent"));
								cardChoice.transform.position = new Vector3(dungeon.transform.position.x + xOffset, 49f + pos, dungeon.transform.position.z);
								cardChoice.transform.parent = dungeon.transform;
							} else if (subNodes[j] == "N")
                            {
								GameObject battle = Instantiate(GameObject.Find("BoonEvent"));
								battle.transform.position = new Vector3(dungeon.transform.position.x + xOffset, 49f + pos, dungeon.transform.position.z);
								battle.transform.parent = dungeon.transform;
							}
							else if (subNodes[j] == "M")
							{
								GameObject battle = Instantiate(GameObject.Find("MycoEvent"));
								battle.transform.position = new Vector3(dungeon.transform.position.x + xOffset, 49f + pos, dungeon.transform.position.z);
								battle.transform.parent = dungeon.transform;
							}
							else if (subNodes[j] == "B")
							{
								GameObject cardChoice = Instantiate(GameObject.Find("CardBattleEvent"));
								cardChoice.transform.position = new Vector3(dungeon.transform.position.x + xOffset, 49f + pos, dungeon.transform.position.z);
								cardChoice.transform.parent = dungeon.transform;
							}
						}
					} else if (nodes[i] == "C")
					{
						GameObject cardChoice = Instantiate(GameObject.Find("CardpackEvent"));
						cardChoice.transform.position = new Vector3(dungeon.transform.position.x - 0.01f, 49f + pos, dungeon.transform.position.z);
						cardChoice.transform.parent = dungeon.transform;
					}
					else if (nodes[i] == "R")
					{
						GameObject cardChoice = Instantiate(GameObject.Find("Rarecardevent"));
						cardChoice.transform.position = new Vector3(dungeon.transform.position.x - 0.01f, 49f + pos, dungeon.transform.position.z);
						cardChoice.transform.parent = dungeon.transform;
					}
					else if (nodes[i] == "P")
					{
						GameObject cardChoice = Instantiate(GameObject.Find("CardcostEvent"));
						cardChoice.transform.position = new Vector3(dungeon.transform.position.x - 0.01f, 49f + pos, dungeon.transform.position.z);
						cardChoice.transform.parent = dungeon.transform;
					}
					else if (nodes[i] == "B")
					{
						GameObject cardChoice = Instantiate(GameObject.Find("CardBattleEvent"));
						cardChoice.transform.position = new Vector3(dungeon.transform.position.x - 0.01f, 49f + pos, dungeon.transform.position.z);
						cardChoice.transform.parent = dungeon.transform;
					}
					else if (nodes[i] == "E")
					{
						GameObject battle = Instantiate(GameObject.Find("EliteBattleEvent"));
						battle.transform.position = new Vector3(dungeon.transform.position.x - 0.01f, 49f + pos, dungeon.transform.position.z);
						battle.transform.parent = dungeon.transform;
					}
					else if (nodes[i] == "K")
					{
						GameObject cardChoice = Instantiate(GameObject.Find("CardBossEvent"));
						cardChoice.transform.position = new Vector3(dungeon.transform.position.x - 0.01f, 49f + pos, dungeon.transform.position.z);
						cardChoice.transform.parent = dungeon.transform;
					} else if (nodes[i] == "S")
					{
						GameObject cardChoice = Instantiate(GameObject.Find("ShopEvent"));
						cardChoice.transform.position = new Vector3(dungeon.transform.position.x - 0.01f, 49f + pos, dungeon.transform.position.z);
						cardChoice.transform.parent = dungeon.transform;
						MakeShopPool(cardChoice);
					}
				}

				if (SaveData.roomId == "NatureDungeon" && !SaveData.doneAreaSecret)
				{
					GameObject cardChoice = Instantiate(GameObject.Find("FishingEvent"));
					cardChoice.transform.position = new Vector3(-1.9f, 49.7f, 0f);
					cardChoice.transform.parent = dungeon.transform;
				}
				else if (SaveData.roomId == "UndeadDungeon" && !SaveData.doneAreaSecret)
				{
					GameObject cardChoice = Instantiate(GameObject.Find("BoneLordEvent"));
					cardChoice.transform.position = new Vector3(20.608f, 50.77f, 0);
					cardChoice.transform.parent = dungeon.transform;
				}
				else if (SaveData.roomId == "TechDungeon" && !SaveData.doneAreaSecret)
				{
					GameObject cardChoice = Instantiate(GameObject.Find("VesselEvent"));
					cardChoice.transform.position = new Vector3(30.65f, 50.4f, 0);
					cardChoice.transform.parent = dungeon.transform;
				}
				else if (SaveData.roomId == "WizardDungeon" && !SaveData.doneAreaSecret)
				{
					List<string> spriteNames = new List<string> { "mox_emerald.png", "mox_ruby.png", "mox_sapphire.png" };
					string mox = spriteNames.GetRandomItem();
					GameObject.Find("PillarMox").GetComponent<SpriteRenderer>().sprite = Tools.getSprite(mox);
					GameObject.Find("PillarMox").GetComponent<SpriteRenderer>().sprite.texture.name = mox;
					GameObject cardChoice = Instantiate(GameObject.Find("MoxEvent"));
					cardChoice.transform.position = new Vector3(40.975f, 50.9f, 0);
					cardChoice.transform.parent = dungeon.transform;
				}
			}

			public static void MakeShopPool(GameObject shop)
			{
				List<CardInfo> pixelCards = CardLoader.GetPixelCards();
				pixelCards.RemoveAll((CardInfo x) => !x.metaCategories.Contains(CardMetaCategory.GBCPack));
				List<CardInfo> cards = pixelCards.FindAll((CardInfo x) => !x.metaCategories.Contains(CardMetaCategory.Rare) && x.temple == infact2.Plugin.functionsnstuff.getTemple(SaveData.roomId));
				List<CardInfo> pool1 = new List<CardInfo>();
				List<CardInfo> pool2 = new List<CardInfo>();
				List<CardInfo> pool3 = new List<CardInfo>();
				int seedCopy = SaveManager.saveFile.randomSeed;
				for (int j = 0; j < 6; j++)
				{
					CardInfo card = cards[SeededRandom.Range(0, cards.Count, seedCopy)];
					pool1.Add(card);
					seedCopy *= 2;
				}
				for (int a = 0; a < 6; a++)
				{
					CardInfo card = cards[SeededRandom.Range(0, cards.Count, seedCopy)];
					pool2.Add(card);
					seedCopy *= 5;
				}
				for (int h = 0; h < 6; h++)
				{
					CardInfo card = cards[SeededRandom.Range(0, cards.Count, seedCopy)];
					pool3.Add(card);
					seedCopy *= 3;
				}
				shop.AddComponent<GBC.ShopNPC>();
				shop.GetComponent<GBC.ShopNPC>().card1Inventory = pool1;
				shop.GetComponent<GBC.ShopNPC>().card3Inventory = pool2;
				shop.GetComponent<GBC.ShopNPC>().card4Inventory = pool3;
				List<CardInfo> starters = new List<CardInfo>();
				shop.GetComponent<GBC.ShopNPC>().temple = infact2.Plugin.functionsnstuff.getTemple(SaveData.roomId);
				switch (infact2.Plugin.functionsnstuff.getTemple(SaveData.roomId))
				{
					case CardTemple.Nature:
						starters.Add(CardLoader.GetCardByName("Squirrel"));
						starters.Add(CardLoader.GetCardByName("Squirrel"));
						break;
					case CardTemple.Undead:
						starters.Add(CardLoader.GetCardByName("Skeleton"));
						starters.Add(CardLoader.GetCardByName("Skeleton"));
						break;
					case CardTemple.Tech:
						starters.Add(CardLoader.GetCardByName("LeapBot"));
						starters.Add(CardLoader.GetCardByName("LeapBot"));
						break;
					case CardTemple.Wizard:
						starters.Add(CardLoader.GetCardByName("MoxEmerald"));
						starters.Add(CardLoader.GetCardByName("MoxRuby"));
						starters.Add(CardLoader.GetCardByName("MoxSapphire"));
						break;
				}
				shop.GetComponent<GBC.ShopNPC>().card2Inventory = starters;
				shop.GetComponent<GBC.ShopNPC>().shopUI = Singleton<GBC.ShopUI>.Instance;
			}

			public static IEnumerator TransitionToGame()
			{
				PauseMenu.pausingDisabled = true;
				yield return new WaitForSeconds(1.3f);
				GameObject Tablet2 = GameObject.Find("Tablet2s");
				for (int i = 0; i < Tablet2.transform.childCount; i++)
				{
					Tablet2.transform.GetChild(i).Find("Glow").gameObject.SetActive(true);
					yield return new WaitForSeconds(0.25f);
				}
				yield return new WaitForSeconds(0.5f);
				SaveManager.saveFile.gbcData.currency = 0;
				Singleton<GBC.CameraEffects>.Instance.FadeIn();
				GameObject.Find("ScreenFade").GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1);
				string dungeonName = "NatureDungeon";
				if (SaveManager.saveFile.gbcData.collection.cardIds[0] == "MoxSapphire")
                {
					dungeonName = "WizardDungeon";
				}
				else if (SaveManager.saveFile.gbcData.collection.cardIds[0] == "Skeleton")
				{
					dungeonName = "UndeadDungeon";
				} else if (SaveManager.saveFile.gbcData.collection.cardIds[0] == "Squirrel")
				{
					dungeonName = "NatureDungeon";
				}
				else if (SaveManager.saveFile.gbcData.collection.cardIds[0] == "LeapBot")
				{
					dungeonName = "TechDungeon";
				}
				for (int i = 0; i < 2; i++)
				{
					SaveManager.saveFile.gbcData.collection.AddCard(CardLoader.GetCardByName("MoxEmerald"));
					SaveManager.saveFile.gbcData.collection.AddCard(CardLoader.GetCardByName("MoxSapphire"));
					SaveManager.saveFile.gbcData.collection.AddCard(CardLoader.GetCardByName("MoxRuby"));
					SaveManager.saveFile.gbcData.collection.AddCard(CardLoader.GetCardByName("Squirrel"));
					SaveManager.saveFile.gbcData.collection.AddCard(CardLoader.GetCardByName("Skeleton"));
					SaveManager.saveFile.gbcData.collection.AddCard(CardLoader.GetCardByName("LeapBot"));
				}
				Singleton<GBC.CameraController>.Instance.SetRoom(GameObject.Find(dungeonName).transform.position, GameObject.Find(dungeonName).name);
				Singleton<GBC.PlayerMovementController>.Instance.transform.position = GameObject.Find(dungeonName).transform.Find("PlayerPositionMarker").position;
				Singleton<GBC.PlayerMovementController>.Instance.SetEnabled(true);
				SaveData.roomId = dungeonName;
				SaveData.runSeed = SaveManager.saveFile.randomSeed * 24 - SaveManager.saveFile.playTime * 2;
				SaveData.floor = 1;
				SaveData.lives = 2;
				SaveData.bountyStars = 0;
				MakeBluep.GenerateSaveHunters();
				SaveData.doneAreaSecret = false;
				SaveManager.SaveToFile();
				GameObject.Find("PixelCamera").transform.Find("CameraText").gameObject.SetActive(true);
				GameObject.Find("FloorText").GetComponent<UnityEngine.UI.Text>().text = "Floor " + SaveData.floor + "\nLives: " + SaveData.lives;
				PlayCorrectMusic();
				SaveData.nodesCompleted = 0;
				Debug.Log(SaveData.roomId);
				GenerateNodes();
				PlaceBarriers();
				SaveData.boon1 = "None";
				SaveData.boon2 = "None";
				SaveData.boon3 = "None";
				PauseMenu.pausingDisabled = false;
				CustomCoroutine.WaitThenExecute(0f, delegate
				{
					AudioController.Instance.PlaySound2D("player_falling", MixerGroup.None, 0.3f, 0f, null, null, null, null, false);
				}, false);
				yield return Singleton<GBC.PlayerMovementController>.Instance.Anim.DropAnimation(5f);
				yield break;
			}

			public static CardTemple getTemple(string ttre)
			{
				CardTemple temple = CardTemple.Nature;
				switch (ttre)
				{
					case "Beasts":
					case "NatureDungeon":
						temple = CardTemple.Nature;
						break;
					case "Dead":
					case "UndeadDungeon":
						temple = CardTemple.Undead;
						break;
					case "Magick":
					case "WizardDungeon":
						temple = CardTemple.Wizard;
						break;
					case "Technology":
					case "TechDungeon":
						temple = CardTemple.Tech;
						break;
				}
				return temple;
			}

			public static string getTempleIndex(int templeId)
            {
				string temple = "nature";
				switch (templeId)
				{
					case 2:
						temple = "tech";
						break;
					case 1:
						temple = "undead";
						break;
					case 3:
						temple = "wizard";
						break;
				}
				return temple;
			}

			public static IEnumerator PlayBoonAnim(string boonName)
            {
				yield return new WaitForSeconds(0f);
				GameObject boon = GameObject.Find("BoonCard");
				boon.layer = 31;
				boon.GetComponent<GBC.PixelCardAnimationController>().SetFaceDown(false, true);
				boon.GetComponent<GBC.PixelPlayableCard>().SetInfo(CardLoader.GetCardByName(boonName));
				boon.name = "BoonCard";
				Tween.LocalPosition(boon.transform, new Vector3(-1.445f, 0.025f, 0), 1.5f, 0);
				yield return new WaitForSeconds(2f);
				AudioController.Instance.PlaySound2D("chipDelay_3", MixerGroup.None, 0.5f, 0f, null, null, null, null, false);
				boon.GetComponent<GBC.PixelCardAnimationController>().SetFaceDown(true, false);
				Tween.LocalPosition(boon.transform, new Vector3(-1.445f, 10f, 0), 0.75f, 0.9f);
				yield return new WaitForSeconds(1.5f);
				boon.GetComponent<GBC.PixelCardAnimationController>().SetFaceDown(false, true);
				yield break;
            }
			public static List<string> returnBoonsAsList()
            {
				List<string> boons = new List<string>();
				if (SaveData.boon1 is not null && SaveData.boon1 != "None")
				{
					boons.Add(SaveData.boon1);
				}
				if (SaveData.boon2 is not null && SaveData.boon2 != "None")
				{
					boons.Add(SaveData.boon2);
				}
				if (SaveData.boon3 is not null && SaveData.boon3 != "None")
				{
					boons.Add(SaveData.boon3);
				}
				return boons;
            }


			public static void setUiPos(int w, int h)
            {
				if (w == 720 && h == 480)
				{
					GameObject.Find("PixelCamera").transform.Find("CameraText").transform.localPosition = new Vector3(-0.25f, 1, 10);

				} else if (w == 1176 && h == 664 || w == 1280 && h == 728 || w == 1360 && h == 768 || w == 1366 && h == 768 || w == 1920 && h == 1080)
				{
					GameObject.Find("PixelCamera").transform.Find("CameraText").transform.localPosition = new Vector3(-0.8f, 1.25f, 10);
				} else if (w == 1280 && h == 720)
                {
					GameObject.Find("PixelCamera").transform.Find("CameraText").transform.localPosition = new Vector3(-0.5f, 1, 10);
				}
				else if (w == 1280 && h == 800)
				{
					GameObject.Find("PixelCamera").transform.Find("CameraText").transform.localPosition = new Vector3(-0.5f, 1.2f, 10);
				}
				else if (w == 1600 && h == 900 || w == 1600 && h == 1024 || w == 1680 && h == 1050 || w == 1680 && h == 1080)
				{
					GameObject.Find("PixelCamera").transform.Find("CameraText").transform.localPosition = new Vector3(-0.4f, 1f, 10);
				}
			}

		}
		//ApplyVideoOptions

		[HarmonyPatch(typeof(GBC.OptionsUI), "ApplyVideoOptions")]
		public class fixUiPos
		{
			public static void Prefix(ref GBC.OptionsUI __instance)
			{ 
				if (__instance.videoOptionsChanged && SceneLoader.ActiveSceneName == "GBC_Starting_Island") {
					functionsnstuff.setUiPos(__instance.availableResolutions[__instance.resolutionField.Value].width, __instance.availableResolutions[__instance.resolutionField.Value].height);
				}
			}
		}
            [HarmonyPatch(typeof(GBC.CameraController), "SetRoom")]
		public class fixSetRoom
		{
			public static bool Prefix(ref GBC.CameraController __instance, ref Vector2 roomPosition, ref string roomId)
			{
				__instance.transform.position = new Vector3(roomPosition.x, roomPosition.y, __instance.transform.position.z);
				if (SceneLoader.ActiveSceneName != "GBC_Starting_Island")
				{
					__instance.GetTempleSaveData().cameraPosition = roomPosition;
					__instance.GetTempleSaveData().roomId = roomId;
				}
				else
				{
					if (roomId == "DeckChooseRoom")
					{
						SaveData.challenges = "0;0;0;0;0;0";
						if (SaveData.highscore > 0)
						{
							GameObject.Find("PixelCamera").transform.Find("CameraText").gameObject.SetActive(true);
							GameObject.Find("FloorText").GetComponent<UnityEngine.UI.Text>().text = "High Score: " + SaveData.highscore;
						}
						GameObject.Find("IntroSequence").transform.Find("BlockingTextBox").gameObject.SetActive(true);
						GameObject.Find("IntroSequence").transform.Find("BlockingTextBox").position = new Vector3(0, 9f, 0);
						GameObject.Find("IntroSequence").transform.Find("BlockingTextBox").gameObject.GetComponentInChildren<UnityEngine.UI.Text>().text = "SELECT A DECK TO USE.\nTHIS WILL OVERRIDE YOUR CURRENT DECK!";
						GameObject.Find("IntroSequence").transform.Find("BlockingTextBox").gameObject.GetComponentInChildren<BoxCollider2D>().size = new Vector2(0, 0);
						GameObject Tablet2 = GameObject.Find("Tablet2s");
						for (int i = 0; i < Tablet2.transform.childCount; i++)
						{
							Tablet2.transform.GetChild(i).Find("DeckPileVolume").gameObject.SetActive(true);
						}
					}
					else if (roomId == "Room")
					{
						GameObject.Find("PixelCamera").transform.Find("CameraText").gameObject.SetActive(false);
						GameObject.Find("IntroSequence").transform.Find("BlockingTextBox").gameObject.SetActive(false);
						GameObject Tablet2 = GameObject.Find("Tablet2s");
						for (int i = 0; i < Tablet2.transform.childCount; i++)
						{
							Tablet2.transform.GetChild(i).Find("DeckPileVolume").gameObject.SetActive(false);
						}
					}
					SaveData.roomId = roomId;
					SaveData.cameraX = roomPosition.x;
					SaveData.cameraY = roomPosition.y;
				}
				return false;
			}
		}




		[HarmonyPatch(typeof(GBC.GBCIntroScene), "Start")]
		public class IntroRoomId
		{
			public static void Prefix()
			{
				SaveData.roomId = "none";
			}
		}

		[HarmonyPatch(typeof(GBC.CameraController), "Start")]
		public class FixCameraStart
		{
			public static void Prefix(ref GBC.CameraController __instance)
			{
				if (SaveData.roomId != "none" && SceneLoader.ActiveSceneName == "GBC_Starting_Island")
				{
					__instance.SetRoom(new Vector2(SaveData.cameraX, SaveData.cameraY), SaveData.roomId);
					if (SaveData.roomId.Contains("Dungeon"))
					{
						functionsnstuff.DecodeNodes();
						functionsnstuff.PlaceBarriers();
					}
				}
			}
		}

		[HarmonyPatch(typeof(GBC.ShopNPC), "InventoryState", MethodType.Getter)]
		public class GetShopInventory
		{
			public static void Prefix(ref GBC.ShopNPC __instance, ref GBC.TempleSaveData.ShopInventoryState __result)
			{
				CardTemple temple = __instance.temple;
				if (SaveData.roomId != "none" && SceneLoader.ActiveSceneName == "GBC_Starting_Island")
				{
					temple = functionsnstuff.getTemple(SaveData.roomId);
				}
				__result = SaveManager.saveFile.gbcData.GetTempleData(temple).shopState;

			}
		}

		[HarmonyPatch(typeof(GBC.PackOpeningUI), "AssignInfoToCards")]
		public class RemoveRareFromCardPack
		{
			public static bool Prefix(ref GBC.PackOpeningUI __instance, CardTemple packType)
			{
				if (SceneLoader.ActiveSceneName != "GBC_Starting_Island")
				{
					return true;
				}
				List<CardInfo> pixelCards = CardLoader.GetPixelCards();
				pixelCards.RemoveAll((CardInfo x) => !x.metaCategories.Contains(CardMetaCategory.GBCPack));
				List<CardInfo> rareOfTemple = pixelCards.FindAll((CardInfo x) => x.metaCategories.Contains(CardMetaCategory.Rare) && x.temple == packType);
				List<CardInfo> otherTemple = pixelCards.FindAll((CardInfo x) => !x.metaCategories.Contains(CardMetaCategory.Rare) && x.temple != packType);
				List<CardInfo> commonOfTemple = pixelCards.FindAll((CardInfo x) => !x.metaCategories.Contains(CardMetaCategory.Rare) && x.temple == packType);
				commonOfTemple.RemoveAll((CardInfo x) => x.metaCategories.Contains(ExcludeFromAct2Endless));
				int currentRandomSeed = SaveManager.SaveFile.GetCurrentRandomSeed();
				for (int i = 0; i < __instance.cards.Count; i++)
				{
					CardInfo info;
					if (i == 1)
					{
						if (!SaveData.roomId.Contains("Dungeon") || SaveData.floor != 1 && SaveData.nodesCompleted == 0)
						{
							info = rareOfTemple[SeededRandom.Range(0, rareOfTemple.Count, currentRandomSeed++)];
						}
						else
						{
							info = commonOfTemple[SeededRandom.Range(0, commonOfTemple.Count, currentRandomSeed++)];
						}
					}
					else if (i < 3)
					{
						if (SaveData.floor != 1 && SaveData.nodesCompleted == 0)
						{
							info = rareOfTemple[SeededRandom.Range(0, rareOfTemple.Count, currentRandomSeed++)];
						}
						else
						{
							info = commonOfTemple[SeededRandom.Range(0, commonOfTemple.Count, currentRandomSeed++)];
						}
					}
					else
					{
						info = otherTemple[SeededRandom.Range(0, otherTemple.Count, currentRandomSeed++)];
					}
					__instance.cards[i].SetInfo(info);
				}

				return false;
			}
		}

		[HarmonyPatch(typeof(GBC.ShopUI), "OnHideEnd")]
		public class ShopExitFix
		{
			public static void Prefix()
            {
				if (SceneLoader.ActiveSceneName == "GBC_Starting_Island")
                {
					Singleton<GBC.PlayerMovementController>.Instance.SetEnabled(true);
					SaveManager.SaveToFile(true);
					SaveData.nodesCompleted += 1;
					functionsnstuff.PlaceBarriers(true);
				}
            }
		}
	

        [HarmonyPatch(typeof(GBC.PackOpeningUI), "OpenPack")]
		public class FixThatLittleTextBlurbThatSaysRare
		{
			public static void Prefix(ref GBC.PackOpeningUI __instance, ref GBC.PackOpeningUI __state)
			{
				__state = __instance;
			}

			public static IEnumerator Postfix(IEnumerator emuemuear, GBC.PackOpeningUI __state, CardTemple packType)
			{
				__state.Show();
				__state.currentPackType = packType;
				__state.panelAnim.transform.localPosition = Vector2.zero;
				__state.previewPanel.gameObject.SetActive(false);
				__state.panelAnim.gameObject.SetActive(true);
				__state.exitButton.gameObject.SetActive(false);
				__state.rareCardElements.SetActive(false);
				__state.SetPackSprites(packType);
				__state.cards.ForEach(delegate (GBC.PixelSelectableCard x)
				{
					x.SetFaceDown(false, true);
					x.SetNewBadgeShown(false);
				});
				__state.AssignInfoToCards(packType);
				yield return new WaitForSeconds(0.25f);
				__state.panelAnim.Play("enter", 0, 0f);
				yield return new WaitForSeconds(0.5f);
				yield return Singleton<GBC.TextBox>.Instance.ShowUntilInput(string.Format(Localization.Translate("You received a {0} card pack!"), Localization.Translate(GBC.PackOpeningUI.GetPackName(packType))), (GBC.TextBox.Style)packType, null, GBC.TextBox.ScreenPosition.ForceBottom, 0f, true, false, null, true, Emotion.Neutral);
				PauseMenu.pausingDisabled = true;
				AudioController.Instance.PlaySound2D("gbc_pack_open", MixerGroup.None, 1f, 0f, null, null, null, null, false);
				__state.panelAnim.Play("jitter", 0, 0f);
				yield return new WaitForSeconds(0.4f);
				__state.panelAnim.Play("open", 0, 0f);
				AudioController.Instance.PlaySound2D("toneless_sharp", MixerGroup.None, 0.5f, 0f, new AudioParams.Pitch(0.5f), null, null, null, false);
				yield return new WaitForSeconds(__state.openPackClip.length);
				Tween.LocalPosition(__state.panelAnim.transform, new Vector2(-0.567f, 0f), 0.1f, 0f, null, Tween.LoopType.None, null, null, true);
				yield return new WaitForSeconds(0.1f);
				__state.previewPanel.gameObject.SetActive(true);
				__state.OnCardCursorEntered(__state.cards[0]);
				__state.SetCardInteractionEnabled(true);
				__state.exitButton.gameObject.SetActive(true);
				using (List<GBC.PixelSelectableCard>.Enumerator enumerator = __state.cards.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						GBC.PixelSelectableCard card = enumerator.Current;
						if (!SaveManager.saveFile.gbcData.collection.Cards.Exists((CardInfo x) => x.name == card.Info.name))
						{
							card.SetNewBadgeShown(true);
						}
					}
				}
				yield break;
			}
		}


		//BOON DATA BABYYY

		private void AddBoons()
		{

			CardInfo testBoon = CardManager.New("infact2", "infact2_boon_clover", "Boon of Luck", 0, 0, "Start each battle with a clover.")
					.AddMetaCategories(BoonsPool)
					.SetPixelPortrait(Tools.getSprite("cloverboon.png"))
					.AddTraits(Trait.LikesHoney)
					.SetBloodCost(0);
			CardInfo fleshBoon = CardManager.New("infact2", "infact2_boon_flesh", "Boon of Flesh", 0, 0, "Mox may be sacrificed.")
				.AddMetaCategories(BoonsPool)
				.AddAppearances(CardAppearanceBehaviour.Appearance.TerrainLayout)
				.SetPixelPortrait(Tools.getSprite("fleshboon.png"))
				.AddTraits(Trait.LikesHoney)
				.SetBloodCost(0);

			CardInfo voltageBoon = CardManager.New("infact2", "infact2_boon_voltage", "Boon of Voltage", 0, 0, "When you sacrifice a beast card, you increase your max energy and gain an energy cell.")
				.AddAppearances(CardAppearanceBehaviour.Appearance.TerrainLayout)
				.SetPixelPortrait(Tools.getSprite("voltageboon.png"))
				.AddTraits(Trait.LikesHoney)
				.AddMetaCategories(BoonsPool)
				.SetBloodCost(0);

			CardInfo reincarnationBoon = CardManager.New("infact2", "infact2_boon_reincarnation", "Boon of Reincarnation", 0, 0, "When a Skeleton is sacrificed, draw a squirrel.")
				.AddAppearances(CardAppearanceBehaviour.Appearance.TerrainLayout)
				.SetPixelPortrait(Tools.getSprite("reincarnationboon.png"))
				.AddTraits(Trait.LikesHoney)
				.AddMetaCategories(BoonsPool)
				.SetBloodCost(0);

			CardInfo necromancyBoon = CardManager.New("infact2", "infact2_boon_necromancy", "Boon of Necromancy", 0, 0, "When a Skeleton dies, a random mox is placed in it's place.")
				.AddAppearances(CardAppearanceBehaviour.Appearance.TerrainLayout)
				.SetPixelPortrait(Tools.getSprite("necromancyboon.png"))
				.AddTraits(Trait.LikesHoney)
				.AddMetaCategories(BoonsPool)
				.SetBloodCost(0);

			CardInfo scrapBoon = CardManager.New("infact2", "infact2_boon_scrap", "Boon of Scrap", 0, 0, "Whenever you reach full energy, you gain a bone.")
				.AddAppearances(CardAppearanceBehaviour.Appearance.TerrainLayout)
				.SetPixelPortrait(Tools.getSprite("scrapboon.png"))
				.AddMetaCategories(BoonsPool)
				.AddTraits(Trait.LikesHoney)
				.SetBloodCost(0);

			CardInfo constructBoon = CardManager.New("infact2", "infact2_boon_construct", "Boon of Construct", 0, 0, "Any tech card that costs more than 2 energy will drop a random mox on death.")
				.AddAppearances(CardAppearanceBehaviour.Appearance.TerrainLayout)
				.SetPixelPortrait(Tools.getSprite("constructboon.png"))
				.AddMetaCategories(BoonsPool)
				.AddTraits(Trait.LikesHoney)
				.SetBloodCost(0);

			CardInfo savinggraceBoon = CardManager.New("infact2", "infact2_boon_savinggrace", "Boon of The Saving Grace", 0, 0, "Whenever you would recieve fatal damage, the scales are only tipped down to 4.\nONCE PER BATTLE")
				.AddAppearances(CardAppearanceBehaviour.Appearance.TerrainLayout)
				.AddMetaCategories(BoonsPool)
				.SetPixelPortrait(Tools.getSprite("savinggraceboon.png"))
				.AddTraits(Trait.LikesHoney)
				.SetBloodCost(0);

			CardInfo powerBoon = CardManager.New("infact2", "infact2_boon_goat", "Boon of Goat's Blood", 0, 0, "Start each battle with a black goat on the board.")
				.AddAppearances(CardAppearanceBehaviour.Appearance.TerrainLayout)
				.AddMetaCategories(BoonsPool)
				.SetPixelPortrait(Tools.getSprite("goatboon.png"))
				.AddTraits(Trait.LikesHoney)
				.SetBloodCost(0);

			CardInfo boneBoon = CardManager.New("infact2", "infact2_boon_bone", "Boon of The Bone Lord", 0, 0, "Start each battle with +3 bones.")
				.AddMetaCategories(BoonsPool)
				.AddAppearances(CardAppearanceBehaviour.Appearance.TerrainLayout)
				.SetPixelPortrait(Tools.getSprite("boneboon.png"))
				.AddTraits(Trait.LikesHoney)
				.SetBloodCost(0);

			CardInfo ambidextrousBoon = CardManager.New("infact2", "infact2_boon_ambidextrous", "Boon of The Ambidextrous", 0, 0, "Draw 2 cards each turn instead of 1, but it will only function if you have less than 20 cards in your deck while battling.")
				.AddMetaCategories(BoonsPool)
				.AddAppearances(CardAppearanceBehaviour.Appearance.TerrainLayout)
				.SetPixelPortrait(Tools.getSprite("ambidextrousboon.png"))
				.AddTraits(Trait.LikesHoney)
				.SetBloodCost(0);

			CardInfo forestBoon = CardManager.New("infact2", "infact2_boon_forest", "Boon of The Forest", 0, 0, "Start each battle with 2 firs in your hand.")
				.AddMetaCategories(BoonsPool)
				.AddAppearances(CardAppearanceBehaviour.Appearance.TerrainLayout)
				.SetPixelPortrait(Tools.getSprite("forestboon.png"))
				.AddTraits(Trait.LikesHoney)
				.SetBloodCost(0);

			CardInfo fossilBoon = CardManager.New("infact2", "infact2_boon_fossil", "Boon of Fossils", 0, 0, "Terrain cards yield 3 bones on death instead of 1.")
				.AddMetaCategories(BoonsPool)
				.AddAppearances(CardAppearanceBehaviour.Appearance.TerrainLayout)
				.SetPixelPortrait(Tools.getSprite("fossilboon.png"))
				.AddTraits(Trait.LikesHoney)
				.SetBloodCost(0);

			CardInfo marrowBoon = CardManager.New("infact2", "infact2_boon_marrow", "Boon of Marrow", 0, 0, "Sacrificing undead cards yields 2 bones instead of 1.")
				.AddMetaCategories(BoonsPool)
				.AddAppearances(CardAppearanceBehaviour.Appearance.TerrainLayout)
				.SetPixelPortrait(Tools.getSprite("marrowboon.png"))
				.AddTraits(Trait.LikesHoney)
				.SetBloodCost(0);

			CardInfo bloodstoneBoon = CardManager.New("infact2", "infact2_boon_bloodstone", "Boon of Bloodstone", 0, 0, "You may sacrifice terrain cards.")
				.AddMetaCategories(BoonsPool)
				.AddAppearances(CardAppearanceBehaviour.Appearance.TerrainLayout)
				.SetPixelPortrait(Tools.getSprite("bloodstoneboon.png"))
				.AddTraits(Trait.LikesHoney)
				.SetBloodCost(0);

			CardInfo ouroborosBoon = CardManager.New("infact2", "infact2_boon_ouroboros", "Boon of Ouroboros", 0, 0, "You get a different effect depending on the region:\nBeast Dungeon:+2 Starting bones each battle.\nDeath Dungeon:+1 Starting Energy each battle\nTech Dungeon:Start the game with two random mox in your hand.\nMagick Dungeon:Start the game with 2 Squirrels in your hand.")
			.AddMetaCategories(BoonsPool)
			.AddAppearances(CardAppearanceBehaviour.Appearance.TerrainLayout)
			.SetPixelPortrait(Tools.getSprite("ouroborosboon.png"))
			.AddTraits(Trait.LikesHoney)
			.SetBloodCost(0);
			/*
			CardInfo photographerBoon = CardManager.New("infact2", "infact2_boon_photographer", "Boon of The Photographer", 0, 0, "For every 3 cards that die, you draw an amalgam of their stats.")
			.AddMetaCategories(BoonsPool)
			.AddAppearances(CardAppearanceBehaviour.Appearance.TerrainLayout)
			.SetPixelPortrait(Tools.getSprite("photographerboon.png"))
			.AddTraits(Trait.LikesHoney)
			.SetBloodCost(0);

			/*CardInfo mycoBoon = CardManager.New("infact2", "infact2_boon_myco", "Boon of The Mycologists", 0, 0, "If you have two of the same cards on the board, they each get +1 to each stat.")
			.AddMetaCategories(BoonsPool)
			.AddAppearances(CardAppearanceBehaviour.Appearance.TerrainLayout)
			.SetPixelPortrait(Tools.getSprite("mycoboon.png"))
			.AddTraits(Trait.LikesHoney)
			.SetBloodCost(0);
			*/
			CardInfo customCard = CardManager.BaseGameCards.CardByName("Skeleton");
			customCard.AddSpecialAbilities(Reincarnation);

			foreach(CardInfo card in CardManager.BaseGameCards)
            {
				if (card.temple == CardTemple.Nature && card.HasCardMetaCategory(CardMetaCategory.GBCPlayable) && card.name != "Squirrel" || card.name == "Goat" || card.name == "Rabbit")
                {
					CardInfo customCard2 = CardManager.BaseGameCards.CardByName(card.name);
					customCard2.AddSpecialAbilities(Voltage);
				} else if (card.temple == CardTemple.Tech && card.HasCardMetaCategory(CardMetaCategory.GBCPlayable))
				{
					CardInfo customCard2 = CardManager.BaseGameCards.CardByName(card.name);
					customCard2.AddSpecialAbilities(Construct);
				}

			}

		}

		public class ReincarantionBoon : SpecialCardBehaviour
		{
			public override bool RespondsToSacrifice()
			{
				return true;
			}

			public override IEnumerator OnSacrifice()
			{
				List<string> boons = functionsnstuff.returnBoonsAsList();
				if (boons.Contains("infact2_boon_reincarnation"))
				{
					yield return new WaitForSeconds(0.1f);
					yield return Singleton<CardSpawner>.Instance.SpawnCardToHand(CardLoader.GetCardByName("Squirrel"));
					base.StartCoroutine(functionsnstuff.PlayBoonAnim("infact2_boon_reincarnation"));
				}
				if (boons.Contains("infact2_boon_marrow"))
				{
					yield return new WaitForSeconds(0.1f);
					yield return Singleton<ResourcesManager>.Instance.AddBones(1, base.PlayableCard.Slot);
					base.StartCoroutine(functionsnstuff.PlayBoonAnim("infact2_boon_marrow"));
				}
				yield break;
			}
		}
		
		public class VoltageBoon : SpecialCardBehaviour
		{
			public override bool RespondsToSacrifice()
			{
				return true;
			}

			public override IEnumerator OnSacrifice()
			{
				List<string> boons = functionsnstuff.returnBoonsAsList();
				if (boons.Contains("infact2_boon_voltage") && SaveManager.saveFile.IsPart2)
				{
					yield return new WaitForSeconds(0.1f);
					yield return Singleton<GBC.PixelResourcesManager>.Instance.AddMaxEnergy(1);
					yield return Singleton<GBC.PixelResourcesManager>.Instance.AddEnergy(1);
					base.StartCoroutine(functionsnstuff.PlayBoonAnim("infact2_boon_voltage"));
				}
				yield break;
			}
		}

		public class ConstructBoon : SpecialCardBehaviour
        {
			public override bool RespondsToOtherCardDie(PlayableCard card, CardSlot deathSlot, bool fromCombat, PlayableCard killer)
			{
				return card == base.Card && fromCombat && base.PlayableCard.OnBoard && base.Card.Info.energyCost > 2 && base.PlayableCard.IsPlayerCard();
			}

			public override IEnumerator OnOtherCardDie(PlayableCard card, CardSlot deathSlot, bool fromCombat, PlayableCard killer)
			{
				List<string> boons = functionsnstuff.returnBoonsAsList();
				if (boons.Contains("infact2_boon_construct"))
				{
					List<string> moxen = new List<string> { "MoxRuby", "MoxEmerald", "MoxSapphire" };
					yield return new WaitForSeconds(0.1f);
					yield return Singleton<BoardManager>.Instance.CreateCardInSlot(CardLoader.GetCardByName(moxen[UnityEngine.Random.RandomRangeInt(0, moxen.Count)]), base.PlayableCard.Slot, 0.1f, true);
					yield return new WaitForSeconds(0.2f);
					base.StartCoroutine(functionsnstuff.PlayBoonAnim("infact2_boon_construct"));
				}
				yield break;
			}
		}

		public class FossilBoon : SpecialCardBehaviour
		{
			public override bool RespondsToDie(bool wasSacrifice, PlayableCard killer)
			{
				return true;
			}

			public override IEnumerator OnDie(bool wasSacrifice, PlayableCard killer)
			{
				List<string> boons = functionsnstuff.returnBoonsAsList();
				if (boons.Contains("infact2_boon_fossil"))
				{
					yield return Singleton<ResourcesManager>.Instance.AddBones(2, base.PlayableCard.Slot);
					yield return new WaitForSeconds(0.25f);
					base.StartCoroutine(functionsnstuff.PlayBoonAnim("infact2_boon_fossil"));
				}
				yield break;
			}
		}

		public class OverclockAct2 : SpecialCardBehaviour
		{
			public override bool RespondsToDie(bool wasSacrifice, PlayableCard killer)
			{
				List<string> sideDeck = new List<string> { "Squirrel", "Skeleton", "LeapBot", "MoxRuby", "MoxSapphire", "MoxEmerald" };
				if (base.PlayableCard.InOpponentQueue) { return false; }
				return SaveData.isChallengeActive("nuzlocke") && sideDeck.IndexOf(base.Card.Info.name) < 0 && base.PlayableCard.slot.IsPlayerSlot;
			}

			public override IEnumerator OnDie(bool wasSacrifice, PlayableCard killer)
			{
				if (SaveData.isChallengeActive("nuzlocke"))
				{
					SaveManager.saveFile.gbcData.collection.RemoveCardByName(base.PlayableCard.Info.name);
					SaveManager.saveFile.gbcData.deck.RemoveCardByName(base.PlayableCard.Info.name);
				}
				yield break;
			}
		}

		public class Bounty : SpecialCardBehaviour
		{
			private void Awake()
			{
				
				Color sceneColor = new Color(1, 1, 1, 1);
				if (base.Card.Info.mods.Count <= 0)
                {
					CardModificationInfo mod = new CardModificationInfo();
					mod.bountyHunterInfo = new BountyHunterInfo();
					mod.bountyHunterInfo.dialogueIndex = 2;
					base.Card.Info.mods.Add(mod);
                }
				Debug.Log(base.Card.Info.mods[0].bountyHunterInfo.dialogueIndex);
				int index = base.Card.Info.mods[0].bountyHunterInfo.dialogueIndex;
				string[] bountyData = SaveData.bountyHunters.Split(';');
				string[] hunterData = bountyData[index].Split(',');
				CardModificationInfo nameMod = new CardModificationInfo();
				nameMod.nameReplacement = hunterData[0];
				base.Card.Info.mods.Add(nameMod);
				GameObject body = Instantiate(base.PlayableCard.gameObject.transform.GetChild(0).GetChild(0).Find("QueuedCardElements").Find("QueuedPortrait").gameObject);
				body.name = "bountyBody";
				body.GetComponent<SpriteRenderer>().sprite = Tools.getSprite("bountybody_" + hunterData[2] + ".png");
				body.transform.parent = base.PlayableCard.gameObject.transform.GetChild(0).GetChild(0).Find("QueuedCardElements");
				body.transform.localPosition = base.PlayableCard.gameObject.transform.GetChild(0).GetChild(0).Find("QueuedCardElements").Find("QueuedPortrait").localPosition;
				GameObject eyes = Instantiate(base.PlayableCard.gameObject.transform.GetChild(0).GetChild(0).Find("QueuedCardElements").Find("QueuedPortrait").gameObject);
				eyes.name = "bountyEyes";
				eyes.GetComponent<SpriteRenderer>().sprite = Tools.getSprite("bountyeyes_" + hunterData[3] + ".png");
				eyes.transform.parent = base.PlayableCard.gameObject.transform.GetChild(0).GetChild(0).Find("QueuedCardElements");
				eyes.transform.localPosition = base.PlayableCard.gameObject.transform.GetChild(0).GetChild(0).Find("QueuedCardElements").Find("QueuedPortrait").localPosition;
				GameObject mouth = Instantiate(base.PlayableCard.gameObject.transform.GetChild(0).GetChild(0).Find("QueuedCardElements").Find("QueuedPortrait").gameObject);
				mouth.name = "bountyMouth";
				mouth.GetComponent<SpriteRenderer>().sprite = Tools.getSprite("bountymouth_" + hunterData[4] + ".png");
				mouth.transform.parent = base.PlayableCard.gameObject.transform.GetChild(0).GetChild(0).Find("QueuedCardElements");
				mouth.transform.localPosition = base.PlayableCard.gameObject.transform.GetChild(0).GetChild(0).Find("QueuedCardElements").Find("QueuedPortrait").localPosition;
				GameObject hat = Instantiate(base.PlayableCard.gameObject.transform.GetChild(0).GetChild(0).Find("QueuedCardElements").Find("QueuedPortrait").gameObject);
				hat.name = "bountyHat";
				sceneColor = new Color(0f, 0.67f, 0.09f, 1);
				int templeId = 0;
				switch (functionsnstuff.getTemple(SaveData.roomId))
                {
					case CardTemple.Tech:
						templeId = 2;
						sceneColor = new Color(0.8f, 0.375f, 0, 1);
						break;
					case CardTemple.Undead:
						templeId = 1;
						sceneColor = new Color(0f, 0.52f, 0.37f, 1);//0 0.52 0.37 1
						break;
					case CardTemple.Wizard:
						templeId = 3;
						sceneColor = new Color(0.32f, 0f, 1, 1);
						break;
				}
				string bountyHunters = "1;";
				for (int i = 1; i < bountyData.Length; i++)
				{
					string[] hunterData2 = bountyData[i].Split(',');
					for (int j = 0; j < hunterData2.Length; j++)
					{
						if (j == 6 && i == index && hunterData2[j] == "-1")
						{
							bountyHunters += "," + templeId;
							continue;
						} else if (j == 6 && i == index && hunterData2[j] != "-1")
                        {
							templeId = Convert.ToInt32(hunterData2[j]);
							bountyHunters += "," + hunterData2[j];
							continue;
                        }
						if (j > 0)
						{
							bountyHunters += "," + hunterData2[j];
						} else
                        {
							bountyHunters += hunterData2[j];
						}
					}
					if (i < bountyData.Length - 1)
					{
						bountyHunters += ";";
					}
				}
				Debug.Log(bountyHunters);
				SaveData.bountyHunters = bountyHunters;
				body.GetComponent<SpriteRenderer>().color = sceneColor;
				hat.GetComponent<SpriteRenderer>().color = sceneColor;
				mouth.GetComponent<SpriteRenderer>().color = sceneColor;
				eyes.GetComponent<SpriteRenderer>().color = sceneColor;
				hat.GetComponent<SpriteRenderer>().sprite = Tools.getSprite("bounty" + functionsnstuff.getTempleIndex(templeId) + "_" + hunterData[5] + ".png");
				hat.transform.parent = base.PlayableCard.gameObject.transform.GetChild(0).GetChild(0).Find("QueuedCardElements");
				hat.transform.localPosition = base.PlayableCard.gameObject.transform.GetChild(0).GetChild(0).Find("QueuedCardElements").Find("QueuedPortrait").localPosition;

				GameObject Pbody = Instantiate(base.PlayableCard.gameObject.transform.GetChild(0).GetChild(0).Find("CardElements").Find("Portrait").gameObject);
				Pbody.name = "bountyBody";
				Pbody.GetComponent<SpriteRenderer>().sprite = body.GetComponent<SpriteRenderer>().sprite;
				Pbody.transform.parent = base.PlayableCard.gameObject.transform.GetChild(0).GetChild(0).Find("CardElements");
				Pbody.transform.localPosition = base.PlayableCard.gameObject.transform.GetChild(0).GetChild(0).Find("CardElements").Find("Portrait").localPosition;
				GameObject Peyes = Instantiate(base.PlayableCard.gameObject.transform.GetChild(0).GetChild(0).Find("CardElements").Find("Portrait").gameObject);
				Peyes.name = "bountyEyes";
				Peyes.GetComponent<SpriteRenderer>().sprite = eyes.GetComponent<SpriteRenderer>().sprite;
				Peyes.transform.parent = base.PlayableCard.gameObject.transform.GetChild(0).GetChild(0).Find("CardElements");
				Peyes.transform.localPosition = base.PlayableCard.gameObject.transform.GetChild(0).GetChild(0).Find("CardElements").Find("Portrait").localPosition;
				GameObject Pmouth = Instantiate(base.PlayableCard.gameObject.transform.GetChild(0).GetChild(0).Find("CardElements").Find("Portrait").gameObject);
				Pmouth.name = "bountyMouth";
				Pmouth.GetComponent<SpriteRenderer>().sprite = mouth.GetComponent<SpriteRenderer>().sprite;
				Pmouth.transform.parent = base.PlayableCard.gameObject.transform.GetChild(0).GetChild(0).Find("CardElements");
				Pmouth.transform.localPosition = base.PlayableCard.gameObject.transform.GetChild(0).GetChild(0).Find("CardElements").Find("Portrait").localPosition;
				GameObject Phat = Instantiate(base.PlayableCard.gameObject.transform.GetChild(0).GetChild(0).Find("CardElements").Find("Portrait").gameObject);
				Phat.name = "bountyHat";
				Pbody.GetComponent<SpriteRenderer>().color = sceneColor;
				Phat.GetComponent<SpriteRenderer>().color = sceneColor;
				Pmouth.GetComponent<SpriteRenderer>().color = sceneColor;
				Peyes.GetComponent<SpriteRenderer>().color = sceneColor;
				Phat.GetComponent<SpriteRenderer>().sprite = hat.GetComponent<SpriteRenderer>().sprite;
				Phat.transform.parent = base.PlayableCard.gameObject.transform.GetChild(0).GetChild(0).Find("CardElements");
				Phat.transform.localPosition = base.PlayableCard.gameObject.transform.GetChild(0).GetChild(0).Find("CardElements").Find("Portrait").localPosition;


			}
			/*
			SaveManager.SaveFile.ResetGBCSaveData();
			StoryEventsData.SetEventCompleted(StoryEvent.StartScreenNewGameUsed, false, true);
			SaveManager.SaveToFile(true);
			*/

            public override bool RespondsToResolveOnBoard()
			{
				return true;
			}

			public override IEnumerator OnResolveOnBoard()
			{
				GBC.TextBox.Style style = GBC.TextBox.Style.Nature;
				int index = base.Card.Info.mods[0].bountyHunterInfo.dialogueIndex;
				string[] bountyData = SaveData.bountyHunters.Split(';');
				string[] hunterData = bountyData[index].Split(',');
				switch (functionsnstuff.getTemple(SaveData.roomId))
				{
					case CardTemple.Tech:
						style = GBC.TextBox.Style.Tech;
						break;
					case CardTemple.Undead:
						style = GBC.TextBox.Style.Undead;
						break;
					case CardTemple.Wizard:
						style = GBC.TextBox.Style.Magic;
						break;
				}
				if (Convert.ToInt32(hunterData[1]) > 0)
				{
					switch (UnityEngine.Random.RandomRangeInt(0, 5))
					{
						case 0:
							yield return Singleton<GBC.TextBox>.Instance.ShowUntilInput("So, we meet again..", style, null, GBC.TextBox.ScreenPosition.ForceBottom, 0f, true, false, null, true, Emotion.Neutral);
							yield return Singleton<GBC.TextBox>.Instance.ShowUntilInput(base.Card.Info.mods[1].nameReplacement + ". Seeking revenge..", style, null, GBC.TextBox.ScreenPosition.ForceBottom, 0f, true, false, null, true, Emotion.Neutral);
							break;
						case 1:
							yield return Singleton<GBC.TextBox>.Instance.ShowUntilInput("Oh.. Its You..", style, null, GBC.TextBox.ScreenPosition.ForceBottom, 0f, true, false, null, true, Emotion.Neutral);
							yield return Singleton<GBC.TextBox>.Instance.ShowUntilInput("The one who destroyed " + base.Card.Info.mods[1].nameReplacement + "... You won't get away with it again.", style, null, GBC.TextBox.ScreenPosition.ForceBottom, 0f, true, false, null, true, Emotion.Neutral);
							break;
						case 2:
							yield return Singleton<GBC.TextBox>.Instance.ShowUntilInput("Aha! Found you!", style, null, GBC.TextBox.ScreenPosition.ForceBottom, 0f, true, false, null, true, Emotion.Neutral);
							yield return Singleton<GBC.TextBox>.Instance.ShowUntilInput("I've come to get my revenge!", style, null, GBC.TextBox.ScreenPosition.ForceBottom, 0f, true, false, null, true, Emotion.Neutral);
							break;
						case 3:
							yield return Singleton<GBC.TextBox>.Instance.ShowUntilInput("Surprise!", style, null, GBC.TextBox.ScreenPosition.ForceBottom, 0f, true, false, null, true, Emotion.Neutral);
							yield return Singleton<GBC.TextBox>.Instance.ShowUntilInput("Didn't expect to see " + base.Card.Info.mods[1].nameReplacement + " again, did ya?", style, null, GBC.TextBox.ScreenPosition.ForceBottom, 0f, true, false, null, true, Emotion.Neutral);
							break;
						case 4:
							yield return Singleton<GBC.TextBox>.Instance.ShowUntilInput("Hey.", style, null, GBC.TextBox.ScreenPosition.ForceBottom, 0f, true, false, null, true, Emotion.Neutral);
							yield return Singleton<GBC.TextBox>.Instance.ShowUntilInput("Unfortunately, " + base.Card.Info.mods[1].nameReplacement + " is back.", style, null, GBC.TextBox.ScreenPosition.ForceBottom, 0f, true, false, null, true, Emotion.Neutral);
							break;
					}
				} else
                {
					switch (UnityEngine.Random.RandomRangeInt(0, 5))
					{
						case 0:
							yield return Singleton<GBC.TextBox>.Instance.ShowUntilInput("Well, Hello there.", style, null, GBC.TextBox.ScreenPosition.ForceBottom, 0f, true, false, null, true, Emotion.Neutral);
							yield return Singleton<GBC.TextBox>.Instance.ShowUntilInput("I am " + base.Card.Info.mods[1].nameReplacement + ". I will be taking that scalp of yours.", style, null, GBC.TextBox.ScreenPosition.ForceBottom, 0f, true, false, null, true, Emotion.Neutral);
							break;
						case 1:
							yield return Singleton<GBC.TextBox>.Instance.ShowUntilInput("Greeetings!", style, null, GBC.TextBox.ScreenPosition.ForceBottom, 0f, true, false, null, true, Emotion.Neutral);
							yield return Singleton<GBC.TextBox>.Instance.ShowUntilInput("They call me.. " + base.Card.Info.mods[1].nameReplacement + ".. You can't defeat ME!", style, null, GBC.TextBox.ScreenPosition.ForceBottom, 0f, true, false, null, true, Emotion.Neutral);
							break;
						case 2:
							yield return Singleton<GBC.TextBox>.Instance.ShowUntilInput("Just what do YOU think you're doing? Wandering around and destroying our dungeons.", style, null, GBC.TextBox.ScreenPosition.ForceBottom, 0f, true, false, null, true, Emotion.Neutral);
							yield return Singleton<GBC.TextBox>.Instance.ShowUntilInput("Let's see you stop " + base.Card.Info.mods[1].nameReplacement + ".", style, null, GBC.TextBox.ScreenPosition.ForceBottom, 0f, true, false, null, true, Emotion.Neutral);
							break;
						case 3:
							yield return Singleton<GBC.TextBox>.Instance.ShowUntilInput("A little mouse wanders into my sight..", style, null, GBC.TextBox.ScreenPosition.ForceBottom, 0f, true, false, null, true, Emotion.Neutral);
							yield return Singleton<GBC.TextBox>.Instance.ShowUntilInput("Just remember that " + base.Card.Info.mods[1].nameReplacement + " can not be defeated.", style, null, GBC.TextBox.ScreenPosition.ForceBottom, 0f, true, false, null, true, Emotion.Neutral);
							break;
						case 4:
							yield return Singleton<GBC.TextBox>.Instance.ShowUntilInput("At last, I have found you..", style, null, GBC.TextBox.ScreenPosition.ForceBottom, 0f, true, false, null, true, Emotion.Neutral);
							yield return Singleton<GBC.TextBox>.Instance.ShowUntilInput(base.Card.Info.mods[1].nameReplacement + " will be your executioner for the evening.", style, null, GBC.TextBox.ScreenPosition.ForceBottom, 0f, true, false, null, true, Emotion.Neutral);
							break;
					}
				}
                    yield break;

			}

			public override bool RespondsToDie(bool wasSacrifice, PlayableCard killer)
			{
				return true;
			}

			public override IEnumerator OnDie(bool wasSacrifice, PlayableCard killer)
			{
				GBC.TextBox.Style style = GBC.TextBox.Style.Nature;
				switch (functionsnstuff.getTemple(SaveData.roomId))
				{
					case CardTemple.Tech:
						style = GBC.TextBox.Style.Tech;
						break;
					case CardTemple.Undead:
						style = GBC.TextBox.Style.Undead;
						break;
					case CardTemple.Wizard:
						style = GBC.TextBox.Style.Magic;
						break;
				}
				string bountyHunters = "";
				int index = base.Card.Info.mods[0].bountyHunterInfo.dialogueIndex;
				string[] bountyData = SaveData.bountyHunters.Split(';');
				for (int i = 0; i < bountyData.Length; i++)
                {
					string[] hunterData = bountyData[i].Split(',');
					for (int j = 0; j < hunterData.Length; j++)
                    {
						if (j == 1 && i == index)
                        {
							bountyHunters += "," + (Convert.ToInt32(hunterData[j]) + 1);
							continue;
                        }
						if (j > 0)
						{
							bountyHunters += "," + hunterData[j];
						} else
                        {
							bountyHunters += hunterData[j];
						}
                    }
					if (i < bountyData.Length - 1)
					{
						bountyHunters += ";";
					}
                }
				SaveData.bountyHunters = bountyHunters;
				
				switch (UnityEngine.Random.RandomRangeInt(0, 5))
				{
					case 0:
						yield return Singleton<GBC.TextBox>.Instance.ShowUntilInput("My injuries.. They are severe.", style, null, GBC.TextBox.ScreenPosition.ForceBottom, 0f, true, false, null, true, Emotion.Neutral);
						yield return Singleton<GBC.TextBox>.Instance.ShowUntilInput("I am forced to retire.. for now.", style, null, GBC.TextBox.ScreenPosition.ForceBottom, 0f, true, false, null, true, Emotion.Neutral);
						break;
					case 1:
						yield return Singleton<GBC.TextBox>.Instance.ShowUntilInput("Argh!! What have you done to me?", style, null, GBC.TextBox.ScreenPosition.ForceBottom, 0f, true, false, null, true, Emotion.Neutral);
						yield return Singleton<GBC.TextBox>.Instance.ShowUntilInput("Everything hurts... But.. I'll be back..", style, null, GBC.TextBox.ScreenPosition.ForceBottom, 0f, true, false, null, true, Emotion.Neutral);
						break;
					case 2:
						yield return Singleton<GBC.TextBox>.Instance.ShowUntilInput("Well.. you stopped me..", style, null, GBC.TextBox.ScreenPosition.ForceBottom, 0f, true, false, null, true, Emotion.Neutral);
						yield return Singleton<GBC.TextBox>.Instance.ShowUntilInput("I WILL get you.. challenger..", style, null, GBC.TextBox.ScreenPosition.ForceBottom, 0f, true, false, null, true, Emotion.Neutral);
						break;
					case 3:
						yield return Singleton<GBC.TextBox>.Instance.ShowUntilInput("I've been scrapped!", style, null, GBC.TextBox.ScreenPosition.ForceBottom, 0f, true, false, null, true, Emotion.Neutral);
						yield return Singleton<GBC.TextBox>.Instance.ShowUntilInput("But this ain't the last of " + base.Card.Info.mods[1].nameReplacement + "!", style, null, GBC.TextBox.ScreenPosition.ForceBottom, 0f, true, false, null, true, Emotion.Neutral);
						break;
					case 4:
						yield return Singleton<GBC.TextBox>.Instance.ShowUntilInput("You beat me fair and square...", style, null, GBC.TextBox.ScreenPosition.ForceBottom, 0f, true, false, null, true, Emotion.Neutral);
						yield return Singleton<GBC.TextBox>.Instance.ShowUntilInput("So I will retreat.. well played.. Expect a rematch soon though..", style, null, GBC.TextBox.ScreenPosition.ForceBottom, 0f, true, false, null, true, Emotion.Neutral);
						break;
				}
				yield break;
			}
		}


            //TERRAIN

          private void AddTerrain()
		{

			//Leshy
			CardInfo boulder = CardManager.New("infact2", "infact2_terrain_boulder", "Boulder", 0, 5, "Ay yo yaw cant stand right here")
					.SetPixelPortrait(Tools.getSprite("boulder.png"))
					.AddSpecialAbilities(Fossil)
					.AddTraits(Trait.Terrain)
					.AddAbilities(Ability.MadeOfStone)
					.AddAppearances(CardAppearanceBehaviour.Appearance.TerrainBackground)
					.SetBloodCost(0);
			boulder.metaCategories = new List<CardMetaCategory>();

			CardInfo fir = CardManager.New("infact2", "infact2_terrain_fir", "Fir", 0, 3, "Ay yo yaw cant stand right here")
				.SetPixelPortrait(Tools.getSprite("fir.png"))
				.AddSpecialAbilities(Fossil)
				.AddTraits(Trait.Terrain)
				.AddAbilities(Ability.Reach)
				.AddAppearances(CardAppearanceBehaviour.Appearance.TerrainBackground)
				.SetBloodCost(0);
			fir.metaCategories = new List<CardMetaCategory>();

			CardInfo stump = CardManager.New("infact2", "infact2_terrain_stump", "Stump", 0, 3, "Ay yo yaw cant stand right here")
			.SetPixelPortrait(Tools.getSprite("stump.png"))
			.AddSpecialAbilities(Fossil)
			.AddTraits(Trait.Terrain)
			.AddAppearances(CardAppearanceBehaviour.Appearance.TerrainBackground)
			.SetBloodCost(0);
			stump.metaCategories = new List<CardMetaCategory>();

			CardInfo snowyfir = CardManager.New("infact2", "infact2_terrain_snowfir", "Snowy Fir", 0, 5, "Ay yo yaw cant stand right here")
				.SetPixelPortrait(Tools.getSprite("snowfir.png"))
				.AddSpecialAbilities(Fossil)
				.AddTraits(Trait.Terrain)
				.AddAbilities(Ability.Reach)
				.AddAppearances(CardAppearanceBehaviour.Appearance.TerrainBackground)
				.SetBloodCost(0);
			snowyfir.metaCategories = new List<CardMetaCategory>();

			CardInfo frozenOpossum = CardManager.New("infact2", "infact2_terrain_frozenopossum", "Frozen Opossum", 0, 5, "Ay yo yaw cant stand right here")
				.SetPixelPortrait(Tools.getSprite("frozenopossum.png"))
				.AddSpecialAbilities(Fossil)
				.SetIceCube("Opossum")
				.AddTraits(Trait.Terrain)
				.AddAbilities(Ability.IceCube)
				.AddAppearances(CardAppearanceBehaviour.Appearance.TerrainBackground)
				.SetBloodCost(0);
			frozenOpossum.metaCategories = new List<CardMetaCategory>();

			//Grimora
			CardInfo kennel = CardManager.New("infact2", "infact2_terrain_kennel", "Kennel", 0, 3, "Ay yo yaw cant stand right here")
					.SetPixelPortrait(Tools.getSprite("kennel.png"))
					.AddSpecialAbilities(Fossil)
					.SetIceCube("Bonehound")
					.AddTraits(Trait.Terrain)
					.AddAbilities(Ability.IceCube)
					.AddAppearances(CardAppearanceBehaviour.Appearance.TerrainBackground)
					.SetBloodCost(0);
			kennel.metaCategories = new List<CardMetaCategory>();

			CardInfo obelisk = CardManager.New("infact2", "infact2_terrain_obelisk", "Obelisk", 0, 3, "Ay yo yaw cant stand right here")
				.SetPixelPortrait(Tools.getSprite("obelisk.png"))
				.AddSpecialAbilities(Fossil)
				.AddTraits(Trait.Terrain)
				.AddAbilities(Ability.MadeOfStone)
				.AddAppearances(CardAppearanceBehaviour.Appearance.TerrainBackground)
				.SetBloodCost(0);
			obelisk.metaCategories = new List<CardMetaCategory>();

			CardInfo disturbedgrave = CardManager.New("infact2", "infact2_terrain_disturbedgrave", "Disturbed Grave", 0, 1, "Ay yo yaw cant stand right here")
				.SetPixelPortrait(Tools.getSprite("disturbedgrave.png"))
				.AddSpecialAbilities(Fossil)
				.SetEvolve(CardLoader.GetCardByName("Zombie"), 2)
				.AddAbilities(Ability.Evolve)
				.AddTraits(Trait.Terrain)
				.AddAppearances(CardAppearanceBehaviour.Appearance.TerrainBackground)
				.SetBloodCost(0);
			disturbedgrave.metaCategories = new List<CardMetaCategory>();

			CardInfo tombstone = CardManager.New("infact2", "infact2_terrain_tombstone", "Tombstone", 0, 5, "Ay yo yaw cant stand right here")
				.SetPixelPortrait(Tools.getSprite("tombstone.png"))
				.AddSpecialAbilities(Fossil)
				.AddAbilities(Ability.PreventAttack)
				.AddTraits(Trait.Terrain)
				.AddAppearances(CardAppearanceBehaviour.Appearance.TerrainBackground)
				.SetBloodCost(0);
			tombstone.metaCategories = new List<CardMetaCategory>();

			CardInfo deadtree = CardManager.New("infact2", "infact2_terrain_deadtree", "Dead Tree", 0, 2, "Ay yo yaw cant stand right here")
				.SetPixelPortrait(Tools.getSprite("deadtree.png"))
				.AddSpecialAbilities(Fossil)
				.AddAbilities(Ability.Reach)
				.AddTraits(Trait.Terrain)
				.AddAppearances(CardAppearanceBehaviour.Appearance.TerrainBackground)
				.SetBloodCost(0);
			deadtree.metaCategories = new List<CardMetaCategory>();

			//P03
			CardInfo annoyfm = CardManager.New("infact2", "infact2_terrain_annoyfm", "Annoy FM", 0, 3, "Ay yo yaw cant stand right here")
					.SetPixelPortrait(Tools.getSprite("annoyfm.png"))
					.AddSpecialAbilities(Fossil)
					.AddTraits(Trait.Terrain)
					.AddAbilities(Ability.BuffEnemy)
					.AddAppearances(CardAppearanceBehaviour.Appearance.TerrainBackground)
					.SetBloodCost(0);
			annoyfm.metaCategories = new List<CardMetaCategory>();

			CardInfo conduittower = CardManager.New("infact2", "infact2_terrain_conduittower", "Conduit Tower", 0, 3, "Ay yo yaw cant stand right here")
				.SetPixelPortrait(Tools.getSprite("conduit tower.png"))
				.AddSpecialAbilities(Fossil)
				.AddTraits(Trait.Terrain)
				.AddAbilities(Ability.ConduitNull)
				.AddAppearances(CardAppearanceBehaviour.Appearance.TerrainBackground)
				.SetBloodCost(0);
			conduittower.metaCategories = new List<CardMetaCategory>();

			CardInfo brokenbot = CardManager.New("infact2", "infact2_terrain_brokenbot", "Broken Bot", 0, 1, "Ay yo yaw cant stand right here")
				.SetPixelPortrait(Tools.getSprite("brokenbot.png"))
				.AddSpecialAbilities(Fossil)
				.AddAbilities(Ability.ExplodeOnDeath)
				.AddTraits(Trait.Terrain)
				.AddAppearances(CardAppearanceBehaviour.Appearance.TerrainBackground)
				.SetBloodCost(0);
			brokenbot.metaCategories = new List<CardMetaCategory>();


			CardInfo railing = CardManager.New("infact2", "infact2_terrain_railing", "Bridge Railing", 0, 6, "Ay yo yaw cant stand right here")
				.SetPixelPortrait(Tools.getSprite("railing.png"))
				.AddSpecialAbilities(Fossil)
				.AddTraits(Trait.Terrain)
				.AddAppearances(CardAppearanceBehaviour.Appearance.TerrainBackground)
				.SetBloodCost(0);
			railing.metaCategories = new List<CardMetaCategory>();

			//Magnificus
			CardInfo pillar = CardManager.New("infact2", "infact2_terrain_pillar", "Pillar", 0, 3, "Ay yo yaw cant stand right here")
					.SetPixelPortrait(Tools.getSprite("pillar.png"))
					.AddSpecialAbilities(Fossil)
					.AddTraits(Trait.Terrain)
					.AddAbilities(Ability.Reach)
					.AddAppearances(CardAppearanceBehaviour.Appearance.TerrainBackground)
					.SetBloodCost(0);
			pillar.metaCategories = new List<CardMetaCategory>();

			CardInfo arch = CardManager.New("infact2", "infact2_terrain_arch", "Arch", 0, 5, "Ay yo yaw cant stand right here")
				.SetPixelPortrait(Tools.getSprite("arch.png"))
				.AddSpecialAbilities(Fossil)
				.AddTraits(Trait.Terrain)
				.AddAppearances(CardAppearanceBehaviour.Appearance.TerrainBackground)
				.SetBloodCost(0);
			arch.metaCategories = new List<CardMetaCategory>();

			CardInfo ruinedarch = CardManager.New("infact2", "infact2_terrain_ruinedarch", "Ruined Arch", 0, 3, "Ay yo yaw cant stand right here")
				.SetPixelPortrait(Tools.getSprite("ruinedarch.png"))
				.AddSpecialAbilities(Fossil)
				.AddTraits(Trait.Terrain)
				.AddAppearances(CardAppearanceBehaviour.Appearance.TerrainBackground)
				.SetBloodCost(0);
			ruinedarch.metaCategories = new List<CardMetaCategory>();

			CardInfo boulderemerald = CardManager.New("infact2", "infact2_terrain_boulderemerald", "Emerald Boulder", 0, 3, "Ay yo yaw cant stand right here")
				.SetPixelPortrait(Tools.getSprite("boulderemerald.png"))
				.AddSpecialAbilities(Fossil)
				.AddTraits(Trait.Terrain, Trait.Gem)
				.AddAbilities(Ability.GainGemGreen)
				.AddAppearances(CardAppearanceBehaviour.Appearance.TerrainBackground)
				.SetBloodCost(0);
			boulderemerald.metaCategories = new List<CardMetaCategory>();

			CardInfo boulderruby = CardManager.New("infact2", "infact2_terrain_boulderruby", "Ruby Boulder", 0, 3, "Ay yo yaw cant stand right here")
				.SetPixelPortrait(Tools.getSprite("boulderruby.png"))
				.AddSpecialAbilities(Fossil)
				.AddTraits(Trait.Terrain, Trait.Gem)
				.AddAbilities(Ability.GainGemOrange)
				.AddAppearances(CardAppearanceBehaviour.Appearance.TerrainBackground)
				.SetBloodCost(0);
			boulderruby.metaCategories = new List<CardMetaCategory>();

			CardInfo bouldersapphire = CardManager.New("infact2", "infact2_terrain_bouldersapphire", "Sapphire Boulder", 0, 3, "Ay yo yaw cant stand right here")
				.SetPixelPortrait(Tools.getSprite("bouldersapphire.png"))
				.AddSpecialAbilities(Fossil)
				.AddTraits(Trait.Terrain, Trait.Gem)
				.AddAbilities(Ability.GainGemBlue)
				.AddAppearances(CardAppearanceBehaviour.Appearance.TerrainBackground)
				.SetBloodCost(0);
			bouldersapphire.metaCategories = new List<CardMetaCategory>();

			CardInfo bountyHunter = CardManager.New("infact2", "infact2_BOUNTYHUNTER", "Bounty Hunter", 0, 0, "Waddup, to all rappers shut up with your shutting up.")
				.SetPixelPortrait(Tools.getSprite("bountyempty.png"))
				.AddSpecialAbilities(BountyHunter)
				.SetBloodCost(0);
			bountyHunter.metaCategories = new List<CardMetaCategory>();
		}

    }

}

