using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Networking;
using static ChatCommands.Utils;
namespace Networking
{
    public class CCMDNetworking : NetworkBehaviour
    {
        [SerializeField] string netHostCommandPrefix = "[COMMAND]";
        [SerializeField] string netCommandPostfix = "[/COMMAND]";

        // Singleton instance
        public static CCMDNetworking instance;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Debug.LogWarning("Multiple instances of CustomNetworkManager found!");
                Destroy(gameObject);
            }
        }

        // Method to send a command from the host to all clients
        internal static void SendHostCommand(string commandInput)
        {
            //if (!ChatCommands.isHost || !ChatCommands.SendHostCommandsSetting.Value)
            //{
            //    return;
            //}

            if (instance == null)
            {
                Debug.LogError("CCMDNetworking instance is not initialized!");
                return;
            }

            // Construct the full command to send to clients
            string commandToClients = instance.netHostCommandPrefix + commandInput + instance.netCommandPostfix;

            DisplayChatMessage("Host Command: " + commandInput+"\nsending to clients via RPC");

            // Call an RPC to execute the command on all clients
            instance.RpcExecuteCommandOnClients(commandToClients);
        }

        // RPC method to execute the command on all clients
        [ClientRpc]
        void RpcExecuteCommandOnClients(string command)
        {
            // Assuming you have a method to handle processing the host command on clients
            DisplayChatMessage("Client Command: " + command);
            ChatCommands.ChatCommands.ProcessNetHostCommand(command);
        }

        // Method to process the host command on clients
        //void ProcessNetHostCommand(string command)
        //{
        //    // Add logic here to process the host command on clients
        //}
    }
}
