using static ChatCommands.Utils;


namespace ChatCommands.Commands
{
    internal class GodMode : CustomChatCommand
    {

        public override string Name => "Toggle God Mode";

        public override string Description => "Toggles if invincibility is enabled.";

        public override string Format => "/godmode";
        public override string AltFormat => "/god";
        public override bool IsHostCommand => false;

        public override void Execute(CommandInput message)
        {
            if (ChatCommands.isHost)
            {
                ChatCommands.enableGod = !ChatCommands.enableGod;
                SendHostCommand("god");
            }
            else
            {
                ChatCommands.enableGod = !ChatCommands.enableGod;
                SendCommandToServer("god");
            }
            Utils.DisplayChatMessage("God Mode: " + (ChatCommands.enableGod ? "Enabled" : "Disabled"));
        }}}