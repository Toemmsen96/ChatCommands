using System;
using System.Collections.Generic;
using UnityEngine;
using GameNetcodeStuff;
using Unity.Netcode;
using System.Linq;
using UnityEngine.AI;
using static ChatCommands.Utils;
namespace ChatCommands
{
    public class oldCommands
    {
        

        public static string SpawnMapObj(string text)
        {
            if (ChatCommands.currentLevel == null|| ChatCommands.currentRound.currentLevel.spawnableMapObjects == null)
            {
                ChatCommands.mls.LogWarning("Unable to send command since currentLevel or spawnableMapObjects is null.");
                ChatCommands.msgtitle = "Command Error";
                ChatCommands.msgbody = "Unable to send command since currentLevel or spawnableMapObjects is null.";
                DisplayChatError(ChatCommands.msgtitle + "\n" + ChatCommands.msgbody);
                return ChatCommands.msgbody + "/" + ChatCommands.msgtitle;
            }

            string[] segments = (text.Substring(1)).Split(' ');
            if (segments.Length < 2)
            {
                ChatCommands.mls.LogWarning("Missing Arguments For Spawn\n'/spawnmapobj <name> (amount=<amount>) (position={random, @me, @<playername>})");
                ChatCommands.msgtitle = "Command Error";
                ChatCommands.msgbody = "Missing Arguments For Spawn\n'/spawnmapobj <name> (amount=<amount>) (position={random, @me, @<playername>})";
                DisplayChatError(ChatCommands.msgtitle + "\n" + ChatCommands.msgbody);
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

        public static string SpawnItemFunc(string text){
            string[] segments = (text.Substring(1)).Split(' ');
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
            while (amount > 0)
            {
                ChatCommands.SpawnItems(position, 1);
                amount--;
            }
            ChatCommands.msgtitle = "Spawned Item";
            ChatCommands.msgbody = "Spawned " + amount + " " + toSpawn + (amount > 1 ? "s" : "") + " with value of:" + value + "\n at position: " + position;
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

        public static string GetScrap()
        {
            SelectableLevel newLevel = ChatCommands.currentLevel;
            if (newLevel == null)
            {
                DisplayChatError("Level is null.");
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
            DisplayChatMessage("Spawnable Scrap: " + output);
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
                DisplayChatError("Player "+playername+" not found!!!");
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
            DisplayChatMessage("<color=#FF00FF>" + ChatCommands.msgtitle + "</color>\n" + ChatCommands.msgbody);
            return ChatCommands.msgbody + "/" + ChatCommands.msgtitle;
        }
        public static string GetMoreHelp()
        {
            ChatCommands.msgtitle = "More Commands";
            ChatCommands.msgbody = "/enemies - See all enemies available to spawn. \n /weather weatherName - Attempt to change weather \n /cheats - list cheat commands \n /override - Override Enemy spawns";
            DisplayChatMessage("<color=#FF00FF>" + ChatCommands.msgtitle + "</color>\n" + ChatCommands.msgbody);
            return ChatCommands.msgbody + "/" + ChatCommands.msgtitle;
        }
        public static string GetCredits()
        {
            ChatCommands.msgtitle = "Credits";
            ChatCommands.msgbody = "ChatCommands by Toemmsen96 and Chrigi";
            DisplayChatMessage("<color=#FF00FF>" + ChatCommands.msgtitle + "</color>\n" + ChatCommands.msgbody);
            return ChatCommands.msgbody + "/" + ChatCommands.msgtitle;
        }

        public static string GetCheats()
        {
            ChatCommands.msgtitle = "Cheats";
            ChatCommands.msgbody = "/god - Toggle GodMode \n /speed - Toggle SpeedHack \n /togglelights - Toggle lights inside building \n /tp - Teleports you to the terminal in your ship, keeping all items on you! \n /tp <playername> teleports you to that player";
            DisplayChatMessage("<color=#FF00FF>" + ChatCommands.msgtitle + "</color>\n" + ChatCommands.msgbody);
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
            DisplayChatMessage("<color=#FF00FF>" + ChatCommands.msgtitle + "</color>\n" + ChatCommands.msgbody);
            return ChatCommands.msgbody + "/" + ChatCommands.msgtitle;
        }

        public static string GetPos()
        {
            ChatCommands.msgtitle = "Position";
            ChatCommands.msgbody = "Your Position is: " + ChatCommands.playerRef.transform.position;
            DisplayChatMessage("<color=#FF00FF>" + ChatCommands.msgtitle + "</color>\n" + ChatCommands.msgbody);
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

        

        public static void ToggleOverrideSpawns()
        {
            ChatCommands.OverrideSpawns = !ChatCommands.OverrideSpawns;
        }
    }
}
