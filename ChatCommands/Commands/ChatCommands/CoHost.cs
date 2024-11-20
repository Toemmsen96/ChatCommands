using static ChatCommands.Utils;
using GameNetcodeStuff;



namespace ChatCommands.Commands
{
    internal class CoHost : CustomChatCommand
    {

        public override string Name => "Give Co-Host to a player";

        public override string Description => "Gives co-host to a player, which allows them to use host commands, when you turned them off for everyone else.";

        public override string Format => "/cohost [playername]";
        public override string AltFormat => "/hostcmd [playername]";
        public override bool IsHostCommand => true;

        public override void Execute(CommandInput message)
        {
            string playername = message.Args[0];
            playername = ConvertPlayername(playername);
            PlayerControllerB[] allPlayerScripts = StartOfRound.Instance.allPlayerScripts;
            bool found = false;
            foreach (PlayerControllerB val3 in allPlayerScripts)
            {
                if (ConvertPlayername(val3.playerUsername).ToLower().Contains(playername.ToLower()))
                {
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                ChatCommands.mls.LogWarning("Player not found");
                DisplayChatError("Player "+playername+" not found!!!");
            }
            else
            {
                bool foundinlist = false;
                foreach (AllowedHostPlayer player in ChatCommands.AllowedHostPlayers)
                {
                    if (ConvertPlayername(player.Name).ToLower().Contains(playername.ToLower()))
                    {
                        player.AllowHostCMD = !player.AllowHostCMD;
                        DisplayChatMessage("Host Commands for " + playername + " set to" + player.AllowHostCMD);
                        foundinlist = true;
                        break;
                    }
                }
                if (!foundinlist)
                {
                    ChatCommands.AllowedHostPlayers.Add(new AllowedHostPlayer(playername, true));
                   DisplayChatMessage("Host Commands for " + playername + " set to true");
                }
            }
        }}}