using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using GameNetcodeStuff;

namespace ChatCommands
{
    public static class Utils
    {
        private static bool logToChat = ChatCommands.LogToChatSetting.Value;
        private static string NetCommandPrefix = "<size=0>CCMD:";
        private static string NetHostCommandPrefix = "<size=0>CHCMD:";
        private static string NetCommandPostfix = ":CCMD</size>";
        internal static bool DisplayAsTooltip = ChatCommands.DisplayChatMessagesAsPopupSetting.Value;

        public static Vector3 CalculateSpawnPosition(string sposition)
        {
            Vector3 position = Vector3.zero;
            if (sposition == "random")
            {
                return position;
            }
            if (sposition.StartsWith("@"))
            {
                PlayerControllerB[] allPlayerScripts;
                try {
                    allPlayerScripts = StartOfRound.Instance.allPlayerScripts;
                }
                catch (Exception e)
                {
                    LogWarning("Error getting allPlayerScripts: " + e);
                    return position;
                }
                if (sposition == "@me")
                {
                    foreach (PlayerControllerB testedPlayer in allPlayerScripts)
                    {
                        ChatCommands.mls.LogInfo($"Checking Playername {testedPlayer.playerUsername}");
                        if (testedPlayer.playerUsername.Replace(" ","").ToLower().Contains(ChatCommands.playerwhocalled.ToLower()))
                        {
                            ChatCommands.mls.LogInfo($"Found player {testedPlayer.playerUsername}");
                            position = testedPlayer.transform.position;
                            break;
                        }
                    }
                }
                else
                {
                    string origplayername = sposition.Substring(1);
                    string playername = ConvertPlayername(origplayername);
                    bool found = false;
                    ChatCommands.mls.LogInfo($"Looking for Playername {playername} or Playername {origplayername}...");
                    foreach (PlayerControllerB testedPlayer in allPlayerScripts)
                    {
                        ChatCommands.mls.LogInfo($"Checking Playername {testedPlayer.playerUsername.Replace(" ","")}");
                        if (testedPlayer.playerUsername.Replace(" ", "").ToLower().Contains(playername.ToLower()) || testedPlayer.playerUsername.Replace(" ", "").ToLower().Contains(origplayername.ToLower()))
                        {
                            position = testedPlayer.transform.position;
                            found = true;
                            ChatCommands.mls.LogInfo($"Found player {testedPlayer.playerUsername}");
                            break;
                        }
                    }
                    if (!found)
                    {
                        ChatCommands.mls.LogWarning("Player not found");
                        DisplayChatMessage("Player not found, spawning in random position");
                    }

                }

            }
            else
            {
                string[] pos = sposition.Split(',');
                if (pos.Length == 3)
                {
                    position = new Vector3(float.Parse(pos[0]), float.Parse(pos[1]), float.Parse(pos[2]));
                }
                else
                {
                    ChatCommands.mls.LogWarning("Position Invalid, Using Default 'random'");
                }
            }
            return position;
        }
        public static void LogInfo(string message)
        {
            ChatCommands.mls.LogInfo(message);
            if (logToChat){
                DisplayChatMessage(message);
            }
        }

        public static void LogWarning(string message)
        {
            ChatCommands.mls.LogWarning(message);
            if (logToChat){
                DisplayChatError(message);
            }
        }

        public static void LogError(string message)
        {
            ChatCommands.mls.LogError(message);
            if (logToChat){
                DisplayChatError(message);
            }
        }
        public static void DisplayChatMessage(string chatMessage)
        {
            try{
            string formattedMessage =
                $"<color=#FF00FF>ChatCommands</color>: <color=#FFFF00>{chatMessage}</color>";

            HUDManager.Instance.ChatMessageHistory.Add(formattedMessage);

            UpdateChatText();
            }
            catch (Exception e){
                ChatCommands.mls.LogError("Error displaying chat message: " + e);
            }
            if (DisplayAsTooltip)
            {
                HUDManager.Instance.DisplayTip("ChatCommands", chatMessage, false, false, "LC_Tip1");
            }
        }
        public static void DisplayChatError(string errorMessage)
        {
            try{
            string formattedMessage =
                $"<color=#FF0000>CCMD: ERROR</color>: <color=#FF0000>{errorMessage}</color>";

            HUDManager.Instance.ChatMessageHistory.Add(formattedMessage);

            UpdateChatText();}catch (Exception e){
                ChatCommands.mls.LogError("Error displaying chat error: " + e);
            }
            if (DisplayAsTooltip)
            {
                HUDManager.Instance.DisplayTip("ChatCommands", errorMessage, true, false, "LC_Tip1");
            }
        }

        private static void UpdateChatText()
        {
            HUDManager.Instance.chatText.text = string.Join("\n", HUDManager.Instance.ChatMessageHistory);
        }

        internal static string ConvertPlayername(string name)
        {
            ChatCommands.mls.LogInfo("Converting name: " + name);
            string convname = new string(name.Where((char c) => char.IsLetter(c)).ToArray());
            convname = Regex.Replace(convname, "[^\\w\\._]", "");
            ChatCommands.mls.LogInfo("Converted name: " + convname);
            return convname;
        }

        internal static bool IsNetCommand(string message){
            return message.StartsWith(NetCommandPrefix) && message.EndsWith(NetCommandPostfix);
        }

        internal static bool IsNetHostCommand(string message){
            return message.StartsWith(NetHostCommandPrefix) && message.EndsWith(NetCommandPostfix);
        }

        internal static string ConvertFromNetCommand(string command)
        {
            LogInfo("Converting command: " + command);
            string convertedCommand = command;
            if (!IsNetCommand(convertedCommand)){
                throw new Exception("Not a valid NetCommand");
            }
            convertedCommand = convertedCommand.Substring(NetCommandPrefix.Length);
            convertedCommand = convertedCommand.Substring(0, convertedCommand.Length - NetCommandPostfix.Length);
            convertedCommand = ChatCommands.PrefixSetting.Value + convertedCommand;
        
            LogInfo("Converted command: " + convertedCommand);
            return convertedCommand;
        }

        internal static string ConvertFromNetHostCommand(string command)
        {
            LogInfo("Converting command: " + command);
            string convertedCommand = command;
            if (!IsNetHostCommand(convertedCommand)){
                throw new Exception("Not a valid NetHostCommand");
            }
            convertedCommand = convertedCommand.Substring(NetHostCommandPrefix.Length);
            convertedCommand = convertedCommand.Substring(0, convertedCommand.Length - NetCommandPostfix.Length);
            convertedCommand = ChatCommands.PrefixSetting.Value + convertedCommand;
            LogInfo("Converted command: " + convertedCommand);
            return convertedCommand;
        }

        internal static string ConvertToNetCommand(string command){
            if (command.StartsWith(ChatCommands.PrefixSetting.Value))
            command = command.Substring(ChatCommands.PrefixSetting.Value.Length);
            LogInfo("Converting to NetCommand: " + command);
            LogInfo("Converted to NetCommand: " + NetCommandPrefix + command + NetCommandPostfix);
            return NetCommandPrefix + command + NetCommandPostfix;
        }
        internal static string ConvertToNetHostCommand(string command){
            if (command.StartsWith(ChatCommands.PrefixSetting.Value))
            command = command.Substring(ChatCommands.PrefixSetting.Value.Length);
            LogInfo("Converting to NetHostCommand: " + command);
            LogInfo("Converted to NetHostCommand: " + NetHostCommandPrefix + command + NetCommandPostfix);
            return NetHostCommandPrefix + command + NetCommandPostfix;
        }

        internal static void SendHostCommand(string commandInput)
        {
            if (!ChatCommands.isHost || !ChatCommands.SendHostCommandsSetting.Value)
            {
                return;
            }
            string commandToClients = ConvertToNetHostCommand(commandInput);
            LogInfo("Sending command to clients: " + commandToClients);
            HUDManager.Instance.AddTextToChatOnServer(commandToClients, -1);
        }

        internal static void SendCommandToServer(string commandInput)
        {
            string commandToServer = ConvertToNetCommand(commandInput);
            LogInfo("Sending command to server: " + commandToServer);
            HUDManager.Instance.AddTextToChatOnServer(commandToServer, -1);
        }

        internal static SelectableLevel GetCurrentLevel()
        {
            return RoundManager.Instance.currentLevel;
        }

        internal static RoundManager GetCurrentRound()
        {
            return RoundManager.Instance;
        }

    }
}