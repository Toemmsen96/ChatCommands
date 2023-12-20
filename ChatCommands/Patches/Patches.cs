using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using ChatCommands;
using System.Runtime.CompilerServices;
using TMPro;
using Unity.Netcode;
using GameNetcodeStuff;
using BepInEx;
using BepInEx.Configuration;

namespace ChatCommands.Patches
{
    internal class Patches
    {
        [HarmonyPatch(typeof(RoundManager), "EnemyCannotBeSpawned")]
        [HarmonyPrefix]
        private static bool OverrideCannotSpawn()
        {
            return false;
        }

        [HarmonyPatch(typeof(RoundManager), "SpawnEnemyFromVent")]
        [HarmonyPrefix]
        private static void logSpawnEnemyFromVent()
        {
            ChatCommands.mls.LogInfo((object)"Attempting to spawn an enemy");
        }

        [HarmonyPatch(typeof(HUDManager), "SubmitChat_performed")]
        [HarmonyPrefix]
        private static void ChatCommandsSubmitted(HUDManager __instance)
        {
            string text = __instance.chatTextField.text;

            // Log the text to ensure it's not null
            ChatCommands.mls.LogInfo($"Received chat input: {text}");

            // Check if text is not null and starts with "/"
            if (!string.IsNullOrEmpty(text) && text.ToLower().StartsWith("/"))
            {
                ChatCommands.ProcessCommandInput(text);
                __instance.chatTextField.text = "";
            }
            else
            {
                // Log an error or handle the case where the text is not valid for commands
                Debug.LogError("Invalid or null chat input.");
            }
        }

        [HarmonyPatch(typeof(RoundManager), "LoadNewLevel")]
        [HarmonyPostfix]
        private static void updateNewInfo(ref EnemyVent[] ___allEnemyVents, ref SelectableLevel ___currentLevel)
        {
            ChatCommands.currentLevel = ___currentLevel;
            ChatCommands.currentLevelVents = ___allEnemyVents;
        }


        [HarmonyPatch(typeof(RoundManager), "AdvanceHourAndSpawnNewBatchOfEnemies")]
        [HarmonyPrefix]
        private static void updateCurrentLevelInfo(ref EnemyVent[] ___allEnemyVents, ref SelectableLevel ___currentLevel)
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
                ChatCommands.mls.LogInfo((object)("Inside: " + enemy2.enemyType.enemyName));
                if (!ChatCommands.enemyRaritys.ContainsKey(enemy2))
                {
                    ChatCommands.enemyRaritys.Add(enemy2, enemy2.rarity);
                }
                int value2 = 0;
                ChatCommands.enemyRaritys.TryGetValue(enemy2, out value2);
                enemy2.rarity = value2;
            }
            foreach (SpawnableEnemyWithRarity outsideEnemy in newLevel.OutsideEnemies)
            {
                ChatCommands.mls.LogInfo((object)("Outside: " + outsideEnemy.enemyType.enemyName));
                if (!ChatCommands.enemyRaritys.ContainsKey(outsideEnemy))
                {
                    ChatCommands.enemyRaritys.Add(outsideEnemy, outsideEnemy.rarity);
                }
                int value3 = 0;
                ChatCommands.enemyRaritys.TryGetValue(outsideEnemy, out value3);
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

        [HarmonyPatch(typeof(TimeOfDay), "SetNewProfitQuota")]
        [HarmonyPostfix]
        private static void patchDeadline(TimeOfDay __instance)
        {

            if (ChatCommands.isHost && ChatCommands.CustomDeadline != int.MinValue)
            {
                __instance.quotaVariables.deadlineDaysAmount = ChatCommands.CustomDeadline;
                __instance.timeUntilDeadline = (float)(__instance.quotaVariables.deadlineDaysAmount + ChatCommands.CustomDeadline) * __instance.totalTime;

                TimeOfDay.Instance.timeUntilDeadline = (int)(TimeOfDay.Instance.totalTime * (float)TimeOfDay.Instance.quotaVariables.deadlineDaysAmount);
                TimeOfDay.Instance.SyncTimeClientRpc(__instance.globalTime, (int)__instance.timeUntilDeadline);
                ((TMP_Text)StartOfRound.Instance.deadlineMonitorText).text = "DEADLINE:\n " + TimeOfDay.Instance.daysUntilDeadline;
            }
        }


        [HarmonyPatch(typeof(RoundManager), "Start")]
        [HarmonyPrefix]
        private static void setIsHost()
        {
            ChatCommands.mls.LogInfo((object)("Host Status: " + ((NetworkBehaviour)RoundManager.Instance).NetworkManager.IsHost));
            ChatCommands.isHost = ((NetworkBehaviour)RoundManager.Instance).NetworkManager.IsHost;
        }

        [HarmonyPatch(typeof(PlayerControllerB), "AllowPlayerDeath")]
        [HarmonyPrefix]
        private static bool overrideDeath()
        {
            if (ChatCommands.isHost)
            {
                return !ChatCommands.enableGod;
            }
            return true;
        }

        [HarmonyPatch(typeof(Terminal), "RunTerminalEvents")]
        [HarmonyPostfix]
        private static void infiniteCredits(ref int ___groupCredits)
        {
            if (ChatCommands.isHost && ChatCommands.EnableInfiniteCredits)
            {
                ___groupCredits = 50000;
            }
        }

        [HarmonyPatch(typeof(PlayerControllerB), "Update")]
        [HarmonyPostfix]
        private static void speedHackFunc(ref float ___jumpForce, ref float ___sprintMeter, ref float ___sprintMultiplier, ref bool ___isSprinting)
        {
            if (ChatCommands.speedHack)
            {
                ___jumpForce = 25f;
                ___sprintMeter = 1f;
                if (___isSprinting) ___sprintMultiplier = 10f;
            }

        }

        [HarmonyPatch(typeof(PlayerControllerB), "Start")]
        [HarmonyPrefix]
        private static void getPlayerRef(ref PlayerControllerB __instance)
        {
            ChatCommands.playerRef = __instance;
        }
    }
}
