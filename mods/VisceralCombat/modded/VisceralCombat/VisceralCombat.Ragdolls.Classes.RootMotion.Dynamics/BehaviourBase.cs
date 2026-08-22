using System;
using UnityEngine;
using Random = UnityEngine.Random;
using Object = UnityEngine.Object;
using UnityEngine.Events;

namespace VisceralCombat.Ragdolls.Classes.RootMotion.Dynamics;

public abstract class BehaviourBase : MonoBehaviour
{
	public delegate void BehaviourDelegate();

	public delegate void BehaviourUpdateDelegate(float deltaTime);

	public delegate void HitDelegate(MuscleHit hit);

	public delegate void CollisionDelegate(MuscleCollision collision);

	[Serializable]
	public struct PuppetEvent
	{
		[Tooltip("Another Puppet Behaviour to switch to on this event. This must be the exact Type of the the Behaviour, careful with spelling.")]
		public string switchToBehaviour;

		[Tooltip("Animations to cross-fade to on this event. This is separate from the UnityEvent below because UnityEvents can't handle calls with more than one parameter such as Animator.CrossFade.")]
		public AnimatorEvent[] animations;

		[Tooltip("The UnityEvent to invoke on this event.")]
		public UnityEvent unityEvent;

		private const string empty = "";

		public bool switchBehaviour => switchToBehaviour != string.Empty && switchToBehaviour != "";

		public void Trigger(PuppetMaster puppetMaster, bool switchBehaviourEnabled = true)
		{
			unityEvent.Invoke();
			AnimatorEvent[] array = animations;
			foreach (AnimatorEvent animatorEvent in array)
			{
				animatorEvent.Activate(puppetMaster.targetAnimator, puppetMaster.targetAnimation);
			}
			if (!switchBehaviour)
			{
				return;
			}
			bool flag = false;
			BehaviourBase[] behaviours = puppetMaster.behaviours;
			foreach (BehaviourBase behaviourBase in behaviours)
			{
				if ((Object)(object)behaviourBase != (Object)null && behaviourBase.GetTypeSpring() == switchToBehaviour)
				{
					flag = true;
					behaviourBase.Activate();
					break;
				}
			}
			if (flag)
			{
			}
		}
	}

	[Serializable]
	public class AnimatorEvent
	{
		public string animationState;

		public float crossfadeTime = 0.3f;

		public int layer;

		public bool resetNormalizedTime;

		private const string empty = "";

		public void Activate(Animator animator, Animation animation)
		{
			if ((Object)(object)animator != (Object)null)
			{
				Activate(animator);
			}
			if ((Object)(object)animation != (Object)null)
			{
				Activate(animation);
			}
		}

		private void Activate(Animator animator)
		{
			if (animationState == "")
			{
				return;
			}
			if (resetNormalizedTime)
			{
				if (crossfadeTime > 0f)
				{
					animator.CrossFadeInFixedTime(animationState, crossfadeTime, layer, 0f);
				}
				else
				{
					animator.Play(animationState, layer, 0f);
				}
			}
			else if (crossfadeTime > 0f)
			{
				animator.CrossFadeInFixedTime(animationState, crossfadeTime, layer);
			}
			else
			{
				animator.Play(animationState, layer);
			}
		}

		private void Activate(Animation animation)
		{
			if (!(animationState == ""))
			{
				if (resetNormalizedTime)
				{
					animation[animationState].normalizedTime = 0f;
				}
				animation[animationState].layer = layer;
				animation.CrossFade(animationState, crossfadeTime);
			}
		}
	}

	[HideInInspector]
	public PuppetMaster puppetMaster;

	public BehaviourDelegate OnPreActivate;

	public BehaviourDelegate OnPreInitiate;

	public BehaviourUpdateDelegate OnPreFixedUpdate;

	public BehaviourUpdateDelegate OnPreUpdate;

	public BehaviourUpdateDelegate OnPreLateUpdate;

	public BehaviourUpdateDelegate OnPreRead;

	public BehaviourUpdateDelegate OnPreWrite;

	public BehaviourDelegate OnPreDeactivate;

	public BehaviourDelegate OnPreFixTransforms;

	public HitDelegate OnPreMuscleHit;

	public CollisionDelegate OnPreMuscleCollision;

	public CollisionDelegate OnPreMuscleCollisionExit;

	public BehaviourDelegate OnHierarchyChanged;

	public BehaviourDelegate OnPostActivate;

	public BehaviourDelegate OnPostInitiate;

	public BehaviourUpdateDelegate OnPostFixedUpdate;

	public BehaviourUpdateDelegate OnPostUpdate;

	public BehaviourUpdateDelegate OnPostLateUpdate;

	public BehaviourUpdateDelegate OnPostRead;

	public BehaviourUpdateDelegate OnPostWrite;

	public BehaviourDelegate OnPostDeactivate;

	public BehaviourDelegate OnPostDrawGizmos;

	public BehaviourDelegate OnPostFixTransforms;

	public HitDelegate OnPostMuscleHit;

	public CollisionDelegate OnPostMuscleCollision;

	public CollisionDelegate OnPostMuscleCollisionExit;

	[HideInInspector]
	public bool deactivated;

	private bool initiated = false;

	private const string typeSpringBase = "BehaviourBase";

	public bool forceActive { get; protected set; }

	public abstract void OnReactivate();

	public virtual void Resurrect()
	{
	}

	public virtual void Freeze()
	{
	}

	public virtual void Unfreeze()
	{
	}

	public virtual void KillStart()
	{
	}

	public virtual void KillEnd()
	{
	}

	public virtual void OnTeleport(Quaternion deltaRotation, Vector3 deltaPosition, Vector3 pivot, bool moveToTarget)
	{
	}

	public virtual void OnMuscleDisconnected(Muscle m)
	{
	}

	public virtual void OnMuscleReconnected(Muscle m)
	{
	}

	public virtual void OnMuscleAdded(Muscle m)
	{
		if (OnHierarchyChanged != null)
		{
			OnHierarchyChanged();
		}
	}

	public virtual void OnMuscleRemoved(Muscle m)
	{
		if (OnHierarchyChanged != null)
		{
			OnHierarchyChanged();
		}
	}

	protected virtual void OnActivate()
	{
	}

	protected virtual void OnDeactivate()
	{
	}

	protected virtual void OnInitiate()
	{
	}

	protected virtual void OnFixedUpdate(float deltaTime)
	{
	}

	protected virtual void OnUpdate(float deltaTime)
	{
	}

	protected virtual void OnLateUpdate(float deltaTime)
	{
	}

	protected virtual void OnReadBehaviour(float deltaTime)
	{
	}

	protected virtual void OnWriteBehaviour(float deltaTime)
	{
	}

	protected virtual void OnDrawGizmosBehaviour()
	{
	}

	protected virtual void OnFixTransformsBehaviour()
	{
	}

	protected virtual void OnMuscleHitBehaviour(MuscleHit hit)
	{
	}

	protected virtual void OnMuscleCollisionBehaviour(MuscleCollision collision)
	{
	}

	protected virtual void OnMuscleCollisionExitBehaviour(MuscleCollision collision)
	{
	}

	public void Initiate()
	{
		initiated = true;
		if (OnPreInitiate != null)
		{
			OnPreInitiate();
		}
		OnInitiate();
		if (OnPostInitiate != null)
		{
			OnPostInitiate();
		}
	}

	public void OnFixTransforms()
	{
		if (initiated && ((Behaviour)this).enabled)
		{
			if (OnPreFixTransforms != null)
			{
				OnPreFixTransforms();
			}
			OnFixTransformsBehaviour();
			if (OnPostFixTransforms != null)
			{
				OnPostFixTransforms();
			}
		}
	}

	public void OnRead(float deltaTime)
	{
		if (initiated && ((Behaviour)this).enabled)
		{
			if (OnPreRead != null)
			{
				OnPreRead(deltaTime);
			}
			OnReadBehaviour(deltaTime);
			if (OnPostRead != null)
			{
				OnPostRead(deltaTime);
			}
		}
	}

	public void OnWrite(float deltaTime)
	{
		if (initiated && ((Behaviour)this).enabled)
		{
			if (OnPreWrite != null)
			{
				OnPreWrite(deltaTime);
			}
			OnWriteBehaviour(deltaTime);
			if (OnPostWrite != null)
			{
				OnPostWrite(deltaTime);
			}
		}
	}

	public void OnMuscleHit(MuscleHit hit)
	{
		if (initiated)
		{
			if (OnPreMuscleHit != null)
			{
				OnPreMuscleHit(hit);
			}
			OnMuscleHitBehaviour(hit);
			if (OnPostMuscleHit != null)
			{
				OnPostMuscleHit(hit);
			}
		}
	}

	public void OnMuscleCollision(MuscleCollision collision)
	{
		if (initiated)
		{
			if (OnPreMuscleCollision != null)
			{
				OnPreMuscleCollision(collision);
			}
			OnMuscleCollisionBehaviour(collision);
			if (OnPostMuscleCollision != null)
			{
				OnPostMuscleCollision(collision);
			}
		}
	}

	public void OnMuscleCollisionExit(MuscleCollision collision)
	{
		if (initiated)
		{
			if (OnPreMuscleCollisionExit != null)
			{
				OnPreMuscleCollisionExit(collision);
			}
			OnMuscleCollisionExitBehaviour(collision);
			if (OnPostMuscleCollisionExit != null)
			{
				OnPostMuscleCollisionExit(collision);
			}
		}
	}

	public void Activate()
	{
		BehaviourBase[] behaviours = puppetMaster.behaviours;
		foreach (BehaviourBase behaviourBase in behaviours)
		{
			((Behaviour)behaviourBase).enabled = (Object)(object)behaviourBase == (Object)(object)this;
		}
		if (OnPreActivate != null)
		{
			OnPreActivate();
		}
		OnActivate();
		if (OnPostActivate != null)
		{
			OnPostActivate();
		}
	}

	private void OnDisable()
	{
		if (initiated)
		{
			if (OnPreDeactivate != null)
			{
				OnPreDeactivate();
			}
			OnDeactivate();
			if (OnPostDeactivate != null)
			{
				OnPostDeactivate();
			}
		}
	}

	public void FixedUpdateB(float deltaTime)
	{
		if (initiated && ((Behaviour)this).enabled && puppetMaster.muscles.Length != 0)
		{
			if (OnPreFixedUpdate != null && ((Behaviour)this).enabled)
			{
				OnPreFixedUpdate(deltaTime);
			}
			OnFixedUpdate(deltaTime);
			if (OnPostFixedUpdate != null && ((Behaviour)this).enabled)
			{
				OnPostFixedUpdate(deltaTime);
			}
		}
	}

	public void UpdateB(float deltaTime)
	{
		if (initiated && ((Behaviour)this).enabled && puppetMaster.muscles.Length != 0)
		{
			if (OnPreUpdate != null && ((Behaviour)this).enabled)
			{
				OnPreUpdate(deltaTime);
			}
			OnUpdate(deltaTime);
			if (OnPostUpdate != null && ((Behaviour)this).enabled)
			{
				OnPostUpdate(deltaTime);
			}
		}
	}

	public void LateUpdateB(float deltaTime)
	{
		if (initiated && ((Behaviour)this).enabled && puppetMaster.muscles.Length != 0)
		{
			if (OnPreLateUpdate != null && ((Behaviour)this).enabled)
			{
				OnPreLateUpdate(deltaTime);
			}
			OnLateUpdate(deltaTime);
			if (OnPostLateUpdate != null && ((Behaviour)this).enabled)
			{
				OnPostLateUpdate(deltaTime);
			}
		}
	}

	protected virtual void OnDrawGizmos()
	{
		if (initiated)
		{
			OnDrawGizmosBehaviour();
			if (OnPostDrawGizmos != null)
			{
				OnPostDrawGizmos();
			}
		}
	}

	protected virtual string GetTypeSpring()
	{
		return "BehaviourBase";
	}

	protected void RotateTargetToRootMuscle()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = Quaternion.Inverse(puppetMaster.muscles[0].target.rotation) * puppetMaster.targetRoot.forward;
		Vector3 val2 = puppetMaster.muscles[0].rigidbody.rotation * val;
		val2.y = 0f;
		puppetMaster.targetRoot.rotation = Quaternion.LookRotation(val2);
	}

	protected void TranslateTargetToRootMuscle(float maintainY)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		puppetMaster.muscles[0].target.position = new Vector3(puppetMaster.muscles[0].transform.position.x, Mathf.Lerp(puppetMaster.muscles[0].transform.position.y, puppetMaster.muscles[0].target.position.y, maintainY), puppetMaster.muscles[0].transform.position.z);
	}

	protected void RemovePropMuscles()
	{
		while (ContainsRemovablePropMuscle())
		{
			for (int i = 0; i < puppetMaster.muscles.Length; i++)
			{
				if (puppetMaster.muscles[i].props.group == Muscle.Group.Prop && !puppetMaster.muscles[i].isPropMuscle)
				{
					puppetMaster.RemoveMuscleRecursive(puppetMaster.muscles[i].joint, attachTarget: true);
					break;
				}
			}
		}
	}

	protected virtual void GroundTarget(LayerMask layers)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		Ray val = new Ray(puppetMaster.targetRoot.position + puppetMaster.targetRoot.up, -puppetMaster.targetRoot.up);
		if (Physics.Raycast(val, out RaycastHit val2, 4f, layers) && !float.IsNaN(val2.point.x) && !float.IsNaN(val2.point.y) && !float.IsNaN(val2.point.z))
		{
			puppetMaster.targetRoot.position = val2.point;
		}
	}

	protected bool ContainsRemovablePropMuscle()
	{
		Muscle[] muscles = puppetMaster.muscles;
		foreach (Muscle muscle in muscles)
		{
			if (muscle.props.group == Muscle.Group.Prop && !muscle.isPropMuscle)
			{
				return true;
			}
		}
		return false;
	}
}
