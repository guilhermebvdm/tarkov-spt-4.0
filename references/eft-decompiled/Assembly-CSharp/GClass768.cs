using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using EFT.AssetsManager;
using EFT.InventoryLogic;
using JetBrains.Annotations;
using UnityEngine;

public class GClass768 : IDisposable
{
	public class GClass769
	{
		public Transform Bone;

		public Item Item;

		public Transform ItemView;

		public void InsertItem(Item item, GameObject itemView)
		{
			Item = item;
			ItemView = itemView.transform;
			itemView.transform.SetParent(Bone, worldPositionStays: false);
			ModPlacer component = itemView.GetComponent<ModPlacer>();
			if (component != null)
			{
				itemView.transform.localPosition = component.ModPosition;
				itemView.transform.localRotation = Quaternion.Euler(component.ModRotation);
				itemView.transform.localScale = component.ModScale;
			}
			else
			{
				itemView.transform.localPosition = Vector3.zero;
				itemView.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
				itemView.transform.localScale = Vector3.one;
			}
			itemView.SetActive(value: true);
		}

		public void RemoveItem()
		{
			if (ItemView != null)
			{
				GInterface236[] components = ItemView.GetComponents<GInterface236>();
				for (int i = 0; i < components.Length; i++)
				{
					components[i].Deinit();
				}
			}
			Item = null;
			ItemView = null;
		}
	}

	[Serializable]
	[CompilerGenerated]
	public class Class424
	{
		public static readonly Class424 class424_0 = new Class424();

		public static Func<EFT.InventoryLogic.IContainer, string> func_0;

		public static Func<EFT.InventoryLogic.IContainer, string> func_1;

		public string method_0(EFT.InventoryLogic.IContainer x)
		{
			if (x != null)
			{
				return x.ID + " in " + x.ParentItem.Id;
			}
			return "null";
		}

		public string method_1(EFT.InventoryLogic.IContainer x)
		{
			if (x != null)
			{
				return x.ID + " in " + x.ParentItem.Id;
			}
			return "null";
		}
	}

	[Serializable]
	[CompilerGenerated]
	public class Class425<TContainer> where TContainer : EFT.InventoryLogic.IContainer
	{
		public static readonly Class425<TContainer> class425_0 = new Class425<TContainer>();

		public static Func<KeyValuePair<EFT.InventoryLogic.IContainer, GClass769>, GClass769> func_0;

		public GClass769 method_0(KeyValuePair<EFT.InventoryLogic.IContainer, GClass769> pair)
		{
			return pair.Value;
		}
	}

	[CompilerGenerated]
	public class Class426<T> where T : EFT.InventoryLogic.IContainer
	{
		public Func<T, bool> whereFunc;

		public bool method_0(KeyValuePair<EFT.InventoryLogic.IContainer, GClass769> pair)
		{
			if (pair.Key is T)
			{
				return whereFunc((T)pair.Key);
			}
			return false;
		}
	}

	public readonly Dictionary<EFT.InventoryLogic.IContainer, GClass769> ContainerBones = new Dictionary<EFT.InventoryLogic.IContainer, GClass769>();

	[NonSerialized]
	public AssetPoolObject AssetPoolObject_0;

	public GameObject GameObject;

	public GClass768(AssetPoolObject poolObject, GameObject gameObject)
	{
		AssetPoolObject_0 = poolObject;
		GameObject = gameObject;
		AssetPoolObject_0.ContainerCollectionView = this;
	}

	public void AddBone(EFT.InventoryLogic.IContainer container, Transform bone)
	{
		AddChildBones(bone);
		ContainerBones[container] = new GClass769
		{
			Bone = bone
		};
	}

	public void AddChildBones(Transform childRoot)
	{
		AssetPoolObject componentInChildren = childRoot.GetComponentInChildren<AssetPoolObject>();
		if (!(componentInChildren != null) || componentInChildren.ContainerCollectionView == null)
		{
			return;
		}
		foreach (KeyValuePair<EFT.InventoryLogic.IContainer, GClass769> containerBone in componentInChildren.ContainerCollectionView.ContainerBones)
		{
			ContainerBones[containerBone.Key] = containerBone.Value;
		}
		componentInChildren.ContainerCollectionView.ContainerBones.Clear();
	}

	public void RemoveBone(EFT.InventoryLogic.IContainer container)
	{
		ContainerBones.Remove(container);
	}

	public void RemoveBones(IEnumerable<EFT.InventoryLogic.IContainer> containers)
	{
		foreach (EFT.InventoryLogic.IContainer container in containers)
		{
			ContainerBones.Remove(container);
		}
	}

	public Transform GetBoneForSlot(EFT.InventoryLogic.IContainer container)
	{
		return GetViewForSlot(container)?.Bone;
	}

	[CanBeNull]
	public GClass769 GetViewForSlot(EFT.InventoryLogic.IContainer container)
	{
		if (ContainerBones.TryGetValue(container, out var value))
		{
			return value;
		}
		Debug.Log("bone " + container.ID + " in " + container.ParentItem.Id + " not found: " + string.Join("\n", ContainerBones.Keys.Select((EFT.InventoryLogic.IContainer x) => (x != null) ? (x.ID + " in " + x.ParentItem.Id) : "null").ToArray()));
		return null;
	}

	public IEnumerable<GClass769> GetBonesWhereSlot<TContainer>(Func<TContainer, bool> whereFunc) where TContainer : EFT.InventoryLogic.IContainer
	{
		return from pair in ContainerBones
			where pair.Key is TContainer && whereFunc((TContainer)pair.Key)
			select pair.Value;
	}

	public void LogEverything()
	{
		Debug.Log(string.Join("\n", ContainerBones.Keys.Select((EFT.InventoryLogic.IContainer x) => (x != null) ? (x.ID + " in " + x.ParentItem.Id) : "null").ToArray()));
	}

	public void Dispose()
	{
		if (GameObject != null)
		{
			GInterface236[] components = GameObject.GetComponents<GInterface236>();
			for (int i = 0; i < components.Length; i++)
			{
				components[i].Deinit();
			}
		}
		foreach (GClass769 value in ContainerBones.Values)
		{
			if (value.Item != null)
			{
				value.RemoveItem();
			}
		}
		if (AssetPoolObject_0 != null)
		{
			AssetPoolObject_0.ContainerCollectionView = null;
		}
	}
}
