using static ChatCommands.Utils;
using GameNetcodeStuff;



namespace ChatCommands.Commands
{
    internal class RevivePlayers : CustomChatCommand
    {

        public override string Name => "Revive all players";

        public override string Description => "Revives everyone.";

        public override string Format => "/revive";
        public override string AltFormat => "/rev";
        public override bool IsHostCommand => true;

        public override void Execute(CommandInput message)
        {
            if (message.Args.Count == 0){
                StartOfRound.Instance.ReviveDeadPlayers();
            }
        }}}