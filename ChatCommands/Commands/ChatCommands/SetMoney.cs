using static ChatCommands.Utils;
using HarmonyLib;

namespace ChatCommands.Commands
{
    internal class SetMoney : CustomChatCommand
    {
        private static int customMoney = 0;
        private static bool infMoney = false;

        public override string Name => "Set Money";

        public override string Description => "Set Terminal Money to defined value, or without value to toggle infinite money.";

        public override string Format => "/setmoney ([value])";
        public override string AltFormat => "/money ([value])";
        public override bool IsHostCommand => true;

        public override void Execute(CommandInput message)
        {
            if (message.Args.Count == 1)
            {
                if (int.TryParse(message.Args[0], out int money))
                {
                    customMoney = money;
                    Terminal term = ChatCommands.FindObjectOfType<Terminal>();
                    term.groupCredits = customMoney;
                    DisplayChatMessage("Money set to: " + customMoney);
                    return;
                }
                else
                {
                    DisplayChatError("Invalid money value");
                    return;
                }
            }
            else
            {
                infMoney = !infMoney;
                DisplayChatMessage("Infinite Money: " + (infMoney ? "Enabled" : "Disabled"));
            }
        }
        [HarmonyPatch(typeof(Terminal), "RunTerminalEvents")]
        [HarmonyPostfix]
        private static void InfiniteCredits(ref int ___groupCredits)
        {
            if (ChatCommands.isHost && infMoney)
            {
                ___groupCredits = 99999;
            }
        }}}