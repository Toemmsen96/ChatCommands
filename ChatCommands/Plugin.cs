using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;
using BepInEx.Configuration;
using GameNetcodeStuff;
using Unity.Netcode;
using TMPro;
using ChatCommands.Patches;
using HarmonyLib.Tools;
using System.Linq;
using static Steamworks.InventoryItem;
using System.Security;
using System.Net;
using static System.Net.Mime.MediaTypeNames;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using Networking;

namespace ChatCommands
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class ChatCommands : BaseUnityPlugin
    {
        private const string modGUID = "toemmsen.ChatCommands";
        private const string modName = "ChatCommands";
        private const string modVersion = "1.1.91";
        private readonly Harmony harmony = new Harmony(modGUID);
        private static ChatCommands instance;
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
        internal static ConfigEntry<bool> SendHostCommandsSetting;
        internal static ConfigEntry<bool> OverrideSpawnsSetting;
        internal static bool OverrideSpawns = false;
        internal static bool AllowHostCommands = false;
        internal static bool enableGod;
        internal static bool EnableInfiniteCredits = false;
        internal static int CustomDeadline = int.MinValue;
        internal static bool usingTerminal = false;
        internal static PlayerControllerB playerRef;
        internal static bool isHost;
        internal static bool speedHack;
        internal static string msgtitle;
        internal static string msgbody;
        internal static string NetCommandPrefix = "<size=0>CCMD:";
        internal static string NetHostCommandPrefix = "<size=0>CHCMD:";
        internal static string NetCommandPostfix = "</size>";
        internal static string playerwhocalled;
        internal static List<AllowedHostPlayer> AllowedHostPlayers = new List<AllowedHostPlayer>();
        internal static int mine = -1;
        internal static int turret = -1;
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
            OverrideSpawns = OverrideSpawnsSetting.Value;
            AllowHostCommands = HostSetting.Value;
            enemyRaritys = new Dictionary<SpawnableEnemyWithRarity, int>();
            levelEnemySpawns = new Dictionary<SelectableLevel, List<SpawnableEnemyWithRarity>>();
            enemyPropCurves = new Dictionary<SpawnableEnemyWithRarity, AnimationCurve>();
            speedHack = false;
            enableGod = false;
            harmony.PatchAll(typeof(ChatCommands));
            harmony.PatchAll(typeof(Patches.Patches));
            mls.LogWarning((object)"\r\n" +
                "  ______                                                                                                       \r\n"+
                " /_  __/  ____   ___    ____ ___    ____ ___    _____  ___    ____    _____                                    \r\n"+
                "  / /    / __ \\ / _ \\  / __ `__ \\  / __ `__ \\  / ___/ / _ \\  / __ \\  / ___/                                    \r\n"+
                " / /    / /_/ //  __/ / / / / / / / / / / / / (__  ) /  __/ / / / / (__  )                                     \r\n"+
                "/_/_____\\____/_\\___/ /_/ /_/_/_/ /_/ /_/ /_/_/____/  \\___/ /_/ /_/ /____/                            __        \r\n"+
                "  / ____/   / /_   ____ _  / /_         / ____/  ____    ____ ___    ____ ___   ____ _   ____   ____/ /   _____\r\n"+
                " / /       / __ \\ / __ `/ / __/        / /      / __ \\  / __ `__ \\  / __ `__ \\ / __ `/  / __ \\ / __  /   / ___/\r\n"+
                "/ /___    / / / // /_/ / / /_         / /___   / /_/ / / / / / / / / / / / / // /_/ /  / / / // /_/ /   (__  ) \r\n"+
                "\\____/   /_/ /_/ \\__,_/  \\__/         \\____/   \\____/ /_/ /_/ /_/ /_/ /_/ /_/ \\__,_/  /_/ /_/ \\__,_/   /____/  \r\n");
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

        internal static void SpawnEnemy(SpawnableEnemyWithRarity enemy, int amount, bool inside, Vector3 location)
        {
            if (!isHost)
            {
                return;
            }
            if (location.x != 0f && location.y != 0f && location.z != 0f && inside)
            {
                try
                {
                    for (int i = 0; i < amount; i++)
                    {
                        currentRound.SpawnEnemyOnServer(location,0.0f, currentLevel.Enemies.IndexOf(enemy));
                    }
                    return;
                }
                catch
                {
                    mls.LogInfo("Failed to spawn enemies, check your command.");
                    return;
                }
            }
            if (location.x != 0f && location.y != 0f && location.z != 0f && !inside)
            {
                try
                {
                    int i = 0;
                    for (; i < amount; i++)
                    {
                        UnityEngine.Object.Instantiate<GameObject>(currentLevel.OutsideEnemies[currentLevel.OutsideEnemies.IndexOf(enemy)].enemyType.enemyPrefab, location, Quaternion.Euler(Vector3.zero)).gameObject.GetComponentInChildren<NetworkObject>().Spawn(true);
                    }
                    mls.LogInfo($"You wanted to spawn: {amount} enemies");
                    mls.LogInfo("Spawned an enemy. Total Spawned: " + i + "at position:" + location);
                    return;
                }
                catch
                {
                    mls.LogInfo("Failed to spawn enemies, check your command.");
                    return;
                }
            }
            if (inside)
            {
                try
                {
                    int i = 0;
                    for (; i < amount; i++)
                    {
                        currentRound.SpawnEnemyOnServer(currentRound.allEnemyVents[UnityEngine.Random.Range(0, currentRound.allEnemyVents.Length)].floorNode.position, currentRound.allEnemyVents[i].floorNode.eulerAngles.y, currentLevel.Enemies.IndexOf(enemy));
                        
                    }
                    mls.LogInfo($"You wanted to spawn: {amount} enemies");
                    mls.LogInfo(("Total Spawned: " + i));
                    return;
                }
                catch
                {
                    mls.LogInfo("Failed to spawn enemies, check your command.");
                    return;
                }
            }
            int j = 0;
            for (; j < amount; j++)
            {
                
                UnityEngine.Object.Instantiate<GameObject>(currentLevel.OutsideEnemies[currentLevel.OutsideEnemies.IndexOf(enemy)].enemyType.enemyPrefab, GameObject.FindGameObjectsWithTag("OutsideAINode")[UnityEngine.Random.Range(0, GameObject.FindGameObjectsWithTag("OutsideAINode").Length - 1)].transform.position, Quaternion.Euler(Vector3.zero)).gameObject.GetComponentInChildren<NetworkObject>().Spawn(true);
            }
            mls.LogInfo($"You wanted to spawn: {amount} enemies");
            mls.LogInfo(("Total Spawned: " + j));
        }

        internal static bool NonHostCommands(string command)
        {
            bool IsNonHostCommand = true;
            switch (command.ToLower())
            {

                case "help":
                    Commands.GetHelp();
                    break;
                case "pos":
                case "position":
                    Commands.GetPos();
                    break;
                case "morehelp":
                    Commands.GetMoreHelp();
                    break;
                case "credits":
                    Commands.GetCredits();
                    break;
                case "cheats":
                    Commands.GetCheats();
                    break;
                case "spawn":
                    Commands.GetSpawn();
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
                case "enemies":
                    Commands.GetEnemies();
                    break;
                case "getscrap":
                    Commands.GetScrap();
                    break;
                case "spawnenemy":
                case "spweny":
                    Commands.SpawnEnemyFunc(command);
                    break;
                case "spawnscrap":
                case "spwscr":
                    Commands.SpawnScrapFunc(command);
                    break;
                case "weather":
                    Commands.ChangeWeather(command);
                    break;
                case "togglelights":
                    Commands.ToggleLights();
                    break;
                case "buy":
                    Commands.BuyFunc(command);
                    break;
                case "god":
                    msgtitle = "God Mode";
                    msgbody = "God Mode set to: " + ToggleGodMode();
                    SendHostCommand(command);
                    break;
                case "speed":
                    msgtitle = "Speed hack";
                    msgbody = "Speed hack set to: " + ToggleSpeedHack();
                    SendHostCommand(command);
                    break;
                case "deadline":
                case "dl":
                    Commands.SetCustomDeadline(command);
                    break;
                case "tp":
                    Commands.Teleport(command);
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
                    Commands.TerminalFunc();
                    break;
                case "hostcmd":
                case "cohost":
                    Commands.SetHostCmds(commandarguments[1]);
                    break;
                case "togglehostcmd":
                case "thcmd":
                    AllowHostCommands = !AllowHostCommands;
                    break;
                case "spawnmapobj":
                case "spwobj":
                    Commands.SpawnMapObj(command);
                    break;
                case "ovr":
                case "override":
                    Commands.ToggleOverrideSpawns();
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
                    ChatCommands.DisplayChatError(msgbody);
                    break;
            }
            HUDManager.Instance.DisplayTip(msgtitle, msgbody, false, false, "LC_Tip1");
        }


        internal static void SendHostCommand(string commandInput)
        {
            if (!isHost || !SendHostCommandsSetting.Value)
            {
                return;
            }
            string commandToClients = ChatCommands.NetHostCommandPrefix + commandInput + ChatCommands.NetCommandPostfix;
            HUDManager.Instance.AddTextToChatOnServer(commandToClients, -1);
        }
        
        public static void ProcessNetHostCommand(string commandInput)
        {
            if (commandInput.ToLower().Contains("god"))
            {
                enableGod = !enableGod;
                msgtitle = "Host sent command:";
                msgbody = "God Mode set to: " + enableGod;
            }
            if (commandInput.ToLower().Contains("speed"))
            {
                speedHack = !playerRef.isSpeedCheating;
                playerRef.isSpeedCheating = speedHack;
                msgtitle = "Host sent command:";
                msgbody = "Speed hack set to: " + speedHack;
            }
            if (commandInput.ToLower().Contains("infammo") || commandInput.ToLower().Contains("ammo"))
            {
                EnableInfiniteAmmo = !EnableInfiniteAmmo;
                msgtitle = "Host sent command:";
                msgbody = "Infinite Ammo: " + EnableInfiniteAmmo;
            }
            HUDManager.Instance.DisplayTip(ChatCommands.msgtitle, ChatCommands.msgbody, false, false, "LC_Tip1");
        }
        public static void ProcessCommand(string commandInput)
        {
            ChatCommands.ProcessCommandInput(commandInput);
            HUDManager.Instance.DisplayTip(ChatCommands.msgtitle, ChatCommands.msgbody, false, false, "LC_Tip1");
        }

        public TextMeshProUGUI chatText;

        public static void DisplayChatMessage(string chatMessage)
        {
            string formattedMessage =
                $"<color=#FF00FF>ChatCommands</color>: <color=#FFFF00>{chatMessage}</color>";

            HUDManager.Instance.ChatMessageHistory.Add(formattedMessage);

            UpdateChatText();
        }
        public static void DisplayChatError(string errorMessage)
        {
            string formattedMessage =
                $"<color=#FF0000>CCMD: ERROR</color>: <color=#FF0000>{errorMessage}</color>";

            HUDManager.Instance.ChatMessageHistory.Add(formattedMessage);

            UpdateChatText();
        }

        private static void UpdateChatText()
        {
            HUDManager.Instance.chatText.text = string.Join("\n", HUDManager.Instance.ChatMessageHistory);
        }

        internal static string ConvertPlayername(string name)
        {
            ChatCommands.mls.LogInfo("Converting name: " + name);
            string convname = new string(name.Where((char c) => char.IsLetter(c)).ToArray());
            convname = (Regex.Replace(convname, "[^\\w\\._]", ""));
            ChatCommands.mls.LogInfo("Converted name: " + convname);
            return convname;
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
