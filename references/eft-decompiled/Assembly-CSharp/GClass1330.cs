using System.Collections.Generic;
using System.IO;
using System.Linq;
using AnimationEventSystem;

public class GClass1330 : GClass1328, GInterface137, IStateBehaviour
{
	public string FullName;

	public int FullNameHash;

	public AnimationEventsContainer EventsContainer;

	public List<AnimationEvent> AnimationEvents;

	AnimationEventsContainer GInterface137.EventsContainer
	{
		get
		{
			return EventsContainer;
		}
		set
		{
			EventsContainer = value;
		}
	}

	int GInterface137.FullNameHash => FullNameHash;

	public GClass1330()
	{
		EventsContainer = new AnimationEventsContainer();
	}

	public override void ResetCache()
	{
		base.ResetCache();
		EventsContainer.ResetCache();
	}

	public override GClass1328 Clone()
	{
		GClass1330 gClass = new GClass1330();
		gClass.FullName = FullName;
		gClass.FullNameHash = FullNameHash;
		gClass.EventsContainer = EventsContainer.Clone() as AnimationEventsContainer;
		if (AnimationEvents != null)
		{
			gClass.AnimationEvents = new List<AnimationEvent>(AnimationEvents.Count);
			for (int i = 0; i < AnimationEvents.Count; i++)
			{
				gClass.AnimationEvents.Add(AnimationEvents[i].Clone() as AnimationEvent);
			}
		}
		return gClass;
	}

	public override void Serialize(BinaryWriter writer)
	{
		writer.Write(FullName);
		writer.Write(FullNameHash);
		if (AnimationEvents == null)
		{
			writer.Write((short)0);
			return;
		}
		writer.Write((short)AnimationEvents.Count);
		for (int i = 0; i < AnimationEvents.Count; i++)
		{
			AnimationEvents[i].Serialize(writer);
		}
	}

	public override void Deserialize(BinaryReader reader)
	{
		FullName = reader.ReadString();
		FullNameHash = reader.ReadInt32();
		short num = reader.ReadInt16();
		if (num == 0)
		{
			AnimationEvents = new List<AnimationEvent>();
			return;
		}
		AnimationEvents = new List<AnimationEvent>(num);
		for (int i = 0; i < num; i++)
		{
			AnimationEvent animationEvent = new AnimationEvent();
			animationEvent.Deserialize(reader);
			AnimationEvents.Add(animationEvent);
		}
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (this == obj)
		{
			return true;
		}
		if (obj.GetType() != GetType())
		{
			return false;
		}
		return method_0((GClass1330)obj);
	}

	public override int GetHashCode()
	{
		return (((((FullName != null) ? FullName.GetHashCode() : 0) * 397) ^ FullNameHash) * 397) ^ ((AnimationEvents != null) ? AnimationEvents.GetHashCode() : 0);
	}

	public override void OnStateEnter(FastAnimatorProcessorClass fastAnimatorProcessor, in AnimatorStateInfoWrapper layerInfo, int layerIndex)
	{
		EventsContainer.OnStateEnter(fastAnimatorProcessor, in layerInfo, layerIndex, AnimationEvents);
	}

	public override void OnStateUpdate(FastAnimatorProcessorClass fastAnimatorProcessor, in AnimatorStateInfoWrapper layerInfo, int layerIndex)
	{
		EventsContainer.OnStateUpdate(fastAnimatorProcessor, in layerInfo, layerIndex, AnimationEvents);
	}

	public override void OnStateExit(FastAnimatorProcessorClass fastAnimatorProcessor, in AnimatorStateInfoWrapper layerInfo, int layerIndex)
	{
		EventsContainer.OnStateExit(fastAnimatorProcessor, in layerInfo, layerIndex, AnimationEvents);
	}

	public override void OnStateMove(FastAnimatorProcessorClass fastAnimatorProcessor, in AnimatorStateInfoWrapper layerInfo, int layerIndex)
	{
	}

	public bool method_0(GClass1330 other)
	{
		if (AnimationEvents == null != (other.AnimationEvents == null))
		{
			return false;
		}
		if (AnimationEvents != null && !AnimationEvents.SequenceEqual(other.AnimationEvents))
		{
			return false;
		}
		if (FullName == other.FullName)
		{
			return FullNameHash == other.FullNameHash;
		}
		return false;
	}
}
