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
        private static string NetCommandPrefix = ChatCommands.NetCommandPrefix;
        private static string NetHostCommandPrefix = ChatCommands.NetHostCommandPrefix;
        private static string NetCommandPostfix = ChatCommands.NetCommandPostfix;
        private static float defaultJumpForce;
        private static string nullChatMessage = "";
        



        [HarmonyPatch(typeof(RoundManager), "EnemyCannotBeSpawned")]
        [HarmonyPrefix]
        private static bool OverrideCannotSpawn()
        {
            return false;
        }

        [HarmonyPatch(typeof(RoundManager), "SpawnEnemyFromVent")]
        [HarmonyPrefix]
        private static void LogSpawnEnemyFromVent()
        {
            ChatCommands.mls.LogInfo("Attempting to spawn an enemy");
        }

        [HarmonyPatch(typeof(HUDManager), "SubmitChat_performed")]
        [HarmonyPrefix]
        private static void ChatCommandsSubmitted(HUDManager __instance)
        {
            string text = __instance.chatTextField.text;
            string localPlayer = GameNetworkManager.Instance.username;
            

            // Log the text to ensure it's not null
            ChatCommands.mls.LogInfo($"Received chat input: {text}");

            // Check if text is not null and starts with the set prefix
            if (!string.IsNullOrEmpty(text) && text.ToLower().StartsWith(ChatCommands.PrefixSetting.Value))
            {
                if (!ChatCommands.NonHostCommands(text))
                {
                    if (!ChatCommands.isHost)
                    {
                        string command = text.Substring((ChatCommands.PrefixSetting.Value).Length);
                        __instance.chatTextField.text = NetCommandPrefix + text + NetCommandPostfix;
                        ChatCommands.mls.LogInfo("Not Host, trying to send command:" + command);
                    }
                    else
                    {
                        if(text.ToLower().Contains("p=@me"))
                        {
                            ChatCommands.playerwhocalled = localPlayer;
                            ChatCommands.mls.LogInfo("Player who called: " + ChatCommands.playerwhocalled);
                        }
                        string command = text.Substring((ChatCommands.PrefixSetting.Value).Length);
                        ChatCommands.ProcessCommandInput(command);
                        __instance.chatTextField.text = nullChatMessage;
                    }
                }
                else
                {
                    __instance.chatTextField.text = nullChatMessage;
                }
                
            }
            else
            {
                // Log an error or handle the case where the text is not valid for commands
                ChatCommands.mls.LogWarning("Invalid input for a command or null chat.");
            }
        }


        [HarmonyPatch(typeof(HUDManager), "AddChatMessage")]
        [HarmonyPrefix]
        private static void ReadChatMessage(HUDManager __instance, ref string chatMessage, ref string nameOfUserWhoTyped)
        {

            ChatCommands.mls.LogInfo("Chat Message: " + chatMessage + " sent by: " + nameOfUserWhoTyped);
            if (chatMessage.StartsWith(NetCommandPrefix) && ChatCommands.isHost && ChatCommands.HostSetting.Value)
            {
                string commandwithpost = chatMessage.Substring((NetCommandPrefix).Length);
                string[]temp = commandwithpost.Split('<');
                string command = temp[0];
                if (command.ToLower().Contains("p=@me")){
                    ChatCommands.playerwhocalled = nameOfUserWhoTyped;
                }
                ChatCommands.mls.LogInfo("Host, trying to handle command: " + command);
                ChatCommands.DisplayChatMessage(nameOfUserWhoTyped + " sent command: "+ ChatCommands.PrefixSetting.Value + command);
                ChatCommands.ProcessCommandInput(command);
                chatMessage = nullChatMessage;
                nameOfUserWhoTyped = nullChatMessage;
                return;
            }
            else if (chatMessage.StartsWith(NetCommandPrefix) && ChatCommands.isHost && !ChatCommands.HostSetting.Value)
            {
                ChatCommands.mls.LogWarning("Host, but not allowing commands, checking player for allowance");
                foreach (AllowedHostPlayer player in ChatCommands.AllowedHostPlayers)
                {
                    if (player.Name.ToLower().Contains(nameOfUserWhoTyped.ToLower()))
                    {
                        ChatCommands.mls.LogInfo("Player is allowed to send commands");
                        string commandwithpost = chatMessage.Substring((NetCommandPrefix).Length);
                        string[] temp = commandwithpost.Split('<');
                        string command = temp[0];
                        if (command.ToLower().Contains("p=@me"))
                        {
                            ChatCommands.playerwhocalled = nameOfUserWhoTyped;
                        }
                        ChatCommands.mls.LogInfo("Host, trying to handle command: " + command);
                        ChatCommands.DisplayChatMessage(nameOfUserWhoTyped + " sent command: " + ChatCommands.PrefixSetting.Value + command);
                        ChatCommands.ProcessCommandInput(command);
                        chatMessage = nullChatMessage;
                        nameOfUserWhoTyped = nullChatMessage;
                        return;
                    }
                }
                ChatCommands.DisplayChatMessage("Host, but not allowing commands");
                return;
            }
            else if (chatMessage.StartsWith(NetHostCommandPrefix) && !ChatCommands.isHost)
            {
                string commandwithpost = chatMessage.Substring((NetHostCommandPrefix).Length);
                string[] temp = commandwithpost.Split('<');
                string command = temp[0];
                ChatCommands.mls.LogInfo("Recieved command from Host, trying to handle command: " + command);
                ChatCommands.ProcessNetHostCommand(command);
                return;
            }
            return;
        }

        [HarmonyPatch(typeof(ShotgunItem), "ItemActivate")]
        [HarmonyPrefix]
        static void ItemActivateGunPatch(ref ShotgunItem __instance)
        {
            if (ChatCommands.EnableInfiniteAmmo)
            {
                __instance.shellsLoaded = 2;
            }
            
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
                ChatCommands.mls.LogInfo("Inside: " + enemy2.enemyType.enemyName);
                if (!ChatCommands.enemyRaritys.ContainsKey(enemy2))
                {
                    ChatCommands.enemyRaritys.Add(enemy2, enemy2.rarity);
                }
                ChatCommands.enemyRaritys.TryGetValue(enemy2, out int value2);
                enemy2.rarity = value2;
            }
            foreach (SpawnableEnemyWithRarity outsideEnemy in newLevel.OutsideEnemies)
            {
                ChatCommands.mls.LogInfo("Outside: " + outsideEnemy.enemyType.enemyName);
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

        [HarmonyPatch(typeof(TimeOfDay), "SetNewProfitQuota")]
        [HarmonyPostfix]
        private static void PatchDeadline(TimeOfDay __instance)
        {

            if (ChatCommands.isHost && ChatCommands.CustomDeadline != int.MinValue)
            {
                __instance.quotaVariables.deadlineDaysAmount = ChatCommands.CustomDeadline;
                __instance.timeUntilDeadline = (float)(__instance.quotaVariables.deadlineDaysAmount + ChatCommands.CustomDeadline) * __instance.totalTime;

                TimeOfDay.Instance.timeUntilDeadline = (int)(TimeOfDay.Instance.totalTime * (float)TimeOfDay.Instance.quotaVariables.deadlineDaysAmount);
                TimeOfDay.Instance.SyncTimeClientRpc(__instance.globalTime, (int)__instance.timeUntilDeadline);
                StartOfRound.Instance.deadlineMonitorText.text = "DEADLINE:\n " + TimeOfDay.Instance.daysUntilDeadline;
            }
        }


        [HarmonyPatch(typeof(RoundManager), "Start")]
        [HarmonyPrefix]
        private static void SetIsHost()
        {
            ChatCommands.mls.LogInfo("Host Status: " + RoundManager.Instance.NetworkManager.IsHost);
            ChatCommands.isHost = RoundManager.Instance.NetworkManager.IsHost;
           
        }

        [HarmonyPatch(typeof(PlayerControllerB), "AllowPlayerDeath")]
        [HarmonyPrefix]
        private static bool OverrideDeath()
        {
            return !ChatCommands.enableGod;
        }

        [HarmonyPatch(typeof(Terminal), "RunTerminalEvents")]
        [HarmonyPostfix]
        private static void InfiniteCredits(ref int ___groupCredits)
        {
            if (ChatCommands.isHost && ChatCommands.EnableInfiniteCredits)
            {
                ___groupCredits = 50000;
            }
        }

        [HarmonyPatch(typeof(PlayerControllerB), "Update")]
        [HarmonyPostfix]
        private static void SpeedHackFunc(ref float ___jumpForce, ref float ___sprintMeter, ref float ___sprintMultiplier, ref bool ___isSprinting)
        {
            if (ChatCommands.speedHack)
            {
                ___jumpForce = 25f;
                ___sprintMeter = 1f;
                if (___isSprinting) ___sprintMultiplier = 10f;
            }
            else
            {
                ___jumpForce = defaultJumpForce;
            }

        }

        [HarmonyPatch(typeof(PlayerControllerB), "Start")]
        [HarmonyPrefix]
        private static void GetPlayerRef(ref PlayerControllerB __instance)
        {
            ChatCommands.playerRef = __instance;
            defaultJumpForce = __instance.jumpForce;
            ChatCommands.mls.LogInfo("Default Jump Force: " + defaultJumpForce);
        }

        [HarmonyPatch(typeof(RoundManager), "FinishGeneratingNewLevelClientRpc")]
        [HarmonyPrefix]
        private static void GetRoundManagerRef(ref RoundManager __instance)
        {
            int len = ChatCommands.currentRound.currentLevel.spawnableMapObjects.Count();
            for (int i = 0; i < len; i++)
            {
                if (ChatCommands.currentRound.currentLevel.spawnableMapObjects[i].prefabToSpawn.name == "Landmine")
                {
                    ChatCommands.mls.LogInfo("Found Mine Index: " + i);
                    ChatCommands.mine = i;
                    break;
                }
                if (ChatCommands.currentRound.currentLevel.spawnableMapObjects[i].prefabToSpawn.name == "Turret")
                {
                    ChatCommands.mls.LogInfo("Found Turret Index: " + i);
                    ChatCommands.turret = i;
                    break;
                }
            }
        }
    }
}
