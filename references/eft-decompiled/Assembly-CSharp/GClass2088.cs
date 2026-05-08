using System;
using UnityEngine;

public abstract class GClass2088
{
	[NonSerialized]
	public const string String_0 = "patron_";

	[NonSerialized]
	public MagazineItemClass MagazineItemClass;

	[NonSerialized]
	public GameObject GameObject_0;

	[NonSerialized]
	public bool Bool_0;

	public void Init(GameObject gameObject, MagazineItemClass magazine, bool isAnimated)
	{
		GameObject_0 = gameObject;
		MagazineItemClass = magazine;
		Bool_0 = isAnimated;
		CacheDataTransforms();
		InitializeVisualOnStart();
	}

	public void OnReturnToPool()
	{
		ReturnVisualPartsToPool();
		Reset();
	}

	public abstract void Update();

	public virtual void Reset()
	{
		MagazineItemClass = null;
		GameObject_0 = null;
		Bool_0 = false;
	}

	public abstract void InitializeVisualOnStart();

	public abstract void ReturnVisualPartsToPool();

	public abstract void CacheDataTransforms();

	public abstract void RefreshVisualParts();

	public GClass2088()
	{
	}
}
