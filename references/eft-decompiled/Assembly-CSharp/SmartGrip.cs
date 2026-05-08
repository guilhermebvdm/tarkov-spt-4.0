using RootMotion.FinalIK;
using UnityEngine;

public class SmartGrip : GripPose
{
	public Quaternion _initial;

	public Transform Targets;

	public Transform Pivot;

	public bool IkEnabled;

	private LimbIK[] limbIK_0;

	public Transform DEBUG_TARGET;

	public float Weight = 1f;

	public override void CacheAndDestroy()
	{
	}

	public override void Awake()
	{
		if (Pivot == null)
		{
			Debug.LogFormat(Pivot, "Pivot is null, would you be so kind and set it? {0}", base.name);
			return;
		}
		base.transform.parent = Pivot;
		_initial = Pivot.localRotation;
		limbIK_0 = GetComponentsInChildren<LimbIK>();
		if (limbIK_0.Length < 5)
		{
			Debug.LogFormat(this, "{0} grip has only {1} fingers", base.name, limbIK_0.Length);
		}
		if (Targets.childCount < 5)
		{
			Debug.LogFormat(this, "{0} grip has only {1} IK targets", base.name, Targets.childCount);
		}
		for (int i = 0; i < Targets.childCount; i++)
		{
			limbIK_0[i].solver.target = Targets.GetChild(i);
		}
		Reset();
	}

	public void LookAt(Vector3 target, bool debug = false)
	{
		if (!IkEnabled)
		{
			EnableIK();
		}
		Pivot.LookAt(target);
		Pivot.localRotation = Quaternion.Lerp(_initial, Pivot.localRotation, Weight);
	}

	public void EnableIK()
	{
		LimbIK[] array = limbIK_0;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].enabled = true;
		}
	}

	public void Reset()
	{
		Pivot.localRotation = _initial;
		LimbIK[] array = limbIK_0;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].enabled = false;
		}
		IkEnabled = false;
	}
}
