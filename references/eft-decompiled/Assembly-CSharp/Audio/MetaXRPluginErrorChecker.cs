using System.Collections;
using UnityEngine;

namespace Audio;

public class MetaXRPluginErrorChecker : MonoBehaviour
{
	public float checkCooldown = 0.3f;

	private bool _isEnabled;

	private Coroutine _checkCoroutine;

	private float _nextCheckTime;

	private double _currentOutputVolume = -80.0;

	private double _currentTimeDelta;

	private readonly float[] _samples = new float[1024];

	public void SetActive(bool checkEnabled)
	{
		if (_isEnabled != checkEnabled)
		{
			GClass722.Instance.LogInfo($"ReverbPluginChecker enabled: {checkEnabled}, check cooldown: {checkCooldown}");
			GClass7.TryStopCoroutine(this, ref _checkCoroutine);
			_isEnabled = checkEnabled;
			if (checkEnabled)
			{
				_checkCoroutine = StartCoroutine(CheckVolumeCoroutine());
			}
		}
	}

	public IEnumerator CheckVolumeCoroutine()
	{
		while (_isEnabled)
		{
			if (Time.realtimeSinceStartup > _nextCheckTime)
			{
				UpdateAudioData();
				UpdateTimeDelta();
				_nextCheckTime = Time.realtimeSinceStartup + checkCooldown;
			}
			if (_currentOutputVolume >= 0.0)
			{
				ResetPlugin();
			}
			yield return null;
		}
	}

	public void ResetPlugin()
	{
		GClass722.Instance.LogWarn("Reverb plugin error, reset plugin...");
		MetaXRAcousticNativeInterface.Interface.ResetReverb();
		UpdateAudioData();
	}

	public void UpdateAudioData()
	{
		AudioListener.GetOutputData(_samples, 0);
		_currentOutputVolume = GClass2313.GetOutputVolume(_samples);
	}

	public void UpdateTimeDelta()
	{
		_currentTimeDelta = Mathf.Abs(Time.time - (float)AudioSettings.dspTime);
	}

	public void OnDestroy()
	{
		SetActive(checkEnabled: false);
	}
}
