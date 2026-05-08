using System;
using System.Linq;
using System.Runtime.CompilerServices;
using EFT.Interactive;
using UnityEngine;

public class GripPose : MonoBehaviour, GInterface26
{
	public enum EGripType
	{
		Common,
		Alternative,
		UnderbarrelWeapon
	}

	public enum EHand
	{
		Left,
		Right
	}

	[Serializable]
	[CompilerGenerated]
	public class Class363
	{
		public static readonly Class363 class363_0 = new Class363();

		public static Func<Transform, Quaternion> func_0;

		public Quaternion method_0(Transform x)
		{
			return x.localRotation;
		}
	}

	[GAttribute10(typeof(EDoorState))]
	public EDoorState DoorState = EDoorState.Locked | EDoorState.Shut | EDoorState.Open;

	public EHand Hand;

	public EGripType GripType;

	public Transform[] FingerTransforms;

	public Quaternion[] Fingers;

	public bool DontCache;

	[SerializeField]
	private bool _cached;

	public bool IsCached => _cached;

	public Vector3 Position
	{
		get
		{
			return base.transform.position;
		}
		set
		{
			base.transform.position = value;
		}
	}

	public Quaternion Rotation => base.transform.rotation;

	public bool IsAlternative => GripType == EGripType.Alternative;

	public Quaternion this[int index]
	{
		get
		{
			if (!IsCached)
			{
				return FingerTransforms[index].localRotation;
			}
			return Fingers[index];
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	public virtual void Awake()
	{
		if (DontCache)
		{
			FingerTransforms = base.transform.GetComponentsInChildren<Transform>();
		}
		else if (!_cached)
		{
			CacheAndDestroy();
		}
	}

	public virtual void CacheAndDestroy()
	{
		FingerTransforms = base.transform.GetComponentsInChildren<Transform>();
		Fingers = FingerTransforms.Select((Transform x) => x.localRotation).ToArray();
		_cached = true;
		for (int num = base.transform.childCount - 1; num >= 0; num--)
		{
			if (!Application.isPlaying)
			{
				UnityEngine.Object.DestroyImmediate(base.transform.GetChild(num).gameObject);
			}
			else
			{
				UnityEngine.Object.Destroy(base.transform.GetChild(num).gameObject);
			}
		}
		FingerTransforms = null;
	}
}
