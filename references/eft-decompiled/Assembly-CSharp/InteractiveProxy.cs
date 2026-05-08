using System.Collections;
using EFT.Interactive;
using UnityEngine;

public class InteractiveProxy : MonoBehaviour
{
	public WorldInteractiveObject Link;

	public GripPose[] Grips;

	public DoorHandle Handle;

	public Light StatusLight;

	public Light FlickerLight;

	public AnimationCurve FlickCurve;

	[SerializeField]
	private Vector3 _viewTarget;

	[SerializeField]
	private Vector3 _interactionPosition;

	public Vector3 GetInteractionPosition(Vector3 yourPosition)
	{
		Vector3 result = base.transform.TransformPoint(_interactionPosition);
		result.y = yourPosition.y;
		return result;
	}

	public void Start()
	{
		Link.OnDoorStateChanged += method_0;
		Link.DoorKeyOpenInteraction = EInteraction.DoorCardOpen;
		method_0(Link, EDoorState.Open, EDoorState.Shut);
		FlickerLight.enabled = false;
	}

	public void method_0(WorldInteractiveObject obj, EDoorState prevstate, EDoorState nextstate)
	{
		StatusLight.color = ((obj.DoorState == EDoorState.Locked) ? Color.red : ((obj.DoorState == EDoorState.Interacting) ? Color.red : Color.green));
	}

	public IEnumerator method_1()
	{
		float num = 0f;
		FlickerLight.enabled = true;
		FlickerLight.color = Color.red;
		while (num < GClass842.GetDuration(FlickCurve))
		{
			FlickerLight.intensity = FlickCurve.Evaluate(num);
			num += Time.deltaTime;
			yield return null;
		}
		FlickerLight.enabled = false;
	}

	public void StartFlicker()
	{
		StartCoroutine(method_1());
	}

	public Vector3 GetViewDirection(Vector3 yourPosition)
	{
		return base.transform.TransformPoint(_viewTarget);
	}

	public void OnDrawGizmos()
	{
		Gizmos.color = Color.blue;
		Gizmos.DrawWireSphere(base.transform.TransformPoint(_viewTarget), 0.05f);
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireSphere(base.transform.TransformPoint(_interactionPosition), 0.05f);
		Gizmos.color = Color.white;
	}

	public void OnValidate()
	{
		Grips = GetComponentsInChildren<GripPose>();
		Handle = GetComponentInChildren<DoorHandle>();
	}
}
