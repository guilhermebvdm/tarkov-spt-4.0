using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;

[ExecuteInEditMode]
public class DoorHandle : MonoBehaviour
{
	private const int int_0 = 14;

	public Quaternion OpenRotation;

	public Quaternion DefaultRotation;

	public AnimationCurve OpenAnimation;

	public AnimationCurve LockedAnimation;

	public bool pos;

	public Vector3 OpenPosition;

	public Vector3 DefaultPosition;

	public AudioClip[] DownSound;

	public AudioClip[] UpSound;

	private float float_0;

	[CompilerGenerated]
	private Action<BetterSource, Vector3> action_0;

	public event Action<BetterSource, Vector3> OnPlaySound
	{
		[CompilerGenerated]
		add
		{
			Action<BetterSource, Vector3> action = action_0;
			Action<BetterSource, Vector3> action2;
			do
			{
				action2 = action;
				Action<BetterSource, Vector3> value2 = (Action<BetterSource, Vector3>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<BetterSource, Vector3> action = action_0;
			Action<BetterSource, Vector3> action2;
			do
			{
				action2 = action;
				Action<BetterSource, Vector3> value2 = (Action<BetterSource, Vector3>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	[ContextMenu("Open Position")]
	public void OpenPos()
	{
		base.transform.localRotation = Quaternion.Slerp(DefaultRotation, OpenRotation, 1f);
		if (pos)
		{
			base.transform.localPosition = Vector3.Lerp(DefaultPosition, OpenPosition, 1f);
		}
	}

	[ContextMenu("Default Position")]
	public void DefPos()
	{
		base.transform.localRotation = Quaternion.Slerp(OpenRotation, DefaultRotation, 1f);
		if (pos)
		{
			base.transform.localPosition = Vector3.Lerp(OpenPosition, DefaultPosition, 1f);
		}
	}

	public void Awake()
	{
		if (pos)
		{
			base.transform.localPosition = DefaultPosition;
			base.transform.localRotation = DefaultRotation;
		}
	}

	public void Open()
	{
		StartCoroutine(OpenCoroutine());
	}

	public IEnumerator OpenCoroutine()
	{
		if (DownSound != null && DownSound.Length != 0)
		{
			AudioClip audioClip = DownSound[UnityEngine.Random.Range(0, DownSound.Length)];
			if (audioClip == null)
			{
				Debug.LogError($"DoorHandle {base.transform.parent} has empty array of sounds");
			}
			else
			{
				BetterSource betterSource = MonoBehaviourSingleton<BetterAudio>.Instance.PlayAtPoint(base.transform.position, audioClip, CameraClass.Instance.Distance(base.transform.position), BetterAudio.AudioSourceGroupType.Environment, 14, 0.8f);
				if (betterSource != null)
				{
					action_0?.Invoke(betterSource, base.transform.position);
				}
			}
		}
		bool flag = false;
		if (OpenAnimation != null && OpenAnimation.length != 0)
		{
			float num = 0f;
			do
			{
				num += Time.deltaTime;
				float t = OpenAnimation.Evaluate(num);
				base.transform.localRotation = Quaternion.Slerp(DefaultRotation, OpenRotation, t);
				if (pos)
				{
					base.transform.localPosition = Vector3.Lerp(DefaultPosition, OpenPosition, t);
				}
				if (UpSound.Length != 0 && !flag && num > OpenAnimation[OpenAnimation.length - 1].time / 2f)
				{
					AudioClip audioClip2 = UpSound[UnityEngine.Random.Range(0, UpSound.Length)];
					if (audioClip2 == null)
					{
						Debug.LogError($"DoorHandle {base.transform.parent} has empty array of sounds");
					}
					else
					{
						BetterSource betterSource2 = MonoBehaviourSingleton<BetterAudio>.Instance.PlayAtPoint(base.transform.position, audioClip2, CameraClass.Instance.Distance(base.transform.position), BetterAudio.AudioSourceGroupType.InteractiveObjects, 14, 0.8f);
						if (betterSource2 != null)
						{
							action_0?.Invoke(betterSource2, base.transform.position);
						}
					}
					flag = true;
				}
				yield return null;
			}
			while (num < OpenAnimation[OpenAnimation.length - 1].time);
		}
		else
		{
			Debug.LogErrorFormat(this, "Door handle doesn't have any open animation:" + GClass6.GetFullPath(base.transform, withSceneName: true));
		}
	}
}
