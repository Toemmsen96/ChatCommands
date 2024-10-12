using static ChatCommands.Utils;
using UnityEngine;
using BepInEx;
using GameNetcodeStuff;
using UnityEngine.AI;

namespace ChatCommands.Commands
{
    internal class TeleportCommand : CustomChatCommand
    {

        public override string Name => "Teleport";

        public override string Description => "Teleport to where you want. Use the following format: position=@(playername/me) or position=(random) or without arguments to teleport to the terminal.";

        public override string Format => "/teleport [position=(random/@<playername>/@me)]";
        public override string AltFormat => "/tp [position=(random/@<playername>/@me)]";
        public override bool IsHostCommand => true;

        private static int CustomDeadline = int.MinValue;

        public override void Execute(CommandInput message)
        {
            string msgtitle = "";
            string msgbody = "";
            Vector3 position = Vector3.zero;
            string posArg = message.Args.Count > 0 ? message.Args[0] : "";
            string sposition = "random";
            if (!posArg.IsNullOrWhiteSpace())
            {
                if (posArg.ToLower().StartsWith("p="))
                {
                    sposition = posArg.ToLower().Substring(2);
                    if (sposition != "random")
                    {
                        position = CalculateSpawnPosition(sposition);
                        if (position == Vector3.zero && sposition != "random")
                        {
                            ChatCommands.mls.LogWarning("Position Invalid, Using Default 'random'");
                            sposition = "random";
                        }
                    }
                    if (sposition == "random")
                    {
                        if (ChatCommands.currentRound != null && ChatCommands.currentLevel != null)
                        {
                            System.Random shipTeleporterSeed;
                            shipTeleporterSeed = new System.Random(StartOfRound.Instance.randomMapSeed + 17 + (int)GameNetworkManager.Instance.localPlayerController.playerClientId);
                            Vector3 position3 = RoundManager.Instance.insideAINodes[shipTeleporterSeed.Next(0, RoundManager.Instance.insideAINodes.Length)].transform.position;
                            Debug.DrawRay(position3, Vector3.up * 1f, Color.red);
                            position3 = RoundManager.Instance.GetRandomNavMeshPositionInBoxPredictable(position3, 10f, default(NavMeshHit), shipTeleporterSeed);
                            Debug.DrawRay(position3 + Vector3.right * 0.01f, Vector3.up * 3f, Color.green);
                            position = position3;
                        }
                        
                    }
                    GameNetworkManager.Instance.localPlayerController.beamUpParticle.Play();
                    GameNetworkManager.Instance.localPlayerController.beamOutBuildupParticle.Play();
                    GameNetworkManager.Instance.localPlayerController.TeleportPlayer(position, false, 0f, false, true);
                    msgtitle = "Teleported";
                    msgbody = "Teleported to " + sposition;
                    
                }
                else
                {
                    string tpname = posArg.ToLower();
                    tpname = ConvertPlayername(tpname);

                    PlayerControllerB[] allPlayerScripts = StartOfRound.Instance.allPlayerScripts;
                    foreach (PlayerControllerB testedplayer in allPlayerScripts)
                    {
                        if (testedplayer.playerUsername.ToLower().Contains(tpname))
                        {
                            GameNetworkManager.Instance.localPlayerController.beamUpParticle.Play();
                            GameNetworkManager.Instance.localPlayerController.beamOutBuildupParticle.Play();
                            GameNetworkManager.Instance.localPlayerController.TeleportPlayer(testedplayer.transform.position, false, 0f, false, true);
                            msgtitle = "Teleported";
                            msgbody = "Teleported to Player:" + testedplayer.playerUsername;
                        }
                    }
                }
            }
            else
            {
                Terminal term = UnityEngine.Object.FindObjectOfType<Terminal>();
                if (term != null)
                {
                    GameNetworkManager.Instance.localPlayerController.beamUpParticle.Play();
                    GameNetworkManager.Instance.localPlayerController.beamOutBuildupParticle.Play();
                    GameNetworkManager.Instance.localPlayerController.TeleportPlayer(term.transform.position, false, 0f, false, true);
                    msgtitle = "Teleported";
                    msgbody = "Teleported to Terminal";
                }
            }
            DisplayChatMessage(msgtitle + "\n" + msgbody);
        }
    }
}