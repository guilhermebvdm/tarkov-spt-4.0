using EFT.Interactive;
using UnityEngine;

public class SpecificDoorStateChangeEventGenerator : MonoBehaviour
{
	[SerializeField]
	private Door _targetDoor;

	public void Awake()
	{
		_targetDoor.OnDoorStateChanged += method_0;
	}

	public void OnDestroy()
	{
		_targetDoor.OnDoorStateChanged -= method_0;
	}

	public void method_0(WorldInteractiveObject obj, EDoorState prevState, EDoorState nextState)
	{
		GlobalEventHandlerClass.Instance.CreateCommonEvent<GClass3550>().Invoke(obj as Door);
	}
}
