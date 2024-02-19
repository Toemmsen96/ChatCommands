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
using System.Collections;

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
                string tpname = array6[1].ToLower();
                tpname = ChatCommands.ConvertPlayername(tpname);
                PlayerControllerB[] allPlayerScripts = StartOfRound.Instance.allPlayerScripts;
                foreach (PlayerControllerB testedplayer in allPlayerScripts)
                {
                    if (testedplayer.playerUsername.ToLower().Contains(tpname))
                    {
                        GameNetworkManager.Instance.localPlayerController.beamUpParticle.Play();
                        GameNetworkManager.Instance.localPlayerController.beamOutBuildupParticle.Play();
                        GameNetworkManager.Instance.localPlayerController.TeleportPlayer(testedplayer.transform.position, false, 0f, false, true);
                        ChatCommands.msgtitle = "Teleported";
                        ChatCommands.msgbody = "Teleported to Player:" + testedplayer.playerUsername;
                    }
                }
            }
            else
            {
                Terminal term = UnityEngine.Object.FindObjectOfType<Terminal>();
                if (term != null)
                {
                    GameNetworkManager.Instance.localPlayerController.beamUpParticle.Play();
                    GameNetworkManager.Instance.localPlayerController.beamOutBuildupParticle.Play();
                    GameNetworkManager.Instance.localPlayerController.TeleportPlayer(term.transform.position, false, 0f, false, true);
                    ChatCommands.msgtitle = "Teleported";
                    ChatCommands.msgbody = "Teleported to Terminal";
                }
            }
            return ChatCommands.msgbody + "/" + ChatCommands.msgtitle;
        }
        public static string SpawnEnemyFunc(string text)
        {

            ChatCommands.msgtitle = "Spawned Enemies";
            string[] array = text.Split(' ');
            if (ChatCommands.currentLevel == null || ChatCommands.levelEnemySpawns == null || ChatCommands.currentLevel.Enemies == null)
            {
                ChatCommands.msgtitle = "Command";
                ChatCommands.msgbody = (ChatCommands.currentLevel == null ? "Unable to send command since currentLevel is null." : "Unable to send command since levelEnemySpawns is null.");
                ChatCommands.DisplayChatError(ChatCommands.msgtitle + "\n" + ChatCommands.msgbody);
                return ChatCommands.msgbody + "/" + ChatCommands.msgtitle;
            }
            if (array.Length < 2)
            {
                ChatCommands.msgtitle = "Command Error";
                ChatCommands.msgbody = "Missing Arguments For Spawn\n'/spawnenemy <name> (amount=<amount>) (state=<state>) (position={random, @me, @<playername>})";
                ChatCommands.DisplayChatError(ChatCommands.msgtitle + "\n" + ChatCommands.msgbody);
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

            if (sposition != "random")
            {
                position = CalculateSpawnPosition(sposition);
                if (position == Vector3.zero && sposition != "random")
                {
                    ChatCommands.mls.LogWarning("Position Invalid, Using Default 'random'");
                    sposition = "random";
                }
            }

            if (array.Length > 1)
            {
                bool flag = false;
                string enemyName = "";
                foreach (SpawnableEnemyWithRarity enemy in ChatCommands.currentLevel.Enemies)
                {
                    if (enemy.enemyType.enemyName.ToLower().Contains(array[1].ToLower()))
                    {
                        try
                        {
                            flag = true;
                            enemyName = enemy.enemyType.enemyName;
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
                        ChatCommands.msgbody = "Spawned: " + enemyName;
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
                                enemyName = outsideEnemy.enemyType.enemyName;
                                ChatCommands.mls.LogInfo(outsideEnemy.enemyType.enemyName);
                                ChatCommands.mls.LogInfo(("The index of " + outsideEnemy.enemyType.enemyName + " is " + ChatCommands.currentLevel.OutsideEnemies.IndexOf(outsideEnemy)));
                                if (sposition == "random")
                                {
                                    ChatCommands.SpawnEnemy(outsideEnemy, amount, inside: false, location: new Vector3(0f, 0f, 0f));
                                }
                                else
                                {
                                    ChatCommands.SpawnEnemy(outsideEnemy, amount, inside: false, location: position);
                                }
                                ChatCommands.mls.LogInfo(("Spawned " + outsideEnemy.enemyType.enemyName));
                            }
                            catch (Exception ex)
                            {
                                ChatCommands.mls.LogInfo("Could not spawn enemy");
                                ChatCommands.mls.LogInfo(("The game tossed an error: " + ex.Message));
                            }
                            ChatCommands.msgbody = "Spawned " + amount + " " + enemyName + (amount > 1 ? "s" : "");
                            break;
                        }
                    }
                }
            }
            return ChatCommands.msgbody + "/" + ChatCommands.msgtitle;
        }
        public static string ToggleLights()
        {
            BreakerBox breakerBox = UnityEngine.Object.FindObjectOfType<BreakerBox>();
            if (breakerBox != null)
            {
                ChatCommands.msgtitle = "Light Change";
                if (breakerBox.isPowerOn)
                {
                    ChatCommands.currentRound.TurnBreakerSwitchesOff();
                    ChatCommands.currentRound.TurnOnAllLights(false);
                    breakerBox.isPowerOn = false;
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

        public static string SpawnMapObj(string text)
        {
            if (ChatCommands.currentLevel == null|| ChatCommands.currentRound.currentLevel.spawnableMapObjects == null)
            {
                ChatCommands.mls.LogWarning("Unable to send command since currentLevel or spawnableMapObjects is null.");
                ChatCommands.msgtitle = "Command Error";
                ChatCommands.msgbody = "Unable to send command since currentLevel or spawnableMapObjects is null.";
                ChatCommands.DisplayChatError(ChatCommands.msgtitle + "\n" + ChatCommands.msgbody);
                return ChatCommands.msgbody + "/" + ChatCommands.msgtitle;
            }

            string[] segments = (text.Substring(1)).Split(' ');
            if (segments.Length < 2)
            {
                ChatCommands.mls.LogWarning("Missing Arguments For Spawn\n'/spawnmapobj <name> (amount=<amount>) (position={random, @me, @<playername>})");
                ChatCommands.msgtitle = "Command Error";
                ChatCommands.msgbody = "Missing Arguments For Spawn\n'/spawnmapobj <name> (amount=<amount>) (position={random, @me, @<playername>})";
                ChatCommands.DisplayChatError(ChatCommands.msgtitle + "\n" + ChatCommands.msgbody);
                return ChatCommands.msgbody + "/" + ChatCommands.msgtitle;
            }
            string toSpawn = segments[1].ToLower();
            int amount = 1;
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
                        ChatCommands.mls.LogInfo($"Amount {amount}");
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
            if (sposition != "random")
            {
                position = CalculateSpawnPosition(sposition);
                if (position == Vector3.zero && sposition != "random")
                {
                    ChatCommands.mls.LogWarning("Position Invalid, Using Default 'random'");
                    sposition = "random";
                }
            }

            if (toSpawn == "mine")
            {
                if (ChatCommands.mine == -1)
                {
                    ChatCommands.mls.LogWarning("Mine not found");
                    ChatCommands.msgtitle = "Command Error";
                    ChatCommands.msgbody = "Mine not spawnable on map";
                    return ChatCommands.msgbody + "/" + ChatCommands.msgtitle;
                }
                for (int i = 0; i < amount; i++)
                {
                    if (sposition == "random")
                    {
                        //need to implement
                        // position = 
                    }
                    ChatCommands.mls.LogInfo("Spawning mine at position:" + position);
                    GameObject gameObject = UnityEngine.Object.Instantiate(ChatCommands.currentRound.currentLevel.spawnableMapObjects[ChatCommands.mine].prefabToSpawn, position, Quaternion.identity, ChatCommands.currentRound.mapPropsContainer.transform);
                    gameObject.GetComponent<NetworkObject>().Spawn(destroyWithScene: true);
                    ChatCommands.msgtitle = "Spawned mine";
                    ChatCommands.msgbody = "Spawned mine at position:" + position;
                } 
            }
            else if (toSpawn == "turret")
            {
                if (ChatCommands.turret == -1)
                {
                    ChatCommands.mls.LogWarning("Turret not found");
                    ChatCommands.msgtitle = "Command Error";
                    ChatCommands.msgbody = "Turret not spawnable on map";
                    return ChatCommands.msgbody + "/" + ChatCommands.msgtitle;
                }
                for (int i = 0; i < amount; i++)
                {
                    if (sposition == "random")
                    {
                        //need to implement
                        // position = 
                    }
                    ChatCommands.mls.LogInfo("Spawning turret at position:" + position);
                    GameObject gameObject = UnityEngine.Object.Instantiate(ChatCommands.currentRound.currentLevel.spawnableMapObjects[ChatCommands.turret].prefabToSpawn, position, Quaternion.identity, ChatCommands.currentRound.mapPropsContainer.transform);
                    gameObject.GetComponent<NetworkObject>().Spawn(destroyWithScene: true);
                    ChatCommands.msgtitle = "Spawned turret";
                    ChatCommands.msgbody = "Spawned turret at position:" + position;
                }
                    
            }
            return ChatCommands.msgtitle + "/" + ChatCommands.msgbody;
        }

        public static string SpawnScrapFunc(string text)
        {
            if (ChatCommands.currentLevel == null)
            {
                ChatCommands.mls.LogWarning("Unable to send command since currentLevel is null.");
                ChatCommands.msgtitle = "Command Error";
                ChatCommands.msgbody = "Unable to send command since currentLevel is null.";
                ChatCommands.DisplayChatError(ChatCommands.msgtitle + "\n" + ChatCommands.msgbody);
                return ChatCommands.msgbody + "/" + ChatCommands.msgtitle;
            }

            string[] segments = (text.Substring(1)).Split(' ');
            if (segments.Length < 2)
            {
                ChatCommands.mls.LogWarning("Missing Arguments For Spawn\n'/spawnscrap <name> (amount=<amount>) (position={random, @me, @<playername>})");
                ChatCommands.msgtitle = "Command Error";
                ChatCommands.msgbody = "Missing Arguments For Spawn\n'/spawnscrap <name> (amount=<amount>) (position={random, @me, @<playername>})";
                HUDManager.Instance.DisplayTip(ChatCommands.msgtitle, ChatCommands.msgbody, true, false, "LC_Tip1");
                return ChatCommands.msgbody + "/" + ChatCommands.msgtitle;
            }
            string toSpawn = segments[1].ToLower();
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
                        break;
                    default:
                        break;
                }
            }
            if (sposition != "random")
            {
                position = CalculateSpawnPosition(sposition);
                if (position == Vector3.zero && sposition != "random")
                {
                    ChatCommands.mls.LogWarning("Position Invalid, Using Default 'random'");
                    sposition = "random";
                }
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
                            component.SetScrapValue(value); // Set Scrap Value
                            component.NetworkObject.Spawn();
                        }
                        ChatCommands.msgtitle = "Spawned gun";
                        ChatCommands.msgbody = "Spawned " + amount + " " + "gun" + (amount > 1 ? "s" : "") + "with value of:" + value + "\n at position: " + position;
                        break;

                    }
                }
            }
            int len = ChatCommands.currentRound.currentLevel.spawnableScrap.Count();
            bool spawnable = false;
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
                        component.SetScrapValue(value); // Set Scrap Value
                        component.NetworkObject.Spawn();
                        ChatCommands.msgtitle = "Spawned " + objToSpawn.name;
                        ChatCommands.msgbody = "Spawned " + amount + " " + objToSpawn.name + (amount > 1 ? "s" : "") + " with value of:" + value + "\n at position: " + position;
                    }
                    spawnable = true;
                    break;
                }
            }
            if (!spawnable)
            {
                ChatCommands.mls.LogWarning("Could not spawn " + toSpawn);
                ChatCommands.msgtitle = "Command Error";
                ChatCommands.msgbody = "Could not spawn " + toSpawn +".\nHave you checked using /getscrap if the scrap you are trying to spawn\n is even spawnable?";
            }
            return ChatCommands.msgbody + "/" + ChatCommands.msgtitle;
        }

        //public static string SpawnHive(string text)
        //{
        //    GameObject hivePrefab = GameObject.Find("Hive");
        //    EnemyType enemyType = EnemyType;
        //    
        //    Debug.Log($"Setting bee random seed: {StartOfRound.Instance.randomMapSeed + 1314 + enemyType.numberSpawned}");
        //    System.Random random = new System.Random(StartOfRound.Instance.randomMapSeed + 1314 + enemyType.numberSpawned);
        //    Vector3 randomNavMeshPositionInBoxPredictable = RoundManager.Instance.GetRandomNavMeshPositionInBoxPredictable(base.transform.position, 10f, RoundManager.Instance.navHit, random, -5);
        //    Debug.Log($"Set bee hive random position: {randomNavMeshPositionInBoxPredictable}");
        //    GameObject gameObject = UnityEngine.Object.Instantiate(hivePrefab, randomNavMeshPositionInBoxPredictable + Vector3.up * 0.5f, Quaternion.Euler(Vector3.zero), RoundManager.Instance.spawnedScrapContainer);
        //    gameObject.SetActive(value: true);
        //    gameObject.GetComponent<NetworkObject>().Spawn();
        //    gameObject.GetComponent<GrabbableObject>().targetFloorPosition = randomNavMeshPositionInBoxPredictable + Vector3.up * 0.5f;
        //    RedLocustBees.SpawnHiveClientRpc(hiveScrapValue: (!(Vector3.Distance(randomNavMeshPositionInBoxPredictable, StartOfRound.Instance.elevatorTransform.transform.position) < 40f)) ? random.Next(50, 150) : random.Next(40, 100), hiveObject: gameObject.GetComponent<NetworkObject>(), hivePosition: randomNavMeshPositionInBoxPredictable + Vector3.up * 0.5f);
        //
        //
        //    return ChatCommands.msgbody + "/" + ChatCommands.msgtitle;
        //}

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
                        ChatCommands.mls.LogInfo(("tried to change the weather to " + array2[1]));
                        break;
                    case "eclipse":
                        ChatCommands.currentRound.timeScript.currentLevelWeather = (LevelWeatherType)5;
                        ChatCommands.mls.LogInfo(("tried to change the weather to " + array2[1]));
                        break;
                    case "flood":
                        ChatCommands.currentRound.timeScript.currentLevelWeather = (LevelWeatherType)4;
                        ChatCommands.mls.LogInfo(("tried to change the weather to " + array2[1]));
                        break;
                    case "dust":
                    case "fog":
                    case "mist":
                        ChatCommands.currentRound.timeScript.currentLevelWeather = (LevelWeatherType)0;
                        ChatCommands.mls.LogInfo(("tried to change the weather to " + array2[1]));
                        break;
                    case "storm":
                        ChatCommands.currentRound.timeScript.currentLevelWeather = (LevelWeatherType)2;
                        ChatCommands.mls.LogInfo(("tried to change the weather to " + array2[1]));
                        break;
                    case "none":
                        ChatCommands.currentRound.timeScript.currentLevelWeather = (LevelWeatherType)(-1);
                        ChatCommands.mls.LogInfo(("tried to change the weather to " + array2[1]));
                        break;
                    default:
                        ChatCommands.mls.LogInfo(("Couldn't figure out what [ " + array2[1] + " ] was."));
                        ChatCommands.msgbody = "Couldn't figure out what [ " + array2[1] + " ] was.";
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
            Terminal terminal = UnityEngine.Object.FindObjectOfType<Terminal>();
            if (terminal != null)
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
                            ChatCommands.mls.LogInfo(("Couldn't parse command [ " + array3[2] + " ]"));
                            ChatCommands.DisplayChatError("Couldn't parse command [ " + array3[2] + " ]");
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
                                terminal.BuyItemsServerRpc(list2.ToArray(), terminal.groupCredits, 0);
                                ChatCommands.msgbody = "Bought " + result2 + " " + item + "s";
                                break;
                            }
                        }
                        if (!flag3)
                        {
                            ChatCommands.mls.LogInfo(("Couldn't figure out what [ " + array3[1] + " ] was."));
                            ChatCommands.DisplayChatError("Couldn't figure out what [ " + array3[1] + " ] was.");
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
                                terminal.BuyItemsServerRpc(array4, terminal.groupCredits, 0);
                                ChatCommands.msgbody = "Bought " + 1 + " " + item2;
                            }
                        }
                        if (!flag4)
                        {
                            ChatCommands.mls.LogInfo(("Couldn't figure out what [ " + array3[1] + " ] was. Trying via int parser."));
                        }
                        if (!int.TryParse(array3[1], out var result3))
                        {
                            ChatCommands.mls.LogInfo(("Couldn't figure out what [ " + array3[1] + " ] was. Int parser failed, please try again."));
                            ChatCommands.DisplayChatError("Couldn't figure out what [ " + array3[1] + " ] was. Int parser failed, please try again.");
                            return ChatCommands.msgbody + "/" + ChatCommands.msgtitle;
                        }
                        int[] array5 = new int[1] { result3 };
                        terminal.BuyItemsServerRpc(array5, terminal.groupCredits, 0);
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
                ChatCommands.DisplayChatError("Level is null.");
                Debug.LogError("newLevel is null.");
                return ChatCommands.msgbody + "/" + ChatCommands.msgtitle;
            }

            // Check if levelEnemySpawns is null
            if (ChatCommands.levelEnemySpawns == null)
            {
                ChatCommands.DisplayChatError("levelEnemySpawns is null.");
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
                ChatCommands.DisplayChatError("Level is null.");
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

        public static string SetHostCmds(string playername)
        {
            PlayerControllerB[] allPlayerScripts = StartOfRound.Instance.allPlayerScripts;
            bool found = false;
            foreach (PlayerControllerB val3 in allPlayerScripts)
            {
                if (val3.playerUsername.ToLower().Contains(playername.ToLower()))
                {
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                ChatCommands.mls.LogWarning("Player not found");
                ChatCommands.DisplayChatError("Player "+playername+" not found!!!");
                ChatCommands.msgtitle = "Set Host Command allowance";
                ChatCommands.msgbody = "Player not found! Check your command";
                return ChatCommands.msgbody + "/" + ChatCommands.msgtitle;
            }
            else
            {
                bool foundinlist = false;
                ChatCommands.msgtitle = "Set Host Command allowance";
                foreach (AllowedHostPlayer player in ChatCommands.AllowedHostPlayers)
                {
                    if (player.Name.ToLower().Contains(playername.ToLower()))
                    {
                        player.AllowHostCMD = !player.AllowHostCMD;
                        ChatCommands.msgbody = "Host Commands for " + playername + " set to" + player.AllowHostCMD;
                        foundinlist = true;
                        break;
                    }
                }
                if (!foundinlist)
                {
                    ChatCommands.AllowedHostPlayers.Add(new AllowedHostPlayer(playername, true));
                    ChatCommands.msgbody = "Host Commands for " + playername + " set to true";
                }
            }
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
            ChatCommands.msgbody = "/enemies - See all enemies available to spawn. \n /weather weatherName - Attempt to change weather \n /cheats - list cheat commands \n /override - Override Enemy spawns";
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
                    "Spawn map objects: /spawnmapobj or /spwobj\n" +
                    "after that put the name of what you want to spawn\n" +
                    "options: a=<num> or amount=<num> for how many to spawn\n" +
                    "p=<pos> or position=<pos> for position where to spawn\n" +
                    "<pos> can be either @me for your coordinates, @playername for coords of player with specific name or random";
            ChatCommands.DisplayChatMessage("<color=#FF00FF>" + ChatCommands.msgtitle + "</color>\n" + ChatCommands.msgbody);
            return ChatCommands.msgbody + "/" + ChatCommands.msgtitle;
        }

        public static string GetPos()
        {
            ChatCommands.msgtitle = "Position";
            ChatCommands.msgbody = "Your Position is: " + ChatCommands.playerRef.transform.position;
            ChatCommands.DisplayChatMessage("<color=#FF00FF>" + ChatCommands.msgtitle + "</color>\n" + ChatCommands.msgbody);
            return ChatCommands.msgbody + "/" + ChatCommands.msgtitle;
        }

        public static bool CheckPrefix(string text)
        {
            string prefix = "/";

            if (ChatCommands.PrefixSetting.Value != "")
            {
                prefix = ChatCommands.PrefixSetting.Value;
            }
            if (!text.ToLower().StartsWith(prefix.ToLower()))
            {
                return false;
            }
            return true;
        }

        private static Vector3 CalculateSpawnPosition(string sposition)
        {
            Vector3 position = Vector3.zero;
            if (sposition == "random")
            {
                return position;
            }
            if (sposition.StartsWith("@"))
            {
                if (sposition == "@me")
                {
                    PlayerControllerB[] allPlayerScripts = StartOfRound.Instance.allPlayerScripts;
                    foreach (PlayerControllerB testedPlayer in allPlayerScripts)
                    {
                        ChatCommands.mls.LogInfo($"Checking Playername {testedPlayer.playerUsername}");
                        if (testedPlayer.playerUsername.Replace(" ","").ToLower().Contains(ChatCommands.playerwhocalled.ToLower()))
                        {
                            ChatCommands.mls.LogInfo($"Found player {testedPlayer.playerUsername}");
                            position = testedPlayer.transform.position;
                            ChatCommands.msgbody += "@" + testedPlayer.playerUsername;
                            break;
                        }
                    }
                }
                else
                {
                    string origplayername = sposition.Substring(1);
                    string playername = ChatCommands.ConvertPlayername(origplayername);
                    PlayerControllerB[] allPlayerScripts = StartOfRound.Instance.allPlayerScripts;
                    bool found = false;
                    ChatCommands.mls.LogInfo($"Looking for Playername {playername} or Playername {origplayername}...");
                    foreach (PlayerControllerB testedPlayer in allPlayerScripts)
                    {
                        ChatCommands.mls.LogInfo($"Checking Playername {testedPlayer.playerUsername.Replace(" ","")}");
                        if (testedPlayer.playerUsername.Replace(" ", "").ToLower().Contains(playername.ToLower()) || testedPlayer.playerUsername.Replace(" ", "").ToLower().Contains(origplayername.ToLower()))
                        {
                            position = testedPlayer.transform.position;
                            ChatCommands.msgbody += "@" + testedPlayer.playerUsername;
                            found = true;
                            ChatCommands.mls.LogInfo($"Found player {testedPlayer.playerUsername}");
                            break;
                        }
                    }
                    if (!found)
                    {
                        ChatCommands.mls.LogWarning("Player not found");
                        ChatCommands.DisplayChatMessage("Player not found, spawning in random position");
                    }

                }

            }
            else
            {
                string[] pos = sposition.Split(',');
                if (pos.Length == 3)
                {
                    position = new Vector3(float.Parse(pos[0]), float.Parse(pos[1]), float.Parse(pos[2]));
                    ChatCommands.msgbody += "position: " + position;
                }
                else
                {
                    ChatCommands.mls.LogWarning("Position Invalid, Using Default 'random'");
                    ChatCommands.msgbody += "position: " + "random";
                }
            }
            return position;
        }

        public static void ToggleOverrideSpawns()
        {
            ChatCommands.OverrideSpawns = !ChatCommands.OverrideSpawns;
        }
    }
}
