using static ChatCommands.Utils;
using UnityEngine;
using System.Linq;
using System;
using Unity.Netcode;
using System.Collections.Generic;

namespace ChatCommands.Commands
{
    internal class BuyStuff : CustomChatCommand
    {

        public override string Name => "Buy Stuff";

        public override string Description => "Buy stuff from the shop. Gets delivered normally.";

        public override string Format => "/buy [itemname]";
        public override string AltFormat => "/buyitem [itemname]";
        public override bool IsHostCommand => true;


        //TODO: Add more items / completely redo this
        public override void Execute(CommandInput message)
        {
            string msgtitle = "Item Buying";
            string msgbody = "";
            Terminal terminal = UnityEngine.Object.FindObjectOfType<Terminal>();
            if (terminal != null)
            {
                List<string> list = new List<string>
                    {
                        "Walkie-Talkie", "Pro Flashlight", "Normal Flashlight", "Shovel", "Lockpicker", "Stun Grenade", "Boom Box", "Inhaler", "Stun Gun", "Jet Pack",
                        "Extension Ladder", "Radar Booster"
                    };
                Dictionary<string, int> dictionary = new Dictionary<string, int>
                    {
                        { "Walkie-Talkie", 0 },
                        { "Pro Flashlight", 4 },
                        { "Normal Flashlight", 1 },
                        { "Shovel", 2 },
                        { "Lockpicker", 3 },
                        { "Stun Grenade", 5 },
                        { "Boom Box", 6 },
                        { "Inhaler", 7 },
                        { "Stun Gun", 8 },
                        { "Jet Pack", 9 },
                        { "Extension Ladder", 10 },
                        { "Radar Booster", 11 }
                    };;
                if (message.Args.Count > 0)
                {
                    bool flag3 = false;
                    if (message.Args.Count > 1)
                    {
                        if (!int.TryParse(message.Args[1], out var result2))
                        {
                            ChatCommands.mls.LogInfo("Couldn't parse command [ " + message.Args[1] + " ]");
                            DisplayChatError("Couldn't parse command [ " + message.Args[1] + " ]");
                            return;
                        }
                        foreach (string item in list)
                        {
                            if (item.ToLower().Contains(message.Args[0]))
                            {
                                flag3 = true;
                                List<int> list2 = new List<int>();
                                for (int i = 0; i < result2; i++)
                                {
                                    list2.Add(dictionary[item]);
                                }
                                terminal.BuyItemsServerRpc(list2.ToArray(), terminal.groupCredits, 0);
                                msgbody = "Bought " + result2 + " " + item + "s";
                                break;
                            }
                        }
                        if (!flag3)
                        {
                            DisplayChatError("Couldn't figure out what [ " + message.Args[0] + " ] was.");
                            return ;
                        }
                    }
                    if (!flag3)
                    {
                        bool flag4 = false;
                        foreach (string item2 in list)
                        {
                            if (item2.ToLower().Contains(message.Args[0]))
                            {
                                flag4 = true;
                                int[] array4 = new int[1] { dictionary[item2] };
                                terminal.BuyItemsServerRpc(array4, terminal.groupCredits, 0);
                                msgbody = "Bought " + 1 + " " + item2;
                            }
                        }
                        if (!flag4)
                        {
                            ChatCommands.mls.LogInfo("Couldn't figure out what [ " + message.Args[0] + " ] was. Trying via int parser.");
                        }
                        if (!int.TryParse(message.Args[0], out var result3))
                        {
                            ChatCommands.mls.LogInfo("Couldn't figure out what [ " + message.Args[0] + " ] was. Int parser failed, please try again.");
                            DisplayChatError("Couldn't figure out what [ " + message.Args[0] + " ] was. Int parser failed, please try again.");
                            return;
                        }
                        int[] array5 = new int[1] { result3 };
                        terminal.BuyItemsServerRpc(array5, terminal.groupCredits, 0);
                        msgbody = "Bought item with ID [" + result3 + "]";
                    }
                }
            DisplayChatMessage(msgtitle+"\n"+msgbody);
            }
        }}}