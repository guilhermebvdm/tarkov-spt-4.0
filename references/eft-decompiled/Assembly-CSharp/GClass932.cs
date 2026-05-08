using System.Collections.Generic;
using Diz.Binding;
using EFT;
using EFT.InventoryLogic;

public class GClass932
{
	public readonly InventoryEquipment equipment;

	public readonly GClass2197 customization;

	public readonly BindableEvent onProgress;

	public readonly int hash;

	public float progress;

	public GClass932(InventoryEquipment equipment, GClass2197 customization)
	{
		this.equipment = equipment;
		this.customization = customization;
		onProgress = new BindableEvent();
		hash = method_0();
		progress = 0f;
	}

	public void Progress(float newProgress)
	{
		progress = newProgress;
		onProgress.Invoke();
	}

	public int method_0()
	{
		int num = 71;
		num = 4331 + GClass928.GetItemHash(equipment);
		foreach (KeyValuePair<EBodyModelPart, MongoID> item in customization)
		{
			item.Deconstruct(out var _, out var value);
			MongoID mongoID = value;
			num = num * 53 + mongoID.GetHashCode();
		}
		return num;
	}
}
