using System;
using UnityEngine;
using Random = UnityEngine.Random;
using Object = UnityEngine.Object;

namespace VisceralCombat.Ragdolls.Classes.RootMotion;

public class GenericBaker : Baker
{
	[Tooltip("If true, produced AnimationClips will be marked as Legacy and usable with the Legacy animation system.")]
	public bool markAsLegacy;

	[Tooltip("Root Transform of the hierarchy to bake.")]
	public Transform root;

	[Tooltip("Root Node used for root motion.")]
	public Transform rootNode;

	[Tooltip("List of Transforms to ignore, rotation curves will not be baked for these Transforms.")]
	public Transform[] ignoreList;

	[Tooltip("LocalPosition curves will be baked for these Transforms only. If you are baking a character, the pelvis bone should be added to this array.")]
	public Transform[] bakePositionList;

	private BakerTransform[] children = new BakerTransform[0];

	private BakerTransform rootChild;

	private int rootChildIndex = -1;

	private void Awake()
	{
		Transform[] componentsInChildren = ((Component)root).GetComponentsInChildren<Transform>();
		children = new BakerTransform[0];
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (!IsIgnored(componentsInChildren[i]))
			{
				Array.Resize(ref children, children.Length + 1);
				bool flag = (Object)(object)componentsInChildren[i] == (Object)(object)rootNode;
				if (flag)
				{
					rootChildIndex = children.Length - 1;
				}
				children[children.Length - 1] = new BakerTransform(componentsInChildren[i], root, BakePosition(componentsInChildren[i]), flag);
			}
		}
	}

	protected override Transform GetCharacterRoot()
	{
		return root;
	}

	protected override void OnStartBaking()
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < children.Length; i++)
		{
			children[i].Reset();
			if (i == rootChildIndex)
			{
				children[i].SetRelativeSpace(root.position, root.rotation);
			}
		}
	}

	protected override void OnSetLoopFrame(float time)
	{
		for (int i = 0; i < children.Length; i++)
		{
			children[i].AddLoopFrame(time);
		}
	}

	protected override void OnSetCurves(ref AnimationClip clip)
	{
		for (int i = 0; i < children.Length; i++)
		{
			children[i].SetCurves(ref clip);
		}
	}

	protected override void OnSetKeyframes(float time, bool lastFrame)
	{
		for (int i = 0; i < children.Length; i++)
		{
			children[i].SetKeyframes(time);
		}
	}

	private bool IsIgnored(Transform t)
	{
		for (int i = 0; i < ignoreList.Length; i++)
		{
			if ((Object)(object)t == (Object)(object)ignoreList[i])
			{
				return true;
			}
		}
		return false;
	}

	private bool BakePosition(Transform t)
	{
		for (int i = 0; i < bakePositionList.Length; i++)
		{
			if ((Object)(object)t == (Object)(object)bakePositionList[i])
			{
				return true;
			}
		}
		return false;
	}
}
