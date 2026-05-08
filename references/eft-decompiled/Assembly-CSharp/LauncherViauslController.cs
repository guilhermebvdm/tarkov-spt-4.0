using System.Collections.Generic;
using UnityEngine;

public class LauncherViauslController : MonoBehaviour
{
	public const string LAUNCHER_SIGHT = "_sight";

	public const string LAUNCHER_SIGHT_MODE = "LauncherSight";

	private Transform transform_0;

	private readonly List<Transform> list_0 = new List<Transform>();

	public LauncherItemClass Launcher;

	public void Awake()
	{
		Init();
	}

	public void Init()
	{
		if (list_0.Count == 0)
		{
			transform_0 = TransformHelperClass.FindTransformRecursiveContains(base.transform, "_sight");
			TransformHelperClass.smethod_3(base.transform, "LauncherSight", list_0, false);
		}
	}

	public void OnEnable()
	{
		if (transform_0 != null && list_0.Count > 0)
		{
			UpdateSightMode();
		}
	}

	public void UpdateSightMode()
	{
		if (Launcher != null && !(transform_0 == null) && list_0.Count != 0)
		{
			Transform transform = list_0[Launcher.SelectedSight];
			transform_0.localRotation = transform.localRotation;
		}
	}
}
