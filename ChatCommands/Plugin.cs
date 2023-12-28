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

namespace ChatCommands
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class ChatCommands : BaseUnityPlugin
    {
        private const string modGUID = "toemmsen.ChatCommands";
        private const string modName = "ChatCommands";
        private const string modVersion = "1.0.0";
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
        private static ConfigEntry<string> PrefixSetting;
        internal static bool enableGod;
        internal static bool EnableInfiniteCredits = false;
        internal static int CustomDeadline = int.MinValue;
        private static bool usingTerminal = false;
        internal static PlayerControllerB playerRef;
        internal static bool isHost;
        internal static bool speedHack;
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }

            PrefixSetting = ((BaseUnityPlugin)this).Config.Bind<string>("Command Settings", "Command Prefix", "/", "An optional prefix for chat commands");
            //mls = BepInEx.Logging.Logger.CreateLogSource(modGUID);
            mls.LogInfo("ChatCommands loaded");
            enemyRaritys = new Dictionary<SpawnableEnemyWithRarity, int>();
            levelEnemySpawns = new Dictionary<SelectableLevel, List<SpawnableEnemyWithRarity>>();
            enemyPropCurves = new Dictionary<SpawnableEnemyWithRarity, AnimationCurve>();
            speedHack = false;
            enableGod = false;
            harmony.PatchAll(typeof(ChatCommands));
            harmony.PatchAll(typeof(Patches.Patches));

            mls.LogInfo((object)"Chat Commands loaded!");
        }


        private static bool ToggleGodMode()
        {
            if (isHost)
            {
                enableGod = !enableGod;
            }
            return enableGod;
        }

        private static void ToggleSpeedHack()
        {
            if (isHost)
            {
                speedHack = !playerRef.isSpeedCheating;
                playerRef.isSpeedCheating = speedHack;
            }
        }

        private static void SpawnEnemy(SpawnableEnemyWithRarity enemy, int amount, bool inside, Vector3 location)
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
                        currentRound.SpawnEnemyOnServer(location, currentLevel.Enemies.IndexOf(enemy));
                    }
                    return;
                }
                catch
                {
                    mls.LogInfo((object)"Failed to spawn enemies, check your command.");
                    return;
                }
            }
            if(location.x != 0f && location.y != 0f && location.z != 0f && !inside)
            {
                try
                {
                    for (int i = 0; i < amount; i++)
                    {
                        UnityEngine.Object.Instantiate<GameObject>(currentLevel.OutsideEnemies[currentLevel.OutsideEnemies.IndexOf(enemy)].enemyType.enemyPrefab, location, Quaternion.Euler(Vector3.zero)).gameObject.GetComponentInChildren<NetworkObject>().Spawn(true);
                    }
                    return;
                }
                catch
                {
                    mls.LogInfo((object)"Failed to spawn enemies, check your command.");
                    return;
                }
            }
            if (inside)
            {
                try
                {
                    for (int i = 0; i < amount; i++)
                    {
                        currentRound.SpawnEnemyOnServer(currentRound.allEnemyVents[UnityEngine.Random.Range(0, currentRound.allEnemyVents.Length)].floorNode.position, currentRound.allEnemyVents[i].floorNode.eulerAngles.y, currentLevel.Enemies.IndexOf(enemy));
                    }
                    return;
                }
                catch
                {
                    mls.LogInfo((object)"Failed to spawn enemies, check your command.");
                    return;
                }
            }
            for (int j = 0; j < amount; j++)
            {
                mls.LogInfo((object)$"You wanted to spawn: {amount} enemies");
                mls.LogInfo((object)("Spawned an enemy. Total Spawned: " + j));
                UnityEngine.Object.Instantiate<GameObject>(currentLevel.OutsideEnemies[currentLevel.OutsideEnemies.IndexOf(enemy)].enemyType.enemyPrefab, GameObject.FindGameObjectsWithTag("OutsideAINode")[UnityEngine.Random.Range(0, GameObject.FindGameObjectsWithTag("OutsideAINode").Length - 1)].transform.position, Quaternion.Euler(Vector3.zero)).gameObject.GetComponentInChildren<NetworkObject>().Spawn(true);
            }
        }

        internal static void ProcessCommandInput(string text)
        {
            string prefix = "/";

            if (PrefixSetting.Value != "")
            {
                prefix = PrefixSetting.Value;
            }
            if (!text.ToLower().StartsWith(prefix.ToLower()))
            {
                return;
            }
            string msgtitle = "default";
            string msgbody = "<color=#FF0000>ERR</color>: unknown";
            if (!isHost)
            {
                msgtitle = "Command";
                msgbody = "Unable to send command since you are not host.";
                HUDManager.Instance.DisplayTip(msgtitle, msgbody, false, false, "LC_Tip1");
                return;
            }
            string command = text.Substring(prefix.Length);

            if (command.ToLower().StartsWith("spawnenemy") || command.ToLower().StartsWith("spweny"))
            {
                msgtitle = "Spawned Enemies";
                string[] array = text.Split(' ');
                if (currentLevel == null || levelEnemySpawns == null || currentLevel.Enemies == null)
                {
                    msgtitle = "Command";
                    msgbody = (currentLevel == null ? "Unable to send command since currentLevel is null." : "Unable to send command since levelEnemySpawns is null.");
                    HUDManager.Instance.DisplayTip(msgtitle, msgbody, true, false, "LC_Tip1");
                    return;
                }
                if (array.Length < 2)
                {
                    msgtitle = "Command Error";
                    msgbody = "Missing Arguments For Spawn\n'/spawnenemy <name> (amount=<amount>) (state=<state>) (position={random, @me, @<playername>})";
                    HUDManager.Instance.DisplayTip(msgtitle, msgbody, true, false, "LC_Tip1");
                    mls.LogWarning("Missing Arguments For Spawn\n'/spawnenemy <name> (amount=<amount>) (state=<state>) (position={random, @me, @<playername>})");
                    return;
                }
                int amount = 1;
                string vstate = "alive";
                Vector3 position = Vector3.zero;
                string sposition = "random";
                var args = array.Skip(2);

                foreach (string arg in args)
                {
                    string[] darg = arg.Split('=');
                    switch (darg[0])
                    {
                        case "a":
                        case "amount":
                            amount = int.Parse(darg[1]);
                            mls.LogInfo($"{amount}");
                            break;
                        case "s":
                        case "state":
                            vstate = darg[1];
                            mls.LogInfo(vstate);
                            break;
                        case "p":
                        case "position":
                            sposition = darg[1];
                            mls.LogInfo(sposition);
                            break;
                        default:
                            break;
                    }
                }

                if (sposition.StartsWith("@"))
                {
                    if (sposition == "@me") position = ((NetworkBehaviour)currentRound.playersManager.localPlayerController).transform.position;
                    else
                    {
                        string pstring = sposition.Substring(1);
                        foreach (PlayerControllerB player in currentRound.playersManager.allPlayerScripts)
                        {
                            if (player.name == pstring)
                            {
                                position = ((NetworkBehaviour)player).transform.position;
                                break;
                            }
                        }
                    }
                }
                else if (sposition != "random")
                {
                    mls.LogWarning("Position Invalid, Using Default 'random'");
                    sposition = "random";
                }
                if (array.Length > 1)
                {
                    bool flag = false;
                    string text5 = "";
                    foreach (SpawnableEnemyWithRarity enemy in currentLevel.Enemies)
                    {
                        if (enemy.enemyType.enemyName.ToLower().Contains(array[1].ToLower()))
                        {
                            try
                            {
                                flag = true;
                                text5 = enemy.enemyType.enemyName;
                                if (sposition == "random")
                                {
                                    SpawnEnemy(enemy, amount, inside: true, location: new Vector3(0f, 0f, 0f));
                                }
                                else
                                {
                                    SpawnEnemy(enemy, amount, inside: true, location: position);
                                }
                                mls.LogInfo((object)("Spawned " + enemy.enemyType.enemyName));
                            }
                            catch
                            {
                                mls.LogInfo((object)"Could not spawn enemy");
                            }
                            msgbody = "Spawned: " + text5;
                            break;
                        }
                    }
                    if (!flag)
                    {
                        foreach (SpawnableEnemyWithRarity outsideEnemy in currentLevel.OutsideEnemies)
                        {
                            if (outsideEnemy.enemyType.enemyName.ToLower().Contains(array[1].ToLower()))
                            {
                                try
                                {
                                    flag = true;
                                    text5 = outsideEnemy.enemyType.enemyName;
                                    mls.LogInfo((object)outsideEnemy.enemyType.enemyName);
                                    mls.LogInfo((object)("The index of " + outsideEnemy.enemyType.enemyName + " is " + currentLevel.OutsideEnemies.IndexOf(outsideEnemy)));
                                    if (sposition == "random")
                                    {
                                        SpawnEnemy(outsideEnemy, amount, inside: false, location: new Vector3(0f, 0f, 0f));
                                    }
                                    else
                                    {
                                        SpawnEnemy(outsideEnemy, amount, inside: false, location: position);
                                    }
                                    mls.LogInfo((object)("Spawned " + outsideEnemy.enemyType.enemyName));
                                }
                                catch (Exception ex)
                                {
                                    mls.LogInfo((object)"Could not spawn enemy");
                                    mls.LogInfo((object)("The game tossed an error: " + ex.Message));
                                }
                                msgbody = "Spawned "+ amount + " " + text5 + (amount>1?"s":"");
                                break;
                            }
                        }
                    }
                }
            }
            if(command.ToLower().StartsWith("spawnscrap") || command.ToLower().StartsWith("spwscr"))
            {
                string[] segments = (text.Substring(1)).Split(' ');
                if (segments.Length < 2)
                {
                    mls.LogWarning("Missing Arguments For Spawn\n'/spawnscrap <name> (amount=<amount>) (position={random, @me, @<playername>})");
                    msgtitle = "Command Error";
                    msgbody = "Missing Arguments For Spawn\n'/spawnscrap <name> (amount=<amount>) (position={random, @me, @<playername>})";
                    HUDManager.Instance.DisplayTip(msgtitle, msgbody, true, false, "LC_Tip1");
                    return;
                }
                string toSpawn = segments[1];
                int amount = 1;
                string vstate = "alive";
                Vector3 position = Vector3.zero;
                string sposition = "random";
                var args = segments.Skip(2);

                foreach (string arg in args)
                {
                    string[] darg = arg.Split('=');
                    switch (darg[0])
                    {
                        case "a":
                        case "amount":
                            amount = int.Parse(darg[1]);
                            mls.LogInfo($"{amount}");
                            break;
                        case "s":
                        case "state":
                            vstate = darg[1];
                            mls.LogInfo(vstate);
                            break;
                        case "p":
                        case "position":
                            sposition = darg[1];
                            mls.LogInfo(sposition);
                            break;
                        default:
                            break;
                    }
                }


                if (sposition.StartsWith("@"))
                {
                    if (sposition == "@me") position = ((NetworkBehaviour)currentRound.playersManager.localPlayerController).transform.position;
                    else
                    {
                        string pstring = sposition.Substring(1);
                        foreach (PlayerControllerB player in currentRound.playersManager.allPlayerScripts)
                        {
                            if (player.name == pstring)
                            {
                                position = ((NetworkBehaviour)player).transform.position;
                                break;
                            }
                        }
                    }

                }
                else if (sposition != "random")
                {
                    mls.LogWarning("Position Invalid, Using Default 'random'");
                    sposition = "random";
                }
                if (toSpawn == "gun")
                {
                    for (int i = 0; i < currentRound.currentLevel.Enemies.Count(); i++)
                    {
                        if (currentRound.currentLevel.Enemies[i].enemyType.name == "Nutcracker")
                        {
                            GameObject nutcra = UnityEngine.Object.Instantiate(currentRound.currentLevel.Enemies[i].enemyType.enemyPrefab, new Vector3(float.MinValue,float.MinValue,float.MinValue), Quaternion.identity);
                            NutcrackerEnemyAI nutcracomponent = nutcra.GetComponent<NutcrackerEnemyAI>();

                            mls.LogInfo("Spawning " + amount + " gun" + (amount > 1 ? "s" : ""));

                            for (int j = 0; j < amount; j++)
                            {
                                GameObject gameObject = UnityEngine.Object.Instantiate(nutcracomponent.gunPrefab, position, Quaternion.identity, currentRound.spawnedScrapContainer);
                                GrabbableObject component = gameObject.GetComponent<GrabbableObject>();
                                component.startFallingPosition = position;
                                component.targetFloorPosition = component.GetItemFloorPosition(position);
                                component.SetScrapValue(1000);
                                component.NetworkObject.Spawn();
                            }
                            msgtitle = "Spawned gun";
                            msgbody = "Spawned " + amount + " gun" + (amount > 1 ? "s" : "");
                            break;

                        }
                    }
                }

                int len = currentRound.currentLevel.spawnableScrap.Count();
                for (int i = 0; i < len; i++)
                {
                    Item scrap = currentRound.currentLevel.spawnableScrap[i].spawnableItem;
                    if (scrap.spawnPrefab.name.ToLower() == toSpawn)
                    {
                        GameObject objToSpawn = scrap.spawnPrefab;
                        bool ra = sposition == "random";
                        RandomScrapSpawn[] source;
                        List<RandomScrapSpawn> list4 = null;
                        if (ra)
                        {
                            source = UnityEngine.Object.FindObjectsOfType<RandomScrapSpawn>();
                            list4 = ((scrap.spawnPositionTypes != null && scrap.spawnPositionTypes.Count != 0) ? source.Where((RandomScrapSpawn x) => scrap.spawnPositionTypes.Contains(x.spawnableItems) && !x.spawnUsed).ToList() : source.ToList());
                        }

                        mls.LogInfo("Spawning " + amount + " " + objToSpawn.name + (amount > 1 ? "s" : ""));
                        for (int j = 0; j < amount; j++)
                        {
                            if (ra)
                            {
                                RandomScrapSpawn randomScrapSpawn = list4[currentRound.AnomalyRandom.Next(0, list4.Count)];
                                position = currentRound.GetRandomNavMeshPositionInRadiusSpherical(randomScrapSpawn.transform.position, randomScrapSpawn.itemSpawnRange, currentRound.navHit) + Vector3.up * scrap.verticalOffset;
                            }
                            GameObject gameObject = UnityEngine.Object.Instantiate(objToSpawn, position, Quaternion.identity, currentRound.spawnedScrapContainer);
                            GrabbableObject component = gameObject.GetComponent<GrabbableObject>();
                            component.startFallingPosition = position;
                            component.targetFloorPosition = component.GetItemFloorPosition(position);
                            component.SetScrapValue(1000);
                            component.NetworkObject.Spawn();
                        }
                        break;
                    }
                }
            }

            if (command.ToLower().StartsWith(prefix + "weather"))
            {
                msgtitle = "Weather Change";
                string[] array2 = text.Split(new char[1] { ' ' });
                if (array2.Length > 1)
                {
                    if (array2[1].ToLower().Contains("rain"))
                    {
                        currentRound.timeScript.currentLevelWeather = (LevelWeatherType)1;
                        mls.LogInfo((object)("tried to change the weather to " + array2[1]));
                    }
                    if (array2[1].ToLower().Contains("eclipse"))
                    {
                        currentRound.timeScript.currentLevelWeather = (LevelWeatherType)5;
                        mls.LogInfo((object)("tried to change the weather to " + array2[1]));
                    }
                    if (array2[1].ToLower().Contains("flood"))
                    {
                        currentRound.timeScript.currentLevelWeather = (LevelWeatherType)4;
                        mls.LogInfo((object)("tried to change the weather to " + array2[1]));
                    }
                    if (array2[1].ToLower().Contains("dust") || array2[1].ToLower().Contains("fog") || array2[1].ToLower().Contains("mist"))
                    {
                        currentRound.timeScript.currentLevelWeather = (LevelWeatherType)0;
                        mls.LogInfo((object)("tried to change the weather to " + array2[1]));
                    }
                    if (array2[1].ToLower().Contains("storm"))
                    {
                        currentRound.timeScript.currentLevelWeather = (LevelWeatherType)2;
                        mls.LogInfo((object)("tried to change the weather to " + array2[1]));
                    }
                    if (array2[1].ToLower().Contains("none"))
                    {
                        currentRound.timeScript.currentLevelWeather = (LevelWeatherType)(-1);
                        mls.LogInfo((object)("tried to change the weather to " + array2[1]));
                    }
                    msgbody = "tried to change the weather to " + array2[1];
                }
            }
            if (command.ToLower().StartsWith("togglelights"))
            {
                BreakerBox val = UnityEngine.Object.FindObjectOfType<BreakerBox>();
                if ((UnityEngine.Object)(object)val != (UnityEngine.Object)null)
                {
                    msgtitle = "Light Change";
                    if (val.isPowerOn)
                    {
                        currentRound.TurnBreakerSwitchesOff();
                        currentRound.TurnOnAllLights(false);
                        val.isPowerOn = false;
                        msgbody = "Turned the lights off";
                    }
                    else
                    {
                        currentRound.PowerSwitchOnClientRpc();
                        msgbody = "Turned the lights on";
                    }
                }
            }
            if (command.ToLower().StartsWith("buy"))
            {
                msgtitle = "Item Buying";
                Terminal val2 = UnityEngine.Object.FindObjectOfType<Terminal>();
                if ((UnityEngine.Object)(object)val2 != (UnityEngine.Object)null)
                {
                    List<string> list = new List<string>
                    {
                        "Walkie-Talkie", "Pro Flashlight", "Normal Flashlight", "Shovel", "Lockpicker", "Stun Grenade", "Boom Box", "Inhaler", "Stun Gun", "Jet Pack",
                        "Extension Ladder", "Radar Booster"
                    };
                    Dictionary<string, int> dictionary = new Dictionary<string, int>
                    {
                        { "Walkie-Talkie", 0 },
                        { "Pro Flashlight", 4 },
                        { "Normal Flashlight", 1 },
                        { "Shovel", 2 },
                        { "Lockpicker", 3 },
                        { "Stun Grenade", 5 },
                        { "Boom Box", 6 },
                        { "Inhaler", 7 },
                        { "Stun Gun", 8 },
                        { "Jet Pack", 9 },
                        { "Extension Ladder", 10 },
                        { "Radar Booster", 11 }
                    };
                    string[] array3 = text.Split(new char[1] { ' ' });
                    if (array3.Length > 1)
                    {
                        bool flag3 = false;
                        if (array3.Length > 2)
                        {
                            if (!int.TryParse(array3[2], out var result2))
                            {
                                mls.LogInfo((object)("Couldn't parse command [ " + array3[2] + " ]"));
                                return;
                            }
                            foreach (string item in list)
                            {
                                if (item.ToLower().Contains(array3[1]))
                                {
                                    flag3 = true;
                                    List<int> list2 = new List<int>();
                                    for (int i = 0; i < result2; i++)
                                    {
                                        list2.Add(dictionary[item]);
                                    }
                                    val2.BuyItemsServerRpc(list2.ToArray(), val2.groupCredits, 0);
                                    msgbody = "Bought " + result2 + " " + item + "s";
                                    break;
                                }
                            }
                            if (!flag3)
                            {
                                mls.LogInfo((object)("Couldn't figure out what [ " + array3[1] + " ] was."));
                                return;
                            }
                        }
                        if (!flag3)
                        {
                            bool flag4 = false;
                            foreach (string item2 in list)
                            {
                                if (item2.ToLower().Contains(array3[1]))
                                {
                                    flag4 = true;
                                    int[] array4 = new int[1] { dictionary[item2] };
                                    val2.BuyItemsServerRpc(array4, val2.groupCredits, 0);
                                    msgbody = "Bought " + 1 + " " + item2;
                                }
                            }
                            if (!flag4)
                            {
                                mls.LogInfo((object)("Couldn't figure out what [ " + array3[1] + " ] was. Trying via int parser."));
                            }
                            if (!int.TryParse(array3[1], out var result3))
                            {
                                mls.LogInfo((object)("Couldn't figure out what [ " + array3[1] + " ] was. Int parser failed, please try again."));
                                return;
                            }
                            int[] array5 = new int[1] { result3 };
                            val2.BuyItemsServerRpc(array5, val2.groupCredits, 0);
                            msgbody = "Bought item with ID [" + result3 + "]";
                        }
                    }
                }
            }
            if (command.ToLower().Contains("god"))
            {
                ToggleGodMode();
                msgtitle = "God Mode";
                msgbody = "God Mode set to: " + enableGod;
            }
            if (command.ToLower().Contains("speed"))
            {
                ToggleSpeedHack();
                msgbody = "Speed hack set to: " + speedHack;
                msgtitle = "Speed hack";
            }
            if (command.ToLower().StartsWith("morehelp"))
            {
                msgtitle = "More Commands";
                msgbody = "/enemies - See all enemies available to spawn. \n /weather weatherName - Attempt to change weather \n /cheats - list cheat commands";
                DisplayChatMessage("<color=#FF00FF>" + msgtitle + "</color>\n" + msgbody);
            }
            if (command.ToLower().StartsWith("help"))
            {
                msgtitle = "Available Commands";
                msgbody = "/buy item - Buy an item \n /togglelights - Toggle lights inside building \n /spawn - help for spawning \n /morehelp - see more commands \n /credits - List credits";
                DisplayChatMessage("<color=#FF00FF>" + msgtitle + "</color>\n" + msgbody);
            }
            if (command.ToLower().Contains("credits"))
            {
                msgtitle = "Credits";
                msgbody = "ChatCommands by Toemmsen96 and Chrigi";
                DisplayChatMessage("<color=#FF00FF>" + msgtitle + "</color>\n" + msgbody);
            }
            if (command.ToLower().Contains("cheats"))
            {
                msgtitle = "Cheats";
                msgbody = "/god - Toggle GodMode \n /speed - Toggle SpeedHack \n /togglelights - Toggle lights inside building \n /tp - Teleports you to the terminal in your ship, keeping all items on you! \n /tp <playername> teleports you to that player";
                DisplayChatMessage("<color=#FF00FF>" + msgtitle + "</color>\n" + msgbody);
            }
            if (command.ToLower().StartsWith("spawn") && !command.ToLower().StartsWith("spawnenemy") && !command.ToLower().StartsWith("spawnscrap"))
            {
                msgtitle = "How To";
                msgbody = "Spawn an enemy: /spawnenemy or /spweny\n" +
                        "Spawn scrap items: /spawnscrap or /spwscr\n" +
                        "after that put the name of what you want to spawn\n" +
                        "options: a=<num> or amount=<num> for how many to spawn\n" +
                        "p=<pos> or position=<pos> for position where to spawn\n" +
                        "<pos> can be either @me for your coordinates, @playername for coords of player with specific name or random";
                DisplayChatMessage("<color=#FF00FF>" + msgtitle + "</color>\n" + msgbody);
            }



            if (command.ToLower().StartsWith("deadline") || command.ToLower().StartsWith("dl"))
            {
                string[] array5 = text.Split(new char[1] { ' ' });
                if (array5.Length > 1)
                {
                    if (int.TryParse(array5[1], out var result4))
                    {
                        CustomDeadline = result4;
                        msgtitle = "Deadline";
                        msgbody = "Deadline set to: " + CustomDeadline;
                    }
                    else
                    {
                        CustomDeadline = int.MinValue;
                        msgtitle = "Deadline";
                        msgbody = "Deadline set to default";
                    }
                }
                else
                {
                    CustomDeadline = int.MinValue;
                    msgtitle = "Deadline";
                    msgbody = "Deadline set to default";
                }
            }
            if (command.ToLower().StartsWith("tp"))
            {
                string[] array6 = text.Split(new char[1] { ' ' });
                if (array6.Length > 1)
                {
                    string value = array6[1].ToLower();
                    PlayerControllerB[] allPlayerScripts = StartOfRound.Instance.allPlayerScripts;
                    foreach (PlayerControllerB val3 in allPlayerScripts)
                    {
                        if (val3.playerUsername.ToLower().Contains(value))
                        {
                            GameNetworkManager.Instance.localPlayerController.beamUpParticle.Play();
                            GameNetworkManager.Instance.localPlayerController.beamOutBuildupParticle.Play();
                            GameNetworkManager.Instance.localPlayerController.TeleportPlayer(((Component)val3).transform.position, false, 0f, false, true);
                            msgtitle = "Teleported";
                            msgbody = "Teleported to Player:" + val3.playerUsername;
                        }
                    }
                }
                else
                {
                    Terminal val4 = FindObjectOfType<Terminal>();
                    if (val4 != null)
                    {
                        GameNetworkManager.Instance.localPlayerController.beamUpParticle.Play();
                        GameNetworkManager.Instance.localPlayerController.beamOutBuildupParticle.Play();
                        GameNetworkManager.Instance.localPlayerController.TeleportPlayer(((Component)val4).transform.position, false, 0f, false, true);
                        msgtitle = "Teleported";
                        msgbody = "Teleported to Terminal";
                    }
                }
            }
            if (command.ToLower().Contains("enemies"))
            {
                string textToDisplay = "";
                SelectableLevel newLevel = currentLevel;
                msgtitle = "Enemies:";
                if (newLevel == null)
                {
                    DisplayChatMessage("<color=#FF0000>ERROR: </color>Level is null.");
                    Debug.LogError("newLevel is null.");
                    return;
                }

                // Check if levelEnemySpawns is null
                if (levelEnemySpawns == null)
                {
                    DisplayChatMessage("<color=#FF0000>ERROR: </color>levelEnemySpawns is null.");
                    Debug.LogError("levelEnemySpawns is null.");
                    return;
                }

                // Attempt to get value from dictionary, check for null
                if (levelEnemySpawns.TryGetValue(newLevel, out var value))
                {
                    newLevel.Enemies = value;
                    textToDisplay += "<color=#FF00FF>Inside: </color><color=#FFFF00>";
                    msgbody = "<color=#FF00FF>Inside: </color><color=#FFFF00>";

                    if (newLevel.Enemies.Count == 0)
                    {
                        textToDisplay += "None";
                        msgbody += "None";
                    }
                    else
                    {
                        foreach (SpawnableEnemyWithRarity enemy2 in newLevel.Enemies)
                        {
                            mls.LogInfo((object)("Inside: " + enemy2.enemyType.enemyName));
                            textToDisplay += enemy2.enemyType.enemyName + ", ";
                            msgbody += enemy2.enemyType.enemyName + ", ";
                        }
                    }

                    textToDisplay += "\n</color><color=#FF00FF>Outside: </color>";
                    msgbody += "\n</color><color=#FF00FF>Outside: </color>";

                    if (newLevel.OutsideEnemies.Count == 0)
                    {
                        textToDisplay += "None";
                        msgbody += "None";
                    }
                    else
                    {
                        foreach (SpawnableEnemyWithRarity outsideEnemy in newLevel.OutsideEnemies)
                        {
                            mls.LogInfo((object)("Outside: " + outsideEnemy.enemyType.enemyName));
                            textToDisplay += outsideEnemy.enemyType.enemyName + ", ";
                            msgbody += outsideEnemy.enemyType.enemyName + ", ";
                        }
                    }

                    DisplayChatMessage(textToDisplay);
                }
            }

            if (command.ToLower().Contains("money"))
            {
                EnableInfiniteCredits = !EnableInfiniteCredits;
            }
            if (command.ToLower().Contains("chatmessage"))
            {
                DisplayChatMessage("This is a test message");
            }
            if (text.ToLower().Contains("getscrap"))
            {
                int len = currentRound.currentLevel.spawnableScrap.Count();
                string output = currentRound.currentLevel.spawnableScrap[0].spawnableItem.spawnPrefab.name;
                for (int i = 1; i < len; i++)
                {
                    output += ", ";
                    output += currentRound.currentLevel.spawnableScrap[i].spawnableItem.spawnPrefab.name;
                }
                //HUDManager.Instance.DisplayTip("Spawnable Scrap", output);
                DisplayChatMessage("Spawnable Scrap: " + output);
            }
            if(text.ToLower().Contains("infammo") || command.ToLower().Contains("ammo"))
            {
                EnableInfiniteAmmo = !EnableInfiniteAmmo;
                msgtitle = "Infinite Ammo";
                msgbody = "Infinite Ammo: "+ EnableInfiniteAmmo;
            }
            if (command.ToLower().Contains("term"))
            {
                usingTerminal = !usingTerminal;
                if (usingTerminal)
                {
                    msgtitle = "Began Using Terminal";
                    msgbody = " ";
                    Terminal val5 = FindObjectOfType<Terminal>();
                    if (val5 == null)
                    {
                        return;
                    }
                    if (!val5.terminalInUse)
                    {
                        val5.BeginUsingTerminal();
                        HUDManager.Instance.ChangeControlTip(0, string.Empty, true);
                        GameNetworkManager.Instance.localPlayerController.inSpecialInteractAnimation = true;
                    }
                }
                else
                {
                    Terminal val5 = FindObjectOfType<Terminal>();
                    if (val5 == null)
                    {
                        return;
                    }
                    val5.QuitTerminal();
                    GameNetworkManager.Instance.localPlayerController.inSpecialInteractAnimation = false;
                    msgtitle = "Stopped using terminal";
                    msgbody = " ";

                }

            }
            HUDManager.Instance.DisplayTip(msgtitle, msgbody, false, false, "LC_Tip1");
        }
        public static void ProcessCommand(string commandInput)
        {
            ChatCommands.ProcessCommandInput(commandInput);
        }

        public TextMeshProUGUI chatText;

        public static void DisplayChatMessage(string chatMessage)
        {
            string formattedMessage =
                $"<color=#FF00FF>ChatCommands</color>: <color=#FFFF00>{chatMessage}</color>";

            HUDManager.Instance.ChatMessageHistory.Add(formattedMessage);

            UpdateChatText();
        }

        private static void UpdateChatText()
        {
            HUDManager.Instance.chatText.text = string.Join("\n", HUDManager.Instance.ChatMessageHistory);
        }

    }

}
