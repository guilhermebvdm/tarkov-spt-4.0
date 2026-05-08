using System;
using System.Linq;
using System.Runtime.CompilerServices;
using EFT;
using UnityEngine;

public class GClass308 : GClass177<GClass27>
{
	[Serializable]
	[CompilerGenerated]
	public class Class1007
	{
		public static readonly Class1007 class1007_0 = new Class1007();

		public static Func<DebugMarker, bool> func_0;

		public static Func<DebugMarker, bool> func_1;

		public bool method_0(DebugMarker marker)
		{
			return marker.Marker == EObjectDebugMarker.BotShootPosition;
		}

		public bool method_1(DebugMarker marker)
		{
			return marker.Marker == EObjectDebugMarker.AimTarget;
		}
	}

	[NonSerialized]
	public float Float_0;

	[NonSerialized]
	public DebugMarker[] DebugMarker_0;

	[NonSerialized]
	public Transform Transform_0;

	[NonSerialized]
	public Transform Transform_1;

	[NonSerialized]
	public GClass179 Gclass179_0;

	public GClass308(BotOwner bot)
		: base(bot)
	{
		DebugMarker_0 = UnityEngine.Object.FindObjectsByType<DebugMarker>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
		Transform_0 = DebugMarker_0.First((DebugMarker marker) => marker.Marker == EObjectDebugMarker.BotShootPosition).transform;
		Transform_1 = DebugMarker_0.First((DebugMarker marker) => marker.Marker == EObjectDebugMarker.AimTarget).transform;
		Gclass179_0 = new GClass179(bot);
	}

	public override void UpdateNodeByBrain(GClass27 data)
	{
		if (Transform_0 == null || Transform_1 == null)
		{
			return;
		}
		BotOwner_0.Memory.ComeToPoint();
		BotOwner_0.SetPose(1f);
		BotOwner_0.StopMove();
		if ((BotOwner_0.Position - Transform_0.position).sqrMagnitude >= 1f)
		{
			BotOwner_0.GetPlayer.Teleport(Transform_0.position);
		}
		if (!BotOwner_0.WeaponManager.UnderbarrelLauncherController.IsActive)
		{
			BotOwner_0.WeaponManager.UnderbarrelLauncherController.TryEnable();
			return;
		}
		Gclass179_0.UpdateNodeByBrain(data);
		if (BotOwner_0.Memory.DangerData._place == null)
		{
			BotOwner_0.Memory.DangerData.SetTarget(new PlaceForCheck(Transform_1.position, PlaceForCheckType.simple), BotOwner_0);
		}
	}
}
