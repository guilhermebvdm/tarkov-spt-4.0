using UnityEngine;

public class GClass1150
{
	public virtual void LogError(EAudioPortalError error, string portalName)
	{
		switch (error)
		{
		case EAudioPortalError.MultipleConnections:
			Debug.LogWarning("Spatial Audio Portal '" + portalName + "' already has been assigned with a pair of rooms that it connects. Each portal can only connect one room pair.");
			break;
		case EAudioPortalError.CanNotSetPortal:
			Debug.LogWarning("Cannot set portal '" + portalName + "' as 'closed', since it is a wall.");
			break;
		}
	}
}
