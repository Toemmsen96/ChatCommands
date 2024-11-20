using static ChatCommands.Utils;
using UnityEngine;
using System.Linq;
using System;
using Unity.Netcode;
using HarmonyLib;

namespace ChatCommands.Commands
{
    internal class SpawnTruck : CustomChatCommand
    {
        public static GameObject truckPrefab;

        public override string Name => "Spawn Truck";

        public override string Description => "Spawns the Truck, either at the default position or at a specified position.";

        public override string Format => "/spawntruck ([p=position])";
        public override string AltFormat => "/spwtrk ([p=position])";
        public override bool IsHostCommand => true;

        public override void Execute(CommandInput message)
        {
            if (message.Args.Count < 1)
            {
                UnityEngine.Object.Instantiate(truckPrefab, StartOfRound.Instance.groundOutsideShipSpawnPosition.position, Quaternion.identity, RoundManager.Instance.VehiclesContainer).gameObject.GetComponent<NetworkObject>().Spawn();
                return;
            } else
            {
                Vector3 position = Vector3.zero;
                position = CalculateSpawnPosition(message.Args[0]);
                if (position == Vector3.zero)
                {
                    position = StartOfRound.Instance.groundOutsideShipSpawnPosition.position;
                }
                UnityEngine.Object.Instantiate(truckPrefab, position, Quaternion.identity, RoundManager.Instance.VehiclesContainer).gameObject.GetComponent<NetworkObject>().Spawn();
            }
        }
        [HarmonyPatch(typeof(QuickMenuManager), "Start")]
        [HarmonyPostfix]
        private static void GetTruckPrefab(QuickMenuManager __instance)
        {
            truckPrefab = __instance.truckPrefab;
        }}
}