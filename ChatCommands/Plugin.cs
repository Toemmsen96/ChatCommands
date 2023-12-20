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
        private static ManualLogSource mls = BepInEx.Logging.Logger.CreateLogSource(modGUID);
        public static Dictionary<SelectableLevel, List<SpawnableEnemyWithRarity>> levelEnemySpawns;
        public static Dictionary<SpawnableEnemyWithRarity, int> enemyRaritys;
        public static Dictionary<SpawnableEnemyWithRarity, AnimationCurve> enemyPropCurves;
        private static SelectableLevel currentLevel;
        private static EnemyVent[] currentLevelVents;
        private static RoundManager currentRound;
        private static ConfigEntry<string> PrefixSetting;
        private static bool enableGod;
        private static bool EnableInfiniteCredits = false;
        private static int CustomDeadline = int.MinValue;
        private static bool usingTerminal = false;
        private static PlayerControllerB playerRef;
        private static bool isHost;
        private static bool speedHack;
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

            mls.LogInfo((object)"Chat Commands loaded!");
        }

        [HarmonyPatch(typeof(PlayerControllerB), "Start")]
        [HarmonyPrefix]
        private static void getPlayerRef(ref PlayerControllerB __instance)
        {
            playerRef = __instance;
        }
        private static bool toggleGodMode()
        {
            if (isHost)
            {
                enableGod = !enableGod;
            }
            return enableGod;
        }

        [HarmonyPatch(typeof(PlayerControllerB), "Update")]
        [HarmonyPostfix]
        private static void speedHackFunc(ref float ___jumpForce, ref float ___sprintMeter, ref float ___sprintMultiplier, ref bool ___isSprinting)
        {
            if (speedHack)
            {
                ___jumpForce = 25f;
                ___sprintMeter = 1f;
                if (___isSprinting) ___sprintMultiplier = 10f;
            }

        }

        private static void toggleSpeedHack()
        {
            if (isHost)
            {
                speedHack = !playerRef.isSpeedCheating;
                playerRef.isSpeedCheating = speedHack;
            }
        }

        [HarmonyPatch(typeof(PlayerControllerB), "AllowPlayerDeath")]
        [HarmonyPrefix]
        private static bool overrideDeath()
        {
            if (isHost)
            {
                return !enableGod;
            }
            return true;
        }

        [HarmonyPatch(typeof(Terminal), "RunTerminalEvents")]
        [HarmonyPostfix]
        private static void infiniteCredits(ref int ___groupCredits)
        {
            if (isHost && EnableInfiniteCredits)
            {
                ___groupCredits = 50000;
            }
        }


        [HarmonyPatch(typeof(TimeOfDay), "SetNewProfitQuota")]
        [HarmonyPostfix]
        private static void patchDeadline(TimeOfDay __instance)
        {

            if (isHost && CustomDeadline != int.MinValue)
            {
                __instance.quotaVariables.deadlineDaysAmount = CustomDeadline;
                __instance.timeUntilDeadline = (float)(__instance.quotaVariables.deadlineDaysAmount + CustomDeadline) * __instance.totalTime;

                TimeOfDay.Instance.timeUntilDeadline = (int)(TimeOfDay.Instance.totalTime * (float)TimeOfDay.Instance.quotaVariables.deadlineDaysAmount);
                TimeOfDay.Instance.SyncTimeClientRpc(__instance.globalTime, (int)__instance.timeUntilDeadline);
                ((TMP_Text)StartOfRound.Instance.deadlineMonitorText).text = "DEADLINE:\n " + TimeOfDay.Instance.daysUntilDeadline;
            }
        }


        [HarmonyPatch(typeof(RoundManager), "Start")]
        [HarmonyPrefix]
        private static void setIsHost()
        {
            mls.LogInfo((object)("Host Status: " + ((NetworkBehaviour)RoundManager.Instance).NetworkManager.IsHost));
            isHost = ((NetworkBehaviour)RoundManager.Instance).NetworkManager.IsHost;
        }

        [HarmonyPatch(typeof(RoundManager), "LoadNewLevel")]
        [HarmonyPrefix]
        private static bool ModifyLevel(ref SelectableLevel newLevel)
        {
            currentRound = RoundManager.Instance;
            if (!levelEnemySpawns.ContainsKey(newLevel))
            {
                List<SpawnableEnemyWithRarity> list = new List<SpawnableEnemyWithRarity>();
                foreach (SpawnableEnemyWithRarity enemy in newLevel.Enemies)
                {
                    list.Add(enemy);
                }
                levelEnemySpawns.Add(newLevel, list);
            }
            levelEnemySpawns.TryGetValue(newLevel, out var value);
            newLevel.Enemies = value;
            foreach (SpawnableEnemyWithRarity enemy2 in newLevel.Enemies)
            {
                mls.LogInfo((object)("Inside: " + enemy2.enemyType.enemyName));
                if (!enemyRaritys.ContainsKey(enemy2))
                {
                    enemyRaritys.Add(enemy2, enemy2.rarity);
                }
                int value2 = 0;
                enemyRaritys.TryGetValue(enemy2, out value2);
                enemy2.rarity = value2;
            }
            foreach (SpawnableEnemyWithRarity outsideEnemy in newLevel.OutsideEnemies)
            {
                mls.LogInfo((object)("Outside: " + outsideEnemy.enemyType.enemyName));
                if (!enemyRaritys.ContainsKey(outsideEnemy))
                {
                    enemyRaritys.Add(outsideEnemy, outsideEnemy.rarity);
                }
                int value3 = 0;
                enemyRaritys.TryGetValue(outsideEnemy, out value3);
                outsideEnemy.rarity = value3;
            }
            foreach (SpawnableEnemyWithRarity enemy3 in newLevel.Enemies)
            {
                if (!enemyPropCurves.ContainsKey(enemy3))
                {
                    enemyPropCurves.Add(enemy3, enemy3.enemyType.probabilityCurve);
                }
                AnimationCurve value4 = new AnimationCurve();
                enemyPropCurves.TryGetValue(enemy3, out value4);
                enemy3.enemyType.probabilityCurve = value4;
            }
            return true;
        }
        [HarmonyPatch(typeof(RoundManager), "LoadNewLevel")]
        [HarmonyPostfix]
        private static void updateNewInfo(ref EnemyVent[] ___allEnemyVents, ref SelectableLevel ___currentLevel)
        {
            currentLevel = ___currentLevel;
            currentLevelVents = ___allEnemyVents;
        }

        [HarmonyPatch(typeof(RoundManager), "AdvanceHourAndSpawnNewBatchOfEnemies")]
        [HarmonyPrefix]
        private static void updateCurrentLevelInfo(ref EnemyVent[] ___allEnemyVents, ref SelectableLevel ___currentLevel)
        {
            currentLevel = ___currentLevel;
            currentLevelVents = ___allEnemyVents;
        }

        [HarmonyPatch(typeof(HUDManager), "SubmitChat_performed")]
        [HarmonyPrefix]
        private static void ChatCommandsSubmitted(HUDManager __instance)
        {
            string text = __instance.chatTextField.text;

            // Log the text to ensure it's not null
            mls.LogInfo($"Received chat input: {text}");

            // Check if text is not null and starts with "/"
            if (!string.IsNullOrEmpty(text) && text.ToLower().StartsWith("/"))
            {
                ProcessCommandInput(text);
                __instance.chatTextField.text = "";
            }
            else
            {
                // Log an error or handle the case where the text is not valid for commands
                Debug.LogError("Invalid or null chat input.");
            }
        }

        private static void SpawnEnemy(SpawnableEnemyWithRarity enemy, int amount, bool inside)
        {
            if (!isHost)
            {
                return;
            }
            mls.LogInfo("SpawnEnemy called...");
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

        [HarmonyPatch(typeof(RoundManager), "SpawnEnemyFromVent")]
        [HarmonyPrefix]
        private static void logSpawnEnemyFromVent()
        {
            mls.LogInfo((object)"Attempting to spawn an enemy");
        }

        [HarmonyPatch(typeof(RoundManager), "EnemyCannotBeSpawned")]
        [HarmonyPrefix]
        private static bool OverrideCannotSpawn()
        {
            return false;
        }

        private static void ProcessCommandInput(string text)
        {
            string text2 = "/";

            if (PrefixSetting.Value != "")
            {
                text2 = PrefixSetting.Value;
            }
            if (!text.ToLower().StartsWith(text2.ToLower()))
            {
                return;
            }
            string text3 = "default";
            string text4 = "unknown error";
            if (!isHost)
            {
                text3 = "Command";
                text4 = "Unable to send command since you are not host.";
                HUDManager.Instance.DisplayTip(text3, text4, false, false, "LC_Tip1");
                return;
            }
            
            if (text.ToLower().StartsWith(text2 + "spawn"))
            {
                text3 = "Spawned Enemies";
                string[] array = text.Split(new char[1] { ' ' });
                if (currentLevel == null || levelEnemySpawns == null || currentLevel.Enemies == null)
                {
                    text3 = "Command";
                    text4 = (currentLevel == null ? "Unable to send command since currentLevel is null." : "Unable to send command since levelEnemySpawns is null.");
                    HUDManager.Instance.DisplayTip(text3, text4, true, false, "LC_Tip1");
                    return;
                }
                if (array.Length == 2)
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
                                SpawnEnemy(enemy, 1, inside: true);
                                mls.LogInfo((object)("Spawned " + enemy.enemyType.enemyName));
                            }
                            catch
                            {
                                mls.LogInfo((object)"Could not spawn enemy");
                            }
                            text4 = "Spawned: " + text5;
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
                                    SpawnEnemy(outsideEnemy, 1, inside: false);
                                    mls.LogInfo((object)("Spawned " + outsideEnemy.enemyType.enemyName));
                                }
                                catch (Exception ex)
                                {
                                    mls.LogInfo((object)"Could not spawn enemy");
                                    mls.LogInfo((object)("The game tossed an error: " + ex.Message));
                                }
                                text4 = "Spawned: " + text5;
                                break;
                            }
                        }
                    }
                }
                if (array.Length > 2)
                {
                    bool flag2 = false;
                    if (int.TryParse(array[2], out var result))
                    {
                        string text6 = "";
                        foreach (SpawnableEnemyWithRarity enemy2 in currentLevel.Enemies)
                        {
                            if (enemy2.enemyType.enemyName.ToLower().Contains(array[1].ToLower()))
                            {
                                flag2 = true;
                                text6 = enemy2.enemyType.enemyName;
                                SpawnEnemy(enemy2, result, inside: true);
                                if (flag2)
                                {
                                    text4 = "Spawned " + result + " " + text6 + "s";
                                    break;
                                }
                            }
                        }
                        if (!flag2)
                        {
                            foreach (SpawnableEnemyWithRarity outsideEnemy2 in currentLevel.OutsideEnemies)
                            {
                                if (outsideEnemy2.enemyType.enemyName.ToLower().Contains(array[1].ToLower()))
                                {
                                    flag2 = true;
                                    text6 = outsideEnemy2.enemyType.enemyName;
                                    try
                                    {
                                        mls.LogInfo((object)("The index of " + outsideEnemy2.enemyType.enemyName + " is " + currentLevel.OutsideEnemies.IndexOf(outsideEnemy2)));
                                        SpawnEnemy(outsideEnemy2, result, inside: false);
                                        mls.LogInfo((object)("Spawned another " + outsideEnemy2.enemyType.enemyName));
                                    }
                                    catch
                                    {
                                        mls.LogInfo((object)"Failed to spawn enemies, check your command.");
                                    }
                                    if (flag2)
                                    {
                                        text4 = "Spawned " + result + " " + text6 + "s";
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        mls.LogInfo((object)"Failed to spawn enemies, check your command.");
                    }
                    mls.LogInfo((object)("Length of input array: " + array.Length));
                }
            }
            if (text.ToLower().StartsWith(text2 + "weather"))
            {
                text3 = "Weather Change";
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
                    text4 = "tried to change the weather to " + array2[1];
                }
            }
            if (text.ToLower().StartsWith(text2 + "togglelights"))
            {
                BreakerBox val = UnityEngine.Object.FindObjectOfType<BreakerBox>();
                if ((UnityEngine.Object)(object)val != (UnityEngine.Object)null)
                {
                    text3 = "Light Change";
                    if (val.isPowerOn)
                    {
                        currentRound.TurnBreakerSwitchesOff();
                        currentRound.TurnOnAllLights(false);
                        val.isPowerOn = false;
                        text4 = "Turned the lights off";
                    }
                    else
                    {
                        currentRound.PowerSwitchOnClientRpc();
                        text4 = "Turned the lights on";
                    }
                }
            }
            if (text.ToLower().StartsWith(text2 + "buy"))
            {
                text3 = "Item Buying";
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
                                    text4 = "Bought " + result2 + " " + item + "s";
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
                                    text4 = "Bought " + 1 + " " + item2;
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
                            text4 = "Bought item with ID [" + result3 + "]";
                        }
                    }
                }
            }
            if (text.ToLower().Contains("god"))
            {
                enableGod = !enableGod;
                text3 = "God Mode";
                text4 = "God Mode set to: " + enableGod;
            }
            if (text.ToLower().Contains("speed"))
            {
                toggleSpeedHack();
                text4 = "Speed hack set to: " + speedHack;
                text3 = "Speed hack";
            }
            if (text.ToLower().Contains("morehelp"))
            {
                text3 = "More Commands";
                text4 = "/tp - Teleports you to the terminal in your ship, keeping all items on you! \n /enemies - See all enemies available to spawn. \n /weather weatherName - Attempt to change weather \n ";
            }
            if (text.ToLower().Contains("help") && !text.ToLower().Contains("morehelp"))
            {
                text3 = "Commands";
                text4 = "/buy item - Buy an item \n /god - Toggle GodMode \n /speed - Toggle SpeedHack \n /togglelights - Toggle all lights inside building \n /spawn enemyName - Attempt to spawn an enemy \n /morehelp - see more commands";
            }

            if (text.ToLower().Contains("deadline") || text.ToLower().Contains("dl"))
            {
                string[] array5 = text.Split(new char[1] { ' ' });
                if (array5.Length > 1)
                {
                    if (int.TryParse(array5[1], out var result4))
                    {
                        CustomDeadline = result4;
                        text3 = "Deadline";
                        text4 = "Deadline set to: " + CustomDeadline;
                    }
                    else
                    {
                        CustomDeadline = int.MinValue;
                        text3 = "Deadline";
                        text4 = "Deadline set to default";
                    }
                }
                else
                {
                    CustomDeadline = int.MinValue;
                    text3 = "Deadline";
                    text4 = "Deadline set to default";
                }
            }
            if (text.ToLower().Contains("tp"))
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
                        }
                    }
                }
                else
                {
                    Terminal val4 = FindObjectOfType<Terminal>();
                    if ((object)val4 != null)
                    {
                        GameNetworkManager.Instance.localPlayerController.beamUpParticle.Play();
                        GameNetworkManager.Instance.localPlayerController.beamOutBuildupParticle.Play();
                        GameNetworkManager.Instance.localPlayerController.TeleportPlayer(((Component)val4).transform.position, false, 0f, false, true);
                    }
                }
            }
            if (text.ToLower().Contains("enemies"))
            {
                string textToDisplay = "";
                SelectableLevel newLevel = currentLevel;
                text3 = "Enemies:";
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
                    text4 = "<color=#FF00FF>Inside: </color><color=#FFFF00>";

                    if (newLevel.Enemies.Count == 0)
                    {
                        textToDisplay += "None";
                        text4 += "None";
                    }
                    else
                    {
                        foreach (SpawnableEnemyWithRarity enemy2 in newLevel.Enemies)
                        {
                            mls.LogInfo((object)("Inside: " + enemy2.enemyType.enemyName));
                            textToDisplay += enemy2.enemyType.enemyName + ", ";
                            text4 += enemy2.enemyType.enemyName + ", ";
                        }
                    }

                    textToDisplay += "\n</color><color=#FF00FF>Outside: </color>";
                    text4 += "\n</color><color=#FF00FF>Outside: </color>";

                    if (newLevel.OutsideEnemies.Count == 0)
                    {
                        textToDisplay += "None";
                        text4 += "None";
                    }
                    else
                    {
                        foreach (SpawnableEnemyWithRarity outsideEnemy in newLevel.OutsideEnemies)
                        {
                            mls.LogInfo((object)("Outside: " + outsideEnemy.enemyType.enemyName));
                            textToDisplay += outsideEnemy.enemyType.enemyName + ", ";
                            text4 += outsideEnemy.enemyType.enemyName + ", ";
                        }
                    }

                    DisplayChatMessage(textToDisplay);
                }
            }

            if (text.ToLower().Contains("money"))
            {
                EnableInfiniteCredits = !EnableInfiniteCredits;
            }
            if (text.ToLower().Contains("chatmessage"))
            {
                DisplayChatMessage("This is a test message");
            }
            if (text.ToLower().Contains("term"))
            {
                usingTerminal = !usingTerminal;
                if (usingTerminal)
                {
                    text3 = "Began Using Terminal";
                    text4 = " ";
                    Terminal val5 = FindObjectOfType<Terminal>();
                    if ((object)val5 == null)
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
                    if ((object)val5 == null)
                    {
                        return;
                    }
                    val5.QuitTerminal();
                    GameNetworkManager.Instance.localPlayerController.inSpecialInteractAnimation = false;
                    text3 = "Stopped using terminal";
                    text4 = " ";

                }

            }
            HUDManager.Instance.DisplayTip(text3, text4, false, false, "LC_Tip1");
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
