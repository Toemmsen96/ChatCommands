using static ChatCommands.Utils;
using UnityEngine;


namespace ChatCommands.Commands
{
    internal class GetCredits : CustomChatCommand
    {

        public override string Name => "View Credits";

        public override string Description => "Shows the credits for the ChatCommands mod.";

        public override string Format => "/getcredits";
        public override string AltFormat => "/credits";
        public override bool IsHostCommand => false;

        public override void Execute(CommandInput message)
        {
            DisplayChatMessage("<color=#FF00FF>Credits:</color>\nChatCommands by Toemmsen96 and Chrigi. Visit the GitHub page for more information and to report issues:\n<color=#0000FF>github.com/Toemmsen96/ChatCommands/</color>");
        }}}