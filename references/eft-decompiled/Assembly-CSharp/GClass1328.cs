using System;
using System.IO;

public class GClass1328
{
	[NonSerialized]
	public GClass1312 Gclass1312_0;

	public GClass1312 AbstractState => Gclass1312_0;

	public static TFastAnimatorBehavior Create<TFastAnimatorBehavior>(GClass1312 state) where TFastAnimatorBehavior : GClass1328, new()
	{
		return new TFastAnimatorBehavior
		{
			Gclass1312_0 = state
		};
	}

	public virtual GClass1328 Clone()
	{
		throw new NotImplementedException();
	}

	public virtual void Serialize(BinaryWriter writer)
	{
	}

	public virtual void Deserialize(BinaryReader reader)
	{
	}

	public virtual void ResetCache()
	{
	}

	public virtual void OnStateEnter(FastAnimatorProcessorClass fastAnimatorProcessor, in AnimatorStateInfoWrapper layerInfo, int layerIndex)
	{
	}

	public virtual void OnStateUpdate(FastAnimatorProcessorClass fastAnimatorProcessor, in AnimatorStateInfoWrapper layerInfo, int layerIndex)
	{
	}

	public virtual void OnStateExit(FastAnimatorProcessorClass fastAnimatorProcessor, in AnimatorStateInfoWrapper layerInfo, int layerIndex)
	{
	}

	public virtual void OnStateMove(FastAnimatorProcessorClass fastAnimatorProcessor, in AnimatorStateInfoWrapper layerInfo, int layerIndex)
	{
	}
}
