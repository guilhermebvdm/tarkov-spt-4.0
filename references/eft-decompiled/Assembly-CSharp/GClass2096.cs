using System;
using System.Collections.Generic;
using AnimationEventSystem;
using EFT;
using EFT.ZombieEventsConsumers;

public class GClass2096 : IPlayerAnimatorEvents
{
	[NonSerialized]
	public List<GInterface212> List_0;

	[NonSerialized]
	public GClass2102 Gclass2102_0;

	[NonSerialized]
	public GClass2101 Gclass2101_0;

	[NonSerialized]
	public GClass2099 Gclass2099_0;

	[NonSerialized]
	public GClass2100 Gclass2100_0;

	[NonSerialized]
	public GClass2098 Gclass2098_0;

	[NonSerialized]
	public GClass2103 Gclass2103_0;

	[NonSerialized]
	public GClass2104 Gclass2104_0;

	[NonSerialized]
	public Class1394 Class1394_0;

	public IVaultingSoundsEvents VaultingSoundsEvents => Gclass2102_0;

	public IVaultingSoundsEvents SprintVaultSoundsEvents => Gclass2101_0;

	public IVaultingSoundsEvents ClimbSoundsEvents => Gclass2099_0;

	public IDropBackPackEvents DropBackpackEvents => Gclass2100_0;

	public IBipodToggleEvents BipodToggleEvents => Gclass2098_0;

	public IZombieFireBulletEvents ZombieFireBulletEvents => Gclass2103_0;

	public IZombieFireEndEvents ZombieFireEndEvents => Gclass2104_0;

	public ILeftHandInteractionEvents LeftHandInteractionEvents => Class1394_0;

	public GClass2096()
	{
		Gclass2102_0 = new GClass2102();
		Gclass2101_0 = new GClass2101();
		Gclass2099_0 = new GClass2099();
		Gclass2100_0 = new GClass2100();
		Gclass2098_0 = new GClass2098();
		Gclass2103_0 = new GClass2103();
		Gclass2104_0 = new GClass2104();
		Class1394_0 = new Class1394();
		List_0 = new List<GInterface212> { Gclass2102_0, Gclass2101_0, Gclass2099_0, Gclass2100_0, Gclass2098_0, Gclass2103_0, Gclass2104_0, Class1394_0 };
	}

	public void DispatchEvents(int functionNameHash, AnimationEventParameter parameter)
	{
		foreach (GInterface212 item in List_0)
		{
			if (item.BindedFunctions.TryGetValue(functionNameHash, out var value))
			{
				value(parameter);
			}
		}
	}
}
