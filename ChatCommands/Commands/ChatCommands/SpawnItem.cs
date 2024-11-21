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

        public override string Description => "Spawns Items at a specified position or at a random position. Args are optional, use them like this: p=@me a=5 v=1234.\n Note: Value doesnt work for everything.";

        public override string Format => "/spawnitem [itemname] ([p=position]) ([a=amount]) ([v=value])";
        public override string AltFormat => "/spwitm [itemname] ([a=amount]) ([p=position]) ([v=value])";
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
                    case "v":
                    case "value":
                        value = int.Parse(darg[1]);
                        ChatCommands.mls.LogInfo($"Value {value}");
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
            ChatCommands.SpawnItems(position, toSpawn, value, amount);
        }}}