using UnityEngine;
using HarmonyLib;
using EFT;

namespace TarkovIRL
{
    internal static class RaycastTester
    {
        public static void CheckRaycast(Player player)
        {
            Vector3 headPosition = player.MainParts[BodyPartType.head].Position;
            Vector3 direction = (headPosition + player.HeadRotation * 1000f) - headPosition;
            float distance = direction.magnitude;

            
            //if (!Physics.Raycast(new Ray(headPosition, direction), out RaycastHit hitInfo, distance, GetLayerMask())) return;


        }
        
    }
}
