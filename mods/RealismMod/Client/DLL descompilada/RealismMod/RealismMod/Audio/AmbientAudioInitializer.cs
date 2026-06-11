// Decompiled with JetBrains decompiler
// Type: RealismMod.Audio.AmbientAudioInitializer
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using EFT;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace RealismMod.Audio;

public class AmbientAudioInitializer
{
  public static void CreateAmbientAudioPlayer(
    Player player,
    Transform parentTransform,
    Dictionary<string, AudioClip> clips,
    bool followPlayer = false,
    float minTime = 15f,
    float maxTime = 90f,
    float volume = 1f,
    float minDistance = 45f,
    float maxDistance = 95f,
    float minDelayBeforePlayback = 60f)
  {
    AmbientAudioComponent ambientAudioComponent = new GameObject("AmbientAudioPlayer").AddComponent<AmbientAudioComponent>();
    ambientAudioComponent.ParentTransform = parentTransform;
    ambientAudioComponent._Player = player;
    ambientAudioComponent.FollowPlayer = followPlayer;
    ambientAudioComponent.MinTimeBetweenClips = minTime;
    ambientAudioComponent.MaxTimeBetweenClips = maxTime;
    ambientAudioComponent.MinDistance = minDistance;
    ambientAudioComponent.MaxDistance = maxDistance;
    ambientAudioComponent.Volume = volume;
    ambientAudioComponent.DelayBeforePlayback = minDelayBeforePlayback;
    foreach (KeyValuePair<string, AudioClip> clip in clips)
      ambientAudioComponent.AudioClips.Add(clip.Value);
  }
}
