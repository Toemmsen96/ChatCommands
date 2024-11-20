using static ChatCommands.Utils;
using UnityEngine;


namespace ChatCommands.Commands
{
    internal class GetPosition : CustomChatCommand
    {

        public override string Name => "Get your current Position";

        public override string Description => "Returns your current position.";

        public override string Format => "/getposition";
        public override string AltFormat => "/getpos";
        public override bool IsHostCommand => false;

        public override void Execute(CommandInput message)
        {
            string output = "<color=yellow>Current Position:</color>\n";
            Vector3 pos = StartOfRound.Instance.localPlayerController.transform.position;
            output += "X: " + pos.x + "\n";
            output += "Y: " + pos.y + "\n";
            output += "Z: " + pos.z + "\n";
            DisplayChatMessage(output);

        }}}