using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;
using Random = UnityEngine.Random;
using Object = UnityEngine.Object;

namespace VisceralCombat.Ragdolls.Classes.RootMotion.Dynamics;

public class PuppetMasterLite : MonoBehaviour
{
	public delegate void PuppetMasterLiteDelegate();

	public enum UpdateMode
	{
		Normal,
		Fixed
	}

	[CompilerGenerated]
	private sealed class _003CActivation_003Ed__21 : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public PuppetMasterLite _003C_003E4__this;

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
		public _003CActivation_003Ed__21(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			switch (_003C_003E1__state)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				if (_003C_003E4__this.blendTime <= 0f)
				{
					_003C_003E4__this.mappingWeight = 1f;
					return false;
				}
				break;
			case 1:
				_003C_003E1__state = -1;
				break;
			}
			if (_003C_003E4__this.mappingWeight < 1f)
			{
				_003C_003E4__this.mappingWeight = Mathf.MoveTowards(_003C_003E4__this.mappingWeight, 1f, Time.deltaTime * (1f / _003C_003E4__this.blendTime));
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

	[CompilerGenerated]
	private sealed class _003CDeactivation_003Ed__23 : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public PuppetMasterLite _003C_003E4__this;

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
		public _003CDeactivation_003Ed__23(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			if (num != 0)
			{
				if (num != 1)
				{
					return false;
				}
				_003C_003E1__state = -1;
			}
			else
			{
				_003C_003E1__state = -1;
				if (!(_003C_003E4__this.blendTime > 0f))
				{
					goto IL_00a0;
				}
			}
			if (_003C_003E4__this.mappingWeight > 0f)
			{
				_003C_003E4__this.mappingWeight = Mathf.MoveTowards(_003C_003E4__this.mappingWeight, 0f, Time.deltaTime * (1f / _003C_003E4__this.blendTime));
				_003C_003E2__current = null;
				_003C_003E1__state = 1;
				return true;
			}
			goto IL_00a0;
			IL_00a0:
			if (_003C_003E4__this.animatorDisabled)
			{
				((Behaviour)_003C_003E4__this.targetAnimator).enabled = true;
			}
			_003C_003E4__this.animatorDisabled = false;
			((Component)_003C_003E4__this).gameObject.SetActive(false);
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

	public Transform targetRoot;

	public bool fixTargetTransforms = true;

	public float blendTime = 0.1f;

	[Range(0f, 1f)]
	public float mappingWeight = 1f;

	[Range(0f, 1f)]
	public float pinWeight = 1f;

	[Range(0f, 1f)]
	public float muscleWeight = 1f;

	public float muscleSpring = 1000f;

	public float muscleDamper = 100f;

	public bool updateJointAnchors = true;

	public bool angularPinning;

	[LargeHeader("Individual Muscle Settings")]
	public MuscleLite[] muscles = new MuscleLite[0];

	public PuppetMasterLiteDelegate OnRead;

	public PuppetMasterLiteDelegate OnWrite;

	private Animator targetAnimator;

	private bool animatorDisabled;

	private bool fixedFrame;

	private UpdateMode updateMode = UpdateMode.Normal;

	private void Start()
	{
		Initiate();
	}

	public void Activate()
	{
		if (!((Component)this).gameObject.activeInHierarchy)
		{
			mappingWeight = 0f;
			MuscleLite[] array = muscles;
			foreach (MuscleLite muscleLite in array)
			{
				muscleLite.Reset();
			}
			((Component)this).gameObject.SetActive(true);
			MuscleLite[] array2 = muscles;
			foreach (MuscleLite muscleLite2 in array2)
			{
				muscleLite2.rigidbody.WakeUp();
				muscleLite2.MoveToTarget();
				muscleLite2.ClearVelocities();
			}
			Read();
			((MonoBehaviour)this).StopAllCoroutines();
			((MonoBehaviour)this).StartCoroutine(Activation());
		}
	}

	[IteratorStateMachine(typeof(_003CActivation_003Ed__21))]
	private IEnumerator Activation()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CActivation_003Ed__21(0)
		{
			_003C_003E4__this = this
		};
	}

	public void Deactivate()
	{
		if (((Component)this).gameObject.activeInHierarchy)
		{
			((MonoBehaviour)this).StopAllCoroutines();
			((MonoBehaviour)this).StartCoroutine(Deactivation());
		}
	}

	[IteratorStateMachine(typeof(_003CDeactivation_003Ed__23))]
	private IEnumerator Deactivation()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CDeactivation_003Ed__23(0)
		{
			_003C_003E4__this = this
		};
	}

	private void Initiate()
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Invalid comparison between Unknown and I4
		if (((Component)targetRoot).gameObject.layer == ((Component)this).gameObject.layer)
		{
			targetAnimator = ((Component)targetRoot).GetComponentInChildren<Animator>();
		}
		if ((Object)(object)targetAnimator != (Object)null && (int)targetAnimator.updateMode == 1)
		{
			updateMode = UpdateMode.Fixed;
		}
		MuscleLite[] array = muscles;
		foreach (MuscleLite muscleLite in array)
		{
			muscleLite.Initiate(muscles);
		}
	}

	private void Update()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Invalid comparison between Unknown and I4
		updateMode = ((!((Object)(object)targetAnimator == (Object)null) && (int)targetAnimator.updateMode == 1) ? UpdateMode.Fixed : UpdateMode.Normal);
		if (updateMode != UpdateMode.Fixed)
		{
			FixTargetTransforms();
		}
	}

	private void FixTargetTransforms()
	{
		if (fixTargetTransforms)
		{
			MuscleLite[] array = muscles;
			foreach (MuscleLite muscleLite in array)
			{
				muscleLite.FixTargetTransforms();
			}
		}
	}

	private void FixedUpdate()
	{
		fixedFrame = true;
		if (updateMode == UpdateMode.Fixed)
		{
			FixTargetTransforms();
			if (((Behaviour)targetAnimator).enabled || (!((Behaviour)targetAnimator).enabled && animatorDisabled))
			{
				((Behaviour)targetAnimator).enabled = false;
				animatorDisabled = true;
				targetAnimator.Update(Time.fixedDeltaTime);
			}
			else
			{
				animatorDisabled = false;
				((Behaviour)targetAnimator).enabled = false;
			}
			Read();
		}
		MuscleLite[] array = muscles;
		foreach (MuscleLite muscleLite in array)
		{
			muscleLite.Update(pinWeight, muscleWeight, muscleSpring, muscleDamper, angularPinning);
		}
	}

	private void LateUpdate()
	{
		if (animatorDisabled)
		{
			((Behaviour)targetAnimator).enabled = true;
		}
		animatorDisabled = false;
		UpdateMode updateMode = this.updateMode;
		UpdateMode updateMode2 = updateMode;
		if (updateMode2 == UpdateMode.Fixed)
		{
			if (fixedFrame)
			{
				Write();
			}
		}
		else
		{
			Read();
			Write();
		}
		fixedFrame = false;
	}

	private void Read()
	{
		if (OnRead != null)
		{
			OnRead();
		}
		MuscleLite[] array = muscles;
		foreach (MuscleLite muscleLite in array)
		{
			muscleLite.Read();
		}
		if (updateJointAnchors)
		{
			MuscleLite[] array2 = muscles;
			foreach (MuscleLite muscleLite2 in array2)
			{
				muscleLite2.UpdateAnchor(supportTranslationAnimation: true);
			}
		}
	}

	private void Write()
	{
		MuscleLite[] array = muscles;
		foreach (MuscleLite muscleLite in array)
		{
			muscleLite.Map(mappingWeight);
		}
		if (OnWrite != null)
		{
			OnWrite();
		}
	}

	private void OnDrawGizmosSelected()
	{
		if (!Application.isPlaying)
		{
			for (int i = 0; i < muscles.Length; i++)
			{
				muscles[i].name = i + ": " + (((Object)(object)muscles[i].joint != (Object)null) ? ((Object)muscles[i].joint).name : "Missing Joint reference!");
			}
		}
	}
}
