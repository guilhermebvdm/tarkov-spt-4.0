using UnityEngine;

namespace RootMotion.Demos;

[RequireComponent(typeof(Animator))]
public class CharacterAnimationSimple : CharacterAnimationBase
{
	[SerializeField]
	private CharacterThirdPerson characterController;

	[SerializeField]
	private float pivotOffset;

	[SerializeField]
	private AnimationCurve moveSpeed;

	private Animator animator_0;

	public override void Start()
	{
		base.Start();
		animator_0 = GetComponentInChildren<Animator>();
	}

	public override Vector3 GetPivotPoint()
	{
		if (pivotOffset == 0f)
		{
			return base.transform.position;
		}
		return base.transform.position + base.transform.forward * pivotOffset;
	}

	public void Update()
	{
		float num = moveSpeed.Evaluate(characterController.animState.moveDirection.z);
		animator_0.SetFloat("Speed", num);
		characterController.Move(characterController.transform.forward * Time.deltaTime * num);
	}
}
