// Decompiled with JetBrains decompiler
// Type: TarkovIRL.RaycastTester
// Assembly: TarkovIRL, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C42939BD-7BF0-4586-ABE5-9D2EFC361A0B
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\TarkovIRL_WeaponsHandlingMod_1.0.0\BepInEx\plugins\TarkovIRL.dll

using EFT;
using UnityEngine;

#nullable disable
namespace TarkovIRL;

internal static class RaycastTester
{
  public static void CheckRaycast(Player player)
  {
    Vector3 position = player.MainParts[(BodyPartType) 0].Position;
    Vector3 vector3 = Vector3.op_Subtraction(Vector3.op_Addition(position, Vector3.op_Multiply(player.HeadRotation, 1000f)), position);
    float magnitude = ((Vector3) ref vector3).magnitude;
  }
}
