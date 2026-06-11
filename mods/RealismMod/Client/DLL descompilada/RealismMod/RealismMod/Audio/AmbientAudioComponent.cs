// Decompiled with JetBrains decompiler
// Type: RealismMod.Audio.AmbientAudioComponent
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using EFT;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace RealismMod.Audio;

public class AmbientAudioComponent : MonoBehaviour
{
  public List<AudioClip> AudioClips = new List<AudioClip>();
  public Transform ParentTransform;
  public bool FollowPlayer = false;
  public float MinTimeBetweenClips = 30f;
  public float MaxTimeBetweenClips = 120f;
  public float MinDistance = 48f;
  public float MaxDistance = 60f;
  public float DelayBeforePlayback = 30f;
  public float Volume = 1f;
  private float _elapsedTime = 0.0f;
  private AudioSource _audioSource;
  private float _randomDistanceFromPlayer;
  private Vector3 _relativePositionFromPlayer;

  public Player _Player { get; set; }

  private void Start()
  {
    this.Volume *= GameWorldController.GetGameVolumeAsFactor();
    this._audioSource = ((Component) this).gameObject.AddComponent<AudioSource>();
    this._audioSource.volume = this.Volume;
    this._audioSource.loop = false;
    this._audioSource.playOnAwake = false;
    this._audioSource.spatialBlend = 1f;
    this._audioSource.maxDistance = 25f;
    this._audioSource.maxDistance = 130f;
    this._audioSource.rolloffMode = (AudioRolloffMode) 1;
    if (this.FollowPlayer)
      ((Component) this).transform.SetParent(this.ParentTransform);
    this.StartCoroutine(this.PlayRandomAudio());
  }

  private void Update()
  {
    this._elapsedTime += Time.deltaTime;
    this._audioSource.volume = PlayerState.EnviroType != 1 && PlayerState.BtrState != 3 ? Mathf.MoveTowards(this._audioSource.volume, this.Volume * GameWorldController.GetGameVolumeAsFactor(), 0.35f * Time.deltaTime) : Mathf.MoveTowards(this._audioSource.volume, this.Volume * 0.5f * GameWorldController.GetGameVolumeAsFactor(), 0.35f * Time.deltaTime);
    if (!this.FollowPlayer)
      return;
    this._relativePositionFromPlayer = Quaternion.op_Multiply(Quaternion.AngleAxis(0.35f * Time.deltaTime, Vector3.up), this._relativePositionFromPlayer);
    ((Component) this).transform.position = Vector3.op_Addition(this.ParentTransform.position, this._relativePositionFromPlayer);
  }

  private IEnumerator PlayRandomAudio()
  {
    while (true)
    {
      if (Utils.PlayerIsReady && (double) this._elapsedTime >= (double) this.DelayBeforePlayback)
      {
        AudioClip selectedClip = this.AudioClips[Random.Range(0, this.AudioClips.Count)];
        this._randomDistanceFromPlayer = Random.Range(this.MinDistance, this.MaxDistance);
        Vector3 randomPosition = Vector3.op_Addition(this.ParentTransform.position, Vector3.op_Multiply(Random.onUnitSphere, this._randomDistanceFromPlayer));
        randomPosition.y = Mathf.Clamp(randomPosition.y, this.ParentTransform.position.y - 25f, this.ParentTransform.position.y + 25f);
        ((Component) this).transform.position = randomPosition;
        this._relativePositionFromPlayer = Vector3.op_Subtraction(((Component) this).transform.position, this.ParentTransform.position);
        if (PluginConfig.ZoneDebug.Value)
        {
          GameObject visualRepresentation = GameObject.CreatePrimitive((PrimitiveType) 3);
          ((Object) visualRepresentation).name = "AmbientAudioPlayerVisual";
          visualRepresentation.transform.parent = ((Component) this).transform;
          visualRepresentation.transform.localScale = Vector3.one;
          visualRepresentation.transform.position = ((Component) this).transform.position;
          visualRepresentation.transform.rotation = ((Component) this.ParentTransform).transform.rotation;
          visualRepresentation.GetComponent<Renderer>().material.color = new Color(1f, 0.0f, 0.0f, 1f);
          visualRepresentation = (GameObject) null;
        }
        this._audioSource.clip = selectedClip;
        this._audioSource.Play();
        yield return (object) new WaitForSeconds(selectedClip.length);
        float waitTime = Random.Range(this.MinTimeBetweenClips, this.MaxTimeBetweenClips);
        yield return (object) new WaitForSeconds(waitTime);
        selectedClip = (AudioClip) null;
        randomPosition = new Vector3();
      }
      yield return (object) null;
    }
  }
}
