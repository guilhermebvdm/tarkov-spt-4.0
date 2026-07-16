using System.Runtime.CompilerServices;
using UnderFire;

#nullable disable
namespace TarkovIRL;

internal static class UnderFireSoftWrapper
{
  [MethodImpl(MethodImplOptions.NoInlining)]
  public static bool GetAdrenaline() => Plugin.isAdrenalineActive;
}

