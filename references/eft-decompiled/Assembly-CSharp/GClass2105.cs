using System;

public class GClass2105
{
	[NonSerialized]
	public DoorInteractionStateClass DoorInteractionStateClass;

	public Func<GClass2105> Transition;

	public GClass2105 GetNextState
	{
		get
		{
			if (Transition != null)
			{
				return Transition();
			}
			return null;
		}
	}

	public GClass2105(DoorInteractionStateClass baseState)
	{
		DoorInteractionStateClass = baseState;
	}

	public virtual void OnEnter()
	{
	}

	public virtual void OnExit()
	{
	}

	public virtual void Update(float dt)
	{
		DoorInteractionStateClass.BaseUpdate();
	}
}
