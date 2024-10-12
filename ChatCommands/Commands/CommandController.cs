using System.Collections.Generic;
using HarmonyLib;
using static ChatCommands.Utils;

namespace ChatCommands.Commands
{
    internal class CommandController
    {
        public static List<CustomChatCommand> Commands { get; } = new List<CustomChatCommand> {
            new SpawnEnemyCommand(),
        };

        private static string nullChatMessage = "";

        [HarmonyPatch(typeof(HUDManager), "SubmitChat_performed")]
        [HarmonyPrefix]
        private static bool ChatCommandsSubmitted(HUDManager __instance)
        {
            string text = __instance.chatTextField.text;
            string localPlayer = GameNetworkManager.Instance.username;
            ChatCommands.playerwhocalled = ConvertPlayername(localPlayer);

            // Log the text to ensure it's not null
            LogInfo($"Received chat input: {text}");

            if(CheckForCommand(text)){
                __instance.chatTextField.text = nullChatMessage;
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(HUDManager), "AddPlayerChatMessageClientRpc")]
        [HarmonyPrefix]
        private static bool ReadChatMessage(HUDManager __instance, ref string chatMessage, ref int playerId)
        {
            string nameOfUserWhoTyped = __instance.playersManager.allPlayerScripts[playerId].playerUsername;
            LogInfo("Chat Message: " + chatMessage + " sent by: " + nameOfUserWhoTyped);
            bool allowExecution = true;
            if (IsNetCommand(chatMessage) && ChatCommands.isHost)
            {
                if (!ChatCommands.AllowHostCommands){
                    allowExecution = false;
                    LogWarning("Host, but not allowing commands, checking player for allowance");
                    foreach (AllowedHostPlayer player in ChatCommands.AllowedHostPlayers)
                    {
                        if (player.Name.ToLower().Contains(nameOfUserWhoTyped.ToLower()))
                        {
                            LogInfo("Player is allowed to send commands");
                            allowExecution = true;
                            break;
                        }
                    }
                }
                if (allowExecution){
                    string command = ConvertFromNetCommand(chatMessage);
                    if (command.ToLower().Contains("p=@me")){
                        ChatCommands.playerwhocalled = ConvertPlayername(nameOfUserWhoTyped);
                    }
                    LogInfo("Host, trying to handle command: " + command);
                    DisplayChatMessage(nameOfUserWhoTyped + " sent command: "+ ChatCommands.PrefixSetting.Value + command);
                    if (CheckForCommand(command)){
                        //ChatCommands.ProcessCommandInput(command);
                        chatMessage = nullChatMessage;
                        return false;
                    } else {
                        return true;
                    }
                }
                else{
                    LogWarning("Player not allowed to send commands");
                    chatMessage = nullChatMessage;
                    return false;
                    }
            }
            else if (IsNetHostCommand(chatMessage) && !ChatCommands.isHost)
            {
                string command = ConvertFromNetHostCommand(chatMessage);
                LogInfo("Recieved command from Host, trying to handle command: " + command);
                if (CheckForCommand(command)){
                    chatMessage = nullChatMessage;
                    return false;
                }
            }
            return true;
        }


        private static bool CheckForCommand(string message){
            foreach (var command in Commands){
                if (command.Handle(message)){
                    return true;
                }
            }
            if (ChatCommands.isHost){
            LogWarning($"Command {message} not found.");}
            else{
                LogWarning($"Command {message} not found. Sending to host.");
                // TODO: Send command to host
            }
            return false;
        }
        
    }
}
