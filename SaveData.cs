using System;
using System.Collections.Generic;
using System.Text;
using APIPlugin;
using InscryptionAPI.Saves;

namespace infact2
{
    public class SaveData
    {
        private const string PluginGuid = "mrfantastik.inscryption.infact2";
        public static string roomId
        {
            get { return ModdedSaveManager.SaveData.GetValue(PluginGuid, "roomId"); }
            set { ModdedSaveManager.SaveData.SetValue(PluginGuid, "roomId", value); }
        }

        public static float cameraX
        {
            get { return ModdedSaveManager.SaveData.GetValueAsFloat(PluginGuid, "cameraX"); }
            set { ModdedSaveManager.SaveData.SetValue(PluginGuid, "cameraX", value); }
        }
        public static float cameraY
        {
            get { return ModdedSaveManager.SaveData.GetValueAsFloat(PluginGuid, "cameraY"); }
            set { ModdedSaveManager.SaveData.SetValue(PluginGuid, "cameraY", value); }
        }

        public static float runSeed
        {
            get { return ModdedSaveManager.SaveData.GetValueAsFloat(PluginGuid, "runSeed"); }
            set { ModdedSaveManager.SaveData.SetValue(PluginGuid, "runSeed", value); }
        }

        public static int floor
        {
            get { return ModdedSaveManager.SaveData.GetValueAsInt(PluginGuid, "runFloor"); }
            set { ModdedSaveManager.SaveData.SetValue(PluginGuid, "runFloor", value); }
        }
        public static string nodeLayout
        {
            get { return ModdedSaveManager.SaveData.GetValue(PluginGuid, "nodeLayout"); }
            set { ModdedSaveManager.SaveData.SetValue(PluginGuid, "nodeLayout", value); }
        }

        public static int nodesCompleted
        {
            get { return ModdedSaveManager.SaveData.GetValueAsInt(PluginGuid, "nodesCompleted"); }
            set { ModdedSaveManager.SaveData.SetValue(PluginGuid, "nodesCompleted", value); }
        }

        public static int lives
        {
            get { return ModdedSaveManager.SaveData.GetValueAsInt(PluginGuid, "lives"); }
            set { ModdedSaveManager.SaveData.SetValue(PluginGuid, "lives", value); }
        }

        public static int highscore
        {
            get { return ModdedSaveManager.SaveData.GetValueAsInt(PluginGuid, "highscore"); }
            set { ModdedSaveManager.SaveData.SetValue(PluginGuid, "highscore", value); }
        }

        public static string boon1
        {
            get { return ModdedSaveManager.SaveData.GetValue(PluginGuid, "boon1"); }
            set { ModdedSaveManager.SaveData.SetValue(PluginGuid, "boon1", value); }
        }

        public static string boon2
        {
            get { return ModdedSaveManager.SaveData.GetValue(PluginGuid, "boon2"); }
            set { ModdedSaveManager.SaveData.SetValue(PluginGuid, "boon2", value); }
        }
        public static string boon3
        {
            get { return ModdedSaveManager.SaveData.GetValue(PluginGuid, "boon3"); }
            set { ModdedSaveManager.SaveData.SetValue(PluginGuid, "boon3", value); }
        }

        public static bool doneAreaSecret
        {
            get { return ModdedSaveManager.SaveData.GetValueAsBoolean(PluginGuid, "doneAreaSecret"); }
            set { ModdedSaveManager.SaveData.SetValue(PluginGuid, "doneAreaSecret", value); }
        }

        public static float bountyStars
        {
            get { return ModdedSaveManager.SaveData.GetValueAsFloat(PluginGuid, "bountyStars"); }
            set { ModdedSaveManager.SaveData.SetValue(PluginGuid, "bountyStars", value); }
        }

        public static string bountyHunters
        {
            get { return ModdedSaveManager.SaveData.GetValue(PluginGuid, "bountyHunters"); }
            set { ModdedSaveManager.SaveData.SetValue(PluginGuid, "bountyHunters", value); }
        }

        public static string challenges
        {
            get { return ModdedSaveManager.SaveData.GetValue(PluginGuid, "challenges"); }
            set { ModdedSaveManager.SaveData.SetValue(PluginGuid, "challenges", value); }
        }

        public static List<string> indexes = new List<string> { "bounty", "boon", "elite", "bridge", "nuzlocke", "nohammer" };
        public static void setChallengeActive(string challengeName, bool active)
        {
            string[] currentChallenges = SaveData.challenges.Split(';');
            if (currentChallenges.Length < indexes.Count)
            {
                string actualChallenges = "";
                for (int i = 0; i < indexes.Count; i++)
                {
                    actualChallenges += "0";
                    if (i < indexes.Count - 1)
                    {
                        actualChallenges += ";";
                    }
                }
                currentChallenges = actualChallenges.Split(';');
            }
            string challengese = "";
            for (int i = 0; i < currentChallenges.Length; i++)
            {
                if (i != indexes.IndexOf(challengeName))
                {
                    challengese += currentChallenges[i];
                } else
                {
                    string activet = active ? "1" : "0";
                    challengese += activet;
                }
                if (i < currentChallenges.Length - 1)
                {
                    challengese += ";";
                }
            }
            SaveData.challenges = challengese;
        }

        public static bool isChallengeActive(string challengeNAME)
        {
            challengeNAME = challengeNAME.ToLower();
            string[] currentChallenges = SaveData.challenges.Split(';');
            if (currentChallenges.Length < indexes.Count)
            {
                string actualChallenges = "";
                for (int i = 0; i < indexes.Count; i++)
                {
                    actualChallenges += "0";
                    if (i < indexes.Count - 1)
                    {
                        actualChallenges += ";";
                    }
                }
                currentChallenges = actualChallenges.Split(';');
            }
            if (currentChallenges[indexes.IndexOf(challengeNAME)] == "1") { return true; }
            return false;
        }
    }
}
