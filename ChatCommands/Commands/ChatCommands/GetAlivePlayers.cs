using static ChatCommands.Utils;
using GameNetcodeStuff;



namespace ChatCommands.Commands
{
    internal class GetAlivePlayers : CustomChatCommand
    {

        public override string Name => "Get Alive Players";

        public override string Description => "Returns a list of all players and if they are alive or dead.";

        public override string Format => "/getalive";
        public override string AltFormat => "/getap";
        public override bool IsHostCommand => true;

        public override void Execute(CommandInput message)
        {
            string output = "";
            foreach (PlayerControllerB player in StartOfRound.Instance.allPlayerScripts)
            {
                output += player.playerUsername + " is " + (player.isPlayerDead ? "Dead" : "Alive") + "\n";
            }
            DisplayChatMessage(output);
        }}}