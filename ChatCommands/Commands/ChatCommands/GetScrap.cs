using static ChatCommands.Utils;


namespace ChatCommands.Commands
{
    internal class GetScrap : CustomChatCommand
    {

        public override string Name => "Get Spawnable Scrap Items";

        public override string Description => "Gets the spawnable scrap items for the current level.";

        public override string Format => "/getscrap";
        public override string AltFormat => "/scrap";
        public override bool IsHostCommand => true;

        public override void Execute(CommandInput message)
        {
            SelectableLevel newLevel = ChatCommands.currentLevel;
            if (newLevel == null)
            {
                DisplayChatError("Level is null.");
                return;
            }
            int len = ChatCommands.currentRound.currentLevel.spawnableScrap.Count;
            string output = ChatCommands.currentRound.currentLevel.spawnableScrap[0].spawnableItem.spawnPrefab.name;

            for (int i = 1; i < len; i++)
            {
                output += ", ";
                output += ChatCommands.currentRound.currentLevel.spawnableScrap[i].spawnableItem.spawnPrefab.name;
            }
            HUDManager.Instance.DisplayTip("Spawnable Scrap", output);
            DisplayChatMessage("Spawnable Scrap: " + output);
        }}}