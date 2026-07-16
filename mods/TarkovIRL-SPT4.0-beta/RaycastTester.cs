using EFT;
using UnityEngine;

#nullable disable
namespace TarkovIRL;

internal static class RaycastTester
{
  public static void CheckRaycast(Player player)
  {
    Vector3 position = player.MainParts[(BodyPartType) 0].Position;
    Vector3 vector3 = ((position + (player.HeadRotation * 1000f)) - position);
    float magnitude = vector3.magnitude;
  }
}

