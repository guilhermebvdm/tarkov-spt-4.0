using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;
using Random = UnityEngine.Random;
using Object = UnityEngine.Object;

namespace VisceralCombat.Ragdolls.Classes.RootMotion.Dynamics;

[HelpURL("http://root-motion.com/puppetmasterdox/html/page11.html")]
[AddComponentMenu("Scripts/RootMotion.Dynamics/PuppetMaster/Behaviours/BehaviourFall")]
public class BehaviourFall : BehaviourBase
{
	[CompilerGenerated]
	private sealed class _003CSmoothActivate_003Ed__23 : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public BehaviourFall _003C_003E4__this;

		private float _003Cfader_003E5__1;

		private Muscle[] _003C_003Es__2;

		private int _003C_003Es__3;

		private Muscle _003Cm_003E5__4;

		private Muscle[] _003C_003Es__5;

		private int _003C_003Es__6;

		private Muscle _003Cm_003E5__7;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CSmoothActivate_003Ed__23(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003C_003Es__2 = null;
			_003Cm_003E5__4 = null;
			_003C_003Es__5 = null;
			_003Cm_003E5__7 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			//IL_00db: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
			switch (_003C_003E1__state)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003C_003E4__this.timer = 0f;
				_003C_003E4__this.endTriggered = false;
				_003C_003E4__this.puppetMaster.targetAnimator.CrossFadeInFixedTime(_003C_003E4__this.stateName, _003C_003E4__this.transitionDuration, _003C_003E4__this.layer, _003C_003E4__this.fixedTime);
				_003C_003Es__2 = _003C_003E4__this.puppetMaster.muscles;
				for (_003C_003Es__3 = 0; _003C_003Es__3 < _003C_003Es__2.Length; _003C_003Es__3++)
				{
					_003Cm_003E5__4 = _003C_003Es__2[_003C_003Es__3];
					_003Cm_003E5__4.state.pinWeightMlp = 0f;
					_003Cm_003E5__4.rigidbody.velocity = _003Cm_003E5__4.mappedVelocity;
					_003Cm_003E5__4.rigidbody.angularVelocity = _003Cm_003E5__4.mappedAngularVelocity;
					_003Cm_003E5__4 = null;
				}
				_003C_003Es__2 = null;
				_003Cfader_003E5__1 = 0f;
				break;
			case 1:
				_003C_003E1__state = -1;
				break;
			}
			if (_003Cfader_003E5__1 < 1f)
			{
				_003Cfader_003E5__1 += Time.deltaTime;
				_003C_003Es__5 = _003C_003E4__this.puppetMaster.muscles;
				for (_003C_003Es__6 = 0; _003C_003Es__6 < _003C_003Es__5.Length; _003C_003Es__6++)
				{
					_003Cm_003E5__7 = _003C_003Es__5[_003C_003Es__6];
					_003Cm_003E5__7.state.pinWeightMlp -= Time.deltaTime;
					_003Cm_003E5__7.state.mappingWeightMlp += Time.deltaTime * _003C_003E4__this.blendMappingSpeed;
					_003Cm_003E5__7 = null;
				}
				_003C_003Es__5 = null;
				_003C_003E2__current = null;
				_003C_003E1__state = 1;
				return true;
			}
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	private const string typeSpring = "BehaviourFall";

	[LargeHeader("Animation State")]
	[Tooltip("Animation State to crossfade to when this behaviour is activated.")]
	public string stateName = "Falling";

	[Tooltip("The duration of crossfading to 'State Name'. Value is in seconds.")]
	public float transitionDuration = 0.4f;

	[Tooltip("Layer index containing the destination state. If no layer is specified or layer is -1, the first state that is found with the given name or hash will be played.")]
	public int layer;

	[Tooltip("Start time of the current destination state. Value is in seconds. If no explicit fixedTime is specified or fixedTime value is float.NegativeInfinity, the state will either be played from the start if it's not already playing, or will continue playing from its current time and no transition will happen.")]
	public float fixedTime;

	[LargeHeader("Blending")]
	[Tooltip("The layers that will be raycasted against to find colliding objects.")]
	public LayerMask raycastLayers;

	[Tooltip("The parameter in the Animator that blends between catch fall and writhe animations.")]
	public string blendParameter = "FallBlend";

	[Tooltip("The height of the pelvis from the ground at which will blend to writhe animation.")]
	public float writheHeight = 4f;

	[Tooltip("The vertical velocity of the pelvis at which will blend to writhe animation.")]
	public float writheYVelocity = 1f;

	[Tooltip("The speed of blendig between the two falling animations.")]
	public float blendSpeed = 3f;

	[Tooltip("The speed of blending in mapping on activation.")]
	public float blendMappingSpeed = 1f;

	[LargeHeader("Ending")]
	[Tooltip("If false, this behaviour will never end.")]
	public bool canEnd;

	[Tooltip("The minimum time since this behaviour activated before it can end.")]
	public float minTime = 1.5f;

	[Tooltip("If the velocity of the pelvis falls below this value, can end the behaviour.")]
	public float maxEndVelocity = 0.5f;

	[Tooltip("Event triggered when all end conditions are met.")]
	public PuppetEvent onEnd;

	private float timer;

	private bool endTriggered;

	protected override string GetTypeSpring()
	{
		return "BehaviourFall";
	}

	[ContextMenu("User Manual")]
	private void OpenUserManual()
	{
		Application.OpenURL("http://root-motion.com/puppetmasterdox/html/page11.html");
	}

	[ContextMenu("Scrpt Reference")]
	private void OpenScriptReference()
	{
		Application.OpenURL("http://root-motion.com/puppetmasterdox/html/class_root_motion_1_1_dynamics_1_1_behaviour_fall.html");
	}

	protected override void OnActivate()
	{
		base.forceActive = true;
		((MonoBehaviour)this).StopAllCoroutines();
		((MonoBehaviour)this).StartCoroutine(SmoothActivate());
	}

	protected override void OnDeactivate()
	{
		base.forceActive = false;
	}

	public override void OnReactivate()
	{
		timer = 0f;
		endTriggered = false;
	}

	[IteratorStateMachine(typeof(_003CSmoothActivate_003Ed__23))]
	private IEnumerator SmoothActivate()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CSmoothActivate_003Ed__23(0)
		{
			_003C_003E4__this = this
		};
	}

	protected override void OnFixedUpdate(float deltaTime)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		if (raycastLayers == -1)
		{
		}
		float blendTarget = GetBlendTarget(GetGroundHeight());
		float num = Mathf.MoveTowards(puppetMaster.targetAnimator.GetFloat(blendParameter), blendTarget, deltaTime * blendSpeed);
		puppetMaster.targetAnimator.SetFloat(blendParameter, num);
		timer += deltaTime;
		if (!endTriggered && canEnd && timer >= minTime && !puppetMaster.isBlending)
		{
			Vector3 velocity = puppetMaster.muscles[0].rigidbody.velocity;
			if (velocity.magnitude < maxEndVelocity)
			{
				endTriggered = true;
				onEnd.Trigger(puppetMaster);
			}
		}
	}

	protected override void OnLateUpdate(float deltaTime)
	{
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		if (!(puppetMaster.muscles[0].state.mappingWeightMlp < 1f) && !puppetMaster.muscles[0].rigidbody.isKinematic && !puppetMaster.isBlending)
		{
			Transform targetRoot = puppetMaster.targetRoot;
			targetRoot.position += puppetMaster.muscles[0].transform.position - puppetMaster.muscles[0].target.position;
			GroundTarget(raycastLayers);
		}
	}

	public override void Resurrect()
	{
		Muscle[] muscles = puppetMaster.muscles;
		foreach (Muscle muscle in muscles)
		{
			muscle.state.pinWeightMlp = 0f;
		}
	}

	private float GetBlendTarget(float groundHeight)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		if (groundHeight > writheHeight)
		{
			return 1f;
		}
		Vector3 val = V3Tools.ExtractVertical(puppetMaster.muscles[0].rigidbody.velocity, puppetMaster.targetRoot.up, 1f);
		float num = val.magnitude;
		if (Vector3.Dot(val, puppetMaster.targetRoot.up) < 0f)
		{
			num = 0f - num;
		}
		if (num > writheYVelocity)
		{
			return 1f;
		}
		return 0f;
	}

	private float GetGroundHeight()
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		if (Physics.Raycast(puppetMaster.muscles[0].rigidbody.position, -puppetMaster.targetRoot.up, out RaycastHit val, 100f, raycastLayers))
		{
			return val.distance;
		}
		return float.PositiveInfinity;
	}

	public override void OnMuscleReconnected(Muscle m)
	{
		base.OnMuscleReconnected(m);
		m.state.pinWeightMlp = 0f;
		m.state.muscleWeightMlp = 1f;
		m.state.muscleDamperMlp = 1f;
		m.state.maxForceMlp = 1f;
		m.state.mappingWeightMlp = 1f;
	}
}
