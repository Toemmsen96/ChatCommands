using static ChatCommands.Utils;
using UnityEngine;
using System.Linq;
using System;
using Unity.Netcode;
using HarmonyLib;

namespace ChatCommands.Commands
{
    internal class SetCustomDeadline : CustomChatCommand
    {

        public override string Name => "Set Custom Deadline";

        public override string Description => "Sets a custom deadline for the game. If no argument is provided, the default deadline will be used.";

        public override string Format => "/deadline [days]";
        public override string AltFormat => "/dl [days]";
        public override bool IsHostCommand => true;

        private static int CustomDeadline = int.MinValue;

        public override void Execute(CommandInput message)
        {
            
            if (message.Args.Count > 0)
            {
                if (int.TryParse(message.Args[0], out var result4))
                {
                    CustomDeadline = result4;
                    DisplayChatMessage("Deadline set to " + CustomDeadline + " days");
                }
                else
                {
                    CustomDeadline = int.MinValue;
                    DisplayChatMessage("Deadline set to default");
                }
            }
            else
            {
                CustomDeadline = int.MinValue;
                DisplayChatMessage("Deadline set to default");
            }
        }


        // TODO: rework this patch to be instant
        [HarmonyPatch(typeof(TimeOfDay), "SetNewProfitQuota")]
        [HarmonyPostfix]
        private static void PatchDeadline(TimeOfDay __instance)
        {

            if (ChatCommands.isHost && CustomDeadline != int.MinValue)
            {
                __instance.quotaVariables.deadlineDaysAmount = CustomDeadline;
                __instance.timeUntilDeadline = (__instance.quotaVariables.deadlineDaysAmount + CustomDeadline) * __instance.totalTime;

                TimeOfDay.Instance.timeUntilDeadline = (int)(TimeOfDay.Instance.totalTime * TimeOfDay.Instance.quotaVariables.deadlineDaysAmount);
                TimeOfDay.Instance.SyncTimeClientRpc(__instance.globalTime, (int)__instance.timeUntilDeadline);
                StartOfRound.Instance.deadlineMonitorText.text = "DEADLINE:\n " + TimeOfDay.Instance.daysUntilDeadline;
            }
        }
        
    }
}