using static ChatCommands.Utils;
using UnityEngine;
using System.Linq;
using System;
using Unity.Netcode;
using System.Collections.Generic;

namespace ChatCommands.Commands
{
    internal class ChangeWeather : CustomChatCommand
    {

        public override string Name => "Change Weather";

        public override string Description => "Change Weather to a specific type.";

        public override string Format => "/changeweather [weathername]";
        public override string AltFormat => "/chwe [weathername]";
        public override bool IsHostCommand => true;


        //TODO: completely redo this
        public override void Execute(CommandInput message)
        {
            string msgtitle = "Weather Change";
            string msgbody = "";
            if (message.Args.Count > 0)
            {
                switch (message.Args[0].ToLower())
                {
                    case "rain":
                        ChatCommands.currentRound.timeScript.currentLevelWeather = (LevelWeatherType)1;
                        break;
                    case "eclipse":
                        ChatCommands.currentRound.timeScript.currentLevelWeather = (LevelWeatherType)5;
                        break;
                    case "flood":
                        ChatCommands.currentRound.timeScript.currentLevelWeather = (LevelWeatherType)4;
                        break;
                    case "dust":
                    case "fog":
                    case "mist":
                        ChatCommands.currentRound.timeScript.currentLevelWeather = (LevelWeatherType)0;
                        break;
                    case "storm":
                        ChatCommands.currentRound.timeScript.currentLevelWeather = (LevelWeatherType)2;
                        break;
                    case "none":
                        ChatCommands.currentRound.timeScript.currentLevelWeather = (LevelWeatherType)(-1);
                        break;
                    default:
                        LogInfo("Couldn't figure out what [ " + message.Args[1] + " ] was.");
                        msgbody = "Couldn't figure out what [ " + message.Args[1] + " ] was.";
                        break;
                }
                msgbody = "tried to change the weather to " + message.Args[0];
            }
            DisplayChatMessage(msgtitle + "\n" + msgbody);
        }}}