using static ChatCommands.Utils;
using HarmonyLib;

namespace ChatCommands.Commands
{
    internal class ToggleHostCmd : CustomChatCommand
    {

        public override string Name => "Toggle Host Commands";

        public override string Description => "Toggle if connecting clients can use host commands.";

        public override string Format => "/togglehostcmd";
        public override string AltFormat => "/thcmd";
        public override bool IsHostCommand => true;

        public override void Execute(CommandInput message)
        {
            ChatCommands.AllowHostCommands = !ChatCommands.AllowHostCommands;
            DisplayChatMessage("Host Commands: " + (ChatCommands.AllowHostCommands ? "Enabled" : "Disabled"));
        }
        }
        }