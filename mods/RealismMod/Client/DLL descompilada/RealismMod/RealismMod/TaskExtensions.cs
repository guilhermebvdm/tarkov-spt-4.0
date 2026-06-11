// Decompiled with JetBrains decompiler
// Type: RealismMod.TaskExtensions
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using System.Collections;
using System.Threading.Tasks;

#nullable disable
namespace RealismMod;

public static class TaskExtensions
{
  public static IEnumerator AsCoroutine(this Task task)
  {
    while (!task.IsCompleted)
      yield return (object) null;
    if (task.IsFaulted)
      throw task.Exception;
  }
}
