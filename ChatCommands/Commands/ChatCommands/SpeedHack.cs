using static ChatCommands.Utils;
using GameNetcodeStuff;
using HarmonyLib;



namespace ChatCommands.Commands
{
    internal class SpeedHack : CustomChatCommand
    {

        private static float defaultJumpForce = 13f;
        private static float moddedJumpForce = 25f;
        private static float moddedSprintSpeed = 5f;
        private static bool speedHack = false;
        public override string Name => "Speed Hack";

        public override string Description => "Toggles Speed Hack, if speed is provided it will set the speed to that value.\n If jump force is provided it will set the jump force to that value.";

        public override string Format => "/speed ([speed]) ([jumpforce])";
        public override string AltFormat => "/speedhack ([speed]) ([jumpforce])";
        public override bool IsHostCommand => false;

        public override void Execute(CommandInput message)
        {
            if (message.Args.Count == 1)
            {
                if (float.TryParse(message.Args[0], out float speed))
                {
                    moddedSprintSpeed = speed;
                    speedHack = true;
                    DisplayChatMessage("Speed Hack enabled with speed: " + moddedSprintSpeed);
                    //ChatCommands.playerRef.isSpeedCheating = speedHack;
                    if (ChatCommands.SendHostCommandsSetting.Value && ChatCommands.isHost)
                    SendHostCommand(message.Command + moddedSprintSpeed);
                    return;
                }
                else
                {
                    DisplayChatError("Invalid speed value");
                    return;
                }
            }
            if (message.Args.Count == 2)
            {
                if (float.TryParse(message.Args[0], out float speed))
                {
                    moddedSprintSpeed = speed;
                    if (float.TryParse(message.Args[1], out float jumpForce))
                    {
                        moddedJumpForce = jumpForce;
                        speedHack = true;
                        DisplayChatMessage("Speed Hack enabled with speed: " + moddedSprintSpeed + " and jump force: " + moddedJumpForce);
                        if (ChatCommands.SendHostCommandsSetting.Value && ChatCommands.isHost)
                        SendHostCommand(message.Command +" "+moddedSprintSpeed+" "+moddedJumpForce);
                        //ChatCommands.playerRef.isSpeedCheating = speedHack;
                        return;
                    }
                    else
                    {
                        DisplayChatError("Invalid jump force value");
                        return;
                    }
                }
                else
                {
                    DisplayChatError("Invalid speed value");
                    return;
                }
            }
            else if (message.Args.Count > 2)
            {
                DisplayChatError("Too many arguments");
                return;
            }
            else
            {
                speedHack = !speedHack;
                if (!speedHack)
                {
                    moddedSprintSpeed = 0f;
                    moddedJumpForce = defaultJumpForce;
                }
                DisplayChatMessage("Speed Hack: " + (speedHack ? "Enabled" : "Disabled"));
                //ChatCommands.playerRef.isSpeedCheating = speedHack;
            }
            
        }

        [HarmonyPatch(typeof(PlayerControllerB), "Update")]
        [HarmonyPrefix]
        private static void SpeedHackFunc(ref float ___jumpForce, ref float ___sprintMeter, ref float ___sprintMultiplier, ref bool ___isSprinting, ref float ___targetFOV)
        {
            defaultJumpForce = Patches.Patches.defaultJumpForce;
            if (speedHack)
            {
                ___jumpForce = moddedJumpForce;
                ___sprintMeter = 1f;
                if (___isSprinting){ 
                    ___sprintMultiplier = moddedSprintSpeed;
                    ___targetFOV = ___targetFOV*moddedSprintSpeed;
                }
                else
                {
                    ___sprintMultiplier = 1f;
                }
            }
            else
            {
                ___jumpForce = defaultJumpForce;
            }

        }
        }
    }