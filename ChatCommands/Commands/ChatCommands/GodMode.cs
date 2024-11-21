using static ChatCommands.Utils;
using HarmonyLib;
using GameNetcodeStuff;


namespace ChatCommands.Commands
{
    internal class GodMode : CustomChatCommand
    {

        private static bool isGod = false;
        public override string Name => "Toggle God Mode";

        public override string Description => "Toggles if invincibility is enabled.";

        public override string Format => "/godmode";
        public override string AltFormat => "/god";
        public override bool IsHostCommand => false;

        public override void Execute(CommandInput message)
        {
            if (ChatCommands.isHost)
            {
                isGod = !isGod;
                SendHostCommand("god");
            }
            else
            {
                isGod = !isGod;
                SendCommandToServer("god");
            }
            Utils.DisplayChatMessage("God Mode: " + (isGod ? "Enabled" : "Disabled"));
        }
        [HarmonyPatch(typeof(PlayerControllerB), "AllowPlayerDeath")]
        [HarmonyPrefix]
        private static bool OverrideDeath()
        {
            return !isGod;
        }

        [HarmonyPatch(typeof(MouthDogAI), "OnCollideWithPlayer")]
        [HarmonyPrefix]
        private static bool OverrideDeath2()
        {
            return !isGod;
        }

        [HarmonyPatch(typeof(ForestGiantAI), "GrabPlayerServerRpc")]
        [HarmonyPrefix]
        private static bool OverrideDeath3()
        {
            return !isGod;
        }}}