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
        private const string modVersion = "1.2.0";
        private readonly Harmony harmony = new Harmony(modGUID);
        private static ChatCommands instance;
        private static ccmdGUI ccmdGUI;
        internal static ManualLogSource mls = BepInEx.Logging.Logger.CreateLogSource(modGUID);
        public static Dictionary<SelectableLevel, List<SpawnableEnemyWithRarity>> levelEnemySpawns;
        public static Dictionary<SpawnableEnemyWithRarity, int> enemyRaritys;
        public static Dictionary<SpawnableEnemyWithRarity, AnimationCurve> enemyPropCurves;
        internal static SelectableLevel currentLevel;
        internal static EnemyVent[] currentLevelVents;
        internal static RoundManager currentRound;
        internal static bool EnableInfiniteAmmo = false;
        internal static ConfigEntry<string> PrefixSetting;
        internal static ConfigEntry<bool> HostSetting;
        internal static ConfigEntry<bool> LogToChatSetting;
        internal static ConfigEntry<bool> SendHostCommandsSetting;
        internal static ConfigEntry<bool> OverrideSpawnsSetting;
        internal static ConfigEntry<bool> EnableDebugModeSetting;
        internal static bool OverrideSpawns = false;
        internal static bool AllowHostCommands = false;
        internal static bool enableGod;
        internal static bool EnableInfiniteCredits = false;
        internal static bool usingTerminal = false;
        internal static PlayerControllerB playerRef;
        internal static bool isHost;
        internal static bool speedHack;
        internal static string playerwhocalled;
        internal static List<AllowedHostPlayer> AllowedHostPlayers = new List<AllowedHostPlayer>();
        internal static int mine = -1;
        internal static int turret = -1;
        private static string msgtitle = ""; //TODO: remove
        private static string msgbody = ""; //TODO: remove
        
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                
            }

            PrefixSetting = instance.Config.Bind<string>("Command Settings", "Command Prefix", "/", "An optional prefix for chat commands");
            HostSetting = instance.Config.Bind<bool>("Command Settings", "Has to be Host", true, "(for server host only): determines if clients can also use the host commands");
            SendHostCommandsSetting = instance.Config.Bind<bool>("Command Settings", "Send Host Commands", true, "(for server host only): determines if commands get sent to the clients, so for example god mode is enabled for them too");
            OverrideSpawnsSetting = instance.Config.Bind<bool>("Command Settings", "Override Spawns", true, "(for server host only): determines if the spawn command overrides the default spawns. If enabled there can be spawned more than one girl etc. Can be toggled ingame by using /override command.");
            EnableDebugModeSetting = instance.Config.Bind<bool>("Command Settings", "Enable Debug Mode", true, "Enables Unity Debug mode");
            LogToChatSetting = instance.Config.Bind<bool>("Command Settings", "Log To Chat", true, "Enables logging to (local) chat");
            OverrideSpawns = OverrideSpawnsSetting.Value;
            AllowHostCommands = HostSetting.Value;
            enemyRaritys = new Dictionary<SpawnableEnemyWithRarity, int>();
            levelEnemySpawns = new Dictionary<SelectableLevel, List<SpawnableEnemyWithRarity>>();
            enemyPropCurves = new Dictionary<SpawnableEnemyWithRarity, AnimationCurve>();
            speedHack = false;
            enableGod = false;
            harmony.PatchAll(typeof(ChatCommands));
            harmony.PatchAll(typeof(Patches.Patches));
            harmony.PatchAll(typeof(CommandController));
            harmony.PatchAll(typeof(SetCustomDeadline));
            harmony.PatchAll(typeof(ccmdGUI));
            //CCMDNetworking newCMDNW = new CCMDNetworking();
            ccmdGUI = new ccmdGUI().InitMenu(instance);
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

        

        private static bool ToggleGodMode()
        {
            if (isHost)
            {
                enableGod = !enableGod;
            }
            return enableGod;
        }

        private static bool ToggleSpeedHack()
        {
            if (isHost)
            {
                speedHack = !playerRef.isSpeedCheating;
                playerRef.isSpeedCheating = speedHack;
            }
            return speedHack;
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
                    return;
                }
                
            }
            DisplayChatError("Could not spawn: " + itemToSpawn);
        }


        internal static bool NonHostCommands(string command)
        {
            bool IsNonHostCommand = true;
            switch (command.ToLower())
            {

                case "help":
                    oldCommands.GetHelp();
                    break;
                case "pos":
                case "position":
                    oldCommands.GetPos();
                    break;
                case "morehelp":
                    oldCommands.GetMoreHelp();
                    break;
                case "credits":
                    oldCommands.GetCredits();
                    break;
                case "cheats":
                    oldCommands.GetCheats();
                    break;
                case "spawn":
                    oldCommands.GetSpawn();
                    break;
                default:
                    IsNonHostCommand = false;
                    break;
            }
            return IsNonHostCommand;
        }

        internal static void ProcessCommandInput(string command)
        {
            msgtitle = "default";
            msgbody = "<color=#FF0000>ERR</color>: unknown";
            string[] commandarguments = command.Split(' ');

            if (NonHostCommands(command))
            {
                HUDManager.Instance.DisplayTip(msgtitle, msgbody, false, false, "LC_Tip1");
                return;
            }

            if (!isHost)
            {
                msgtitle = "Command";
                msgbody = "Unable to send command since you are not host.";
                HUDManager.Instance.DisplayTip(msgtitle, msgbody, false, false, "LC_Tip1");
                return;
            }

            switch (commandarguments[0])
            {
                case "spawnitem":
                case "spwitm":
                    oldCommands.SpawnItemFunc(command);
                    break;
                case "god":
                    msgtitle = "God Mode";
                    msgbody = "God Mode set to: " + ToggleGodMode();
                    SendHostCommand(command);
                    //CCMDNetworking.SendHostCommand(command);
                    break;
                case "speed":
                    msgtitle = "Speed hack";
                    msgbody = "Speed hack set to: " + ToggleSpeedHack();
                    SendHostCommand(command);
                    break;
                case "money":
                    EnableInfiniteCredits = !EnableInfiniteCredits;
                    msgtitle = "Infinite Credits";
                    msgbody = "Infinite Credits: " + EnableInfiniteCredits;
                    break;
                case "ammo":
                case "infammo":
                    EnableInfiniteAmmo = !EnableInfiniteAmmo;
                    msgtitle = "Infinite Ammo";
                    msgbody = "Infinite Ammo: " + EnableInfiniteAmmo;
                    SendHostCommand(command);
                    break;
                case "term":
                case "terminal":
                    oldCommands.TerminalFunc();
                    break;
                case "hostcmd":
                case "cohost":
                    oldCommands.SetHostCmds(commandarguments[1]);
                    break;
                case "togglehostcmd":
                case "thcmd":
                    AllowHostCommands = !AllowHostCommands;
                    break;
                case "spawnmapobj":
                case "spwobj":
                    oldCommands.SpawnMapObj(command);
                    break;
                case "ovr":
                case "override":
                    oldCommands.ToggleOverrideSpawns();
                    msgtitle = "Override Spawns";
                    msgbody = "Override Spawns set to: " + OverrideSpawns;
                    break;
                case "spawnhive":
                case "spwhive":
                    //Commands.SpawnHive(command);
                    //break;
                default:
                    msgtitle = "Command";
                    msgbody = "Unknown command: " + commandarguments[0];
                    DisplayChatError(msgbody);
                    break;
            }
            HUDManager.Instance.DisplayTip(msgtitle, msgbody, false, false, "LC_Tip1");
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
