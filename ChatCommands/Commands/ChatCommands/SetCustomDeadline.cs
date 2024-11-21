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

        public override string Description => "Sets a custom deadline for the game. If no argument is provided, the custom deadline will be toggled.";

        public override string Format => "/deadline [days]";
        public override string AltFormat => "/dl [days]";
        public override bool IsHostCommand => true;
        private static bool EnableCustomDeadline = false;
        private static int CustomDeadline = 6;
        private static bool SetNewCustomDeadline = false;

        public override void Execute(CommandInput message)
        {
            
            if (message.Args.Count > 0)
            {
                if (int.TryParse(message.Args[0], out var result4))
                {
                    CustomDeadline = result4;
                    DisplayChatMessage("Deadline set to " + CustomDeadline + " days");
                    EnableCustomDeadline = true;
                    SetNewCustomDeadline = true;
                }
                else
                {
                    CustomDeadline = int.MinValue;
                    DisplayChatMessage("Deadline set to default");
                    EnableCustomDeadline = false;
                    SetNewCustomDeadline = false;
                }
            }
            else
            {
                EnableCustomDeadline = !EnableCustomDeadline;
                DisplayChatMessage($"Deadline set to" + (EnableCustomDeadline ? "custom" : "default"));
            }
        }


        [HarmonyPatch(typeof(TimeOfDay), "SetBuyingRateForDay")]
        [HarmonyPrefix]
        private static void PatchDeadline(TimeOfDay __instance)
        {

            if (ChatCommands.isHost && EnableCustomDeadline && SetNewCustomDeadline)
            {
                __instance.quotaVariables.deadlineDaysAmount = CustomDeadline;
                __instance.timeUntilDeadline = (__instance.quotaVariables.deadlineDaysAmount + CustomDeadline) * __instance.totalTime;

                TimeOfDay.Instance.timeUntilDeadline = (int)(TimeOfDay.Instance.totalTime * TimeOfDay.Instance.quotaVariables.deadlineDaysAmount);
                TimeOfDay.Instance.SyncTimeClientRpc(__instance.globalTime, (int)__instance.timeUntilDeadline);
                StartOfRound.Instance.deadlineMonitorText.text = "DEADLINE:\n " + TimeOfDay.Instance.daysUntilDeadline;
                SetNewCustomDeadline = false;
                }
            else if (ChatCommands.isHost && EnableCustomDeadline){
	            StartOfRound.Instance.companyBuyingRate = 1f - (float)(TimeOfDay.Instance.daysUntilDeadline/CustomDeadline);
                if (TimeOfDay.Instance.daysUntilDeadline == 0)
                {
                    StartOfRound.Instance.companyBuyingRate = 1f;
                }
            }
        }
        [HarmonyPatch(typeof(TimeOfDay), "SetNewProfitQuota")]
        [HarmonyPostfix]
        private static void ResetDeadline(TimeOfDay __instance)
        {
            if (ChatCommands.isHost && EnableCustomDeadline && !SetNewCustomDeadline){
                SetNewCustomDeadline = true;
            }

        }
        
    }
}