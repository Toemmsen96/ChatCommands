using static ChatCommands.Utils;
using HarmonyLib;

namespace ChatCommands.Commands
{
    internal class ToggleOverrideSpawns : CustomChatCommand
    {

        private static bool OverrideSpawns = false;
        public override string Name => "Toggle Overrride Spawns";

        public override string Description => "Toggles if Monster Spawns are overriden or not. This affects how many monsters spawn with the spawn command and natural spawns.";

        public override string Format => "/override";
        public override string AltFormat => "/ovr";
        public override bool IsHostCommand => true;
        public override void Execute(CommandInput message)
        { 
            OverrideSpawns = !OverrideSpawns;
            DisplayChatMessage("Override Spawns: " + (OverrideSpawns ? "Enabled" : "Disabled"));
        }
        
        [HarmonyPatch(typeof(RoundManager), "EnemyCannotBeSpawned")]
        [HarmonyPrefix]
        private static bool OverrideCannotSpawn()
        {
            if (OverrideSpawns)
            {
                return false;
            }
            else
            {
                return true;
            }
        }}}