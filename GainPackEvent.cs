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
	public class GainCardEvent : ManagedBehaviour
	{
			public CardTemple PackChoice { get; set; }

		public List<String> options = new List<string> { "Nature", "Dead"};

		public string selected = "Moo";
	}
}

