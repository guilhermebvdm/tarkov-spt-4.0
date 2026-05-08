using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

public class GClass792
{
	[Serializable]
	[CompilerGenerated]
	public class Class441
	{
		public static readonly Class441 class441_0 = new Class441();

		public static Func<Transform, string> func_0;

		public string method_0(Transform x)
		{
			return x.name;
		}
	}

	[CompilerGenerated]
	public class Class442
	{
		public Dictionary<string, Transform> skinBonesDictionary;

		public bool removeSpaceOne;

		public bool method_0(Transform transform)
		{
			return skinBonesDictionary.ContainsKey(removeSpaceOne ? smethod_0(transform.name) : transform.name);
		}

		public KeyValuePair<string, Transform> method_1(Transform bone)
		{
			return new KeyValuePair<string, Transform>(removeSpaceOne ? smethod_0(bone.name) : bone.name, bone);
		}
	}

	public static string smethod_0(string name)
	{
		if (!name.EndsWith(" 1"))
		{
			return name;
		}
		return name.Substring(0, name.Length - 2);
	}

	public static SkinnedMeshRenderer Skin(SkinnedMeshRenderer renderer, Transform target, bool removeSpaceOne = false)
	{
		if (!(renderer == null) && !(target == null))
		{
			Dictionary<string, Transform> skinBonesDictionary = renderer.bones.ToDictionary((Transform x) => x.name);
			IEnumerable<KeyValuePair<string, Transform>> enumerable = from bone in target.GetComponentsInChildren<Transform>()
				where skinBonesDictionary.ContainsKey(removeSpaceOne ? smethod_0(bone.name) : bone.name)
				select new KeyValuePair<string, Transform>(removeSpaceOne ? smethod_0(bone.name) : bone.name, bone);
			Dictionary<string, Transform> dictionary = new Dictionary<string, Transform>();
			foreach (KeyValuePair<string, Transform> item in enumerable)
			{
				if (dictionary.ContainsKey(item.Key))
				{
					Debug.LogWarning("[Skinner] Dictionary already contains " + item.Key);
				}
				else
				{
					dictionary.Add(item.Key, item.Value);
				}
			}
			Dictionary<Transform, Transform> dictionary2 = new Dictionary<Transform, Transform>();
			Transform[] bones = renderer.bones;
			foreach (Transform transform in bones)
			{
				if (dictionary2.ContainsKey(transform))
				{
					Debug.LogErrorFormat("[Skinner] There is already transform '{0}'", transform.name);
				}
				if (!dictionary.ContainsKey(transform.name))
				{
					Debug.LogErrorFormat("[Skinner] There is no transform with name '{0}' in '{1}'", transform.name, target.name);
					continue;
				}
				dictionary2[transform] = dictionary[transform.name];
				if (dictionary2[transform] == null)
				{
					Debug.LogErrorFormat("[Skinner] There is no transform with name '{0}' in '{1}'", transform.name, target.name);
				}
			}
			Transform transform2 = renderer.transform;
			while (transform2.parent != null)
			{
				transform2 = transform2.parent;
			}
			return smethod_1(renderer, dictionary2, dictionary[renderer.rootBone.name]);
		}
		return null;
	}

	public static SkinnedMeshRenderer smethod_1(SkinnedMeshRenderer source, Dictionary<Transform, Transform> bindedTransforms, Transform targetRoot)
	{
		SkinnedMeshRenderer skinnedMeshRenderer = UnityEngine.Object.Instantiate(source);
		skinnedMeshRenderer.name = source.name;
		skinnedMeshRenderer.transform.localScale = source.transform.localScale;
		skinnedMeshRenderer.transform.localPosition = source.transform.localPosition;
		skinnedMeshRenderer.transform.localRotation = source.transform.localRotation;
		skinnedMeshRenderer.rootBone = targetRoot;
		Transform[] array = new Transform[source.bones.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = bindedTransforms[source.bones[i]];
		}
		skinnedMeshRenderer.bones = array;
		return skinnedMeshRenderer;
	}

	public static void smethod_2(GameObject target, Transform targetRoot, string path)
	{
		string[] array = path.Split('/');
		Transform transform = targetRoot;
		for (int i = 0; i < array.Length; i++)
		{
			Transform transform2 = transform.Find(array[i]);
			bool flag = i == array.Length - 1;
			if (transform2 == null || flag)
			{
				GameObject gameObject = null;
				gameObject = ((!flag) ? new GameObject(array[i]) : target);
				gameObject.transform.parent = transform;
				gameObject.transform.localPosition = Vector3.zero;
				gameObject.transform.localRotation = Quaternion.identity;
				gameObject.transform.localScale = Vector3.one;
				transform = gameObject.transform;
			}
			else
			{
				transform = transform2;
			}
		}
	}
}
