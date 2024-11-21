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
            HUDManager.Instance.chatTextField.characterLimit = 999;
        }


        [HarmonyPatch(typeof(RoundManager), "LoadNewLevel")]
        [HarmonyPrefix]
        private static bool ModifyLevel(ref SelectableLevel newLevel)
        {
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



        [HarmonyPatch(typeof(PlayerControllerB), "Start")]
        [HarmonyPrefix]
        private static void SetDefaultJumpForce(ref PlayerControllerB __instance)
        {
            defaultJumpForce = __instance.jumpForce;
            LogInfo("Default Jump Force: " + defaultJumpForce);
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
