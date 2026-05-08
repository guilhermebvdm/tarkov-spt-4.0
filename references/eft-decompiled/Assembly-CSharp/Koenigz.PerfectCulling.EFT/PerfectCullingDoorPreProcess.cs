using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EFT.Interactive;
using UnityEngine;

namespace Koenigz.PerfectCulling.EFT;

public class PerfectCullingDoorPreProcess : PerfectCullingCrossSceneGroupPreProcess
{
	[Serializable]
	[CompilerGenerated]
	public class Class847
	{
		public static readonly Class847 class847_0 = new Class847();

		public static Comparison<PerfectCullingCrossSceneContentDoors> comparison_0;

		public int method_0(PerfectCullingCrossSceneContentDoors x, PerfectCullingCrossSceneContentDoors y)
		{
			if (x.ContentGroupId >= y.ContentGroupId)
			{
				return 1;
			}
			return -1;
		}
	}

	[HideInInspector]
	[SerializeField]
	private GuidReference[] _doorContent = Array.Empty<GuidReference>();

	public IReadOnlyCollection<GuidReference> DoorContent => _doorContent;

	public override PerfectCullingBakeGroup[] CollectBakeGroups()
	{
		return Array.Empty<PerfectCullingBakeGroup>();
	}

	public override BakeBatch[] CreateBakeBatches(PerfectCullingBakeGroup[] inputGroups)
	{
		return Array.Empty<BakeBatch>();
	}

	public override BakeBatch[] GetBakeBatches()
	{
		BakeBatch bakeBatch = new BakeBatch();
		bakeBatch.Groups = method_1();
		return new BakeBatch[1] { bakeBatch };
	}

	public override PerfectCullingBakeGroup[] PrebakeTransform(BakeBatch batch, ICollection<GameObject> tempObjects, out GClass1221.Mode writeMode)
	{
		writeMode = GClass1221.Mode.Overwrite;
		List<PerfectCullingBakeGroup> list = new List<PerfectCullingBakeGroup>();
		int num = 0;
		PerfectCullingBakeGroup[] groups = batch.Groups;
		foreach (PerfectCullingBakeGroup perfectCullingBakeGroup in groups)
		{
			if (perfectCullingBakeGroup.groupType == PerfectCullingBakeGroup.GroupType.Door)
			{
				MeshRenderer meshRenderer = smethod_3(smethod_0(perfectCullingBakeGroup));
				tempObjects.Add(meshRenderer.gameObject);
				list.Add(new PerfectCullingBakeGroup
				{
					groupType = PerfectCullingBakeGroup.GroupType.User,
					renderers = new Renderer[1] { meshRenderer },
					serializedGroupId = num
				});
			}
			else if (perfectCullingBakeGroup.groupType == PerfectCullingBakeGroup.GroupType.User)
			{
				list.Add(new PerfectCullingBakeGroup
				{
					groupType = PerfectCullingBakeGroup.GroupType.User,
					renderers = perfectCullingBakeGroup.renderers,
					serializedGroupId = num
				});
			}
			num++;
		}
		return list.ToArray();
	}

	public static WorldInteractiveObject smethod_0(PerfectCullingBakeGroup cullingGroup)
	{
		Renderer[] renderers = cullingGroup.renderers;
		int num = 0;
		WorldInteractiveObject componentInParent;
		while (true)
		{
			if (num < renderers.Length)
			{
				Renderer renderer = renderers[num];
				if (!(renderer == null))
				{
					componentInParent = renderer.GetComponentInParent<WorldInteractiveObject>();
					if (componentInParent != null)
					{
						break;
					}
				}
				num++;
				continue;
			}
			return null;
		}
		return componentInParent;
	}

	public override void OnEndContentCollect()
	{
		method_0();
	}

	public override PerfectCullingBakeGroup[] PrepareRuntimeContent()
	{
		return method_1();
	}

	public void method_0()
	{
	}

	public static Vector3 smethod_1(WorldInteractiveObject door)
	{
		GameObject obj = UnityEngine.Object.Instantiate(door.gameObject);
		obj.transform.position = Vector3.zero;
		obj.transform.rotation = Quaternion.identity;
		Vector3 size = smethod_2(obj.transform).size;
		UnityEngine.Object.DestroyImmediate(obj.gameObject);
		return size;
	}

	public static Bounds smethod_2(Transform root)
	{
		Bounds result = default(Bounds);
		MeshRenderer[] componentsInChildren = root.GetComponentsInChildren<MeshRenderer>();
		foreach (MeshRenderer meshRenderer in componentsInChildren)
		{
			result.Encapsulate(meshRenderer.bounds);
		}
		return result;
	}

	public static MeshRenderer smethod_3(WorldInteractiveObject doorObj)
	{
		GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
		UnityEngine.Object.DestroyImmediate(gameObject.GetComponent<Collider>());
		gameObject.transform.position = doorObj.transform.position;
		Vector3 vector = smethod_1(doorObj) * 0.5f;
		switch (doorObj.DoorAxis)
		{
		case WorldInteractiveObject.EDoorAxis.X:
		case WorldInteractiveObject.EDoorAxis.XNegative:
		{
			gameObject.transform.up = doorObj.transform.right;
			float num3 = Mathf.Max(vector.y, vector.z) * 4f;
			gameObject.transform.localScale = new Vector3(num3, vector.x, num3);
			break;
		}
		case WorldInteractiveObject.EDoorAxis.Y:
		case WorldInteractiveObject.EDoorAxis.YNegative:
		{
			gameObject.transform.up = doorObj.transform.up;
			float num2 = Mathf.Max(vector.x, vector.z) * 4f;
			gameObject.transform.localScale = new Vector3(num2, vector.y, num2);
			break;
		}
		case WorldInteractiveObject.EDoorAxis.Z:
		case WorldInteractiveObject.EDoorAxis.ZNegative:
		{
			gameObject.transform.up = doorObj.transform.forward;
			float num = Mathf.Max(vector.x, vector.y) * 4f;
			gameObject.transform.localScale = new Vector3(num, vector.z, num);
			break;
		}
		}
		MeshRenderer component = gameObject.GetComponent<MeshRenderer>();
		component.sharedMaterial = PerfectCullingResourcesLocator.Instance.ProxyLightMaterial;
		return component;
	}

	public override int GetBakeHash()
	{
		return 48879;
	}

	public PerfectCullingBakeGroup[] method_1()
	{
		List<PerfectCullingCrossSceneContentDoors> list = new List<PerfectCullingCrossSceneContentDoors>();
		GuidReference[] doorContent = _doorContent;
		foreach (GuidReference guidReference in doorContent)
		{
			list.Add(guidReference.gameObject.GetComponent<PerfectCullingCrossSceneContentDoors>());
		}
		list.Sort((PerfectCullingCrossSceneContentDoors x, PerfectCullingCrossSceneContentDoors y) => (x.ContentGroupId >= y.ContentGroupId) ? 1 : (-1));
		List<PerfectCullingBakeGroup> list2 = new List<PerfectCullingBakeGroup>();
		foreach (PerfectCullingCrossSceneContentDoors item in list)
		{
			list2.AddRange(item.BakeGroups);
		}
		return list2.ToArray();
	}

	public override SharedOccluder GenerateSharedOccluder()
	{
		return GClass1239.GenerateSharedOccluder(null, base.Group, method_1());
	}
}
