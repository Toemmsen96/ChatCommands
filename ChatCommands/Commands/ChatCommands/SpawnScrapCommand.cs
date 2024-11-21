using static ChatCommands.Utils;
using UnityEngine;
using System.Linq;
using System;
using Unity.Netcode;
using System.Collections.Generic;

namespace ChatCommands.Commands
{
    internal class SpawnScrapCommand : CustomChatCommand
    {

        public override string Name => "Spawn Scrap";

        public override string Description => "Spawns Scrap at the specified location. Either use the player's position or specify a position. Position, value and amount are optional. Use them with the following format: position=@(playername/me) amount=(number) value=(number)";

        public override string Format => "/spawnscrap [scrapname] ([position]) ([amount]) ([value])";
        public override string AltFormat => "/spwscr [scrapname] ([amount]) ([value]) ([position])";
        public override bool IsHostCommand => true;

        public override void Execute(CommandInput message)
        {
            string msgtitle = "";
            string msgbody = "";
            if (GetCurrentLevel() == null)
            {
                DisplayChatError("Unable to send command since currentLevel is null.");
                return;
            }
            if (message.Args.Count < 1)
            {
                msgtitle = "Command Error";
                msgbody = "Missing Arguments For Spawn\n'/spawnscrap <name> (amount=<amount>) (position={random, @me, @<playername>})";
                DisplayChatError(msgtitle + "\n" + msgbody);
                HUDManager.Instance.DisplayTip(msgtitle, msgbody, true, false, "LC_Tip1");
                return;
            }
            string toSpawn = message.Args[0].ToLower();
            int amount = 1;
            Vector3 position = Vector3.zero;
            string sposition = "random";
            int value = 1000;
            var args = message.Args.Skip(1).ToArray();

            foreach (string arg in args)
            {
                string[] darg = arg.Split('=');
                switch (darg[0])
                {
                    case "v":
                    case "value":
                        value = int.Parse(darg[1]);
                        LogInfo($"Value {value}");
                        break;
                    case "a":
                    case "amount":
                        amount = int.Parse(darg[1]);
                        LogInfo($"Amount {amount}");
                        break;
                    case "p":
                    case "position":
                        sposition = darg[1];
                        LogInfo(sposition);
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
                    LogWarning("Position Invalid, Using Default 'random'");
                    sposition = "random";
                }
            }

            if (toSpawn == "gun")
            {
                for (int i = 0; i < GetCurrentLevel().Enemies.Count(); i++)
                {
                    if (GetCurrentLevel().Enemies[i].enemyType.name == "Nutcracker")
                    {
                        GameObject nutcra = UnityEngine.Object.Instantiate(GetCurrentLevel().Enemies[i].enemyType.enemyPrefab, new Vector3(float.MinValue, float.MinValue, float.MinValue), Quaternion.identity);
                        NutcrackerEnemyAI nutcracomponent = nutcra.GetComponent<NutcrackerEnemyAI>();

                        LogInfo("Spawning " + amount + " gun" + (amount > 1 ? "s" : ""));

                        for (int j = 0; j < amount; j++)
                        {
                            GameObject gameObject = UnityEngine.Object.Instantiate(nutcracomponent.gunPrefab, position, Quaternion.identity, GetCurrentRound().spawnedScrapContainer);
                            GrabbableObject component = gameObject.GetComponent<GrabbableObject>();
                            component.startFallingPosition = position;
                            component.targetFloorPosition = component.GetItemFloorPosition(position);
                            component.SetScrapValue(value); // Set Scrap Value
                            component.NetworkObject.Spawn();
                        }
                        msgtitle = "Spawned gun";
                        msgbody = "Spawned " + amount + " " + "gun" + (amount > 1 ? "s" : "") + "with value of:" + value + "\n at position: " + position;
                        break;
                    }
                }
            }
            int len = GetCurrentLevel().spawnableScrap.Count();
            bool spawnable = false;
            for (int i = 0; i < len; i++)
            {
                Item scrap = GetCurrentLevel().spawnableScrap[i].spawnableItem;
                if (scrap.spawnPrefab.name.ToLower() == toSpawn)
                {
                    GameObject objToSpawn = scrap.spawnPrefab;
                    bool ra = sposition == "random";
                    RandomScrapSpawn[] source;
                    List<RandomScrapSpawn> list4 = null;
                    if (ra)
                    {
                        source = UnityEngine.Object.FindObjectsOfType<RandomScrapSpawn>();
                        list4 = (scrap.spawnPositionTypes != null && scrap.spawnPositionTypes.Count != 0) ? source.Where((RandomScrapSpawn x) => scrap.spawnPositionTypes.Contains(x.spawnableItems) && !x.spawnUsed).ToList() : source.ToList();
                    }

                    LogInfo("Spawning " + amount + " " + objToSpawn.name + (amount > 1 ? "s" : ""));
                    for (int j = 0; j < amount; j++)
                    {
                        if (ra)
                        {
                            RandomScrapSpawn randomScrapSpawn = list4[GetCurrentRound().AnomalyRandom.Next(0, list4.Count)];
                            position = GetCurrentRound().GetRandomNavMeshPositionInRadiusSpherical(randomScrapSpawn.transform.position, randomScrapSpawn.itemSpawnRange, GetCurrentRound().navHit) + Vector3.up * scrap.verticalOffset;
                        }
                        GameObject gameObject = UnityEngine.Object.Instantiate(objToSpawn, position, Quaternion.identity, GetCurrentRound().spawnedScrapContainer);
                        GrabbableObject component = gameObject.GetComponent<GrabbableObject>();
                        component.startFallingPosition = position;
                        component.targetFloorPosition = component.GetItemFloorPosition(position);
                        component.SetScrapValue(value); // Set Scrap Value
                        component.NetworkObject.Spawn();
                        msgtitle = "Spawned " + objToSpawn.name;
                        msgbody = "Spawned " + amount + " " + objToSpawn.name + (amount > 1 ? "s" : "") + " with value of:" + value + "\n at position: " + position;
                    }
                    spawnable = true;
                    break;
                }
            }
            if (!spawnable)
            {
                LogWarning("Could not spawn " + toSpawn);
                msgtitle = "Command Error";
                msgbody = "Could not spawn " + toSpawn +".\nHave you checked using /getscrap if the scrap you are trying to spawn\n is even spawnable?";
            }
            DisplayChatMessage(msgtitle + "\n" + msgbody);
        }
    }
}