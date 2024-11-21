using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using BepInEx.Configuration;
using GameNetcodeStuff;
using Unity.Netcode;
using ChatCommands.Commands;
using static ChatCommands.Utils;


namespace ChatCommands
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class ChatCommands : BaseUnityPlugin
    {
        private const string modGUID = "toemmsen.ChatCommands";
        private const string modName = "ChatCommands";
        private const string modVersion = "2.0.0";
        private readonly Harmony harmony = new Harmony(modGUID);
        private static ChatCommands instance;
        internal static ManualLogSource mls = BepInEx.Logging.Logger.CreateLogSource(modGUID);
        public static Dictionary<SelectableLevel, List<SpawnableEnemyWithRarity>> levelEnemySpawns;
        public static Dictionary<SpawnableEnemyWithRarity, int> enemyRaritys;
        public static Dictionary<SpawnableEnemyWithRarity, AnimationCurve> enemyPropCurves;
        internal static ConfigEntry<string> PrefixSetting;
        internal static ConfigEntry<bool> HostSetting;
        internal static ConfigEntry<bool> LogToChatSetting;
        internal static ConfigEntry<bool> SendHostCommandsSetting;
        internal static ConfigEntry<bool> OverrideSpawnsSetting;
        internal static ConfigEntry<bool> EnableDebugModeSetting;
        internal static ConfigEntry<bool> DisplayChatMessagesAsPopupSetting;
        internal static bool OverrideSpawns = false;
        internal static bool AllowHostCommands = false;
        internal static bool isHost;
        internal static string playerwhocalled;
        internal static List<AllowedHostPlayer> AllowedHostPlayers = new List<AllowedHostPlayer>();
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                
            }
            DisplayChatMessagesAsPopupSetting = instance.Config.Bind<bool>("Command Settings", "Display Chat Messages As Popup", false, "Determines if chat messages are displayed as popup messages");
            PrefixSetting = instance.Config.Bind<string>("Command Settings", "Command Prefix", "/", "An optional prefix for chat commands");
            HostSetting = instance.Config.Bind<bool>("Command Settings", "Has to be Host", true, "(for server host only): determines if clients can also use the host commands");
            SendHostCommandsSetting = instance.Config.Bind<bool>("Command Settings", "Send Host Commands", true, "(for server host only): determines if commands get sent to the clients, so for example god mode is enabled for them too");
            OverrideSpawnsSetting = instance.Config.Bind<bool>("Command Settings", "Override Spawns", true, "(for server host only): determines if the spawn command overrides the default spawns. If enabled there can be spawned more than one girl etc. Can be toggled ingame by using /override command.");
            EnableDebugModeSetting = instance.Config.Bind<bool>("Command Settings", "Enable Debug Mode", true, "Enables Unity Debug mode");
            LogToChatSetting = instance.Config.Bind<bool>("Command Settings", "Log To Chat", false, "Enables logging to (local) chat");
            OverrideSpawns = OverrideSpawnsSetting.Value;
            AllowHostCommands = HostSetting.Value;
            enemyRaritys = new Dictionary<SpawnableEnemyWithRarity, int>();
            levelEnemySpawns = new Dictionary<SelectableLevel, List<SpawnableEnemyWithRarity>>();
            enemyPropCurves = new Dictionary<SpawnableEnemyWithRarity, AnimationCurve>();
            harmony.PatchAll(typeof(ChatCommands));
            harmony.PatchAll(typeof(Patches.Patches));
            harmony.PatchAll(typeof(CommandController));
            harmony.PatchAll(typeof(SetCustomDeadline));
            harmony.PatchAll(typeof(SpeedHack));
            harmony.PatchAll(typeof(SpawnTruck));
            harmony.PatchAll(typeof(InfiniteAmmo));
            harmony.PatchAll(typeof(GodMode));
            harmony.PatchAll(typeof(SetMoney));
            harmony.PatchAll(typeof(SpawnMapObject));
            harmony.PatchAll(typeof(ToggleOverrideSpawns));
            //CCMDNetworking newCMDNW = new CCMDNetworking();
            mls.LogWarning((object)"\n" +
                "  ______                                                                                                       \n"+
                " /_  __/  ____   ___    ____ ___    ____ ___    _____  ___    ____    _____                                    \n"+
                "  / /    / __ \\ / _ \\  / __ `__ \\  / __ `__ \\  / ___/ / _ \\  / __ \\  / ___/                                    \n"+
                " / /    / /_/ //  __/ / / / / / / / / / / / / (__  ) /  __/ / / / / (__  )                                     \n"+
                "/_/_____\\____/_\\___/ /_/ /_/_/_/ /_/ /_/ /_/_/____/  \\___/ /_/ /_/ /____/                            __        \n"+
                "  / ____/   / /_   ____ _  / /_         / ____/  ____    ____ ___    ____ ___   ____ _   ____   ____/ /   _____\n"+
                " / /       / __ \\ / __ `/ / __/        / /      / __ \\  / __ `__ \\  / __ `__ \\ / __ `/  / __ \\ / __  /   / ___/\n"+
                "/ /___    / / / // /_/ / / /_         / /___   / /_/ / / / / / / / / / / / / // /_/ /  / / / // /_/ /   (__  ) \n"+
                "\\____/   /_/ /_/ \\__,_/  \\__/         \\____/   \\____/ /_/ /_/ /_/ /_/ /_/ /_/ \\__,_/  /_/ /_/ \\__,_/   /____/  \n");
            mls.LogInfo("ChatCommands loaded");
        }

        internal static void SpawnItems(Vector3 location, string itemToSpawn, int value, int amount){
            DisplayChatMessage("Trying to spawn: " + itemToSpawn);
            foreach (var x in StartOfRound.Instance.allItemsList.itemsList)
            {
                if (x.name.ToLower().StartsWith(itemToSpawn))
                {
                    for (int i = 0; i < amount; i++)
                    {
                        GameObject obj = UnityEngine.Object.Instantiate(x.spawnPrefab, location, Quaternion.identity, StartOfRound.Instance.propsContainer);
                        obj.GetComponent<GrabbableObject>().fallTime = 0f;
                        obj.GetComponent<GrabbableObject>().SetScrapValue(value);
                        obj.GetComponent<NetworkObject>().Spawn();
                    }
                    DisplayChatMessage("Spawned " + amount + " " + itemToSpawn + (amount > 1 ? "s" : "") + " with value of:" + value + "\n at position: " + location);
                    return;
                }
                
            }
            DisplayChatError("Could not spawn: " + itemToSpawn);
        }
    }

    internal class AllowedHostPlayer
    {
        public string Name { get; set; }
        public bool AllowHostCMD { get; set; }

        public AllowedHostPlayer(string name, bool isActive)
        {
            Name = name;
            AllowHostCMD = isActive;
        }
    }
}
