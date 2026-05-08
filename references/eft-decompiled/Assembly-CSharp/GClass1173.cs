using System;
using UnityEngine;

public class GClass1173 : GInterface463
{
	[NonSerialized]
	public const int Int_0 = 50;

	[NonSerialized]
	public const int Int_1 = 5;

	[NonSerialized]
	public GClass944<SourceContainerClass> Gclass944_0 = new GClass944<SourceContainerClass>(50, 5);

	[NonSerialized]
	public int Int_2;

	public SourceContainerClass GetContainer(Transform transform, EOcclusionTest test)
	{
		SourceContainerClass sourceContainerClass = Gclass944_0.GetObject();
		sourceContainerClass.Clear();
		sourceContainerClass.Initialize(transform, test, Int_2);
		Int_2++;
		return sourceContainerClass;
	}

	public void ReturnContainer(SourceContainerClass container)
	{
		container.Clear();
		Gclass944_0.PutObject(container);
	}
}
