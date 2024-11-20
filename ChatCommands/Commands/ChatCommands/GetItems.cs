using static ChatCommands.Utils;


namespace ChatCommands.Commands
{
    internal class GetItems : CustomChatCommand
    {

        public override string Name => "Get ALL Spawnable Items";

        public override string Description => "Gets the spawnable items you can spawn with /spawnitem or /spwitm.";

        public override string Format => "/getitems";
        public override string AltFormat => "/items";
        public override bool IsHostCommand => false;

        public override void Execute(CommandInput message)
        {
            string output = "<color=yellow>Spawnable Items:</color>\n";
            foreach (var x in StartOfRound.Instance.allItemsList.itemsList)
            {
                output += x.name + "\n";
            }
            DisplayChatMessage(output);

        }}}