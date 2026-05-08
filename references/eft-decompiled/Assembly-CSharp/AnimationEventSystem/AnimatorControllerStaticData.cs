using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace AnimationEventSystem;

[CreateAssetMenu]
public class AnimatorControllerStaticData : ScriptableObject
{
	[SerializeField]
	private List<EventsCollection> _stateHashToEventsCollection;

	[SerializeField]
	private List<LActionSetup> _stateHashToLActionSetups;

	[CanBeNull]
	public List<AnimationEvent> GetEventsByIndex(int index)
	{
		if (IsValidListIndex(index))
		{
			return _stateHashToEventsCollection[index].AnimationEvents;
		}
		Debug.LogErrorFormat("Invalid index for {0}", index);
		return null;
	}

	public LActionSetup GetLActionSetupById(int lActionSetupId)
	{
		return _stateHashToLActionSetups[lActionSetupId];
	}

	public bool IsValidListIndex(int index)
	{
		if (index >= 0)
		{
			return index < _stateHashToEventsCollection.Count;
		}
		return false;
	}
}
