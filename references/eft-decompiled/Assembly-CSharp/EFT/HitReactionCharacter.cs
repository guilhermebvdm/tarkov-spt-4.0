using RootMotion.FinalIK;
using UnityEngine;

namespace EFT;

public class HitReactionCharacter : MonoBehaviour
{
	[SerializeField]
	private string mixingAnim;

	[SerializeField]
	private Transform recursiveMixingTransform;

	[SerializeField]
	private Camera cam;

	[SerializeField]
	private HitReaction hitReaction;

	[SerializeField]
	private float hitForce = 1f;

	public void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			Ray ray = cam.ScreenPointToRay(Input.mousePosition);
			RaycastHit hitInfo = default(RaycastHit);
			if (Physics.Raycast(ray, out hitInfo, 100f) && TryGetComponent<BodyPartCollider>(out var component))
			{
				hitReaction.Hit(component.BodyPartColliderType, component.BodyPartType, ray.direction * hitForce, hitInfo.point);
			}
		}
	}
}
