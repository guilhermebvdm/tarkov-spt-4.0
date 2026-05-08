using System;
using System.Runtime.CompilerServices;

public class GClass425
{
	[NonSerialized]
	[CompilerGenerated]
	public EInteraction? Nullable_0;

	[NonSerialized]
	[CompilerGenerated]
	public EPhraseTrigger? Nullable_1;

	public EInteraction? GetureAnswer
	{
		[CompilerGenerated]
		get
		{
			return Nullable_0;
		}
	}

	public EPhraseTrigger? PhraseTrigger
	{
		[CompilerGenerated]
		get
		{
			return Nullable_1;
		}
	}

	public static GClass425 Create(EInteraction gestureByRequester)
	{
		EPhraseTrigger ePhraseTrigger = EPhraseTrigger.PhraseNone;
		EInteraction eInteraction = EInteraction.None;
		switch (gestureByRequester)
		{
		case EInteraction.FriendlyGesture:
			eInteraction = EInteraction.FriendlyGesture;
			break;
		case EInteraction.GetOffGesture:
			eInteraction = EInteraction.GetOffGesture;
			break;
		case EInteraction.OkGesture:
			if (GClass856.IsTrue100(50f))
			{
				eInteraction = (GClass856.IsTrue100(50f) ? EInteraction.OkGesture : EInteraction.NoGesture);
			}
			else
			{
				ePhraseTrigger = (GClass856.IsTrue100(50f) ? EPhraseTrigger.Gogogo : EPhraseTrigger.OnGoodWork);
			}
			break;
		case EInteraction.ThereGesture:
		case EInteraction.HoldGesture:
		case EInteraction.NoGesture:
			if (GClass856.IsTrue100(50f))
			{
				eInteraction = EInteraction.HoldGesture;
			}
			else
			{
				ePhraseTrigger = (GClass856.IsTrue100(50f) ? EPhraseTrigger.Look : EPhraseTrigger.Repeat);
			}
			break;
		case EInteraction.ComeWithMeGesture:
			if (GClass856.IsTrue100(50f))
			{
				eInteraction = (GClass856.IsTrue100(50f) ? EInteraction.FriendlyGesture : EInteraction.ComeWithMeGesture);
			}
			else
			{
				ePhraseTrigger = (GClass856.IsTrue100(50f) ? EPhraseTrigger.DontKnow : EPhraseTrigger.Negative);
			}
			break;
		}
		if (eInteraction != EInteraction.None)
		{
			return new GClass425(eInteraction);
		}
		if (ePhraseTrigger != EPhraseTrigger.PhraseNone)
		{
			return new GClass425(ePhraseTrigger);
		}
		return new GClass425(EPhraseTrigger.OnMutter);
	}

	public GClass425(EInteraction g)
	{
		Nullable_1 = null;
		Nullable_0 = g;
	}

	public GClass425(EPhraseTrigger g)
	{
		Nullable_1 = g;
		Nullable_0 = null;
	}
}
