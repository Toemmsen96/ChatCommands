using static ChatCommands.Utils;

namespace ChatCommands.Commands
{
    internal class ToggleOverrideSpawns : CustomChatCommand
    {

        public override string Name => "Toggle Overrride Spawns";

        public override string Description => "Toggles if Monster Spawns are overriden or not. This affects how many monsters spawn with the spawn command and natural spawns.";

        public override string Format => "/override";
        public override string AltFormat => "/ovr";
        public override bool IsHostCommand => true;
        public override void Execute(CommandInput message)
        { 
            ChatCommands.OverrideSpawns = !ChatCommands.OverrideSpawns;
            DisplayChatMessage("Override Spawns: " + (ChatCommands.OverrideSpawns ? "Enabled" : "Disabled"));
        }}}