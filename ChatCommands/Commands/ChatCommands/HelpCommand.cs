using static ChatCommands.Utils;
using UnityEngine;
using System.Linq;
using System;
using Unity.Netcode;
using HarmonyLib;

namespace ChatCommands.Commands
{
    internal class HelpCommand : CustomChatCommand
    {

        public override string Name => "View Help";

        public override string Description => "Displays a list of available commands with their format. Use /help [command] to get more information about a specific command.";

        public override string Format => "/help ([command])";
        public override string AltFormat => "/h ([command])";
        public override bool IsHostCommand => false;

        public override void Execute(CommandInput message)
        {
            if (message.Args.Count == 0)
            {
                string commandList = "";
                foreach (var command in Commands.CommandController.Commands)
                {
                    commandList += $"{command.Format} - Alt: {command.AltFormat}\n";
                }
                DisplayChatMessage(commandList);
            }
            else
            {
                string commandName = message.Args[0].ToLower();
                var command = Commands.CommandController.Commands.FirstOrDefault(x => x.Format.ToLower() == commandName || x.AltFormat.ToLower() == commandName || x.Name.ToLower() == commandName);
                if (command != null)
                {
                    DisplayChatMessage($"Command: {command.Name}\nDescription: {command.Description}\nFormat: {command.Format}\nAlternative Format: {command.AltFormat}\nHost Command: {command.IsHostCommand}");
                }
                else
                {
                    DisplayChatError("Command not found.");
                }
            }
        }
    }
}