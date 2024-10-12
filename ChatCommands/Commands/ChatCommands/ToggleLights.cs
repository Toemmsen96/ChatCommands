using static ChatCommands.Utils;

namespace ChatCommands.Commands
{
    internal class ToggleLights : CustomChatCommand
    {

        public override string Name => "Toggle Lights";

        public override string Description => "Toggles the lights on and off.";

        public override string Format => "/togglelights";
        public override string AltFormat => "/toglig";
        public override bool IsHostCommand => true;

        public override void Execute(CommandInput message)
        {
            BreakerBox breakerBox = UnityEngine.Object.FindObjectOfType<BreakerBox>();
            if (breakerBox != null)
            {
                
                if (breakerBox.isPowerOn)
                {
                    ChatCommands.currentRound.TurnBreakerSwitchesOff();
                    ChatCommands.currentRound.TurnOnAllLights(false);
                    breakerBox.isPowerOn = false;
                    DisplayChatMessage("Turned the lights off");
                }
                else
                {
                    ChatCommands.currentRound.PowerSwitchOnClientRpc();
                    DisplayChatMessage("Turned the lights on");
                }
            }
            else
            {
                DisplayChatError("Unable to find BreakerBox, need to be in a level with lights to use this command.");
            }
        }
    }
}