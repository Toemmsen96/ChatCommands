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

        public override string Description => "Displays a list of available commands and their descriptions.";

        public override string Format => "/help";
        public override string AltFormat => "/h";
        public override bool IsHostCommand => false;

        public override void Execute(CommandInput message)
        {
            string msgtitle = "Available Commands";
            string msgbody = "";
            foreach (CustomChatCommand command in CommandController.Commands){
                msgbody += command.Name + " - " + command.Description+(command.IsHostCommand?" Needs Host":" Doesn't need Host")+"\n Format: " + command.Format +" Alternative Format: "+command.AltFormat+"\n";
            }
            DisplayChatMessage(msgtitle + "\n" + msgbody);
        }
    }
}