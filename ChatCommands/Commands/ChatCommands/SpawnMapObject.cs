using static ChatCommands.Utils;
using UnityEngine;
using System.Linq;
using System;
using Unity.Netcode;
using System.Collections.Generic;

namespace ChatCommands.Commands
{
    internal class SpawnMapObject : CustomChatCommand
    {

        public override string Name => "Spawn Map Object";

        public override string Description => "Spawns Map Object at the specified location. Either use the player's position or specify a position. Position and amount are optional. Use them with the following format: position=@(playername/me) amount=(number)";

        public override string Format => "/spawnobject [objectname] ([position]) ([amount])";
        public override string AltFormat => "/spwobj [objectname] ([amount]) ([position])";
        public override bool IsHostCommand => true;

        public override void Execute(CommandInput message)
        {
            if (ChatCommands.currentLevel == null|| ChatCommands.currentRound.currentLevel.spawnableMapObjects == null)
            {
                LogError("Unable to send command since currentLevel or spawnableMapObjects is null.");
                DisplayChatError("Unable to send command since currentLevel or spawnableMapObjects is null.");
                return;
            }

            if (message.Args.Count < 1)
            {
                LogError("Missing Arguments For Spawn\n'/spawnmapobj <name> (amount=<amount>) (position={random, @me, @<playername>})");
                DisplayChatError("Missing Arguments For Spawn\n'/spawnmapobj <name> (amount=<amount>) (position={random, @me, @<playername>})");
                return;
            }
            string toSpawn = message.Args[0].ToLower();
            int amount = 1;
            Vector3 position = Vector3.zero;
            string sposition = "random";
            var args = message.Args.Skip(1);

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

            if (toSpawn == "mine" || toSpawn == "landmine")
            {
                if (Patches.Patches.minePrefab == null)
                {
                    ChatCommands.mls.LogWarning("Mine not found");
                    return;
                }
                for (int i = 0; i < amount; i++)
                {
                    if (sposition == "random")
                    {
                        if (UnityEngine.Random.value > 0.5f)
                        {
                            position = ChatCommands.currentRound.allEnemyVents[UnityEngine.Random.Range(0, ChatCommands.currentRound.allEnemyVents.Length)].floorNode.position;
                        }
                        else
                        {
                            position = GameObject.FindGameObjectsWithTag("OutsideAINode")[UnityEngine.Random.Range(0, GameObject.FindGameObjectsWithTag("OutsideAINode").Length)].transform.position;
                        }
                    }
                    
                    ChatCommands.mls.LogInfo("Spawning mine at position:" + position);
                    GameObject gameObject = UnityEngine.Object.Instantiate(Patches.Patches.minePrefab, position, Quaternion.identity, ChatCommands.currentRound.mapPropsContainer.transform);
                    gameObject.GetComponent<NetworkObject>().Spawn(destroyWithScene: true);
                    DisplayChatMessage("Spawned mine at position:" + position);
                } 
            }
            else if (toSpawn == "turret")
            {
                if (Patches.Patches.turretPrefab == null)
                {
                    LogWarning("Turret not found");
                    return;
                }
                for (int i = 0; i < amount; i++)
                {
                    if (sposition == "random")
                    {

                        if (UnityEngine.Random.value > 0.5f)
                        {
                            position = ChatCommands.currentRound.allEnemyVents[UnityEngine.Random.Range(0, ChatCommands.currentRound.allEnemyVents.Length)].floorNode.position;
                        }
                        else
                        {
                            position = GameObject.FindGameObjectsWithTag("OutsideAINode")[UnityEngine.Random.Range(0, GameObject.FindGameObjectsWithTag("OutsideAINode").Length)].transform.position;
                        }}
                    LogInfo("Spawning turret at position:" + position);
                    GameObject gameObject = UnityEngine.Object.Instantiate(Patches.Patches.turretPrefab, position, Quaternion.identity, ChatCommands.currentRound.mapPropsContainer.transform);
                    gameObject.GetComponent<NetworkObject>().Spawn(destroyWithScene: true);
                    DisplayChatMessage("Spawned turret at position:" + position);
                }
                    
            }}
            public void AddToSpawnableMapObjects(SpawnableMapObject newObject)
            {
                // Create a new array with one extra slot
                SpawnableMapObject[] newArray = new SpawnableMapObject[RoundManager.Instance.currentLevel.spawnableMapObjects.Length + 1];
                
                // Copy the old array elements to the new array
                for (int i = 0; i < RoundManager.Instance.currentLevel.spawnableMapObjects.Length; i++)
                {
                    newArray[i] = RoundManager.Instance.currentLevel.spawnableMapObjects[i];
                }
                
                // Add the new object at the end of the new array
                newArray[RoundManager.Instance.currentLevel.spawnableMapObjects.Length] = newObject;
                
                // Point the old array to the new array
                RoundManager.Instance.currentLevel.spawnableMapObjects = newArray;
            }
}}