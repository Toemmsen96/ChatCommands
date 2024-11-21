using static ChatCommands.Utils;
using HarmonyLib;

namespace ChatCommands.Commands
{
    internal class InfiniteAmmo : CustomChatCommand
    {
        private static bool EnableInfiniteAmmo = false;
        public override string Name => "Infinite Ammo";

        public override string Description => "Toggle Infinite Ammo.";

        public override string Format => "/infammo";
        public override string AltFormat => "/ammo";
        public override bool IsHostCommand => false;

        public override void Execute(CommandInput message)
        {      
            EnableInfiniteAmmo = !EnableInfiniteAmmo;
            DisplayChatMessage("Infinite Ammo: " + (EnableInfiniteAmmo ? "Enabled" : "Disabled"));
        }
      
        [HarmonyPatch(typeof(ShotgunItem), "ItemActivate")]
        [HarmonyPrefix]
        static void ItemActivateGunPatch(ref ShotgunItem __instance)
        {
            if (EnableInfiniteAmmo)
            {
                __instance.shellsLoaded = 2;
            }
            
        }}}