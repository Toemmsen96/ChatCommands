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

namespace ChatCommands
{
    public class Commands
    {
        public static string SetCustomDeadline(string text)
        {
            string[] array5 = text.Split(new char[1] { ' ' });
            if (array5.Length > 1)
            {
                if (int.TryParse(array5[1], out var result4))
                {
                    ChatCommands.CustomDeadline = result4;
                    ChatCommands.msgtitle = "Deadline";
                    ChatCommands.msgbody = "Deadline set to: " + ChatCommands.CustomDeadline;
                }
                else
                {
                    ChatCommands.CustomDeadline = int.MinValue;
                    ChatCommands.msgtitle = "Deadline";
                    ChatCommands.msgbody = "Deadline set to default";
                }
            }
            else
            {
                ChatCommands.CustomDeadline = int.MinValue;
                ChatCommands.msgtitle = "Deadline";
                ChatCommands.msgbody = "Deadline set to default";
            }
            return ChatCommands.msgbody + "/" + ChatCommands.msgtitle;
        }

        public static string Teleport(string text)
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
                        ChatCommands.msgtitle = "Teleported";
                        ChatCommands.msgbody = "Teleported to Player:" + val3.playerUsername;
                    }
                }
            }
            else
            {
                Terminal val4 = ChatCommands.FindObjectOfType<Terminal>();
                if (val4 != null)
                {
                    GameNetworkManager.Instance.localPlayerController.beamUpParticle.Play();
                    GameNetworkManager.Instance.localPlayerController.beamOutBuildupParticle.Play();
                    GameNetworkManager.Instance.localPlayerController.TeleportPlayer(((Component)val4).transform.position, false, 0f, false, true);
                    ChatCommands.msgtitle = "Teleported";
                    ChatCommands.msgbody = "Teleported to Terminal";
                }
            }
            return ChatCommands.msgbody + "/" + ChatCommands.msgtitle;
        }
        public static string SpawnEnemyFunc(string text, string playerwhocalled)
        {
            ChatCommands.msgtitle = "Spawned Enemies";
            string[] array = text.Split(' ');
            if (ChatCommands.currentLevel == null || ChatCommands.levelEnemySpawns == null || ChatCommands.currentLevel.Enemies == null)
            {
                ChatCommands.msgtitle = "Command";
                ChatCommands.msgbody = (ChatCommands.currentLevel == null ? "Unable to send command since currentLevel is null." : "Unable to send command since levelEnemySpawns is null.");
                HUDManager.Instance.DisplayTip(ChatCommands.msgtitle, ChatCommands.msgbody, true, false, "LC_Tip1");
                return ChatCommands.msgbody + "/" + ChatCommands.msgtitle;
            }
            if (array.Length < 2)
            {
                ChatCommands.msgtitle = "Command Error";
                ChatCommands.msgbody = "Missing Arguments For Spawn\n'/spawnenemy <name> (amount=<amount>) (state=<state>) (position={random, @me, @<playername>})";
                HUDManager.Instance.DisplayTip(ChatCommands.msgtitle, ChatCommands.msgbody, true, false, "LC_Tip1");
                ChatCommands.mls.LogWarning("Missing Arguments For Spawn\n'/spawnenemy <name> (amount=<amount>) (state=<state>) (position={random, @me, @<playername>})");
                return ChatCommands.msgbody + "/" + ChatCommands.msgtitle;
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
                        ChatCommands.mls.LogInfo($"{amount}");
                        break;
                    case "s":
                    case "state":
                        vstate = darg[1];
                        ChatCommands.mls.LogInfo(vstate);
                        break;
                    case "p":
                    case "position":
                        sposition = darg[1];
                        ChatCommands.mls.LogInfo(sposition);
                        break;
                    default:
                        break;
                }
            }

            if (sposition.StartsWith("@"))
            {
                if (sposition == "@me") position = ((NetworkBehaviour)ChatCommands.currentRound.playersManager.localPlayerController).transform.position;
                else
                {
                    string playername = sposition.ToLower().Substring(1);
                    PlayerControllerB[] allPlayerScripts = StartOfRound.Instance.allPlayerScripts;
                    foreach (PlayerControllerB val3 in allPlayerScripts)
                    {
                        if (val3.playerUsername.ToLower().Contains(playername))
                        {
                            position = ((Component)val3).transform.position;
                            ChatCommands.msgbody += "@" + val3.playerUsername;
                            break;
                        }
                    }
                }
            }
            else if (sposition != "random")
            {
                ChatCommands.mls.LogWarning("Position Invalid, Using Default 'random'");
                sposition = "random";
            }
            if (array.Length > 1)
            {
                bool flag = false;
                string text5 = "";
                foreach (SpawnableEnemyWithRarity enemy in ChatCommands.currentLevel.Enemies)
                {
                    if (enemy.enemyType.enemyName.ToLower().Contains(array[1].ToLower()))
                    {
                        try
                        {
                            flag = true;
                            text5 = enemy.enemyType.enemyName;
                            if (sposition == "random")
                            {
                                ChatCommands.SpawnEnemy(enemy, amount, inside: true, location: new Vector3(0f, 0f, 0f));
                            }
                            else
                            {
                                ChatCommands.SpawnEnemy(enemy, amount, inside: true, location: position);
                            }
                            ChatCommands.mls.LogInfo((object)("Spawned " + enemy.enemyType.enemyName));
                        }
                        catch
                        {
                            ChatCommands.mls.LogInfo((object)"Could not spawn enemy");
                        }
                        ChatCommands.msgbody = "Spawned: " + text5;
                        break;
                    }
                }
                if (!flag)
                {
                    foreach (SpawnableEnemyWithRarity outsideEnemy in ChatCommands.currentLevel.OutsideEnemies)
                    {
                        if (outsideEnemy.enemyType.enemyName.ToLower().Contains(array[1].ToLower()))
                        {
                            try
                            {
                                flag = true;
                                text5 = outsideEnemy.enemyType.enemyName;
                                ChatCommands.mls.LogInfo((object)outsideEnemy.enemyType.enemyName);
                                ChatCommands.mls.LogInfo((object)("The index of " + outsideEnemy.enemyType.enemyName + " is " + ChatCommands.currentLevel.OutsideEnemies.IndexOf(outsideEnemy)));
                                if (sposition == "random")
                                {
                                    ChatCommands.SpawnEnemy(outsideEnemy, amount, inside: false, location: new Vector3(0f, 0f, 0f));
                                }
                                else
                                {
                                    ChatCommands.SpawnEnemy(outsideEnemy, amount, inside: false, location: position);
                                }
                                ChatCommands.mls.LogInfo((object)("Spawned " + outsideEnemy.enemyType.enemyName));
                            }
                            catch (Exception ex)
                            {
                                ChatCommands.mls.LogInfo((object)"Could not spawn enemy");
                                ChatCommands.mls.LogInfo((object)("The game tossed an error: " + ex.Message));
                            }
                            ChatCommands.msgbody = "Spawned " + amount + " " + text5 + (amount > 1 ? "s" : "");
                            break;
                        }
                    }
                }
            }
            return ChatCommands.msgbody + "/" + ChatCommands.msgtitle;
        }
        public static string ToggleLights(string text)
        {
            BreakerBox val = UnityEngine.Object.FindObjectOfType<BreakerBox>();
            if ((UnityEngine.Object)(object)val != (UnityEngine.Object)null)
            {
                ChatCommands.msgtitle = "Light Change";
                if (val.isPowerOn)
                {
                    ChatCommands.currentRound.TurnBreakerSwitchesOff();
                    ChatCommands.currentRound.TurnOnAllLights(false);
                    val.isPowerOn = false;
                    ChatCommands.msgbody = "Turned the lights off";
                }
                else
                {
                    ChatCommands.currentRound.PowerSwitchOnClientRpc();
                    ChatCommands.msgbody = "Turned the lights on";
                }
            }
            return ChatCommands.msgbody + "/" + ChatCommands.msgtitle;
        }

        public static string SpawnScrapFunc(string text, string playerwhocalled)
        {
            string[] segments = (text.Substring(1)).Split(' ');
            if (segments.Length < 2)
            {
                ChatCommands.mls.LogWarning("Missing Arguments For Spawn\n'/spawnscrap <name> (amount=<amount>) (position={random, @me, @<playername>})");
                ChatCommands.msgtitle = "Command Error";
                ChatCommands.msgbody = "Missing Arguments For Spawn\n'/spawnscrap <name> (amount=<amount>) (position={random, @me, @<playername>})";
                HUDManager.Instance.DisplayTip(ChatCommands.msgtitle, ChatCommands.msgbody, true, false, "LC_Tip1");
                return ChatCommands.msgbody + "/" + ChatCommands.msgtitle;
            }
            string toSpawn = segments[1];
            int amount = 1;
            Vector3 position = Vector3.zero;
            string sposition = "random";
            int value = 1000;
            var args = segments.Skip(2);

            foreach (string arg in args)
            {
                string[] darg = arg.Split('=');
                switch (darg[0])
                {
                    case "v":
                    case "value":
                        value = int.Parse(darg[1]);
                        ChatCommands.mls.LogInfo($"Value {value}");
                        break;
                    case "a":
                    case "amount":
                        amount = int.Parse(darg[1]);
                        ChatCommands.mls.LogInfo($"Amount {amount}");
                        break;
                    case "p":
                    case "position":
                        sposition = darg[1];
                        ChatCommands.mls.LogInfo(sposition);
                        if (sposition.StartsWith("@"))
                        {
                            if (sposition == "@me") position = ((NetworkBehaviour)ChatCommands.currentRound.playersManager.localPlayerController).transform.position;
                            else
                            {

                                string playername = sposition.Substring(1);
                                PlayerControllerB[] allPlayerScripts = StartOfRound.Instance.allPlayerScripts;
                                foreach (PlayerControllerB val3 in allPlayerScripts)
                                {
                                    if (val3.playerUsername.ToLower().Contains(playername))
                                    {
                                        position = ((Component)val3).transform.position;
                                        ChatCommands.msgbody += "@" + val3.playerUsername;
                                        break;
                                    }
                                }
                            }

                        }
                        else if (sposition != "random")
                        {
                            ChatCommands.mls.LogWarning("Position Invalid, Using Default 'random'");
                            sposition = "random";
                        }
                        if (toSpawn == "gun")
                        {
                            for (int i = 0; i < ChatCommands.currentRound.currentLevel.Enemies.Count(); i++)
                            {
                                if (ChatCommands.currentRound.currentLevel.Enemies[i].enemyType.name == "Nutcracker")
                                {
                                    GameObject nutcra = UnityEngine.Object.Instantiate(ChatCommands.currentRound.currentLevel.Enemies[i].enemyType.enemyPrefab, new Vector3(float.MinValue, float.MinValue, float.MinValue), Quaternion.identity);
                                    NutcrackerEnemyAI nutcracomponent = nutcra.GetComponent<NutcrackerEnemyAI>();

                                    ChatCommands.mls.LogInfo("Spawning " + amount + " gun" + (amount > 1 ? "s" : ""));

                                    for (int j = 0; j < amount; j++)
                                    {
                                        GameObject gameObject = UnityEngine.Object.Instantiate(nutcracomponent.gunPrefab, position, Quaternion.identity, ChatCommands.currentRound.spawnedScrapContainer);
                                        GrabbableObject component = gameObject.GetComponent<GrabbableObject>();
                                        component.startFallingPosition = position;
                                        component.targetFloorPosition = component.GetItemFloorPosition(position);
                                        component.SetScrapValue(value);
                                        component.NetworkObject.Spawn();
                                    }
                                    ChatCommands.msgtitle = "Spawned gun";
                                    ChatCommands.msgbody = "Spawned " + amount + " gun" + (amount > 1 ? "s" : "");
                                    break;

                                }
                            }
                        }
                        int len = ChatCommands.currentRound.currentLevel.spawnableScrap.Count();
                        for (int i = 0; i < len; i++)
                        {
                            Item scrap = ChatCommands.currentRound.currentLevel.spawnableScrap[i].spawnableItem;
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

                                ChatCommands.mls.LogInfo("Spawning " + amount + " " + objToSpawn.name + (amount > 1 ? "s" : ""));
                                for (int j = 0; j < amount; j++)
                                {
                                    if (ra)
                                    {
                                        RandomScrapSpawn randomScrapSpawn = list4[ChatCommands.currentRound.AnomalyRandom.Next(0, list4.Count)];
                                        position = ChatCommands.currentRound.GetRandomNavMeshPositionInRadiusSpherical(randomScrapSpawn.transform.position, randomScrapSpawn.itemSpawnRange, ChatCommands.currentRound.navHit) + Vector3.up * scrap.verticalOffset;
                                    }
                                    GameObject gameObject = UnityEngine.Object.Instantiate(objToSpawn, position, Quaternion.identity, ChatCommands.currentRound.spawnedScrapContainer);
                                    GrabbableObject component = gameObject.GetComponent<GrabbableObject>();
                                    component.startFallingPosition = position;
                                    component.targetFloorPosition = component.GetItemFloorPosition(position);
                                    component.SetScrapValue(value);
                                    component.NetworkObject.Spawn();
                                }
                                break;
                            }
                        }
                        break;
                }
            }
            return ChatCommands.msgbody + "/" + ChatCommands.msgtitle;
        }

        public static string ChangeWeather(string text)
        {
            ChatCommands.msgtitle = "Weather Change";
            string[] array2 = text.Split(new char[1] { ' ' });
            if (array2.Length > 1)
            {
                switch (array2[1].ToLower())
                {
                    case "rain":
                        ChatCommands.currentRound.timeScript.currentLevelWeather = (LevelWeatherType)1;
                        ChatCommands.mls.LogInfo((object)("tried to change the weather to " + array2[1]));
                        break;
                    case "eclipse":
                        ChatCommands.currentRound.timeScript.currentLevelWeather = (LevelWeatherType)5;
                        ChatCommands.mls.LogInfo((object)("tried to change the weather to " + array2[1]));
                        break;
                    case "flood":
                        ChatCommands.currentRound.timeScript.currentLevelWeather = (LevelWeatherType)4;
                        ChatCommands.mls.LogInfo((object)("tried to change the weather to " + array2[1]));
                        break;
                    case "dust":
                    case "fog":
                    case "mist":
                        ChatCommands.currentRound.timeScript.currentLevelWeather = (LevelWeatherType)0;
                        ChatCommands.mls.LogInfo((object)("tried to change the weather to " + array2[1]));
                        break;
                    case "storm":
                        ChatCommands.currentRound.timeScript.currentLevelWeather = (LevelWeatherType)2;
                        ChatCommands.mls.LogInfo((object)("tried to change the weather to " + array2[1]));
                        break;
                    case "none":
                        ChatCommands.currentRound.timeScript.currentLevelWeather = (LevelWeatherType)(-1);
                        ChatCommands.mls.LogInfo((object)("tried to change the weather to " + array2[1]));
                        break;
                    default:
                        ChatCommands.mls.LogInfo((object)("Couldn't figure out what [ " + array2[1] + " ] was."));
                        break;
                }
                ChatCommands.msgbody = "tried to change the weather to " + array2[1];
            }
            return ChatCommands.msgbody + "/" + ChatCommands.msgtitle;
        }

        public static string TerminalFunc()
        {
            ChatCommands.usingTerminal = !ChatCommands.usingTerminal;
            if (ChatCommands.usingTerminal)
            {
                ChatCommands.msgtitle = "Began Using Terminal";
                ChatCommands.msgbody = " ";
                Terminal val5 = ChatCommands.FindObjectOfType<Terminal>();
                if (val5 == null)
                {
                    return ChatCommands.msgbody + "/" + ChatCommands.msgtitle;
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
                Terminal val5 = ChatCommands.FindObjectOfType<Terminal>();
                if (val5 == null)
                {
                    return ChatCommands.msgbody + "/" + ChatCommands.msgtitle; ;
                }
                val5.QuitTerminal();
                GameNetworkManager.Instance.localPlayerController.inSpecialInteractAnimation = false;
                ChatCommands.msgtitle = "Stopped using terminal";
                ChatCommands.msgbody = " ";

            }
            return ChatCommands.msgbody + "/" + ChatCommands.msgtitle;
        }


        public static string BuyFunc(string text)
        {
            ChatCommands.msgtitle = "Item Buying";
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
                            ChatCommands.mls.LogInfo((object)("Couldn't parse command [ " + array3[2] + " ]"));
                            return ChatCommands.msgbody + "/" + ChatCommands.msgtitle;
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
                                ChatCommands.msgbody = "Bought " + result2 + " " + item + "s";
                                break;
                            }
                        }
                        if (!flag3)
                        {
                            ChatCommands.mls.LogInfo((object)("Couldn't figure out what [ " + array3[1] + " ] was."));
                            return ChatCommands.msgbody + "/" + ChatCommands.msgtitle;
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
                                ChatCommands.msgbody = "Bought " + 1 + " " + item2;
                            }
                        }
                        if (!flag4)
                        {
                            ChatCommands.mls.LogInfo((object)("Couldn't figure out what [ " + array3[1] + " ] was. Trying via int parser."));
                        }
                        if (!int.TryParse(array3[1], out var result3))
                        {
                            ChatCommands.mls.LogInfo((object)("Couldn't figure out what [ " + array3[1] + " ] was. Int parser failed, please try again."));
                            return ChatCommands.msgbody + "/" + ChatCommands.msgtitle;
                        }
                        int[] array5 = new int[1] { result3 };
                        val2.BuyItemsServerRpc(array5, val2.groupCredits, 0);
                        ChatCommands.msgbody = "Bought item with ID [" + result3 + "]";
                    }
                }
            }
            return ChatCommands.msgbody + "/" + ChatCommands.msgtitle;
        }

        public static string GetEnemies()
        {
            string textToDisplay = "";
            SelectableLevel newLevel = ChatCommands.currentLevel;
            ChatCommands.msgtitle = "Enemies:";
            if (newLevel == null)
            {
                ChatCommands.DisplayChatMessage("<color=#FF0000>ERROR: </color>Level is null.");
                Debug.LogError("newLevel is null.");
                return ChatCommands.msgbody + "/" + ChatCommands.msgtitle;
            }

            // Check if levelEnemySpawns is null
            if (ChatCommands.levelEnemySpawns == null)
            {
                ChatCommands.DisplayChatMessage("<color=#FF0000>ERROR: </color>levelEnemySpawns is null.");
                Debug.LogError("levelEnemySpawns is null.");
                return ChatCommands.msgbody + "/" + ChatCommands.msgtitle;
            }

            // Attempt to get value from dictionary, check for null
            if (ChatCommands.levelEnemySpawns.TryGetValue(newLevel, out var value))
            {
                newLevel.Enemies = value;
                textToDisplay += "<color=#FF00FF>Inside: </color><color=#FFFF00>";
                ChatCommands.msgbody = "<color=#FF00FF>Inside: </color><color=#FFFF00>";

                if (newLevel.Enemies.Count == 0)
                {
                    textToDisplay += "None";
                    ChatCommands.msgbody += "None";
                }
                else
                {
                    foreach (SpawnableEnemyWithRarity enemy2 in newLevel.Enemies)
                    {
                        ChatCommands.mls.LogInfo((object)("Inside: " + enemy2.enemyType.enemyName));
                        textToDisplay += enemy2.enemyType.enemyName + ", ";
                        ChatCommands.msgbody += enemy2.enemyType.enemyName + ", ";
                    }
                }

                textToDisplay += "\n</color><color=#FF00FF>Outside: </color>";
                ChatCommands.msgbody += "\n</color><color=#FF00FF>Outside: </color>";

                if (newLevel.OutsideEnemies.Count == 0)
                {
                    textToDisplay += "None";
                    ChatCommands.msgbody += "None";
                }
                else
                {
                    foreach (SpawnableEnemyWithRarity outsideEnemy in newLevel.OutsideEnemies)
                    {
                        ChatCommands.mls.LogInfo((object)("Outside: " + outsideEnemy.enemyType.enemyName));
                        textToDisplay += outsideEnemy.enemyType.enemyName + ", ";
                        ChatCommands.msgbody += outsideEnemy.enemyType.enemyName + ", ";
                    }
                }

                ChatCommands.DisplayChatMessage(textToDisplay);
            }
            return ChatCommands.msgbody + "/" + ChatCommands.msgtitle;
        }
        public static string GetScrap()
        {
            SelectableLevel newLevel = ChatCommands.currentLevel;
            if (newLevel == null)
            {
                ChatCommands.DisplayChatMessage("<color=#FF0000>ERROR: </color>Level is null.");
                Debug.LogError("Current Level is null.");
                return ChatCommands.msgbody + "/" + ChatCommands.msgtitle;
            }
            int len = ChatCommands.currentRound.currentLevel.spawnableScrap.Count();
            string output = ChatCommands.currentRound.currentLevel.spawnableScrap[0].spawnableItem.spawnPrefab.name;

            for (int i = 1; i < len; i++)
            {
                output += ", ";
                output += ChatCommands.currentRound.currentLevel.spawnableScrap[i].spawnableItem.spawnPrefab.name;
            }
            //HUDManager.Instance.DisplayTip("Spawnable Scrap", output);
            ChatCommands.msgtitle = "Spawnable Scrap";
            ChatCommands.msgbody = "listed in chat";
            ChatCommands.DisplayChatMessage("Spawnable Scrap: " + output);
            return ChatCommands.msgbody + "/" + ChatCommands.msgtitle;
        }

        public static string GetHelp()
        {
            ChatCommands.msgtitle = "Available Commands";
            ChatCommands.msgbody = "/buy item - Buy an item \n /togglelights - Toggle lights inside building \n /spawn - help for spawning \n /morehelp - see more commands \n /credits - List credits";
            ChatCommands.DisplayChatMessage("<color=#FF00FF>" + ChatCommands.msgtitle + "</color>\n" + ChatCommands.msgbody);
            return ChatCommands.msgbody + "/" + ChatCommands.msgtitle;
        }
        public static string GetMoreHelp()
        {
            ChatCommands.msgtitle = "More Commands";
            ChatCommands.msgbody = "/enemies - See all enemies available to spawn. \n /weather weatherName - Attempt to change weather \n /cheats - list cheat commands";
            ChatCommands.DisplayChatMessage("<color=#FF00FF>" + ChatCommands.msgtitle + "</color>\n" + ChatCommands.msgbody);
            return ChatCommands.msgbody + "/" + ChatCommands.msgtitle;
        }
        public static string GetCredits()
        {
            ChatCommands.msgtitle = "Credits";
            ChatCommands.msgbody = "ChatCommands by Toemmsen96 and Chrigi";
            ChatCommands.DisplayChatMessage("<color=#FF00FF>" + ChatCommands.msgtitle + "</color>\n" + ChatCommands.msgbody);
            return ChatCommands.msgbody + "/" + ChatCommands.msgtitle;
        }

        public static string GetCheats()
        {
            ChatCommands.msgtitle = "Cheats";
            ChatCommands.msgbody = "/god - Toggle GodMode \n /speed - Toggle SpeedHack \n /togglelights - Toggle lights inside building \n /tp - Teleports you to the terminal in your ship, keeping all items on you! \n /tp <playername> teleports you to that player";
            ChatCommands.DisplayChatMessage("<color=#FF00FF>" + ChatCommands.msgtitle + "</color>\n" + ChatCommands.msgbody);
            return ChatCommands.msgbody + "/" + ChatCommands.msgtitle;
        }

        public static string GetSpawn()
        {
            ChatCommands.msgtitle = "How To";
            ChatCommands.msgbody = "Spawn an enemy: /spawnenemy or /spweny\n" +
                    "Spawn scrap items: /spawnscrap or /spwscr\n" +
                    "after that put the name of what you want to spawn\n" +
                    "options: a=<num> or amount=<num> for how many to spawn\n" +
                    "p=<pos> or position=<pos> for position where to spawn\n" +
                    "<pos> can be either @me for your coordinates, @playername for coords of player with specific name or random";
            ChatCommands.DisplayChatMessage("<color=#FF00FF>" + ChatCommands.msgtitle + "</color>\n" + ChatCommands.msgbody);
            return ChatCommands.msgbody + "/" + ChatCommands.msgtitle;
        }
    }
}
