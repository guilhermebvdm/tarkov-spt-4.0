using UnityEngine;

namespace VisceralCombat.Ragdolls.Classes.RootMotion.Dynamics;

[AddComponentMenu("Scripts/RootMotion.Dynamics/PuppetMaster/Behaviours/BehaviourTemplate")]
public class BehaviourTemplate : BehaviourBase
{
	private const string typeSpring = "BehaviourTemplate";

	public SubBehaviourCOM centerOfMass;

	public LayerMask groundLayers;

	public PuppetEvent onLoseBalance;

	public float loseBalanceAngle = 60f;

	protected override string GetTypeSpring()
	{
		return "BehaviourTemplate";
	}

	protected override void OnInitiate()
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		centerOfMass.Initiate(this, groundLayers);
	}

	protected override void OnActivate()
	{
	}

	public override void OnReactivate()
	{
	}

	protected override void OnDeactivate()
	{
	}

	protected override void OnFixedUpdate(float deltaTime)
	{
		if (centerOfMass.angle > loseBalanceAngle)
		{
			onLoseBalance.Trigger(puppetMaster);
		}
	}

	protected override void OnLateUpdate(float deltaTime)
	{
	}

	protected override void OnMuscleHitBehaviour(MuscleHit hit)
	{
		if (((Behaviour)this).enabled)
		{
		}
	}

	protected override void OnMuscleCollisionBehaviour(MuscleCollision m)
	{
		if (((Behaviour)this).enabled)
		{
		}
	}
}
