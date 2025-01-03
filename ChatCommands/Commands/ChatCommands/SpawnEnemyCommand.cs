using static ChatCommands.Utils;
using UnityEngine;
using System.Linq;
using System;
using Unity.Netcode;

namespace ChatCommands.Commands
{
    internal class SpawnEnemyCommand : CustomChatCommand
    {

        public override string Name => "Spawn Enemy";

        public override string Description => "Spawns an enemy at the specified location. Either use the player's position or specify a position. Position, state and amount are optional. Use them with the following format: position=@(playername/me) amount=(number) state=(alive/dead)";

        public override string Format => "/spawnenemy [enemyname] ([p=position]) ([a=amount]) ([s=state])";
        public override string AltFormat => "/spweny [enemyname] ([a=amount]) ([s=state]) ([p=position])";
        public override bool IsHostCommand => true;

        public override void Execute(CommandInput message)
        {

            string msgtitle = "Spawned Enemies";
            string msgbody = "";
            if (GetCurrentLevel() == null || ChatCommands.levelEnemySpawns == null || GetCurrentLevel().Enemies == null)
            {
                msgtitle = "Command";
                msgbody = GetCurrentLevel() == null ? "Unable to send command since currentLevel is null." : "Unable to send command since levelEnemySpawns is null.";
                DisplayChatError(msgtitle + "\n" + msgbody);
            }
            if (message.Args.Count < 1)
            {
                msgtitle = "Command Error";
                msgbody = "Missing Arguments For Spawn\n'/spawnenemy <name> (amount=<amount>) (state=<state>) (position={random, @me, @<playername>})";
                DisplayChatError(msgtitle + "\n" + msgbody);
            }
            int amount = 1;
            string vstate = "alive";
            Vector3 position = Vector3.zero;
            string sposition = "random";
            var args = message.Args.Skip(1).ToArray();
            string inputName = message.Args[0];

            foreach (string arg in args)
            {
                string[] darg = arg.Split('=');
                switch (darg[0])
                {
                    case "a":
                    case "amount":
                        amount = int.Parse(darg[1]);
                        LogInfo($"{amount}");
                        break;
                    case "s":
                    case "state":
                        vstate = darg[1];
                        LogInfo(vstate);
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
                    ChatCommands.mls.LogWarning("Position Invalid, Using Default 'random'");
                    sposition = "random";
                }
            }
            if (inputName.Length > 1)
            {
                bool flag = false;
                string enemyName = "";
                foreach (SpawnableEnemyWithRarity enemy in GetCurrentLevel().Enemies)
                {
                    if (enemy.enemyType.enemyName.ToLower().Contains(inputName.ToLower()))
                    {
                        try
                        {
                            flag = true;
                            enemyName = enemy.enemyType.enemyName;
                            if (sposition == "random")
                            {
                                SpawnEnemy(enemy, amount, inside: true, location: new Vector3(0f, 0f, 0f));
                            }
                            else
                            {
                                SpawnEnemy(enemy, amount, inside: true, location: position);
                            }
                            LogInfo("Spawned " + enemy.enemyType.enemyName);
                        }
                        catch
                        {
                            LogInfo("Could not spawn enemy");
                        }
                        msgbody = "Spawned: " + enemyName;
                        break;
                    }
                }
                if (!flag)
                {
                    foreach (SpawnableEnemyWithRarity outsideEnemy in GetCurrentLevel().OutsideEnemies)
                    {
                        if (outsideEnemy.enemyType.enemyName.ToLower().Contains(inputName.ToLower()))
                        {
                            try
                            {
                                flag = true;
                                enemyName = outsideEnemy.enemyType.enemyName;
                                LogInfo(outsideEnemy.enemyType.enemyName);
                                LogInfo("The index of " + outsideEnemy.enemyType.enemyName + " is " + GetCurrentLevel().OutsideEnemies.IndexOf(outsideEnemy));
                                if (sposition == "random")
                                {
                                    SpawnEnemy(outsideEnemy, amount, inside: false, location: new Vector3(0f, 0f, 0f));
                                }
                                else
                                {
                                    SpawnEnemy(outsideEnemy, amount, inside: false, location: position);
                                }
                                LogInfo("Spawned " + outsideEnemy.enemyType.enemyName);
                            }
                            catch (Exception ex)
                            {
                                LogInfo("Could not spawn enemy");
                                LogInfo("The game tossed an error: " + ex.Message);
                            }
                            msgbody = "Spawned " + amount + " " + enemyName + (amount > 1 ? "s" : "");
                            break;
                        }
                    }
                }
            }
            }

        internal static void SpawnEnemy(SpawnableEnemyWithRarity enemy, int amount, bool inside, Vector3 location)
        {
            if (!ChatCommands.isHost)
            {
                return;
            }
            if (location.x != 0f && location.y != 0f && location.z != 0f && inside)
            {
                try
                {
                    for (int i = 0; i < amount; i++)
                    {
                        GetCurrentRound().SpawnEnemyOnServer(location,0.0f, GetCurrentLevel().Enemies.IndexOf(enemy));
                    }
                    return;
                }
                catch
                {
                    LogWarning("Failed to spawn enemies, check your command.");
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
                        UnityEngine.Object.Instantiate<GameObject>(GetCurrentLevel().OutsideEnemies[GetCurrentLevel().OutsideEnemies.IndexOf(enemy)].enemyType.enemyPrefab, location, Quaternion.Euler(Vector3.zero)).gameObject.GetComponentInChildren<NetworkObject>().Spawn(true);
                    }
                    LogInfo($"You wanted to spawn: {amount} enemies");
                    LogInfo("Spawned an enemy. Total Spawned: " + i + "at position:" + location);
                    return;
                }
                catch
                {
                    LogInfo("Failed to spawn enemies, check your command.");
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
                        GetCurrentRound().SpawnEnemyOnServer(GetCurrentRound().allEnemyVents[UnityEngine.Random.Range(0, GetCurrentRound().allEnemyVents.Length)].floorNode.position, GetCurrentRound().allEnemyVents[i].floorNode.eulerAngles.y, GetCurrentLevel().Enemies.IndexOf(enemy));
                        
                    }
                    LogInfo($"You wanted to spawn: {amount} enemies");
                    LogInfo(("Total Spawned: " + i));
                    return;
                }
                catch
                {
                    LogInfo("Failed to spawn enemies, check your command.");
                    return;
                }
            }
            int j = 0;
            for (; j < amount; j++)
            {
                
                UnityEngine.Object.Instantiate<GameObject>(GetCurrentLevel().OutsideEnemies[GetCurrentLevel().OutsideEnemies.IndexOf(enemy)].enemyType.enemyPrefab, GameObject.FindGameObjectsWithTag("OutsideAINode")[UnityEngine.Random.Range(0, GameObject.FindGameObjectsWithTag("OutsideAINode").Length - 1)].transform.position, Quaternion.Euler(Vector3.zero)).gameObject.GetComponentInChildren<NetworkObject>().Spawn(true);
            }
            LogInfo($"You wanted to spawn: {amount} enemies");
            LogInfo("Total Spawned: " + j);
        }

}
}
