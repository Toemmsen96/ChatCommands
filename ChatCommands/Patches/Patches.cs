using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GameNetcodeStuff;
using static ChatCommands.Utils;


namespace ChatCommands.Patches
{
    internal class Patches
    {

        internal static float defaultJumpForce;

        [HarmonyPatch(typeof(RoundManager), "EnemyCannotBeSpawned")]
        [HarmonyPrefix]
        private static bool OverrideCannotSpawn()
        {
            if (ChatCommands.OverrideSpawns)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        [HarmonyPatch(typeof(RoundManager), "SpawnEnemyFromVent")]
        [HarmonyPrefix]
        private static void LogSpawnEnemyFromVent()
        {
            LogInfo("Attempting to spawn an enemy");
        }

        [HarmonyPatch(typeof(RoundManager), "LoadNewLevel")]
        [HarmonyPostfix]
        private static void UpdateNewInfo(ref EnemyVent[] ___allEnemyVents, ref SelectableLevel ___currentLevel)
        {
            ChatCommands.currentLevel = ___currentLevel;
            ChatCommands.currentLevelVents = ___allEnemyVents;
            HUDManager.Instance.chatTextField.characterLimit = 999;
        }


        [HarmonyPatch(typeof(RoundManager), "AdvanceHourAndSpawnNewBatchOfEnemies")]
        [HarmonyPrefix]
        private static void UpdateCurrentLevelInfo(ref EnemyVent[] ___allEnemyVents, ref SelectableLevel ___currentLevel)
        {
            ChatCommands.currentLevel = ___currentLevel;
            ChatCommands.currentLevelVents = ___allEnemyVents;
        }


        [HarmonyPatch(typeof(RoundManager), "LoadNewLevel")]
        [HarmonyPrefix]
        private static bool ModifyLevel(ref SelectableLevel newLevel)
        {
            ChatCommands.currentRound = RoundManager.Instance;
            if (!ChatCommands.levelEnemySpawns.ContainsKey(newLevel))
            {
                List<SpawnableEnemyWithRarity> list = new List<SpawnableEnemyWithRarity>();
                foreach (SpawnableEnemyWithRarity enemy in newLevel.Enemies)
                {
                    list.Add(enemy);
                }
                ChatCommands.levelEnemySpawns.Add(newLevel, list);
            }
            ChatCommands.levelEnemySpawns.TryGetValue(newLevel, out var value);
            newLevel.Enemies = value;
            foreach (SpawnableEnemyWithRarity enemy2 in newLevel.Enemies)
            {
                LogInfo("Inside: " + enemy2.enemyType.enemyName);
                if (!ChatCommands.enemyRaritys.ContainsKey(enemy2))
                {
                    ChatCommands.enemyRaritys.Add(enemy2, enemy2.rarity);
                }
                ChatCommands.enemyRaritys.TryGetValue(enemy2, out int value2);
                enemy2.rarity = value2;
            }
            foreach (SpawnableEnemyWithRarity outsideEnemy in newLevel.OutsideEnemies)
            {
                LogInfo("Outside: " + outsideEnemy.enemyType.enemyName);
                if (!ChatCommands.enemyRaritys.ContainsKey(outsideEnemy))
                {
                    ChatCommands.enemyRaritys.Add(outsideEnemy, outsideEnemy.rarity);
                }
                ChatCommands.enemyRaritys.TryGetValue(outsideEnemy, out int value3);
                outsideEnemy.rarity = value3;
            }
            foreach (SpawnableEnemyWithRarity enemy3 in newLevel.Enemies)
            {
                if (!ChatCommands.enemyPropCurves.ContainsKey(enemy3))
                {
                    ChatCommands.enemyPropCurves.Add(enemy3, enemy3.enemyType.probabilityCurve);
                }
                AnimationCurve value4 = new AnimationCurve();
                ChatCommands.enemyPropCurves.TryGetValue(enemy3, out value4);
                enemy3.enemyType.probabilityCurve = value4;
            }
            return true;
        }


        [HarmonyPatch(typeof(RoundManager), "Start")]
        [HarmonyPrefix]
        private static void SetIsHost()
        {
            LogInfo("Host Status: " + RoundManager.Instance.NetworkManager.IsHost);
            ChatCommands.isHost = RoundManager.Instance.NetworkManager.IsHost;
           
        }

        [HarmonyPatch(typeof(PlayerControllerB), "AllowPlayerDeath")]
        [HarmonyPrefix]
        private static bool OverrideDeath()
        {
            return !ChatCommands.enableGod;
        }

        [HarmonyPatch(typeof(MouthDogAI), "OnCollideWithPlayer")]
        [HarmonyPrefix]
        private static bool OverrideDeath2()
        {
            return !ChatCommands.enableGod;
        }

        [HarmonyPatch(typeof(ForestGiantAI), "GrabPlayerServerRpc")]
        [HarmonyPrefix]
        private static bool OverrideDeath3()
        {
            return !ChatCommands.enableGod;
        }







        [HarmonyPatch(typeof(PlayerControllerB), "Start")]
        [HarmonyPrefix]
        private static void GetPlayerRef(ref PlayerControllerB __instance)
        {
            ChatCommands.playerRef = __instance;
            defaultJumpForce = __instance.jumpForce;
            LogInfo("Default Jump Force: " + defaultJumpForce);
        }

        [HarmonyPatch(typeof(RoundManager), "FinishGeneratingNewLevelClientRpc")]
        [HarmonyPrefix]
        private static void GetRoundManagerRef(ref RoundManager __instance)
        {
            if (ChatCommands.isHost)
            {
                LogInfo("Host, getting mine ref...");
                int len = ChatCommands.currentRound.currentLevel.spawnableMapObjects.Count();
                for (int i = 0; i < len; i++)
                {
                    if (ChatCommands.currentRound.currentLevel.spawnableMapObjects[i].prefabToSpawn.name == "Landmine")
                    {
                        LogInfo("Found Mine Index: " + i);
                        ChatCommands.mine = i;
                        break;
                    }
                    if (ChatCommands.currentRound.currentLevel.spawnableMapObjects[i].prefabToSpawn.name == "Turret")
                    {
                        LogInfo("Found Turret Index: " + i);
                        ChatCommands.turret = i;
                        break;
                    }
                }
            }
            
        }

        // Patch game to think its in Unity Editor
        [HarmonyPatch(typeof(Application), "get_isEditor")]
        [HarmonyPostfix]
        private static void IsEditorPatch(ref bool __result)
        {
            __result = ChatCommands.EnableDebugModeSetting.Value;
            return;
        }


        
    }
}
