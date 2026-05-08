using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using UnityEngine;

public class TransformLinks : MonoBehaviour
{
	[Serializable]
	public struct CachedTransform
	{
		public Vector3 Position;

		public Quaternion Rotation;

		public Transform Transform;

		public void Reset()
		{
			Transform.localPosition = Position;
			Transform.localRotation = Rotation;
			Transform.gameObject.SetActive(value: true);
		}
	}

	[Serializable]
	[CompilerGenerated]
	public class Class432
	{
		public static readonly Class432 class432_0 = new Class432();

		public static Func<Transform, bool> func_0;

		public static Func<Transform, CachedTransform> func_1;

		public bool method_0(Transform x)
		{
			return x != null;
		}

		public CachedTransform method_1(Transform t)
		{
			return new CachedTransform
			{
				Position = t.localPosition,
				Rotation = t.localRotation,
				Transform = t
			};
		}
	}

	[CompilerGenerated]
	public class Class433
	{
		public Transform parent;

		public Transform method_0(string x)
		{
			return TransformHelperClass.FindTransform(parent, x);
		}
	}

	public Transform[] Transforms;

	public Transform Self;

	[SerializeField]
	private CachedTransform[] _cachedTransforms;

	[CanBeNull]
	public Transform GetTransform(ECharacterWeaponBones bone)
	{
		return Transforms[(int)bone];
	}

	[CanBeNull]
	public Transform GetTransformOutOfRangeSafe(ECharacterWeaponBones bone)
	{
		if ((int)bone >= Transforms.Length)
		{
			return null;
		}
		return Transforms[(int)bone];
	}

	public Transform GetCachedTransform(int index)
	{
		return _cachedTransforms[index].Transform;
	}

	public void GatherIK(Transform[] markers, Transform[] gripReferences, Transform[] elbowBends)
	{
		elbowBends[0] = GetTransform(ECharacterWeaponBones.Bend_Goal_Left);
		elbowBends[1] = GetTransform(ECharacterWeaponBones.Bend_Goal_Right);
		markers[0] = GetTransform(ECharacterWeaponBones.weapon_L_IK_marker);
		markers[1] = GetTransform(ECharacterWeaponBones.weapon_R_IK_marker);
		gripReferences[0] = GetTransform(ECharacterWeaponBones.weapon_L_hand_marker);
		gripReferences[1] = GetTransform(ECharacterWeaponBones.weapon_R_hand_marker);
	}

	public void GatherUnderbarrelWeaponIK(Transform launcherRoot, Transform[] elbowBends)
	{
		elbowBends[0] = TransformHelperClass.FindTransform(launcherRoot, "Bend_Goal_Left");
	}

	public void CacheTransforms(Transform parent, IEnumerable<string> cachedBoneNames)
	{
		Self = base.gameObject.transform;
		string[] names = Enum.GetNames(typeof(ECharacterWeaponBones));
		Transforms = new Transform[names.Length];
		foreach (string text in names)
		{
			int num = (int)Enum.Parse(typeof(ECharacterWeaponBones), text);
			string text2 = (text.Contains("Human") ? ("Base " + text) : text);
			Transforms[num] = TransformHelperClass.FindTransform(parent, text2);
		}
		_cachedTransforms = (from x in cachedBoneNames
			select TransformHelperClass.FindTransform(parent, x) into t
			where t != null
			select new CachedTransform
			{
				Position = t.localPosition,
				Rotation = t.localRotation,
				Transform = t
			}).ToArray();
	}

	public void ResetPositions()
	{
		CachedTransform[] cachedTransforms = _cachedTransforms;
		foreach (CachedTransform cachedTransform in cachedTransforms)
		{
			cachedTransform.Reset();
		}
	}
}
