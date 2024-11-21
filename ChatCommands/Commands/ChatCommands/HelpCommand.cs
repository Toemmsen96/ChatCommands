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

        public override string Description => "Displays a list of available commands with their format. Use <color=#00FFFF>/help [command]</color> to get more information about a specific command.";

        public override string Format => "/help ([command])";
        public override string AltFormat => "/h ([command])";
        public override bool IsHostCommand => false;

        public override void Execute(CommandInput message)
        {
            if (message.Args.Count == 0)
            {
                string commandList = "";
                string logCommandList = ""; // For logging purposes
                foreach (var command in Commands.CommandController.Commands)
                {
                    commandList += $"<color=#00FFFF>{command.Format}</color> - Alt: {command.AltFormat}\n";
                    logCommandList += $"{command.Name}: Format: {command.Format} - Alt: {command.AltFormat}\nDescription: {command.Description}, {(command.IsHostCommand?"For Host only":"Host and Client")}\n";
                }
                LogInfo(logCommandList);
                DisplayChatMessage(commandList);
            }
            else
            {
                string commandName = message.Args[0].ToLower();
                //var command = Commands.CommandController.Commands.FirstOrDefault(x => x.Format.ToLower() == commandName || x.AltFormat.ToLower() == commandName || x.Name.ToLower() == commandName);
                var command = null as CustomChatCommand;
                foreach (var cmd in Commands.CommandController.Commands)
                {
                    if (cmd.Format.ToLower().Contains(commandName) || cmd.AltFormat.ToLower().Contains(commandName) || cmd.Name.ToLower().Contains(commandName))
                    {
                        command = cmd;
                        break;
                    }
                }
                if (command != null)
                {
                    DisplayChatMessage($"<color=#771615>Command</color>: <color=#00FFFF>{command.Name}</color>\n<color=#771615>Description</color>: {command.Description}\n<color=#771615>Format</color>: {command.Format}\n<color=#771615>Alternative Format</color>: {command.AltFormat}\nHost only Command: {command.IsHostCommand}");
                }
                else
                {
                    DisplayChatError("Command not found.");
                }
            }
        }
    }
}