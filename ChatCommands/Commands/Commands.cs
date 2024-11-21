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
        private static string msgtitle = "";
        private static string msgbody = "";

        public static string SpawnMapObj(string text)
        {
            
            return msgtitle + "/" + msgbody;
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
        //    return msgbody + "/" + msgtitle;
        //}



        public static string GetHelp()
        {
            msgtitle = "Available Commands";
            msgbody = "/buy item - Buy an item \n /togglelights - Toggle lights inside building \n /spawn - help for spawning \n /morehelp - see more commands \n /credits - List credits";
            DisplayChatMessage("<color=#FF00FF>" + msgtitle + "</color>\n" + msgbody);
            return msgbody + "/" + msgtitle;
        }
        public static string GetMoreHelp()
        {
            msgtitle = "More Commands";
            msgbody = "/enemies - See all enemies available to spawn. \n /weather weatherName - Attempt to change weather \n /cheats - list cheat commands \n /override - Override Enemy spawns";
            DisplayChatMessage("<color=#FF00FF>" + msgtitle + "</color>\n" + msgbody);
            return msgbody + "/" + msgtitle;
        }
        public static string GetCredits()
        {
            msgtitle = "Credits";
            msgbody = "ChatCommands by Toemmsen96 and Chrigi";
            DisplayChatMessage("<color=#FF00FF>" + msgtitle + "</color>\n" + msgbody);
            return msgbody + "/" + msgtitle;
        }

        public static string GetCheats()
        {
            msgtitle = "Cheats";
            msgbody = "/god - Toggle GodMode \n /speed - Toggle SpeedHack \n /togglelights - Toggle lights inside building \n /tp - Teleports you to the terminal in your ship, keeping all items on you! \n /tp <playername> teleports you to that player";
            DisplayChatMessage("<color=#FF00FF>" + msgtitle + "</color>\n" + msgbody);
            return msgbody + "/" + msgtitle;
        }

        public static string GetSpawn()
        {
            msgtitle = "How To";
            msgbody = "Spawn an enemy: /spawnenemy or /spweny\n" +
                    "Spawn scrap items: /spawnscrap or /spwscr\n" +
                    "Spawn map objects: /spawnmapobj or /spwobj\n" +
                    "after that put the name of what you want to spawn\n" +
                    "options: a=<num> or amount=<num> for how many to spawn\n" +
                    "p=<pos> or position=<pos> for position where to spawn\n" +
                    "<pos> can be either @me for your coordinates, @playername for coords of player with specific name or random";
            DisplayChatMessage("<color=#FF00FF>" + msgtitle + "</color>\n" + msgbody);
            return msgbody + "/" + msgtitle;
        }

        public static string GetPos()
        {
            msgtitle = "Position";
            msgbody = "Your Position is: " + ChatCommands.playerRef.transform.position;
            DisplayChatMessage("<color=#FF00FF>" + msgtitle + "</color>\n" + msgbody);
            return msgbody + "/" + msgtitle;
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
