using System;
using System.Collections.Generic;
using EFT;
using UnityEngine;

public class AnimationSnatcher : MonoBehaviour
{
	[Serializable]
	public class SnatcherPair
	{
		public enum ECopyMode
		{
			Local,
			World
		}

		public Transform Source;

		public Transform Destination;

		public ECopyMode CopyMode;

		public EPointOfView PointOfView = EPointOfView.ThirdPerson;

		public bool RespectPointOfView = true;

		public float PositionXZWeight = 1f;

		public float PositionYWeight = 1f;

		public float RotationWeight = 1f;

		[field: NonSerialized]
		public virtual bool IsDuplicating { get; set; }

		public SnatcherPair(Transform source, Transform destination)
		{
			Source = source;
			Destination = destination;
		}

		public virtual void SetWeight(float xz, float y, float r)
		{
		}

		public virtual void Process(EPointOfView pointOfView)
		{
			if (IsDuplicating && (!RespectPointOfView || PointOfView == pointOfView) && PositionXZWeight + PositionYWeight + RotationWeight >= float.Epsilon)
			{
				method_0(Source, Destination);
			}
		}

		public void method_0(Transform source, Transform destination)
		{
			if (CopyMode == ECopyMode.Local)
			{
				destination.localPosition = SeparateLerp(destination.localPosition, source.localPosition, PositionXZWeight, PositionYWeight);
				destination.localRotation = Quaternion.Lerp(destination.localRotation, source.localRotation, RotationWeight);
			}
			else if (CopyMode == ECopyMode.World)
			{
				destination.position = SeparateLerp(destination.position, source.position, PositionXZWeight, PositionYWeight);
				destination.rotation = Quaternion.Lerp(destination.rotation, source.rotation, RotationWeight);
			}
		}

		public Vector3 SeparateLerp(Vector3 v1, Vector3 v2, float weightXZ, float weightY)
		{
			float x = Mathf.Lerp(v1.x, v2.x, weightXZ);
			float y = Mathf.Lerp(v1.y, v2.y, weightY);
			float z = Mathf.Lerp(v1.z, v2.z, weightXZ);
			return new Vector3(x, y, z);
		}
	}

	[Serializable]
	public class HierarchySnatcherPair : SnatcherPair
	{
		public string Name;

		public bool ProcessOnlyChildren;

		public bool foldOut = true;

		public bool _isCopyHierarchy;

		public string SearchPrefix = "";

		[NonSerialized]
		public bool IsDuplicating_1;

		public List<SnatcherPair> ChildrenPairs;

		public bool IsCopyHierarchy
		{
			get
			{
				return _isCopyHierarchy;
			}
			set
			{
				if (value != _isCopyHierarchy)
				{
					_isCopyHierarchy = value;
					method_1(_isCopyHierarchy);
				}
			}
		}

		public override bool IsDuplicating
		{
			get
			{
				return IsDuplicating_1;
			}
			set
			{
				if (IsDuplicating_1 == value)
				{
					return;
				}
				IsDuplicating_1 = value;
				foreach (SnatcherPair childrenPair in ChildrenPairs)
				{
					childrenPair.IsDuplicating = IsDuplicating_1;
				}
			}
		}

		public override void SetWeight(float xz, float y, float r)
		{
			PositionXZWeight = xz;
			PositionYWeight = y;
			RotationWeight = r;
			for (int i = 0; i < ChildrenPairs.Count; i++)
			{
				SnatcherPair snatcherPair = ChildrenPairs[i];
				snatcherPair.PositionXZWeight = xz;
				snatcherPair.PositionYWeight = y;
				snatcherPair.RotationWeight = r;
			}
		}

		public override void Process(EPointOfView pointOfView)
		{
			if (!RespectPointOfView || pointOfView == PointOfView)
			{
				if (!ProcessOnlyChildren)
				{
					base.Process(pointOfView);
				}
				for (int i = 0; i < ChildrenPairs.Count; i++)
				{
					ChildrenPairs[i].Process(pointOfView);
				}
			}
		}

		public HierarchySnatcherPair(Transform source, Transform destination)
			: base(source, destination)
		{
			ChildrenPairs = new List<SnatcherPair>();
		}

		public void method_1(bool enabled)
		{
			if (enabled)
			{
				ChildrenPairs = new List<SnatcherPair>();
				{
					foreach (Transform item in method_2(Source))
					{
						Transform transform = TransformHelperClass.FindTransformRecursive(Destination, item.name + SearchPrefix);
						if (null == transform)
						{
							transform = TransformHelperClass.FindTransformRecursive(Destination, item.name + SearchPrefix + " 1");
						}
						if (null == transform)
						{
							Debug.LogWarning("there is no destination object for " + item.name + SearchPrefix);
							continue;
						}
						SnatcherPair snatcherPair = new SnatcherPair(item, transform);
						snatcherPair.IsDuplicating = IsDuplicating;
						snatcherPair.CopyMode = CopyMode;
						snatcherPair.PointOfView = PointOfView;
						ChildrenPairs.Add(snatcherPair);
					}
					return;
				}
			}
			ChildrenPairs.Clear();
		}

		public List<Transform> method_2(Transform source)
		{
			Queue<Transform> queue = new Queue<Transform>();
			List<Transform> list = new List<Transform>();
			foreach (Transform item3 in source)
			{
				queue.Enqueue(item3);
			}
			while (queue.Count > 0)
			{
				Transform transform = queue.Dequeue();
				list.Add(transform);
				foreach (Transform item4 in transform)
				{
					queue.Enqueue(item4);
				}
			}
			return list;
		}
	}

	public List<HierarchySnatcherPair> MainPairs = new List<HierarchySnatcherPair>(1);

	public List<HierarchySnatcherPair> LeftHandPost = new List<HierarchySnatcherPair>(1);

	public List<HierarchySnatcherPair> RightHandPost = new List<HierarchySnatcherPair>(1);

	public const string PALM_LEFT_TRANSFORM = "Base HumanLPalm";

	public const string PALM_RIGHT_TRANSFORM = "Base HumanRPalm";

	public const string WEAPON_ROOT_TRANSFORM = "Weapon_root";

	public const string RIBCAGE_TRANSFORM_NAME = "Base HumanRibcage";

	public const string LEFT_ARM = "Base HumanLCollarbone";

	public const string RIGHT_ARM = "Base HumanRCollarbone";

	public Transform First;

	public Transform Third;

	public List<string> Ignore = new List<string>();

	private EPointOfView epointOfView_0;

	public void Start()
	{
		Ignore.Add("Weapon_root 1");
	}

	public void Reset(Transform from, Transform to, EPointOfView pointOfView)
	{
		UpdateWeaponRoot(from, to);
		SetPointOfView(pointOfView);
	}

	public void InitHands(Transform HandsContainerTransform, Transform bodyTransform)
	{
		First = HandsContainerTransform;
		Third = bodyTransform;
		LeftHandPost.Clear();
		RightHandPost.Clear();
		Transform source = TransformHelperClass.FindTransformRecursive(First, "Base HumanRPalm");
		Transform destination = TransformHelperClass.FindTransformRecursive(Third, "Base HumanRPalm");
		RightHandPost.Add(new HierarchySnatcherPair(source, destination)
		{
			IsCopyHierarchy = true,
			ProcessOnlyChildren = true,
			IsDuplicating = true,
			Name = "Right Fingers"
		});
		RightHandPost.Add(new HierarchySnatcherPair(source, destination)
		{
			CopyMode = SnatcherPair.ECopyMode.World,
			PositionXZWeight = 0f,
			PositionYWeight = 0f,
			IsDuplicating = true,
			Name = "Right Palm Rotation"
		});
		Transform source2 = TransformHelperClass.FindTransformRecursive(First, "Base HumanLPalm");
		Transform destination2 = TransformHelperClass.FindTransformRecursive(Third, "Base HumanLPalm");
		LeftHandPost.Add(new HierarchySnatcherPair(source2, destination2)
		{
			IsCopyHierarchy = true,
			ProcessOnlyChildren = true,
			IsDuplicating = true,
			Name = "Left Fingers"
		});
		Transform source3 = TransformHelperClass.FindTransformRecursive(First, "Base HumanLForearm1");
		Transform destination3 = TransformHelperClass.FindTransformRecursive(Third, "Base HumanLForearm1");
		HierarchySnatcherPair hierarchySnatcherPair = new HierarchySnatcherPair(source3, destination3);
		hierarchySnatcherPair.CopyMode = SnatcherPair.ECopyMode.World;
		hierarchySnatcherPair.PointOfView = EPointOfView.ThirdPerson;
		hierarchySnatcherPair.Name = "L Arm Full";
		hierarchySnatcherPair._isCopyHierarchy = true;
		hierarchySnatcherPair.ChildrenPairs = new List<SnatcherPair>();
		string[] obj = new string[3] { "Base HumanLForearm2", "Base HumanLForearm3", "Base HumanLPalm" };
		int num = 0;
		string[] array = obj;
		foreach (string text in array)
		{
			Transform source4 = TransformHelperClass.FindTransformRecursive(First, text);
			Transform destination4 = TransformHelperClass.FindTransformRecursive(Third, text);
			HierarchySnatcherPair hierarchySnatcherPair2 = new HierarchySnatcherPair(source4, destination4);
			hierarchySnatcherPair2.CopyMode = ((num >= 2) ? SnatcherPair.ECopyMode.World : SnatcherPair.ECopyMode.Local);
			hierarchySnatcherPair2.PointOfView = EPointOfView.ThirdPerson;
			hierarchySnatcherPair.ChildrenPairs.Add(hierarchySnatcherPair2);
			num++;
		}
		hierarchySnatcherPair.IsDuplicating = true;
		hierarchySnatcherPair.ProcessOnlyChildren = true;
		hierarchySnatcherPair.SetWeight(0f, 0f, 1f);
		LeftHandPost.Add(hierarchySnatcherPair);
	}

	public void UpdateWeaponRoot(Transform HandsContainerTransform, Transform bodyTransform)
	{
		First = HandsContainerTransform;
		Third = bodyTransform;
		MainPairs.Clear();
		SetWeaponSnatchingPreferences(SnatcherPair.ECopyMode.World, respect: false);
		SetWeaponSnatchingWeights(1f, 1f, 0f);
	}

	public Transform[] TransformToArray(Transform parent)
	{
		List<Transform> list = new List<Transform>();
		foreach (Transform item in parent)
		{
			if (!Ignore.Contains(item.gameObject.name))
			{
				list.Add(item);
				list.AddRange(TransformToArray(item));
			}
		}
		return list.ToArray();
	}

	public void SetPointOfView(EPointOfView pointOfView)
	{
		epointOfView_0 = pointOfView;
	}

	public void SetWeaponSnatchingPreferences(SnatcherPair.ECopyMode mode, bool respect)
	{
		if (MainPairs != null && MainPairs.Count > 0)
		{
			MainPairs[0].CopyMode = mode;
			MainPairs[0].RespectPointOfView = respect;
		}
	}

	public void SetWeaponSnatchingWeights(float xz, float y, float r)
	{
		if (MainPairs != null && MainPairs.Count > 0)
		{
			MainPairs[0].PositionXZWeight = xz;
			MainPairs[0].PositionYWeight = y;
			MainPairs[0].RotationWeight = r;
		}
	}

	public void SetPalmSnatchingWeight(float left, float right)
	{
		if (LeftHandPost.Count > 1)
		{
			LeftHandPost[0].SetWeight(left, left, left);
			LeftHandPost[1].SetWeight(0f, 0f, left);
		}
		if (RightHandPost.Count > 1)
		{
			RightHandPost[0].SetWeight(right, right, right);
			RightHandPost[1].SetWeight(0f, 0f, right);
		}
	}

	public void method_0()
	{
		if (base.enabled)
		{
			for (int i = 0; i < MainPairs.Count; i++)
			{
				MainPairs[i].Process(epointOfView_0);
			}
		}
	}

	public void method_1()
	{
		for (int i = 0; i < LeftHandPost.Count; i++)
		{
			LeftHandPost[i].Process(epointOfView_0);
		}
		for (int j = 0; j < RightHandPost.Count; j++)
		{
			RightHandPost[j].Process(epointOfView_0);
		}
	}
}
