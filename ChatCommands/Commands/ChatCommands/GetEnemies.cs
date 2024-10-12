using static ChatCommands.Utils;
using UnityEngine;
using BepInEx;
using GameNetcodeStuff;
using UnityEngine.AI;

namespace ChatCommands.Commands
{
    internal class GetEnemies : CustomChatCommand
    {

        public override string Name => "Get Spawnable Enemies";

        public override string Description => "Gets the spawnable enemies for the current level.";

        public override string Format => "/getenemies";
        public override string AltFormat => "/enemies";
        public override bool IsHostCommand => true;

        public override void Execute(CommandInput message)
        {
            string textToDisplay = "";
            SelectableLevel newLevel = ChatCommands.currentLevel;
            string msgtitle = "Enemies:";
            string msgbody = "";
            if (newLevel == null)
            {
                DisplayChatError("Level is null.");
            }

            // Check if levelEnemySpawns is null
            if (ChatCommands.levelEnemySpawns == null)
            {
                DisplayChatError("levelEnemySpawns is null.");
            }

            // Attempt to get value from dictionary, check for null
            if (ChatCommands.levelEnemySpawns.TryGetValue(newLevel, out var value))
            {
                newLevel.Enemies = value;
                textToDisplay += "<color=#FF00FF>Inside: </color><color=#FFFF00>";
                msgbody = "<color=#FF00FF>Inside: </color><color=#FFFF00>";

                if (newLevel.Enemies.Count == 0)
                {
                    textToDisplay += "None";
                    msgbody += "None";
                }
                else
                {
                    foreach (SpawnableEnemyWithRarity enemy2 in newLevel.Enemies)
                    {
                        LogInfo("Inside: " + enemy2.enemyType.enemyName);
                        textToDisplay += enemy2.enemyType.enemyName + ", ";
                        msgbody += enemy2.enemyType.enemyName + ", ";
                    }
                }

                textToDisplay += "\n</color><color=#FF00FF>Outside: </color>";
                msgbody += "\n</color><color=#FF00FF>Outside: </color>";

                if (newLevel.OutsideEnemies.Count == 0)
                {
                    textToDisplay += "None";
                    msgbody += "None";
                }
                else
                {
                    foreach (SpawnableEnemyWithRarity outsideEnemy in newLevel.OutsideEnemies)
                    {
                        LogInfo("Outside: " + outsideEnemy.enemyType.enemyName);
                        textToDisplay += outsideEnemy.enemyType.enemyName + ", ";
                        msgbody += outsideEnemy.enemyType.enemyName + ", ";
                    }
                }

                DisplayChatMessage(textToDisplay);
            }
            DisplayChatMessage(msgtitle+"\n"+msgbody);
        }
    }
}
