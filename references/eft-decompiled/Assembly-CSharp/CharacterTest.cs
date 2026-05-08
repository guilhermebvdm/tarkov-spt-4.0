using UnityEngine;

public class CharacterTest : MonoBehaviour
{
	public Transform DestPoint;

	private CharacterController characterController_0;

	public void Start()
	{
		characterController_0 = GetComponent<CharacterController>();
	}

	public void Update()
	{
		characterController_0.Move((DestPoint.position - base.transform.position).normalized);
	}
}
