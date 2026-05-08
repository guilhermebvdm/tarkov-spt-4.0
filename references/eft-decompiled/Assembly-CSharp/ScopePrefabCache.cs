using System;
using System.Linq;
using System.Runtime.CompilerServices;
using EFT.CameraControl;
using JetBrains.Annotations;
using UnityEngine;

public class ScopePrefabCache : MonoBehaviour
{
	[Serializable]
	public class ScopeModeInfo
	{
		public GameObject ModeGameObject;

		public CollimatorSight CollimatorSight;

		public OpticSight OpticSight;

		public bool IgnoreOpticsForCameraPlane;
	}

	[Serializable]
	public struct DistaneAngle
	{
		public float Distance;

		public float Angle;
	}

	[Serializable]
	[CompilerGenerated]
	public class Class564
	{
		public static readonly Class564 class564_0 = new Class564();

		public static Func<ScopeModeInfo, bool> func_0;

		public static Func<ScopeModeInfo, bool> func_1;

		public bool method_0(ScopeModeInfo sm)
		{
			return sm.OpticSight != null;
		}

		public bool method_1(ScopeModeInfo sm)
		{
			return sm.CollimatorSight != null;
		}
	}

	[SerializeField]
	public bool CanChangeAngleByDistance;

	[SerializeField]
	public Transform WeaponScopeAxis;

	[SerializeField]
	public DistaneAngle[] AngleByRange;

	private const string string_0 = "mode_";

	[SerializeField]
	private ScopeModeInfo[] _scopeModeInfos = new ScopeModeInfo[0];

	private int int_0;

	[CompilerGenerated]
	private bool bool_0;

	[CompilerGenerated]
	private bool bool_1;

	public bool HasOptics
	{
		[CompilerGenerated]
		get
		{
			return bool_0;
		}
		[CompilerGenerated]
		set
		{
			bool_0 = value;
		}
	}

	public bool CurrentModHasOptics => _scopeModeInfos[CurrentModeId].OpticSight != null;

	public bool CurrentModIgnoreOpticsForCameraPlane => _scopeModeInfos[CurrentModeId].IgnoreOpticsForCameraPlane;

	public OpticSight CurrentModOpticSight => _scopeModeInfos[CurrentModeId].OpticSight;

	public bool HasCollimators
	{
		[CompilerGenerated]
		get
		{
			return bool_1;
		}
		[CompilerGenerated]
		set
		{
			bool_1 = value;
		}
	}

	public int ModesCount => _scopeModeInfos.Length;

	public OpticSight FirstOptic
	{
		get
		{
			ScopeModeInfo[] scopeModeInfos = _scopeModeInfos;
			int num = 0;
			ScopeModeInfo scopeModeInfo;
			while (true)
			{
				if (num < scopeModeInfos.Length)
				{
					scopeModeInfo = scopeModeInfos[num];
					if (scopeModeInfo.OpticSight != null)
					{
						break;
					}
					num++;
					continue;
				}
				throw new Exception("ScopePrefabCache doesn't contain optic");
			}
			return scopeModeInfo.OpticSight;
		}
	}

	public CollimatorSight FirstCollimator
	{
		get
		{
			ScopeModeInfo[] scopeModeInfos = _scopeModeInfos;
			int num = 0;
			ScopeModeInfo scopeModeInfo;
			while (true)
			{
				if (num < scopeModeInfos.Length)
				{
					scopeModeInfo = scopeModeInfos[num];
					if (scopeModeInfo.CollimatorSight != null)
					{
						break;
					}
					num++;
					continue;
				}
				throw new Exception("ScopePrefabCache doesn't contain collimator");
			}
			return scopeModeInfo.CollimatorSight;
		}
	}

	public int CurrentModeId => int_0;

	[CanBeNull]
	public OpticSight GetOpticSight(int index)
	{
		return GetScopeModeInfo(index).OpticSight;
	}

	public ScopeModeInfo GetScopeModeInfo(int index)
	{
		return _scopeModeInfos[index];
	}

	public void Awake()
	{
		HasOptics = _scopeModeInfos.Any((ScopeModeInfo sm) => sm.OpticSight != null);
		HasCollimators = _scopeModeInfos.Any((ScopeModeInfo sm) => sm.CollimatorSight != null);
	}

	public void RotateToAngleByDistance(float distance)
	{
		if (CanChangeAngleByDistance && WeaponScopeAxis != null && method_0(distance, out var angle))
		{
			Quaternion localRotation = WeaponScopeAxis.localRotation;
			WeaponScopeAxis.transform.localRotation = Quaternion.Euler(angle, localRotation.y, localRotation.z);
		}
	}

	public bool method_0(float distance, out float angle)
	{
		angle = 0f;
		if (AngleByRange == null)
		{
			return false;
		}
		DistaneAngle[] angleByRange = AngleByRange;
		int num = 0;
		DistaneAngle distaneAngle;
		while (true)
		{
			if (num < angleByRange.Length)
			{
				distaneAngle = angleByRange[num];
				if (distaneAngle.Distance == distance)
				{
					break;
				}
				num++;
				continue;
			}
			return false;
		}
		angle = distaneAngle.Angle;
		return true;
	}

	public bool IsOpticBone(Transform bone)
	{
		ScopeModeInfo[] scopeModeInfos = _scopeModeInfos;
		int num = 0;
		while (true)
		{
			if (num < scopeModeInfos.Length)
			{
				ScopeModeInfo scopeModeInfo = scopeModeInfos[num];
				if (scopeModeInfo.OpticSight != null && scopeModeInfo.OpticSight.ScopeTransform == bone)
				{
					break;
				}
				num++;
				continue;
			}
			return false;
		}
		return true;
	}

	public void SetMode(int modeId)
	{
		if (_scopeModeInfos.Length < 2)
		{
			return;
		}
		if (modeId >= 0 && modeId < _scopeModeInfos.Length)
		{
			if (CurrentModeId != modeId)
			{
				if (CurrentModeId >= 0)
				{
					_scopeModeInfos[CurrentModeId].ModeGameObject.SetActive(value: false);
				}
				int_0 = modeId;
				_scopeModeInfos[CurrentModeId].ModeGameObject.SetActive(value: true);
			}
		}
		else
		{
			Debug.LogErrorFormat(this, "Attempting to enable mode '{0}', '{1}'", modeId, this);
		}
	}

	public void LookAt(Vector3 point, Vector3 worldUp)
	{
		for (int i = 0; i < _scopeModeInfos.Length; i++)
		{
			ScopeModeInfo scopeModeInfo = _scopeModeInfos[i];
			if (scopeModeInfo.CollimatorSight != null)
			{
				scopeModeInfo.CollimatorSight.LookAt(point, worldUp);
			}
			if (scopeModeInfo.OpticSight != null)
			{
				scopeModeInfo.OpticSight.LookAt(point, worldUp);
			}
		}
	}

	public void LookAtCollimatorOnly(Vector3 point, Vector3 worldUp)
	{
		for (int i = 0; i < _scopeModeInfos.Length; i++)
		{
			ScopeModeInfo scopeModeInfo = _scopeModeInfos[i];
			if (scopeModeInfo.CollimatorSight != null)
			{
				scopeModeInfo.CollimatorSight.LookAt(point, worldUp);
			}
		}
	}

	public Transform GetLensCenter()
	{
		return FirstCollimator.transform;
	}

	public Vector3 GetLocalCollimatorCameraTarget(Vector3 worldCameraTarget)
	{
		return FirstCollimator.transform.InverseTransformPoint(worldCameraTarget);
	}

	public Vector3 GetLocalOpticCameraTarget(Vector3 worldCameraTarget)
	{
		return FirstOptic.transform.InverseTransformPoint(worldCameraTarget);
	}

	public Vector3 GetLensTransformForward()
	{
		return FirstCollimator.transform.forward;
	}

	public Vector3 GetCollimatorWorldCameraPosition(Vector3 localCameraTarget)
	{
		return _scopeModeInfos[0].CollimatorSight.transform.TransformPoint(localCameraTarget);
	}

	public Vector3 GetOpticsWorldCameraPosition(Vector3 localCameraTarget)
	{
		return FirstOptic.transform.TransformPoint(localCameraTarget);
	}

	public float GetAnyOpticsDistanceToCamera()
	{
		if (_scopeModeInfos[CurrentModeId].OpticSight != null)
		{
			return _scopeModeInfos[CurrentModeId].OpticSight.DistanceToCamera;
		}
		return FirstOptic.DistanceToCamera;
	}
}
