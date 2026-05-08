using System;
using System.Runtime.CompilerServices;
using EFT;
using EFT.HealthSystem;

public abstract class GClass485 : GClass484
{
	[NonSerialized]
	public IHealthController IhealthController_0;

	[NonSerialized]
	public EBodyPart? Nullable_0;

	[NonSerialized]
	public EBodyPart[] EbodyPart_0;

	[NonSerialized]
	[CompilerGenerated]
	public MedsItemClass MedsItemClass;

	public bool HaveSmth2Use => CurUsingMeds != null;

	public MedsItemClass CurUsingMeds
	{
		[CompilerGenerated]
		get
		{
			return MedsItemClass;
		}
		[CompilerGenerated]
		set
		{
			MedsItemClass = value;
		}
	}

	public GClass485(BotOwner owner, Action<bool> callback)
		: base(owner, callback)
	{
		EbodyPart_0 = new EBodyPart[7];
		EbodyPart_0[0] = EBodyPart.Head;
		EbodyPart_0[1] = EBodyPart.Chest;
		EbodyPart_0[2] = EBodyPart.Stomach;
		EbodyPart_0[3] = EBodyPart.LeftArm;
		EbodyPart_0[4] = EBodyPart.RightArm;
		EbodyPart_0[5] = EBodyPart.LeftLeg;
		EbodyPart_0[6] = EBodyPart.RightLeg;
	}
}
