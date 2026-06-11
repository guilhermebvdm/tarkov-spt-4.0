// Decompiled with JetBrains decompiler
// Type: RealismMod.Audio.RealismAudioController
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using BepInEx.Configuration;
using EFT;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

#nullable disable
namespace RealismMod.Audio;

public class RealismAudioController : MonoBehaviour
{
  private Player _Player;
  private AudioSource _foodPoisoningSource;
  private AudioSource _gasMaskAudioSource;
  private AudioSource _gasAnalyserSource;
  private AudioSource _geigerAudioSource;
  private AudioSource _toggleDeviceSource;
  private AudioSource _heartBeatSource;
  public Dictionary<string, AudioClip> HitAudioClips = new Dictionary<string, AudioClip>();
  public Dictionary<string, AudioClip> GasMaskAudioClips = new Dictionary<string, AudioClip>();
  public Dictionary<string, AudioClip> HazardZoneClips = new Dictionary<string, AudioClip>();
  public Dictionary<string, AudioClip> DeviceAudioClips = new Dictionary<string, AudioClip>();
  public Dictionary<string, AudioClip> RadEventAudioClips = new Dictionary<string, AudioClip>();
  public Dictionary<string, AudioClip> GasEventAudioClips = new Dictionary<string, AudioClip>();
  public Dictionary<string, AudioClip> GasEventLongAudioClips = new Dictionary<string, AudioClip>();
  public Dictionary<string, AudioClip> InteractableClips = new Dictionary<string, AudioClip>();
  public Dictionary<string, AudioClip> FoodPoisoningSfx = new Dictionary<string, AudioClip>();
  public Dictionary<string, AudioClip> HeartbeatSfx = new Dictionary<string, AudioClip>();
  private const float GAS_DELAY = 4f;
  private const float RAD_DELAY = 3f;
  private const float GAS_DEVICE_VOLUME = 0.28f;
  private const float GEIGER_VOLUME = 0.32f;
  private const float BASE_BREATH_VOLUME = 0.3f;
  private const float TOGGLE_DEVICE_VOLUME = 0.6f;
  private const float HEARTBEAT_TENSE_FADE_SPEED = 0.005f;
  public bool ClipsAreReady = false;
  private float _currentBreathClipLength = 0.0f;
  private float _breathTimer = 0.0f;
  private float _breathCountdown = 2.5f;
  private float _coughTimer = 0.0f;
  private bool _breathedOut = false;
  private bool _muteGeiger = false;
  private bool _muteGasAnalyser = false;
  private float _currentGasClipLength = 0.0f;
  private float _gasDeviceTimer = 0.0f;
  private float _currentGeigerClipLength = 0.0f;
  private float _geigerDeviceTimer = 0.0f;
  private float _heartBeatSfxVolModi = 1f;

  private void Start()
  {
    this._gasMaskAudioSource = ((Component) this).gameObject.AddComponent<AudioSource>();
    this._gasAnalyserSource = ((Component) this).gameObject.AddComponent<AudioSource>();
    this._geigerAudioSource = ((Component) this).gameObject.AddComponent<AudioSource>();
    this._toggleDeviceSource = ((Component) this).gameObject.AddComponent<AudioSource>();
    this._foodPoisoningSource = ((Component) this).gameObject.AddComponent<AudioSource>();
    this._heartBeatSource = ((Component) this).gameObject.AddComponent<AudioSource>();
    this.SetUpAudio(this._gasMaskAudioSource);
    this.SetUpAudio(this._gasAnalyserSource, spatialBlend: 1f);
    this.SetUpAudio(this._geigerAudioSource, spatialBlend: 1f);
    this.SetUpAudio(this._toggleDeviceSource, 0.6f, 0.37f);
    this.SetUpAudio(this._foodPoisoningSource, 0.65f);
    this.SetUpAudio(this._heartBeatSource);
  }

  private void Update()
  {
    if (Utils.IsInHideout || !Utils.PlayerIsReady || Object.op_Equality((Object) this._Player, (Object) null))
      return;
    ((Component) this).transform.position = ((Component) this._Player).gameObject.transform.position;
    this.DoHeartBeatSfx();
    this.DoBreathSfx();
    this.DoGasAnalyserAudio();
    this.DoGeigerAudio();
  }

  private async Task LoadAudioClipHelper(
    string[] fileDirectories,
    Dictionary<string, AudioClip> clips)
  {
    string[] strArray = fileDirectories;
    for (int index = 0; index < strArray.Length; ++index)
    {
      string fileDir = strArray[index];
      Dictionary<string, AudioClip> dictionary = clips;
      string key = Path.GetFileName(fileDir);
      AudioClip audioClip = await this.RequestAudioClip(fileDir);
      dictionary[key] = audioClip;
      dictionary = (Dictionary<string, AudioClip>) null;
      key = (string) null;
      audioClip = (AudioClip) null;
      fileDir = (string) null;
    }
    strArray = (string[]) null;
  }

  private async Task<AudioClip> RequestAudioClip(string path)
  {
    string extension = Path.GetExtension(path);
    AudioType audioType = (AudioType) 20;
    string str = extension;
    switch (str)
    {
      case ".wav":
        audioType = (AudioType) 20;
        break;
      case ".ogg":
        audioType = (AudioType) 14;
        break;
    }
    str = (string) null;
    UnityWebRequest uwr = UnityWebRequestMultimedia.GetAudioClip(path, audioType);
    UnityWebRequestAsyncOperation sendWeb = uwr.SendWebRequest();
    while (!((AsyncOperation) sendWeb).isDone)
      await Task.Yield();
    if (uwr.isNetworkError || uwr.isHttpError)
    {
      Utils.Logger.LogError((object) "Realism Mod: Failed To Fetch Audio Clip");
      return (AudioClip) null;
    }
    AudioClip audioclip = DownloadHandlerAudioClip.GetContent(uwr);
    return audioclip;
  }

  public IEnumerator LoadAudioClipsCoroutine()
  {
    yield return (object) this.LoadAudioClips().AsCoroutine();
    this.ClipsAreReady = true;
  }

  public async Task LoadAudioClips()
  {
    string[] hitSoundsDir = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + "\\BepInEx\\plugins\\Realism\\sounds\\hitsounds");
    string[] gasMaskDir = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + "\\BepInEx\\plugins\\Realism\\sounds\\gasmask");
    string[] hazardDir = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + "\\BepInEx\\plugins\\Realism\\sounds\\zones");
    string[] deviceDir = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + "\\BepInEx\\plugins\\Realism\\sounds\\devices");
    string[] gasEventAmbient = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + "\\BepInEx\\plugins\\Realism\\sounds\\zones\\mapgas\\default");
    string[] radEventAmbient = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + "\\BepInEx\\plugins\\Realism\\sounds\\zones\\maprads");
    string[] gasEventLongAmbient = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + "\\BepInEx\\plugins\\Realism\\sounds\\zones\\mapgas\\long");
    string[] foodPoisoning = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + "\\BepInEx\\plugins\\Realism\\sounds\\health\\foodpoisoning");
    string[] interactable = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + "\\BepInEx\\plugins\\Realism\\sounds\\zones\\interactable");
    string[] heartbeat = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + "\\BepInEx\\plugins\\Realism\\sounds\\health\\heartbeat");
    this.HitAudioClips.Clear();
    this.GasMaskAudioClips.Clear();
    this.HazardZoneClips.Clear();
    this.DeviceAudioClips.Clear();
    this.GasEventAudioClips.Clear();
    this.RadEventAudioClips.Clear();
    this.InteractableClips.Clear();
    this.FoodPoisoningSfx.Clear();
    this.HeartbeatSfx.Clear();
    await this.LoadAudioClipHelper(hitSoundsDir, this.HitAudioClips);
    await this.LoadAudioClipHelper(gasMaskDir, this.GasMaskAudioClips);
    await this.LoadAudioClipHelper(hazardDir, this.HazardZoneClips);
    await this.LoadAudioClipHelper(deviceDir, this.DeviceAudioClips);
    await this.LoadAudioClipHelper(gasEventAmbient, this.GasEventAudioClips);
    await this.LoadAudioClipHelper(radEventAmbient, this.RadEventAudioClips);
    await this.LoadAudioClipHelper(gasEventLongAmbient, this.GasEventLongAudioClips);
    await this.LoadAudioClipHelper(foodPoisoning, this.FoodPoisoningSfx);
    await this.LoadAudioClipHelper(heartbeat, this.HeartbeatSfx);
    await this.LoadAudioClipHelper(interactable, this.InteractableClips);
    hitSoundsDir = (string[]) null;
    gasMaskDir = (string[]) null;
    hazardDir = (string[]) null;
    deviceDir = (string[]) null;
    gasEventAmbient = (string[]) null;
    radEventAmbient = (string[]) null;
    gasEventLongAmbient = (string[]) null;
    foodPoisoning = (string[]) null;
    interactable = (string[]) null;
    heartbeat = (string[]) null;
  }

  public void RunReInitPlayer()
  {
    this._Player = Utils.GetYourPlayer();
    ((Component) this).transform.position = ((Component) this._Player).gameObject.transform.position;
  }

  private void SetUpAudio(
    AudioSource source,
    float vol = 1f,
    float spatialBlend = 0.0f,
    float minDistance = 5f,
    float maxDistance = 10f)
  {
    source.volume = vol * GameWorldController.GetGameVolumeAsFactor();
    source.spatialBlend = spatialBlend;
    source.minDistance = minDistance;
    source.maxDistance = maxDistance;
  }

  private void DoHeartBeatSfx()
  {
    bool flag = Plugin.RealHealthController.HasPositiveAdrenalineEffect || Plugin.RealHealthController.HasNegativeAdrenalineEffect;
    this._heartBeatSfxVolModi = !Plugin.RealHealthController.HasPositiveAdrenalineEffect ? (!Plugin.RealHealthController.HasNegativeAdrenalineEffect ? 0.0f : Mathf.Lerp(this._heartBeatSfxVolModi, 0.1f, 0.005f)) : PluginConfig.HeartBeatVolume.Value;
    if (!Object.op_Inequality((Object) this._heartBeatSource, (Object) null))
      return;
    if (!this._heartBeatSource.isPlaying & flag)
    {
      this._heartBeatSource.clip = this.HeartbeatSfx["heartbeat-tense.wav"];
      this._heartBeatSource.volume = this._heartBeatSfxVolModi * GameWorldController.GetGameVolumeAsFactor();
      this._heartBeatSource.Play();
    }
    if (!flag)
      this._heartBeatSource.Stop();
  }

  private void DoBreathSfx()
  {
    this._breathTimer += Time.deltaTime;
    this._coughTimer += Time.deltaTime;
    if (GearController.HasGasMask && (double) this._breathCountdown > 0.0)
      this._breathCountdown -= Time.deltaTime;
    else
      this._breathCountdown = 2.5f;
    if ((double) this._breathTimer > (double) this._currentBreathClipLength && (double) this._breathCountdown <= 0.0 && GearController.HasGasMask && (!this._Player.Speaker.Busy || !this._Player.Speaker.Speaking))
    {
      this.PlayGasMaskBreathing(this._breathedOut);
      this._breathTimer = 0.0f;
      this._breathedOut = !this._breathedOut;
    }
    if ((double) this._coughTimer <= 5.0)
      return;
    this.CoughController();
    this._coughTimer = 0.0f;
  }

  public void PlayFoodPoisoningSFXInRaid(float vol = 0.5f)
  {
    this._foodPoisoningSource.clip = GClass835.RandomElement<KeyValuePair<string, AudioClip>>((IEnumerable<KeyValuePair<string, AudioClip>>) this.FoodPoisoningSfx).Value;
    this._foodPoisoningSource.volume = vol * GameWorldController.GetGameVolumeAsFactor();
    this._foodPoisoningSource.Play();
  }

  private void CoughController()
  {
    if (!Plugin.RealHealthController.DoCoughingAudio)
      return;
    this._Player.Speaker.Play((EPhraseTrigger) 29, (ETagStatus) 8194, true, new int?());
  }

  private float GetBreathVolume()
  {
    return Mathf.Max((0.3f + GameWorldController.GetHeadsetVolume()) * ((2f - PlayerState.BaseStaminaPerc) * PluginConfig.GasMaskBreathVolume.Value * GameWorldController.GetGameVolumeAsFactor()), 0.0f);
  }

  private string GetAudioFromOtherStates()
  {
    if ((double) HazardTracker.TotalToxicity >= 70.0 || (double) HazardTracker.TotalRadiation >= 85.0 || Plugin.RealHealthController.IsCoughingInGas)
      return "Dying";
    if ((double) HazardTracker.TotalToxicity >= 50.0 || (double) PlayerState.BaseStaminaPerc <= 0.55 || (double) HazardTracker.TotalRadiation >= 70.0 || Plugin.RealHealthController.HasPositiveAdrenalineEffect)
      return "BadlyInjured";
    return (double) HazardTracker.TotalToxicity >= 30.0 || (double) PlayerState.BaseStaminaPerc <= 0.800000011920929 || (double) HazardTracker.TotalRadiation >= 50.0 ? "Injured" : "Healthy";
  }

  private string ChooseAudioClip(string healthStatus, string desiredClip)
  {
    if (desiredClip == "Dying" || healthStatus == "Dying")
      return "Dying";
    if (desiredClip == "BadlyInjured" || healthStatus == "BadlyInjured")
      return "BadlyInjured";
    return desiredClip == "Injured" || healthStatus == "Injured" ? "Injured" : "Healthy";
  }

  public void PlayGasMaskBreathing(bool breathOut)
  {
    int num = Random.Range(1, 4);
    string str = this.ChooseAudioClip(this._Player.HealthStatus.ToString(), this.GetAudioFromOtherStates());
    AudioClip gasMaskAudioClip = this.GasMaskAudioClips[$"{(breathOut ? "out" : "in")}_{str}{num.ToString()}.wav"];
    this._currentBreathClipLength = gasMaskAudioClip.length;
    float breathVolume = this.GetBreathVolume();
    this._Player.SpeechSource.SetLowPassFilterParameters(0.99f, (ESoundOcclusionType) 1, 1600f, 5000f, 1f, true);
    this._gasMaskAudioSource.volume = breathVolume;
    this._gasMaskAudioSource.clip = gasMaskAudioClip;
    this._gasMaskAudioSource.Play();
  }

  private float GetGasDelayTime()
  {
    return Object.op_Equality((Object) Plugin.RealHealthController.PlayerHazardBridge, (Object) null) ? 4f : (float) (4.0 * (1.0 - (double) HazardTracker.BaseTotalToxicityRate));
  }

  private float GeRadDelayTime()
  {
    if (Object.op_Equality((Object) Plugin.RealHealthController.PlayerHazardBridge, (Object) null))
      return 1f;
    return (double) HazardTracker.BaseTotalRadiationRate >= 0.14000000059604645 ? 0.0f : (float) (3.0 * (1.0 - (double) Mathf.Pow(HazardTracker.BaseTotalRadiationRate, 0.35f)));
  }

  private void PlayToggleSfx(string clip)
  {
    this._toggleDeviceSource.clip = this.DeviceAudioClips[clip];
    this._toggleDeviceSource.volume = (0.6f + GameWorldController.GetHeadsetVolume()) * GameWorldController.GetGameVolumeAsFactor();
    this._toggleDeviceSource.Play();
  }

  private void DoDetectors()
  {
    if (!GearController.HasGasAnalyser || !GameWorldController.GameStarted || !Utils.PlayerIsReady)
      return;
    this.DoGasAnalyserAudio();
    this.DoGeigerAudio();
  }

  public void DoGasAnalyserAudio()
  {
    if (!GearController.HasGasAnalyser)
      return;
    this._gasDeviceTimer += Time.deltaTime;
    KeyboardShortcut keyboardShortcut1 = PluginConfig.MuteGasAnalyserKey.Value;
    int num;
    if (Input.GetKeyDown(((KeyboardShortcut) ref keyboardShortcut1).MainKey))
    {
      KeyboardShortcut keyboardShortcut2 = PluginConfig.MuteGasAnalyserKey.Value;
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      num = ((KeyboardShortcut) ref keyboardShortcut2).Modifiers.All<KeyCode>(RealismAudioController.\u003C\u003EO.\u003C0\u003E__GetKey ?? (RealismAudioController.\u003C\u003EO.\u003C0\u003E__GetKey = new Func<KeyCode, bool>(Input.GetKey))) ? 1 : 0;
    }
    else
      num = 0;
    if (num != 0)
    {
      this._muteGasAnalyser = !this._muteGasAnalyser;
      this.PlayToggleSfx("switch_off.wav");
    }
    if ((double) this._gasDeviceTimer > (double) this._currentGasClipLength && (double) this._gasDeviceTimer >= (double) this.GetGasDelayTime())
    {
      PlayerZoneBridge playerHazardBridge = Plugin.RealHealthController.PlayerHazardBridge;
      if (Object.op_Inequality((Object) this._Player, (Object) null) && Object.op_Inequality((Object) playerHazardBridge, (Object) null) && (double) HazardTracker.BaseTotalToxicityRate > 0.0)
      {
        this.PlayGasAnalyserClips(this._Player);
        this._gasDeviceTimer = 0.0f;
      }
    }
  }

  public void DoGeigerAudio()
  {
    if (!GearController.HasGeiger)
      return;
    this._geigerDeviceTimer += Time.deltaTime;
    KeyboardShortcut keyboardShortcut1 = PluginConfig.MuteGeigerKey.Value;
    int num;
    if (Input.GetKeyDown(((KeyboardShortcut) ref keyboardShortcut1).MainKey))
    {
      KeyboardShortcut keyboardShortcut2 = PluginConfig.MuteGeigerKey.Value;
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      num = ((KeyboardShortcut) ref keyboardShortcut2).Modifiers.All<KeyCode>(RealismAudioController.\u003C\u003EO.\u003C0\u003E__GetKey ?? (RealismAudioController.\u003C\u003EO.\u003C0\u003E__GetKey = new Func<KeyCode, bool>(Input.GetKey))) ? 1 : 0;
    }
    else
      num = 0;
    if (num != 0)
    {
      this._muteGeiger = !this._muteGeiger;
      this.PlayToggleSfx("switch_off.wav");
    }
    if ((double) this._geigerDeviceTimer > (double) this._currentGeigerClipLength && (double) this._geigerDeviceTimer >= (double) this.GeRadDelayTime())
    {
      PlayerZoneBridge playerHazardBridge = Plugin.RealHealthController.PlayerHazardBridge;
      if (Object.op_Inequality((Object) this._Player, (Object) null) && Object.op_Inequality((Object) playerHazardBridge, (Object) null) && (double) HazardTracker.BaseTotalRadiationRate > 0.0)
      {
        this.PlayGeigerClips(this._Player);
        this._geigerDeviceTimer = 0.0f;
      }
    }
  }

  public string GetGasAnalsyerClip(float gasLevel, out float volumeModi)
  {
    float num = gasLevel;
    if ((double) num > 0.0099999997764825821)
    {
      if ((double) num > 0.019999999552965164)
      {
        if ((double) num > 0.039999999105930328)
        {
          if ((double) num > 0.079999998211860657)
          {
            if ((double) num > 0.11999999731779099)
            {
              if ((double) num > 0.15999999642372131)
              {
                if ((double) num > 0.18999999761581421)
                {
                  if ((double) num > 0.18999999761581421)
                  {
                    volumeModi = 0.8f;
                    return "gasBeep7.wav";
                  }
                  volumeModi = 1f;
                  return (string) null;
                }
                volumeModi = 1f;
                return "gasBeep6.wav";
              }
              volumeModi = 1f;
              return "gasBeep5.wav";
            }
            volumeModi = 1f;
            return "gasBeep4.wav";
          }
          volumeModi = 1.1f;
          return "gasBeep3.wav";
        }
        volumeModi = 1.1f;
        return "gasBeep2.wav";
      }
      volumeModi = 1.1f;
      return "gasBeep1.wav";
    }
    volumeModi = 1f;
    return (string) null;
  }

  public string[] GetGeigerClip(float radLevel)
  {
    float num = radLevel;
    if ((double) num > 0.014999999664723873)
    {
      if ((double) num > 0.029999999329447746)
      {
        if ((double) num > 0.059999998658895493)
        {
          if ((double) num > 0.10000000149011612)
          {
            if ((double) num > 0.14000000059604645)
            {
              if ((double) num > 0.20000000298023224)
              {
                if ((double) num <= 0.20000000298023224)
                  return (string[]) null;
                return new string[4]
                {
                  "geiger7.wav",
                  "geiger7_1.wav",
                  "geiger7_2.wav",
                  "geiger7_3.wav"
                };
              }
              return new string[4]
              {
                "geiger6.wav",
                "geiger6_1.wav",
                "geiger6_2.wav",
                "geiger6_3.wav"
              };
            }
            return new string[4]
            {
              "geiger5.wav",
              "geiger5_1.wav",
              "geiger5_2.wav",
              "geiger5_3.wav"
            };
          }
          return new string[4]
          {
            "geiger4.wav",
            "geiger4_1.wav",
            "geiger4_2.wav",
            "geiger4_3.wav"
          };
        }
        return new string[4]
        {
          "geiger3.wav",
          "geiger3_1.wav",
          "geiger3_2.wav",
          "geiger3_3.wav"
        };
      }
      return new string[4]
      {
        "geiger2.wav",
        "geiger2_1.wav",
        "geiger2_2.wav",
        "geiger2_3.wav"
      };
    }
    return new string[4]
    {
      "geiger1.wav",
      "geiger1_1.wav",
      "geiger1_2.wav",
      "geiger1_3.wav"
    };
  }

  private float GetDeviceVolume(float baseVol, float additionalModi = 1f)
  {
    baseVol += GameWorldController.GetHeadsetVolume();
    float num = PluginConfig.DeviceVolume.Value * additionalModi * GameWorldController.GetGameVolumeAsFactor();
    return Mathf.Max(baseVol * num, 0.0f);
  }

  public void PlayGasAnalyserClips(Player player)
  {
    float volumeModi = 1f;
    string gasAnalsyerClip = this.GetGasAnalsyerClip(HazardTracker.BaseTotalToxicityRate, out volumeModi);
    if (gasAnalsyerClip == null)
      return;
    AudioClip deviceAudioClip = this.DeviceAudioClips[gasAnalsyerClip];
    this._currentGasClipLength = deviceAudioClip.length;
    this._gasAnalyserSource.volume = this._muteGasAnalyser ? 0.0f : this.GetDeviceVolume(0.32f, volumeModi);
    this._gasAnalyserSource.clip = deviceAudioClip;
    this._gasAnalyserSource.Play();
  }

  public void PlayGeigerClips(Player player)
  {
    string[] geigerClip = this.GetGeigerClip(HazardTracker.BaseTotalRadiationRate);
    if (geigerClip == null)
      return;
    int index = Random.Range(0, geigerClip.Length);
    AudioClip deviceAudioClip = this.DeviceAudioClips[geigerClip[index]];
    this._currentGeigerClipLength = deviceAudioClip.length;
    this._geigerAudioSource.volume = this._muteGeiger ? 0.0f : this.GetDeviceVolume(0.32f);
    this._geigerAudioSource.clip = deviceAudioClip;
    this._geigerAudioSource.Play();
  }
}
