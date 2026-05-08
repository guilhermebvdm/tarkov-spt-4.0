using System;
using System.Collections;
using System.Collections.Generic;
using Comfort.Common;
using EFT;
using EFT.Ballistics;
using EFT.CameraControl;
using UnityEngine;

public class FlyingBulletSoundPlayer : BulletSoundPlayer
{
	[SerializeField]
	private AudioClip[] _sources;

	[SerializeField]
	[GAttribute6(0f, 10f, -1f)]
	private Vector2 _minMaxRadius = new Vector2(0f, 10f);

	[SerializeField]
	private AnimationCurve _vignette;

	[SerializeField]
	private float _vignetteTime = 0.4f;

	[SerializeField]
	private float _vignetteDelta = 0.2f;

	private List<BallisticsCalculator> list_0;

	private Dictionary<string, int> dictionary_0;

	private string[] string_0 = new string[20];

	private int int_0;

	private PrismEffects prismEffects_0;

	private float float_0 = 0.4f;

	private IEnumerator ienumerator_0;

	private Player player_0;

	private Coroutine coroutine_0;

	private float float_1;

	public override void Init(Camera camera, PlayerCameraController playerCameraController)
	{
		prismEffects_0 = camera.GetComponent<PrismEffects>();
		float_0 = prismEffects_0.vignetteEnd;
		player_0 = playerCameraController.Player;
		float_1 = 0f;
		if (player_0 == null)
		{
			Debug.LogError("Sorry Dimon, tyt nikogo");
		}
	}

	public override void TryShot(EftBulletClass shot, Camera camera, string id, int currentIndex)
	{
		if (!dictionary_0.ContainsKey(id))
		{
			dictionary_0.Add(id, 0);
		}
		else if (dictionary_0[id] < currentIndex)
		{
			Vector3 start = shot.PositionHistory[dictionary_0[id]];
			Vector3 end = shot.PositionHistory[currentIndex];
			Vector3 vector = GClass897.CalculateNormalFromPoint(start, end, camera.transform.position);
			if (vector != Vector3.zero)
			{
				method_1(shot, camera.transform.forward, vector);
			}
			dictionary_0[id] = currentIndex;
		}
	}

	public void Awake()
	{
		dictionary_0 = new Dictionary<string, int>();
		GClass897.OnSniperZoneShoot += method_0;
	}

	public void OnDestroy()
	{
		GClass897.OnSniperZoneShoot -= method_0;
		method_7();
	}

	public void method_0(SonicBulletSoundPlayer.GClass898 obj)
	{
		StartCoroutine(method_4());
	}

	public void method_1(EftBulletClass shot, Vector3 forward, Vector3 normal)
	{
		float magnitude = normal.magnitude;
		if (!(magnitude > _minMaxRadius.y))
		{
			string id = shot.Ammo.Id + shot.FragmentIndex;
			BallisticCollider hittedBallisticCollider = shot.HittedBallisticCollider;
			if (hittedBallisticCollider != null && hittedBallisticCollider.GetType() == typeof(BodyPartCollider))
			{
				method_2(id);
				return;
			}
			method_7();
			coroutine_0 = StartCoroutine(method_3(id, forward, normal, magnitude));
		}
	}

	public void method_2(string id)
	{
		int num = ++int_0 % string_0.Length;
		string_0[num] = id;
	}

	public IEnumerator method_3(string id, Vector3 forward, Vector3 normal, float magnitude)
	{
		yield return null;
		yield return null;
		if (Array.IndexOf(string_0, id) <= 0)
		{
			method_2(id);
			float num = Mathf.InverseLerp(_minMaxRadius.y, _minMaxRadius.x, magnitude);
			if (Time.time > float_1)
			{
				int num2 = UnityEngine.Random.Range(0, _sources.Length - 1);
				AudioClip audioClip = _sources[num2];
				float panStereo = GClass897.CalculatePan(forward, normal);
				Singleton<BetterAudio>.Instance.PlayNonspatial(audioClip, BetterAudio.AudioSourceGroupType.Environment, panStereo, num);
				float_1 = Time.time + audioClip.length;
			}
			method_5(num);
			player_0.IncreaseAwareness(5f * (1f + num));
		}
	}

	public IEnumerator method_4()
	{
		yield return null;
		yield return null;
		int num = UnityEngine.Random.Range(0, _sources.Length - 1);
		float panStereo = UnityEngine.Random.Range(-0.9f, 0.9f);
		float num2 = Mathf.InverseLerp(_minMaxRadius.y, _minMaxRadius.x, UnityEngine.Random.Range(0.5f, 1f));
		Singleton<BetterAudio>.Instance.PlayNonspatial(_sources[num], BetterAudio.AudioSourceGroupType.Environment, panStereo, num2);
		method_5(num2);
		player_0.IncreaseAwareness(5f * (1f + num2));
	}

	public void method_5(float coef)
	{
		if (ienumerator_0 != null)
		{
			StopCoroutine(ienumerator_0);
		}
		prismEffects_0.vignetteEnd = float_0;
		ienumerator_0 = method_6(prismEffects_0.vignetteEnd, coef);
		StartCoroutine(ienumerator_0);
	}

	public IEnumerator method_6(float savedValue, float coef)
	{
		float time = Time.time;
		while (Time.time < time + _vignetteTime)
		{
			float num = Time.time - time;
			num /= _vignetteTime;
			float num2 = _vignette.Evaluate(num);
			num2 *= _vignetteDelta * coef;
			prismEffects_0.vignetteEnd = savedValue - num2;
			yield return null;
		}
		prismEffects_0.vignetteEnd = savedValue;
	}

	public void method_7()
	{
		if (coroutine_0 != null)
		{
			StopCoroutine(coroutine_0);
			coroutine_0 = null;
		}
	}
}
