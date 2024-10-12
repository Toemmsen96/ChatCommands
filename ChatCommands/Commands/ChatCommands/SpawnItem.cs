using static ChatCommands.Utils;
using UnityEngine;
using System.Linq;
using System;
using Unity.Netcode;

namespace ChatCommands.Commands
{
    internal class SpawnItem : CustomChatCommand
    {

        public override string Name => "Spawn Item";

        public override string Description => "Spawns Items";

        public override string Format => "/spawnitem [enemyname] ([position]) ([amount]) ([state])";
        public override string AltFormat => "/spwitm [enemyname] ([amount]) ([state]) ([position])";
        public override bool IsHostCommand => true;

        public override void Execute(CommandInput message)
        {
            string toSpawn = message.Args[0].ToLower();
            int amount = 1;
            Vector3 position = Vector3.zero;
            string sposition = "random";
            int value = 1000;
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
            while (amount > 0)
            {
                ChatCommands.SpawnItems(position, 1);
                amount--;
            }
            DisplayChatMessage("Spawned " + amount + " " + toSpawn + (amount > 1 ? "s" : "") + " with value of:" + value + "\n at position: " + position);
        }}}