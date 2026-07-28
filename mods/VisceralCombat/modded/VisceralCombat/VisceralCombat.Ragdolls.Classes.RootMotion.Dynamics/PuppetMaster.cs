using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace VisceralCombat.Ragdolls.Classes.RootMotion.Dynamics;

[HelpURL("https://www.youtube.com/watch?v=LYusqeqHAUc")]
[AddComponentMenu("Scripts/RootMotion.Dynamics/PuppetMaster/Puppet Master")]
public class PuppetMaster : MonoBehaviour
{
	[Serializable]
	public enum Mode
	{
		Active,
		Kinematic,
		Disabled
	}

	public delegate void UpdateDelegate();

	public delegate void MuscleDelegate(Muscle muscle);

	[Serializable]
	public enum UpdateMode
	{
		Normal,
		AnimatePhysics,
		FixedUpdate
	}

	[Serializable]
	public enum State
	{
		Alive,
		Dead,
		Frozen
	}

	[Serializable]
	public struct StateSettings
	{
		[Tooltip("How much does it take to weigh out muscle weight to deadMuscleWeight?")]
		public float killDuration;

		[Tooltip("The muscle weight mlp while the puppet is Dead.")]
		public float deadMuscleWeight;

		[Tooltip("The muscle damper add while the puppet is Dead.")]
		public float deadMuscleDamper;

		[Tooltip("The max square velocity of the ragdoll bones for freezing the puppet.")]
		public float maxFreezeSqrVelocity;

		[Tooltip("If true, PuppetMaster, all its behaviours and the ragdoll will be destroyed when the puppet is frozen.")]
		public bool freezePermanently;

		[Tooltip("If true, will enable angular limits when killing the puppet.")]
		public bool enableAngularLimitsOnKill;

		[Tooltip("If true, will enable internal collisions when killing the puppet.")]
		public bool enableInternalCollisionsOnKill;

		public static StateSettings Default => new StateSettings(1f);

		public StateSettings(float killDuration, float deadMuscleWeight = 0.01f, float deadMuscleDamper = 2f, float maxFreezeSqrVelocity = 0.02f, bool freezePermanently = false, bool enableAngularLimitsOnKill = true, bool enableInternalCollisionsOnKill = true)
		{
			this.killDuration = killDuration;
			this.deadMuscleWeight = deadMuscleWeight;
			this.deadMuscleDamper = deadMuscleDamper;
			this.maxFreezeSqrVelocity = maxFreezeSqrVelocity;
			this.freezePermanently = freezePermanently;
			this.enableAngularLimitsOnKill = enableAngularLimitsOnKill;
			this.enableInternalCollisionsOnKill = enableInternalCollisionsOnKill;
		}
	}

	[CompilerGenerated]
	private sealed class _003CActiveToDisabled_003Ed__174 : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public PuppetMaster _003C_003E4__this;

		private Muscle[] _003C_003Es__1;

		private int _003C_003Es__2;

		private Muscle _003Cm_003E5__3;

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
		public _003CActiveToDisabled_003Ed__174(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003C_003Es__1 = null;
			_003Cm_003E5__3 = null;
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
					_003C_003E4__this.mappingBlend = 0f;
					goto IL_00af;
				}
			}
			if (_003C_003E4__this.mappingBlend > 0f)
			{
				_003C_003E4__this.mappingBlend = Mathf.Max(_003C_003E4__this.mappingBlend - Time.deltaTime / _003C_003E4__this.blendTime, 0f);
				_003C_003E2__current = null;
				_003C_003E1__state = 1;
				return true;
			}
			goto IL_00af;
			IL_00af:
			_003C_003Es__1 = _003C_003E4__this.muscles;
			for (_003C_003Es__2 = 0; _003C_003Es__2 < _003C_003Es__1.Length; _003C_003Es__2++)
			{
				_003Cm_003E5__3 = _003C_003Es__1[_003C_003Es__2];
				if (!_003Cm_003E5__3.state.isDisconnected)
				{
					((Component)_003Cm_003E5__3.rigidbody).gameObject.SetActive(false);
				}
				_003Cm_003E5__3 = null;
			}
			_003C_003Es__1 = null;
			_003C_003E4__this.activeMode = Mode.Disabled;
			_003C_003E4__this.isSwitchingMode = false;
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
	private sealed class _003CActiveToKinematic_003Ed__175 : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public PuppetMaster _003C_003E4__this;

		private Muscle[] _003C_003Es__1;

		private int _003C_003Es__2;

		private Muscle _003Cm_003E5__3;

		private Muscle[] _003C_003Es__4;

		private int _003C_003Es__5;

		private Muscle _003Cm_003E5__6;

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
		public _003CActiveToKinematic_003Ed__175(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003C_003Es__1 = null;
			_003Cm_003E5__3 = null;
			_003C_003Es__4 = null;
			_003Cm_003E5__6 = null;
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
					_003C_003E4__this.mappingBlend = 0f;
					goto IL_00af;
				}
			}
			if (_003C_003E4__this.mappingBlend > 0f)
			{
				_003C_003E4__this.mappingBlend = Mathf.Max(_003C_003E4__this.mappingBlend - Time.deltaTime / _003C_003E4__this.blendTime, 0f);
				_003C_003E2__current = null;
				_003C_003E1__state = 1;
				return true;
			}
			goto IL_00af;
			IL_00af:
			_003C_003Es__1 = _003C_003E4__this.muscles;
			for (_003C_003Es__2 = 0; _003C_003Es__2 < _003C_003Es__1.Length; _003C_003Es__2++)
			{
				_003Cm_003E5__3 = _003C_003Es__1[_003C_003Es__2];
				if (!_003Cm_003E5__3.state.isDisconnected)
				{
					_003Cm_003E5__3.SetKinematic(to: true);
				}
				_003Cm_003E5__3 = null;
			}
			_003C_003Es__1 = null;
			_003C_003Es__4 = _003C_003E4__this.muscles;
			for (_003C_003Es__5 = 0; _003C_003Es__5 < _003C_003Es__4.Length; _003C_003Es__5++)
			{
				_003Cm_003E5__6 = _003C_003Es__4[_003C_003Es__5];
				if (!_003Cm_003E5__6.state.isDisconnected)
				{
					_003Cm_003E5__6.MoveToTarget();
				}
				_003Cm_003E5__6 = null;
			}
			_003C_003Es__4 = null;
			_003C_003E4__this.activeMode = Mode.Kinematic;
			_003C_003E4__this.isSwitchingMode = false;
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
	private sealed class _003CAliveToDead_003Ed__226 : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public bool freeze;

		public PuppetMaster _003C_003E4__this;

		private float _003Crange_003E5__1;

		private Muscle[] _003C_003Es__2;

		private int _003C_003Es__3;

		private Muscle _003Cm_003E5__4;

		private BehaviourBase[] _003C_003Es__5;

		private int _003C_003Es__6;

		private BehaviourBase _003Cbehaviour_003E5__7;

		private float _003CmW_003E5__8;

		private Muscle[] _003C_003Es__9;

		private int _003C_003Es__10;

		private Muscle _003Cm_003E5__11;

		private Muscle[] _003C_003Es__12;

		private int _003C_003Es__13;

		private Muscle _003Cm_003E5__14;

		private BehaviourBase[] _003C_003Es__15;

		private int _003C_003Es__16;

		private BehaviourBase _003Cbehaviour_003E5__17;

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
		public _003CAliveToDead_003Ed__226(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003C_003Es__2 = null;
			_003Cm_003E5__4 = null;
			_003C_003Es__5 = null;
			_003Cbehaviour_003E5__7 = null;
			_003C_003Es__9 = null;
			_003Cm_003E5__11 = null;
			_003C_003Es__12 = null;
			_003Cm_003E5__14 = null;
			_003C_003Es__15 = null;
			_003Cbehaviour_003E5__17 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			//IL_0154: Unknown result type (might be due to invalid IL or missing references)
			//IL_0170: Unknown result type (might be due to invalid IL or missing references)
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
				_003C_003E4__this.isKilling = true;
				_003C_003E4__this.mode = Mode.Active;
				if (_003C_003E4__this.stateSettings.enableAngularLimitsOnKill && !_003C_003E4__this.angularLimits)
				{
					_003C_003E4__this.angularLimits = true;
					_003C_003E4__this.angularLimitsEnabledOnKill = true;
				}
				if (_003C_003E4__this.stateSettings.enableInternalCollisionsOnKill && !_003C_003E4__this.internalCollisions)
				{
					_003C_003E4__this.internalCollisions = true;
					_003C_003E4__this.internalCollisionsEnabledOnKill = true;
				}
				_003C_003Es__2 = _003C_003E4__this.muscles;
				for (_003C_003Es__3 = 0; _003C_003Es__3 < _003C_003Es__2.Length; _003C_003Es__3++)
				{
					_003Cm_003E5__4 = _003C_003Es__2[_003C_003Es__3];
					if (!_003Cm_003E5__4.state.isDisconnected)
					{
						_003Cm_003E5__4.state.pinWeightMlp = 0f;
						_003Cm_003E5__4.state.muscleDamperAdd = _003C_003E4__this.stateSettings.deadMuscleDamper;
						_003Cm_003E5__4.rigidbody.velocity = _003Cm_003E5__4.mappedVelocity;
						_003Cm_003E5__4.rigidbody.angularVelocity = _003Cm_003E5__4.mappedAngularVelocity;
					}
					_003Cm_003E5__4 = null;
				}
				_003C_003Es__2 = null;
				_003Crange_003E5__1 = _003C_003E4__this.muscles[0].state.muscleWeightMlp - _003C_003E4__this.stateSettings.deadMuscleWeight;
				_003C_003Es__5 = _003C_003E4__this.behaviours;
				for (_003C_003Es__6 = 0; _003C_003Es__6 < _003C_003Es__5.Length; _003C_003Es__6++)
				{
					_003Cbehaviour_003E5__7 = _003C_003Es__5[_003C_003Es__6];
					_003Cbehaviour_003E5__7.KillStart();
					_003Cbehaviour_003E5__7 = null;
				}
				_003C_003Es__5 = null;
				if (!(_003C_003E4__this.stateSettings.killDuration > 0f) || !(_003Crange_003E5__1 > 0f))
				{
					goto IL_037d;
				}
				_003CmW_003E5__8 = _003C_003E4__this.muscles[0].state.muscleWeightMlp;
			}
			if (_003CmW_003E5__8 > _003C_003E4__this.stateSettings.deadMuscleWeight)
			{
				_003CmW_003E5__8 = Mathf.Max(_003CmW_003E5__8 - Time.deltaTime * (_003Crange_003E5__1 / _003C_003E4__this.stateSettings.killDuration), _003C_003E4__this.stateSettings.deadMuscleWeight);
				_003C_003Es__9 = _003C_003E4__this.muscles;
				for (_003C_003Es__10 = 0; _003C_003Es__10 < _003C_003Es__9.Length; _003C_003Es__10++)
				{
					_003Cm_003E5__11 = _003C_003Es__9[_003C_003Es__10];
					_003Cm_003E5__11.state.muscleWeightMlp = _003CmW_003E5__8;
					_003Cm_003E5__11 = null;
				}
				_003C_003Es__9 = null;
				_003C_003E2__current = null;
				_003C_003E1__state = 1;
				return true;
			}
			goto IL_037d;
			IL_037d:
			_003C_003Es__12 = _003C_003E4__this.muscles;
			for (_003C_003Es__13 = 0; _003C_003Es__13 < _003C_003Es__12.Length; _003C_003Es__13++)
			{
				_003Cm_003E5__14 = _003C_003Es__12[_003C_003Es__13];
				_003Cm_003E5__14.state.muscleWeightMlp = _003C_003E4__this.stateSettings.deadMuscleWeight;
				_003Cm_003E5__14 = null;
			}
			_003C_003Es__12 = null;
			_003C_003E4__this.SetAnimationEnabled(to: false);
			_003C_003E4__this.isKilling = false;
			_003C_003E4__this.activeState = State.Dead;
			if (freeze)
			{
				_003C_003E4__this.freezeFlag = true;
			}
			_003C_003Es__15 = _003C_003E4__this.behaviours;
			for (_003C_003Es__16 = 0; _003C_003Es__16 < _003C_003Es__15.Length; _003C_003Es__16++)
			{
				_003Cbehaviour_003E5__17 = _003C_003Es__15[_003C_003Es__16];
				_003Cbehaviour_003E5__17.KillEnd();
				_003Cbehaviour_003E5__17 = null;
			}
			_003C_003Es__15 = null;
			if (_003C_003E4__this.OnDeath != null)
			{
				_003C_003E4__this.OnDeath();
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
	private sealed class _003CDisabledToActive_003Ed__171 : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public PuppetMaster _003C_003E4__this;

		private Muscle[] _003C_003Es__1;

		private int _003C_003Es__2;

		private Muscle _003Cm_003E5__3;

		private Muscle[] _003C_003Es__4;

		private int _003C_003Es__5;

		private Muscle _003Cm_003E5__6;

		private Muscle[] _003C_003Es__7;

		private int _003C_003Es__8;

		private Muscle _003Cm_003E5__9;

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
		public _003CDisabledToActive_003Ed__171(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003C_003Es__1 = null;
			_003Cm_003E5__3 = null;
			_003C_003Es__4 = null;
			_003Cm_003E5__6 = null;
			_003C_003Es__7 = null;
			_003Cm_003E5__9 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			//IL_0132: Unknown result type (might be due to invalid IL or missing references)
			//IL_014e: Unknown result type (might be due to invalid IL or missing references)
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
				_003C_003Es__1 = _003C_003E4__this.muscles;
				for (_003C_003Es__2 = 0; _003C_003Es__2 < _003C_003Es__1.Length; _003C_003Es__2++)
				{
					_003Cm_003E5__3 = _003C_003Es__1[_003C_003Es__2];
					if (!_003Cm_003E5__3.state.isDisconnected)
					{
						_003Cm_003E5__3.Reset();
					}
					_003Cm_003E5__3 = null;
				}
				_003C_003Es__1 = null;
				_003C_003Es__4 = _003C_003E4__this.muscles;
				for (_003C_003Es__5 = 0; _003C_003Es__5 < _003C_003Es__4.Length; _003C_003Es__5++)
				{
					_003Cm_003E5__6 = _003C_003Es__4[_003C_003Es__5];
					if (!_003Cm_003E5__6.state.isDisconnected)
					{
						((Component)_003Cm_003E5__6.rigidbody).gameObject.SetActive(true);
						_003Cm_003E5__6.SetKinematic(to: false);
						_003Cm_003E5__6.rigidbody.WakeUp();
						_003Cm_003E5__6.rigidbody.velocity = _003Cm_003E5__6.mappedVelocity;
						_003Cm_003E5__6.rigidbody.angularVelocity = _003Cm_003E5__6.mappedAngularVelocity;
					}
					_003Cm_003E5__6 = null;
				}
				_003C_003Es__4 = null;
				_003C_003E4__this.FlagInternalCollisionsForUpdate();
				_003C_003Es__7 = _003C_003E4__this.muscles;
				for (_003C_003Es__8 = 0; _003C_003Es__8 < _003C_003Es__7.Length; _003C_003Es__8++)
				{
					_003Cm_003E5__9 = _003C_003Es__7[_003C_003Es__8];
					if (!_003Cm_003E5__9.state.isDisconnected)
					{
						_003Cm_003E5__9.MoveToTarget();
					}
					_003Cm_003E5__9 = null;
				}
				_003C_003Es__7 = null;
				_003C_003E4__this.Read();
				if (!(_003C_003E4__this.blendTime > 0f))
				{
					_003C_003E4__this.mappingBlend = 1f;
					goto IL_02b9;
				}
			}
			if (_003C_003E4__this.mappingBlend < 1f)
			{
				_003C_003E4__this.mappingBlend = Mathf.Clamp(_003C_003E4__this.mappingBlend + Time.deltaTime / _003C_003E4__this.blendTime, 0f, 1f);
				_003C_003E2__current = null;
				_003C_003E1__state = 1;
				return true;
			}
			goto IL_02b9;
			IL_02b9:
			_003C_003E4__this.activeMode = Mode.Active;
			_003C_003E4__this.isSwitchingMode = false;
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
	private sealed class _003CKinematicToActive_003Ed__173 : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public PuppetMaster _003C_003E4__this;

		private Muscle[] _003C_003Es__1;

		private int _003C_003Es__2;

		private Muscle _003Cm_003E5__3;

		private Muscle[] _003C_003Es__4;

		private int _003C_003Es__5;

		private Muscle _003Cm_003E5__6;

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
		public _003CKinematicToActive_003Ed__173(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003C_003Es__1 = null;
			_003Cm_003E5__3 = null;
			_003C_003Es__4 = null;
			_003Cm_003E5__6 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			//IL_009c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
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
				_003C_003Es__1 = _003C_003E4__this.muscles;
				for (_003C_003Es__2 = 0; _003C_003Es__2 < _003C_003Es__1.Length; _003C_003Es__2++)
				{
					_003Cm_003E5__3 = _003C_003Es__1[_003C_003Es__2];
					if (!_003Cm_003E5__3.state.isDisconnected)
					{
						_003Cm_003E5__3.SetKinematic(to: false);
						_003Cm_003E5__3.rigidbody.WakeUp();
						_003Cm_003E5__3.rigidbody.velocity = _003Cm_003E5__3.mappedVelocity;
						_003Cm_003E5__3.rigidbody.angularVelocity = _003Cm_003E5__3.mappedAngularVelocity;
					}
					_003Cm_003E5__3 = null;
				}
				_003C_003Es__1 = null;
				_003C_003Es__4 = _003C_003E4__this.muscles;
				for (_003C_003Es__5 = 0; _003C_003Es__5 < _003C_003Es__4.Length; _003C_003Es__5++)
				{
					_003Cm_003E5__6 = _003C_003Es__4[_003C_003Es__5];
					if (!_003Cm_003E5__6.state.isDisconnected)
					{
						_003Cm_003E5__6.MoveToTarget();
					}
					_003Cm_003E5__6 = null;
				}
				_003C_003Es__4 = null;
				_003C_003E4__this.Read();
				if (!(_003C_003E4__this.blendTime > 0f))
				{
					_003C_003E4__this.mappingBlend = 1f;
					goto IL_0215;
				}
			}
			if (_003C_003E4__this.mappingBlend < 1f)
			{
				_003C_003E4__this.mappingBlend = Mathf.Clamp(_003C_003E4__this.mappingBlend + Time.deltaTime / _003C_003E4__this.blendTime, 0f, 1f);
				_003C_003E2__current = null;
				_003C_003E1__state = 1;
				return true;
			}
			goto IL_0215;
			IL_0215:
			_003C_003E4__this.activeMode = Mode.Active;
			_003C_003E4__this.isSwitchingMode = false;
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

	[Tooltip("Humanoid Config allows you to easily share PuppetMaster properties, including individual muscle props between Humanoid puppets.")]
	public PuppetMasterHumanoidConfig humanoidConfig;

	public Transform targetRoot;

	[LargeHeader("Simulation")]
	[Tooltip("Sets/sets the state of the puppet (Alive, Dead or Frozen). Frozen means the ragdoll will be deactivated once it comes to stop in dead state.")]
	public State state;

	[ContextMenuItem("Reset To Default", "ResetStateSettings")]
	[Tooltip("Settings for killing and freezing the puppet.")]
	public StateSettings stateSettings = StateSettings.Default;

	[Tooltip("Active mode means all muscles are active and the character is physically simulated. Kinematic mode sets rigidbody.isKinematic to true for all the muscles and simply updates their position/rotation to match the target's. Disabled mode disables the ragdoll. Switching modes is done by simply changing this value, blending in/out will be handled automatically by the PuppetMaster.")]
	public Mode mode;

	[Tooltip("The time of blending when switching from Active to Kinematic/Disabled or from Kinematic/Disabled to Active. Switching from Kinematic to Disabled or vice versa will be done instantly.")]
	public float blendTime = 0.1f;

	[Tooltip("If true, will fix the target character's Transforms to their default local positions and rotations in each update cycle to avoid drifting from additive reading-writing. Use this only if the target contains unanimated bones.")]
	public bool fixTargetTransforms = true;

	[Tooltip("Rigidbody.solverIterationCount for the muscles of this Puppet.")]
	public int solverIterationCount = 6;

	[Tooltip("If true, will draw the target's pose as green lines in the Scene view. This runs in the Editor only. If you wish to profile PuppetMaster, switch this off.")]
	public bool visualizeTargetPose = true;

	[LargeHeader("Master Weights")]
	[Tooltip("The weight of mapping the animated character to the ragdoll pose.")]
	[Range(0f, 1f)]
	public float mappingWeight = 1f;

	[Tooltip("The weight of pinning the muscles to the position of their animated targets using simple AddForce.")]
	[Range(0f, 1f)]
	public float pinWeight = 1f;

	[Tooltip("The normalized strength of the muscles.")]
	[Range(0f, 1f)]
	public float muscleWeight = 1f;

	[LargeHeader("Joint and Muscle Settings")]
	[Tooltip("The positionSpring of the ConfigurableJoints' Slerp Drive.")]
	public float muscleSpring = 100f;

	[Tooltip("The positionDamper of the ConfigurableJoints' Slerp Drive.")]
	public float muscleDamper = 0f;

	[Tooltip("Adjusts the slope of the pinWeight curve. Has effect only while interpolating pinWeight from 0 to 1 and back.")]
	[Range(1f, 8f)]
	public float pinPow = 4f;

	[Tooltip("Reduces pinning force the farther away the target is. Bigger value loosens the pinning, resulting in sloppier behaviour.")]
	[Range(0f, 100f)]
	public float pinDistanceFalloff = 5f;

	[Tooltip("If disabled, only world space AddForce will be used to pin the ragdoll to the animation while 'Pin Weight' > 0. If enabled, AddTorque will also be used for rotational pinning. Keep it disabled if you don't see any noticeable improvement from it to avoid wasting CPU resources.")]
	public bool angularPinning;

	[Tooltip("When the target has animated bones between the muscle bones, the joint anchors need to be updated in every update cycle because the muscles' targets move relative to each other in position space. This gives much more accurate results, but is computationally expensive so consider leaving it off.")]
	public bool updateJointAnchors = true;

	[Tooltip("Enable this if any of the target's bones has translation animation.")]
	public bool supportTranslationAnimation;

	[Tooltip("Should the joints use angular limits? If the PuppetMaster fails to match the target's pose, it might be because the joint limits are too stiff and do not allow for such motion. Uncheck this to see if the limits are clamping the range of your puppet's animation. Since the joints are actuated, most PuppetMaster simulations will not actually require using joint limits at all.")]
	public bool angularLimits;

	[Tooltip("Should the muscles collide with each other? Consider leaving this off while the puppet is pinned for performance and better accuracy.  Since the joints are actuated, most PuppetMaster simulations will not actually require internal collisions at all.")]
	public bool internalCollisions;

	[LargeHeader("Individual Muscle Settings")]
	[Tooltip("The Muscles managed by this PuppetMaster.")]
	public Muscle[] muscles = new Muscle[0];

	[HideInInspector]
	public PropMuscle[] propMuscles = new PropMuscle[0];

	public UpdateDelegate OnPostInitiate;

	public UpdateDelegate OnRead;

	public UpdateDelegate OnWrite;

	public UpdateDelegate OnPostLateUpdate;

	public UpdateDelegate OnFixTransforms;

	public UpdateDelegate OnHierarchyChanged;

	public MuscleDelegate OnMuscleRemoved;

	public MuscleDelegate OnMuscleDisconnected;

	public MuscleDelegate OnMuscleReconnected;

	private Animator _targetAnimator;

	[NonSerialized]
	[HideInInspector]
	public BehaviourBase[] behaviours = new BehaviourBase[0];

	[HideInInspector]
	public List<SolverManager> solvers = new List<SolverManager>();

	[NonSerialized]
	[HideInInspector]
	public bool manualInternalCollisionControl;

	[NonSerialized]
	[HideInInspector]
	public bool manualAngularLimitControl;

	[HideInInspector]
	public bool mapDisconnectedMuscles = true;

	private bool internalCollisionsEnabled = true;

	private bool angularLimitsEnabled = true;

	private bool fixedFrame;

	private int lastSolverIterationCount;

	private bool isLegacy;

	private bool animatorDisabled;

	private bool awakeFailed;

	private bool interpolated;

	private bool freezeFlag;

	private bool hasBeenDisabled;

	private bool hierarchyIsFlat;

	private bool teleport;

	private Vector3 teleportPosition;

	private Quaternion teleportRotation = Quaternion.identity;

	private bool teleportMoveToTarget;

	private bool rebuildFlag;

	private bool onPostRebuildFlag;

	private bool[] disconnectMuscleFlags = new bool[0];

	private MuscleDisconnectMode[] muscleDisconnectModes = new MuscleDisconnectMode[0];

	private bool[] disconnectDeactivateFlags = new bool[0];

	private bool[] reconnectMuscleFlags = new bool[0];

	private Muscle[] defaultMuscles = new Muscle[0];

	private Vector3 rebuildPelvisPos;

	private Quaternion rebuildPelvisRot = Quaternion.identity;

	private float simulationDeltaTime;

	private bool readInFixedUpdate;

	private Mode activeMode;

	private Mode lastMode;

	private float mappingBlend = 1f;

	public UpdateDelegate OnFreeze;

	public UpdateDelegate OnUnfreeze;

	public UpdateDelegate OnDeath;

	public UpdateDelegate OnResurrection;

	private State activeState;

	private State lastState;

	private bool angularLimitsEnabledOnKill;

	private bool internalCollisionsEnabledOnKill;

	private bool animationDisabledbyStates;

	[HideInInspector]
	public bool storeTargetMappedState = true;

	private bool targetMappedStateStored;

	private bool targetMappedStateSampled;

	private bool sampleTargetMappedState;

	private bool hasProp;

	public Animator targetAnimator
	{
		get
		{
			if ((Object)(object)_targetAnimator == (Object)null)
			{
				_targetAnimator = ((Component)targetRoot).GetComponentInChildren<Animator>();
			}
			if ((Object)(object)_targetAnimator == (Object)null && (Object)(object)targetRoot.parent != (Object)null)
			{
				_targetAnimator = ((Component)targetRoot.parent).GetComponentInChildren<Animator>();
			}
			return _targetAnimator;
		}
		set
		{
			_targetAnimator = value;
		}
	}

	public Animation targetAnimation { get; private set; }

	public bool isActive => ((Component)this).gameObject.activeInHierarchy && initiated && (activeMode == Mode.Active || isBlending);

	public bool initiated { get; private set; }

	public UpdateMode updateMode => ((int)targetUpdateMode == 1) ? (isLegacy ? UpdateMode.AnimatePhysics : UpdateMode.FixedUpdate) : UpdateMode.Normal;

	public bool controlsAnimator => ((Behaviour)this).isActiveAndEnabled && isActive && initiated && updateMode == UpdateMode.FixedUpdate;

	public bool isBlending => isSwitchingMode || isSwitchingState;

	private bool autoSimulate => true;

	private AnimatorUpdateMode targetUpdateMode
	{
		get
		{
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)targetAnimator != (Object)null)
			{
				return targetAnimator.updateMode;
			}
			if ((Object)(object)targetAnimation != (Object)null)
			{
				return (AnimatorUpdateMode)(targetAnimation.animatePhysics ? 1 : 0);
			}
			return (AnimatorUpdateMode)0;
		}
	}

	public bool isSwitchingMode { get; private set; }

	public bool isSwitchingState => activeState != state;

	public bool isKilling { get; private set; }

	public bool isAlive => activeState == State.Alive;

	public bool isFrozen => activeState == State.Frozen;

	[ContextMenu("User Manual (Setup)")]
	private void OpenUserManualSetup()
	{
		Application.OpenURL("http://root-motion.com/puppetmasterdox/html/page4.html");
	}

	[ContextMenu("User Manual (Component)")]
	private void OpenUserManualComponent()
	{
		Application.OpenURL("http://root-motion.com/puppetmasterdox/html/page5.html");
	}

	[ContextMenu("User Manual (Performance)")]
	private void OpenUserManualPerformance()
	{
		Application.OpenURL("http://root-motion.com/puppetmasterdox/html/page8.html");
	}

	[ContextMenu("Scrpt Reference")]
	private void OpenScriptReference()
	{
		Application.OpenURL("http://root-motion.com/puppetmasterdox/html/class_root_motion_1_1_dynamics_1_1_puppet_master.html");
	}

	[ContextMenu("TUTORIAL VIDEO (SETUP)")]
	private void OpenSetupTutorial()
	{
		Application.OpenURL("https://www.youtube.com/watch?v=mIN9bxJgfOU&index=2&list=PLVxSIA1OaTOuE2SB9NUbckQ9r2hTg4mvL");
	}

	[ContextMenu("TUTORIAL VIDEO (COMPONENT)")]
	private void OpenComponentTutorial()
	{
		Application.OpenURL("https://www.youtube.com/watch?v=LYusqeqHAUc");
	}

	private void ResetStateSettings()
	{
		stateSettings = StateSettings.Default;
	}

	public void Teleport(Vector3 position, Quaternion rotation, bool moveToTarget)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		teleport = true;
		teleportPosition = position;
		teleportRotation = rotation;
		teleportMoveToTarget = moveToTarget;
		if (activeMode == Mode.Disabled)
		{
			Read();
		}
	}

	public void SetInternalCollisionsManual(bool collide, bool useInternalCollisionIgnores)
	{
		for (int i = 0; i < muscles.Length; i++)
		{
			for (int j = i; j < muscles.Length; j++)
			{
				if (i != j)
				{
					if (collide)
					{
						muscles[i].ResetInternalCollisions(muscles[j], useInternalCollisionIgnores);
					}
					else
					{
						muscles[i].IgnoreInternalCollisions(muscles[j]);
					}
				}
			}
		}
	}

	public void SetAngularLimitsManual(bool limited)
	{
		for (int i = 0; i < muscles.Length; i++)
		{
			if (!muscles[i].state.isDisconnected)
			{
				muscles[i].IgnoreAngularLimits(!limited);
			}
		}
	}

	private void OnDisable()
	{
		if (!((Component)this).gameObject.activeInHierarchy && initiated && Application.isPlaying)
		{
			Muscle[] array = muscles;
			foreach (Muscle muscle in array)
			{
				muscle.Reset();
			}
		}
		isSwitchingMode = false;
		activeState = state;
		isKilling = false;
		freezeFlag = false;
		hasBeenDisabled = true;
	}

	private void OnEnable()
	{
		if (!((Component)this).gameObject.activeInHierarchy || !initiated || !hasBeenDisabled || !Application.isPlaying)
		{
			return;
		}
		isSwitchingMode = false;
		activeMode = mode;
		lastMode = mode;
		mappingBlend = ((mode == Mode.Active) ? 1f : 0f);
		activeState = state;
		lastState = state;
		isKilling = false;
		freezeFlag = false;
		SetAnimationEnabled(state == State.Alive);
		if (state == State.Alive && (Object)(object)targetAnimator != (Object)null && ((Component)targetAnimator).gameObject.activeInHierarchy)
		{
			targetAnimator.Update(0.001f);
		}
		Muscle[] array = muscles;
		foreach (Muscle muscle in array)
		{
			muscle.state.pinWeightMlp = ((state == State.Alive) ? 1f : 0f);
			muscle.state.muscleWeightMlp = ((state == State.Alive) ? 1f : stateSettings.deadMuscleWeight);
			muscle.state.muscleDamperAdd = 0f;
		}
		if (state != State.Frozen && mode != Mode.Disabled)
		{
			ActivateRagdoll(mode == Mode.Kinematic);
			BehaviourBase[] array2 = behaviours;
			foreach (BehaviourBase behaviourBase in array2)
			{
				((Component)behaviourBase).gameObject.SetActive(true);
			}
		}
		else
		{
			Muscle[] array3 = muscles;
			foreach (Muscle muscle2 in array3)
			{
				((Component)muscle2.joint).gameObject.SetActive(false);
			}
			if (state == State.Frozen)
			{
				BehaviourBase[] array4 = behaviours;
				foreach (BehaviourBase behaviourBase2 in array4)
				{
					if (((Component)behaviourBase2).gameObject.activeSelf)
					{
						behaviourBase2.deactivated = true;
						((Component)behaviourBase2).gameObject.SetActive(false);
					}
				}
				if (stateSettings.freezePermanently)
				{
					if (behaviours.Length != 0 && (Object)(object)behaviours[0] != (Object)null)
					{
						Object.Destroy((Object)(object)((Component)((Component)behaviours[0]).transform.parent).gameObject);
					}
					Object.Destroy((Object)(object)((Component)this).gameObject);
					return;
				}
			}
		}
		BehaviourBase[] array5 = behaviours;
		foreach (BehaviourBase behaviourBase3 in array5)
		{
			behaviourBase3.OnReactivate();
		}
	}

	public void Awake()
	{
		if (muscles.Length != 0)
		{
			Initiate();
			if (!initiated)
			{
				awakeFailed = true;
			}
		}
	}

	public void Start()
	{
		if (!initiated && !awakeFailed)
		{
			Initiate();
		}
		if (initiated)
		{
			SolverManager[] componentsInChildren = ((Component)targetRoot).GetComponentsInChildren<SolverManager>();
			solvers.AddRange(componentsInChildren);
		}
	}

	public Transform FindTargetRootRecursive(Transform t)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Expected O, but got Unknown
		if ((Object)(object)t.parent == (Object)null)
		{
			return null;
		}
		foreach (Transform item in t.parent)
		{
			Transform val = item;
			if ((Object)(object)val == (Object)(object)((Component)this).transform)
			{
				return t;
			}
		}
		return FindTargetRootRecursive(t.parent);
	}

	private void Initiate()
	{
		initiated = false;
		if (muscles.Length != 0 && (Object)(object)muscles[0].target != (Object)null && (Object)(object)targetRoot == (Object)null)
		{
			targetRoot = FindTargetRootRecursive(muscles[0].target);
		}
		if ((Object)(object)targetRoot != (Object)null && (Object)(object)targetAnimator == (Object)null)
		{
			targetAnimator = ((Component)targetRoot).GetComponentInChildren<Animator>();
			if ((Object)(object)targetAnimator == (Object)null)
			{
				targetAnimation = ((Component)targetRoot).GetComponentInChildren<Animation>();
			}
		}
		if (!IsValid(log: true))
		{
			return;
		}
		if ((Object)(object)humanoidConfig != (Object)null && (Object)(object)targetAnimator != (Object)null && targetAnimator.isHuman)
		{
			humanoidConfig.ApplyTo(this);
		}
		isLegacy = (Object)(object)targetAnimator == (Object)null && (Object)(object)targetAnimation != (Object)null;
		behaviours = ((Component)((Component)this).transform).GetComponentsInChildren<BehaviourBase>();
		if (behaviours.Length == 0 && (Object)(object)((Component)this).transform.parent != (Object)null)
		{
			behaviours = ((Component)((Component)this).transform.parent).GetComponentsInChildren<BehaviourBase>();
		}
		for (int i = 0; i < muscles.Length; i++)
		{
			muscles[i].Initiate(muscles);
			if (behaviours.Length != 0)
			{
				muscles[i].broadcaster = ((Component)muscles[i].joint).gameObject.GetComponent<MuscleCollisionBroadcaster>();
				if ((Object)(object)muscles[i].broadcaster == (Object)null)
				{
					muscles[i].broadcaster = ((Component)muscles[i].joint).gameObject.AddComponent<MuscleCollisionBroadcaster>();
				}
				muscles[i].broadcaster.puppetMaster = this;
				muscles[i].broadcaster.muscleIndex = i;
			}
			muscles[i].jointBreakBroadcaster = ((Component)muscles[i].joint).gameObject.GetComponent<JointBreakBroadcaster>();
			if ((Object)(object)muscles[i].jointBreakBroadcaster == (Object)null)
			{
				muscles[i].jointBreakBroadcaster = ((Component)muscles[i].joint).gameObject.AddComponent<JointBreakBroadcaster>();
			}
			muscles[i].jointBreakBroadcaster.puppetMaster = this;
			muscles[i].jointBreakBroadcaster.muscleIndex = i;
		}
		UpdateHierarchies();
		PropMuscle[] array = propMuscles;
		foreach (PropMuscle propMuscle in array)
		{
			propMuscle.OnInitiate();
		}
		hierarchyIsFlat = HierarchyIsFlat();
		FlagInternalCollisionsForUpdate();
		FlagAngularLimitsForUpdate();
		initiated = true;
		BehaviourBase[] array2 = behaviours;
		foreach (BehaviourBase behaviourBase in array2)
		{
			behaviourBase.puppetMaster = this;
		}
		BehaviourBase[] array3 = behaviours;
		foreach (BehaviourBase behaviourBase2 in array3)
		{
			behaviourBase2.Initiate();
		}
		SwitchStates();
		SwitchModes();
		Muscle[] array4 = muscles;
		foreach (Muscle muscle in array4)
		{
			muscle.Read();
		}
		StoreTargetMappedState();
		if ((Object)(object)Singleton<PuppetMasterSettings>.instance != (Object)null)
		{
			Singleton<PuppetMasterSettings>.instance.Register(this);
		}
		bool flag = false;
		BehaviourBase[] array5 = behaviours;
		foreach (BehaviourBase behaviourBase3 in array5)
		{
			if (behaviourBase3 is BehaviourPuppet && ((Behaviour)behaviourBase3).enabled)
			{
				ActivateBehaviour(behaviourBase3);
				flag = true;
				break;
			}
		}
		if (!flag && behaviours.Length != 0)
		{
			BehaviourBase[] array6 = behaviours;
			foreach (BehaviourBase behaviourBase4 in array6)
			{
				if (((Behaviour)behaviourBase4).enabled)
				{
					ActivateBehaviour(behaviourBase4);
					break;
				}
			}
		}
		defaultMuscles = (Muscle[])muscles.Clone();
		if (OnPostInitiate != null)
		{
			OnPostInitiate();
		}
		if (!autoSimulate)
		{
			((Behaviour)this).enabled = false;
		}
	}

	private void ActivateBehaviour(BehaviourBase behaviour)
	{
		BehaviourBase[] array = behaviours;
		foreach (BehaviourBase behaviourBase in array)
		{
			((Behaviour)behaviourBase).enabled = (Object)(object)behaviourBase == (Object)(object)behaviour;
			if (((Behaviour)behaviourBase).enabled)
			{
				behaviourBase.Activate();
			}
		}
	}

	private void OnDestroy()
	{
		if ((Object)(object)Singleton<PuppetMasterSettings>.instance != (Object)null)
		{
			Singleton<PuppetMasterSettings>.instance.Unregister(this);
		}
	}

	private bool IsInterpolated()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Invalid comparison between Unknown and I4
		if (!initiated)
		{
			return false;
		}
		Muscle[] array = muscles;
		foreach (Muscle muscle in array)
		{
			if ((int)muscle.rigidbody.interpolation > 0)
			{
				return true;
			}
		}
		return false;
	}

	private void OnRebuild()
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		rebuildFlag = false;
		if (activeMode == Mode.Disabled)
		{
			return;
		}
		rebuildPelvisPos = defaultMuscles[0].target.position;
		rebuildPelvisRot = defaultMuscles[0].target.rotation;
		Muscle[] array = defaultMuscles;
		foreach (Muscle muscle in array)
		{
			muscle.Rebuild();
		}
		Muscle[] array2 = defaultMuscles;
		foreach (Muscle muscle2 in array2)
		{
			if (!ContainsJoint(muscle2.joint))
			{
				AddMuscle(muscle2.joint, muscle2.target, muscle2.rebuildConnectedBody, muscle2.rebuildTargetParent);
			}
		}
		FlagInternalCollisionsForUpdate();
		FlagAngularLimitsForUpdate();
		BehaviourBase[] array3 = behaviours;
		foreach (BehaviourBase behaviourBase in array3)
		{
			behaviourBase.OnReactivate();
		}
		onPostRebuildFlag = true;
	}

	public void OnPreSimulate(float deltaTime)
	{
		simulationDeltaTime = deltaTime;
		BehaviourBase[] array = behaviours;
		foreach (BehaviourBase behaviourBase in array)
		{
			behaviourBase.UpdateB(deltaTime);
			behaviourBase.FixedUpdateB(deltaTime);
		}
		if (!initiated)
		{
			return;
		}
		if (rebuildFlag)
		{
			OnRebuild();
		}
		PropMuscle[] array2 = propMuscles;
		foreach (PropMuscle propMuscle in array2)
		{
			propMuscle.OnUpdate();
		}
		ProcessDisconnects();
		ProcessReconnects();
		if (muscles.Length == 0)
		{
			return;
		}
		interpolated = IsInterpolated();
		if (!isActive)
		{
			if (teleport)
			{
				Read();
			}
			return;
		}
		pinWeight = Mathf.Clamp(pinWeight, 0f, 1f);
		muscleWeight = Mathf.Clamp(muscleWeight, 0f, 1f);
		muscleSpring = Mathf.Clamp(muscleSpring, 0f, muscleSpring);
		muscleDamper = Mathf.Clamp(muscleDamper, 0f, muscleDamper);
		pinPow = Mathf.Clamp(pinPow, 1f, 8f);
		pinDistanceFalloff = Mathf.Max(pinDistanceFalloff, 0f);
		FixTargetTransforms();
		if ((Object)(object)targetAnimator != (Object)null)
		{
			if (((Behaviour)targetAnimator).enabled)
			{
				((Behaviour)targetAnimator).enabled = false;
			}
			targetAnimator.Update(deltaTime);
		}
		foreach (SolverManager solver in solvers)
		{
			if ((Object)(object)solver != (Object)null)
			{
				solver.UpdateSolverExternal();
			}
		}
		if (OnRead != null)
		{
			OnRead();
		}
		BehaviourBase[] array3 = behaviours;
		foreach (BehaviourBase behaviourBase2 in array3)
		{
			behaviourBase2.OnRead(deltaTime);
		}
		Read();
		if (!isFrozen)
		{
			UpdateInternalCollisions();
			UpdateAngularLimits();
			if (solverIterationCount != lastSolverIterationCount)
			{
				for (int l = 0; l < muscles.Length; l++)
				{
					muscles[l].rigidbody.solverIterations = solverIterationCount;
				}
				lastSolverIterationCount = solverIterationCount;
			}
			for (int m = 0; m < muscles.Length; m++)
			{
				muscles[m].Update(pinWeight, muscleWeight, muscleSpring, muscleDamper, pinPow, pinDistanceFalloff, rotationTargetChanged: true, angularPinning, deltaTime);
			}
		}
		if (updateMode == UpdateMode.AnimatePhysics)
		{
			FixTargetTransforms();
		}
	}

	public void OnPostSimulate()
	{
		//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
		BehaviourBase[] array = behaviours;
		foreach (BehaviourBase behaviourBase in array)
		{
			behaviourBase.LateUpdateB(simulationDeltaTime);
		}
		if (muscles.Length == 0)
		{
			return;
		}
		if (initiated)
		{
			SwitchStates();
			SwitchModes();
			if (!isFrozen)
			{
				mappingWeight = Mathf.Clamp(mappingWeight, 0f, 1f);
				float num = mappingWeight * mappingBlend;
				if (num > 0f)
				{
					if (isActive)
					{
						for (int j = 0; j < muscles.Length; j++)
						{
							muscles[j].Map(num);
						}
					}
				}
				else if (activeMode == Mode.Kinematic)
				{
					MoveToTarget();
				}
				BehaviourBase[] array2 = behaviours;
				foreach (BehaviourBase behaviourBase2 in array2)
				{
					behaviourBase2.OnWrite(simulationDeltaTime);
				}
				if (OnWrite != null)
				{
					OnWrite();
				}
				StoreTargetMappedState();
				Muscle[] array3 = muscles;
				foreach (Muscle muscle in array3)
				{
					muscle.CalculateMappedVelocity();
				}
			}
			if (mapDisconnectedMuscles)
			{
				for (int m = 0; m < muscles.Length; m++)
				{
					muscles[m].MapDisconnected();
				}
			}
			if (freezeFlag)
			{
				OnFreezeFlag();
			}
		}
		if (onPostRebuildFlag)
		{
			defaultMuscles[0].target.position = rebuildPelvisPos;
			defaultMuscles[0].target.rotation = rebuildPelvisRot;
			Muscle[] array4 = muscles;
			foreach (Muscle muscle2 in array4)
			{
				muscle2.MoveToTarget();
				muscle2.ClearVelocities();
			}
			onPostRebuildFlag = false;
		}
		if (OnPostLateUpdate != null)
		{
			OnPostLateUpdate();
		}
	}

	protected virtual void FixedUpdate()
	{
		BehaviourBase[] array = behaviours;
		foreach (BehaviourBase behaviourBase in array)
		{
			behaviourBase.FixedUpdateB(Time.deltaTime);
		}
		if (!initiated || !autoSimulate)
		{
			return;
		}
		if (rebuildFlag)
		{
			OnRebuild();
		}
		PropMuscle[] array2 = propMuscles;
		foreach (PropMuscle propMuscle in array2)
		{
			propMuscle.OnUpdate();
		}
		ProcessDisconnects();
		ProcessReconnects();
		if (muscles.Length == 0)
		{
			return;
		}
		interpolated = IsInterpolated();
		fixedFrame = true;
		if (!isActive)
		{
			if (teleport)
			{
				Read();
			}
			return;
		}
		pinWeight = Mathf.Clamp(pinWeight, 0f, 1f);
		muscleWeight = Mathf.Clamp(muscleWeight, 0f, 1f);
		muscleSpring = Mathf.Clamp(muscleSpring, 0f, muscleSpring);
		muscleDamper = Mathf.Clamp(muscleDamper, 0f, muscleDamper);
		pinPow = Mathf.Clamp(pinPow, 1f, 8f);
		pinDistanceFalloff = Mathf.Max(pinDistanceFalloff, 0f);
		if (updateMode == UpdateMode.FixedUpdate)
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
			foreach (SolverManager solver in solvers)
			{
				if ((Object)(object)solver != (Object)null)
				{
					solver.UpdateSolverExternal();
				}
			}
			if (OnRead != null)
			{
				OnRead();
			}
			BehaviourBase[] array3 = behaviours;
			foreach (BehaviourBase behaviourBase2 in array3)
			{
				behaviourBase2.OnRead(Time.deltaTime);
			}
			Read();
			readInFixedUpdate = true;
		}
		if (!isFrozen)
		{
			UpdateInternalCollisions();
			UpdateAngularLimits();
			if (solverIterationCount != lastSolverIterationCount)
			{
				for (int l = 0; l < muscles.Length; l++)
				{
					muscles[l].rigidbody.solverIterations = solverIterationCount;
				}
				lastSolverIterationCount = solverIterationCount;
			}
			for (int m = 0; m < muscles.Length; m++)
			{
				muscles[m].Update(pinWeight, muscleWeight, muscleSpring, muscleDamper, pinPow, pinDistanceFalloff, rotationTargetChanged: true, angularPinning, Time.fixedDeltaTime);
			}
		}
		if (updateMode == UpdateMode.AnimatePhysics)
		{
			FixTargetTransforms();
		}
	}

	protected virtual void Update()
	{
		BehaviourBase[] array = behaviours;
		foreach (BehaviourBase behaviourBase in array)
		{
			behaviourBase.UpdateB(Time.deltaTime);
		}
		if (initiated && autoSimulate && muscles.Length != 0)
		{
			if (animatorDisabled)
			{
				((Behaviour)targetAnimator).enabled = true;
				animatorDisabled = false;
			}
			if (updateMode == UpdateMode.Normal)
			{
				FixTargetTransforms();
			}
		}
	}

	protected virtual void LateUpdate()
	{
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		BehaviourBase[] array = behaviours;
		foreach (BehaviourBase behaviourBase in array)
		{
			behaviourBase.LateUpdateB(Time.deltaTime);
		}
		if (!autoSimulate || muscles.Length == 0)
		{
			return;
		}
		OnLateUpdate();
		if (onPostRebuildFlag)
		{
			defaultMuscles[0].target.position = rebuildPelvisPos;
			defaultMuscles[0].target.rotation = rebuildPelvisRot;
			Muscle[] array2 = muscles;
			foreach (Muscle muscle in array2)
			{
				muscle.MoveToTarget();
				muscle.ClearVelocities();
			}
			onPostRebuildFlag = false;
		}
		if (OnPostLateUpdate != null)
		{
			OnPostLateUpdate();
		}
	}

	protected virtual void OnLateUpdate()
	{
		if (!initiated)
		{
			return;
		}
		if (animatorDisabled)
		{
			((Behaviour)targetAnimator).enabled = true;
			animatorDisabled = false;
		}
		bool flag = updateMode == UpdateMode.Normal || (!readInFixedUpdate && fixedFrame);
		readInFixedUpdate = false;
		bool flag2 = flag && isActive;
		if (flag)
		{
			if (OnRead != null)
			{
				OnRead();
			}
			BehaviourBase[] array = behaviours;
			foreach (BehaviourBase behaviourBase in array)
			{
				behaviourBase.OnRead(Time.deltaTime);
			}
		}
		if (flag2)
		{
			Read();
		}
		SwitchStates();
		SwitchModes();
		switch (updateMode)
		{
		case UpdateMode.FixedUpdate:
			if (!fixedFrame && !interpolated)
			{
				return;
			}
			break;
		case UpdateMode.AnimatePhysics:
			if (!fixedFrame && !interpolated)
			{
				return;
			}
			break;
		}
		fixedFrame = false;
		if (!isFrozen)
		{
			mappingWeight = Mathf.Clamp(mappingWeight, 0f, 1f);
			float num = mappingWeight * mappingBlend;
			if (num > 0f)
			{
				if (isActive)
				{
					for (int j = 0; j < muscles.Length; j++)
					{
						muscles[j].Map(num);
					}
				}
			}
			else if (activeMode == Mode.Kinematic)
			{
				MoveToTarget();
			}
			BehaviourBase[] array2 = behaviours;
			foreach (BehaviourBase behaviourBase2 in array2)
			{
				behaviourBase2.OnWrite(Time.deltaTime);
			}
			if (OnWrite != null)
			{
				OnWrite();
			}
			StoreTargetMappedState();
			Muscle[] array3 = muscles;
			foreach (Muscle muscle in array3)
			{
				muscle.CalculateMappedVelocity();
			}
		}
		if (mapDisconnectedMuscles)
		{
			for (int m = 0; m < muscles.Length; m++)
			{
				muscles[m].MapDisconnected();
			}
		}
		if (freezeFlag)
		{
			OnFreezeFlag();
		}
	}

	private void MoveToTarget()
	{
		if ((Object)(object)Singleton<PuppetMasterSettings>.instance == (Object)null || ((Object)(object)Singleton<PuppetMasterSettings>.instance != (Object)null && Singleton<PuppetMasterSettings>.instance.UpdateMoveToTarget(this)))
		{
			Muscle[] array = muscles;
			foreach (Muscle muscle in array)
			{
				muscle.MoveToTarget();
			}
		}
	}

	private void Read()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Expected O, but got Unknown
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0205: Unknown result type (might be due to invalid IL or missing references)
		//IL_020a: Unknown result type (might be due to invalid IL or missing references)
		//IL_020f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0293: Unknown result type (might be due to invalid IL or missing references)
		//IL_0295: Unknown result type (might be due to invalid IL or missing references)
		//IL_0297: Unknown result type (might be due to invalid IL or missing references)
		if (teleport)
		{
			GameObject val = new GameObject();
			val.transform.position = (((Object)(object)((Component)this).transform.parent != (Object)null) ? ((Component)this).transform.parent.position : Vector3.zero);
			val.transform.rotation = (((Object)(object)((Component)this).transform.parent != (Object)null) ? ((Component)this).transform.parent.rotation : Quaternion.identity);
			Transform parent = ((Component)this).transform.parent;
			Transform parent2 = targetRoot.parent;
			((Component)this).transform.parent = val.transform;
			targetRoot.parent = val.transform;
			Vector3 position = ((Component)this).transform.parent.position;
			Quaternion val2 = QuaTools.FromToRotation(targetRoot.rotation, teleportRotation);
			((Component)this).transform.parent.rotation = val2 * ((Component)this).transform.parent.rotation;
			Vector3 val3 = teleportPosition - targetRoot.position;
			Transform parent3 = ((Component)this).transform.parent;
			parent3.position += val3;
			((Component)this).transform.parent = parent;
			targetRoot.parent = parent2;
			Object.Destroy((Object)(object)val);
			muscles[0].targetMappedPosition = position + val2 * (muscles[0].targetMappedPosition - position) + val3;
			muscles[0].targetSampledPosition = position + val2 * (muscles[0].targetSampledPosition - position) + val3;
			muscles[0].targetMappedRotation = val2 * muscles[0].targetMappedRotation;
			muscles[0].targetSampledRotation = val2 * muscles[0].targetSampledRotation;
			if (teleportMoveToTarget)
			{
				Muscle[] array = muscles;
				foreach (Muscle muscle in array)
				{
					muscle.MoveToTarget();
				}
			}
			Muscle[] array2 = muscles;
			foreach (Muscle muscle2 in array2)
			{
				muscle2.ClearVelocities();
			}
			BehaviourBase[] array3 = behaviours;
			foreach (BehaviourBase behaviourBase in array3)
			{
				behaviourBase.OnTeleport(val2, val3, position, teleportMoveToTarget);
			}
			teleport = false;
		}
		if (!isAlive)
		{
			return;
		}
		Muscle[] array4 = muscles;
		foreach (Muscle muscle3 in array4)
		{
			muscle3.Read();
		}
		if (isAlive && updateJointAnchors)
		{
			for (int m = 0; m < muscles.Length; m++)
			{
				muscles[m].UpdateAnchor(supportTranslationAnimation);
			}
		}
	}

	private void FixTargetTransforms()
	{
		if (!isAlive)
		{
			return;
		}
		if (OnFixTransforms != null)
		{
			OnFixTransforms();
		}
		BehaviourBase[] array = behaviours;
		foreach (BehaviourBase behaviourBase in array)
		{
			behaviourBase.OnFixTransforms();
		}
		if ((!fixTargetTransforms && !hasProp) || !isActive)
		{
			return;
		}
		mappingWeight = Mathf.Clamp(mappingWeight, 0f, 1f);
		float num = mappingWeight * mappingBlend;
		if (num <= 0f)
		{
			return;
		}
		for (int j = 0; j < muscles.Length; j++)
		{
			if (fixTargetTransforms || muscles[j].props.group == Muscle.Group.Prop)
			{
				muscles[j].FixTargetTransforms();
			}
		}
	}

	private void VisualizeTargetPose()
	{
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		if (!visualizeTargetPose || !Application.isEditor || !isActive)
		{
			return;
		}
		Muscle[] array = muscles;
		foreach (Muscle muscle in array)
		{
			if (!((Object)(object)((Joint)muscle.joint).connectedBody != (Object)null) || !((Object)(object)muscle.connectedBodyTarget != (Object)null))
			{
				continue;
			}
			bool flag = true;
			Muscle[] array2 = muscles;
			foreach (Muscle muscle2 in array2)
			{
				if (muscle != muscle2 && (Object)(object)((Joint)muscle2.joint).connectedBody == (Object)(object)muscle.rigidbody)
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				VisualizeHierarchy(muscle.target, Color.cyan);
			}
		}
	}

	private void VisualizeHierarchy(Transform t, Color color)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < t.childCount; i++)
		{
			VisualizeHierarchy(t.GetChild(i), color);
		}
	}

	public void FlagInternalCollisionsForUpdate()
	{
		if (!manualInternalCollisionControl)
		{
			internalCollisionsEnabled = !internalCollisions;
		}
	}

	private void UpdateInternalCollisions()
	{
		if (!manualInternalCollisionControl && internalCollisionsEnabled != internalCollisions)
		{
			if (internalCollisions)
			{
				ResetInternalCollisions();
			}
			else
			{
				IgnoreInternalCollisions();
			}
		}
	}

	public void UpdateInternalCollisions(Muscle m)
	{
		if (manualInternalCollisionControl)
		{
			return;
		}
		Muscle[] array = muscles;
		foreach (Muscle muscle in array)
		{
			if (muscle != m)
			{
				if (internalCollisions)
				{
					m.ResetInternalCollisions(muscle, useInternalCollisionIgnores: true);
				}
				else
				{
					m.IgnoreInternalCollisions(muscle);
				}
			}
		}
	}

	private void IgnoreInternalCollisions()
	{
		if (manualInternalCollisionControl)
		{
			return;
		}
		for (int i = 0; i < muscles.Length; i++)
		{
			for (int j = i; j < muscles.Length; j++)
			{
				if (i != j)
				{
					muscles[i].IgnoreInternalCollisions(muscles[j]);
				}
			}
		}
		internalCollisions = false;
		internalCollisionsEnabled = false;
	}

	public void IgnoreInternalCollisions(Muscle m)
	{
		if (manualInternalCollisionControl)
		{
			return;
		}
		Muscle[] array = muscles;
		foreach (Muscle muscle in array)
		{
			if (muscle != m)
			{
				m.IgnoreInternalCollisions(muscle);
			}
		}
	}

	private void ResetInternalCollisions()
	{
		if (manualInternalCollisionControl)
		{
			return;
		}
		for (int i = 0; i < muscles.Length; i++)
		{
			for (int j = i; j < muscles.Length; j++)
			{
				if (i != j)
				{
					muscles[i].ResetInternalCollisions(muscles[j], useInternalCollisionIgnores: true);
				}
			}
		}
		internalCollisions = true;
		internalCollisionsEnabled = true;
	}

	public void ResetInternalCollisions(Muscle m, bool useInternalCollisionIgnores)
	{
		if (manualInternalCollisionControl)
		{
			return;
		}
		Muscle[] array = muscles;
		foreach (Muscle muscle in array)
		{
			if (muscle != m)
			{
				m.ResetInternalCollisions(muscle, useInternalCollisionIgnores);
			}
		}
	}

	public void FlagAngularLimitsForUpdate()
	{
		if (!manualAngularLimitControl)
		{
			angularLimitsEnabled = !angularLimits;
		}
	}

	private void UpdateAngularLimits()
	{
		if (manualAngularLimitControl || angularLimitsEnabled == angularLimits)
		{
			return;
		}
		for (int i = 0; i < muscles.Length; i++)
		{
			if (!muscles[i].state.isDisconnected)
			{
				muscles[i].IgnoreAngularLimits(!angularLimits);
			}
		}
		angularLimitsEnabled = angularLimits;
	}

	public bool AddPropMuscle(ConfigurableJoint addPropMuscleTo, Vector3 position, Quaternion rotation, Vector3 additionalPinOffset, Transform targetParent = null, PuppetMasterProp initiateWithProp = null)
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Expected O, but got Unknown
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Expected O, but got Unknown
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		if (!initiated)
		{
			return false;
		}
		if ((Object)(object)addPropMuscleTo != (Object)null)
		{
			bool flag = HierarchyIsFlat();
			Muscle muscle = GetMuscle(addPropMuscleTo);
			if (muscle != null)
			{
				GameObject val = new GameObject("Prop Muscle " + ((Object)addPropMuscleTo).name);
				val.layer = ((Component)addPropMuscleTo).gameObject.layer;
				val.transform.parent = (flag ? ((Component)this).transform : ((Component)addPropMuscleTo).transform);
				val.transform.position = position;
				val.transform.rotation = rotation;
				val.AddComponent<Rigidbody>();
				GameObject val2 = new GameObject("Prop Muscle Target " + ((Object)addPropMuscleTo).name);
				val2.gameObject.layer = ((Component)muscle.target).gameObject.layer;
				val2.transform.parent = (((Object)(object)targetParent != (Object)null) ? targetParent : muscle.target);
				val2.transform.position = val.transform.position;
				val2.transform.rotation = val.transform.rotation;
				ConfigurableJoint val3 = val.AddComponent<ConfigurableJoint>();
				val3.xMotion = (ConfigurableJointMotion)0;
				val3.yMotion = (ConfigurableJointMotion)0;
				val3.zMotion = (ConfigurableJointMotion)0;
				val3.angularXMotion = (ConfigurableJointMotion)0;
				val3.angularYMotion = (ConfigurableJointMotion)0;
				val3.angularZMotion = (ConfigurableJointMotion)0;
				Muscle.Props props = new Muscle.Props();
				props.group = Muscle.Group.Prop;
				AddMuscle(val3, val2.transform, ((Component)addPropMuscleTo).GetComponent<Rigidbody>(), ((Object)(object)targetParent != (Object)null) ? targetParent : muscle.target, props);
				muscles[muscles.Length - 1].isPropMuscle = true;
				PropMuscle propMuscle = val.AddComponent<PropMuscle>();
				propMuscle.puppetMaster = this;
				propMuscle.additionalPinOffset = additionalPinOffset;
				propMuscle.currentProp = initiateWithProp;
				if (additionalPinOffset != Vector3.zero)
				{
					propMuscle.AddAdditionalPin();
				}
				Array.Resize(ref propMuscles, propMuscles.Length + 1);
				propMuscles[propMuscles.Length - 1] = propMuscle;
				propMuscle.OnInitiate();
				return true;
			}
			return false;
		}
		return false;
	}

	public bool IsDisconnecting(int muscleIndex)
	{
		return disconnectMuscleFlags[muscleIndex];
	}

	public bool IsReconnecting(int muscleIndex)
	{
		return reconnectMuscleFlags[muscleIndex];
	}

	public void DisconnectMuscleRecursive(int index, MuscleDisconnectMode disconnectMode = MuscleDisconnectMode.Sever, bool deactivate = false)
	{
		if (index >= 0 && index < muscles.Length)
		{
			disconnectMuscleFlags[index] = true;
			muscleDisconnectModes[index] = disconnectMode;
			disconnectDeactivateFlags[index] = deactivate;
		}
	}

	public void ReconnectMuscleRecursive(int index)
	{
		if (index < 0 || index >= muscles.Length)
		{
			return;
		}
		if (index > 0)
		{
			index = GetHighestDisconnectedParentIndex(index);
		}
		reconnectMuscleFlags[index] = true;
		if (muscles[index].state.resetFlag)
		{
			((Component)muscles[index].joint).gameObject.SetActive(false);
		}
		for (int i = 0; i < muscles[index].childIndexes.Length; i++)
		{
			int num = muscles[index].childIndexes[i];
			if (muscles[num].state.resetFlag)
			{
				((Component)muscles[num].joint).gameObject.SetActive(false);
			}
		}
	}

	public void AddMuscle(ConfigurableJoint joint, Transform target, Rigidbody connectTo, Transform targetParent, Muscle.Props muscleProps = null, bool forceTreeHierarchy = false, bool forceLayers = true)
	{
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0212: Unknown result type (might be due to invalid IL or missing references)
		//IL_0224: Unknown result type (might be due to invalid IL or missing references)
		if (!CheckIfInitiated() || !initiated || ContainsJoint(joint) || (Object)(object)target == (Object)null || (Object)(object)connectTo == (Object)(object)((Component)joint).GetComponent<Rigidbody>() || activeMode == Mode.Disabled)
		{
			return;
		}
		if (muscleProps == null)
		{
			muscleProps = new Muscle.Props();
		}
		Muscle muscle = new Muscle();
		muscle.props = muscleProps;
		muscle.joint = joint;
		muscle.target = target;
		((Component)muscle.joint).transform.parent = (((hierarchyIsFlat || (Object)(object)connectTo == (Object)null) && !forceTreeHierarchy) ? ((Component)this).transform : ((Component)connectTo).transform);
		AnimationBlocker component = ((Component)target).GetComponent<AnimationBlocker>();
		if ((Object)(object)component != (Object)null)
		{
			Object.Destroy((Object)(object)component);
		}
		if (forceLayers)
		{
			((Component)joint).gameObject.layer = ((Component)this).gameObject.layer;
			((Component)target).gameObject.layer = ((Component)targetRoot).gameObject.layer;
		}
		if ((Object)(object)connectTo != (Object)null)
		{
			muscle.target.parent = targetParent;
			Vector3 val = GetMuscle(connectTo).transform.InverseTransformPoint(muscle.target.position);
			Quaternion val2 = Quaternion.Inverse(GetMuscle(connectTo).transform.rotation) * muscle.target.rotation;
			((Component)joint).transform.position = ((Component)connectTo).transform.TransformPoint(val);
			((Component)joint).transform.rotation = ((Component)connectTo).transform.rotation * val2;
			((Joint)joint).connectedBody = connectTo;
			joint.xMotion = (ConfigurableJointMotion)0;
			joint.yMotion = (ConfigurableJointMotion)0;
			joint.zMotion = (ConfigurableJointMotion)0;
		}
		muscle.Initiate(muscles);
		if ((Object)(object)connectTo != (Object)null)
		{
			muscle.rigidbody.velocity = connectTo.velocity;
			muscle.rigidbody.angularVelocity = connectTo.angularVelocity;
		}
		if (!internalCollisions)
		{
			for (int i = 0; i < muscles.Length; i++)
			{
				muscle.IgnoreInternalCollisions(muscles[i]);
			}
		}
		Array.Resize(ref muscles, muscles.Length + 1);
		muscles[muscles.Length - 1] = muscle;
		muscle.index = muscles.Length - 1;
		muscle.IgnoreAngularLimits(!angularLimits);
		if (behaviours.Length != 0)
		{
			muscle.broadcaster = ((Component)muscle.joint).gameObject.AddComponent<MuscleCollisionBroadcaster>();
			muscle.broadcaster.puppetMaster = this;
			muscle.broadcaster.muscleIndex = muscles.Length - 1;
		}
		muscle.jointBreakBroadcaster = ((Component)muscle.joint).gameObject.AddComponent<JointBreakBroadcaster>();
		muscle.jointBreakBroadcaster.puppetMaster = this;
		muscle.jointBreakBroadcaster.muscleIndex = muscles.Length - 1;
		UpdateHierarchies();
		CheckMassVariation(100f, log: true);
		BehaviourBase[] array = behaviours;
		foreach (BehaviourBase behaviourBase in array)
		{
			behaviourBase.OnMuscleAdded(muscle);
		}
	}

	public void Rebuild()
	{
		rebuildFlag = true;
	}

	public void RemoveMuscleRecursive(ConfigurableJoint joint, bool attachTarget, bool blockTargetAnimation = false, MuscleRemoveMode removeMode = MuscleRemoveMode.Sever)
	{
		//IL_03eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0410: Unknown result type (might be due to invalid IL or missing references)
		//IL_041d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0422: Unknown result type (might be due to invalid IL or missing references)
		//IL_046e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0487: Unknown result type (might be due to invalid IL or missing references)
		if (!CheckIfInitiated() || (Object)(object)joint == (Object)null || !ContainsJoint(joint))
		{
			return;
		}
		int muscleIndex = GetMuscleIndex(joint);
		Muscle[] array = new Muscle[muscles.Length - (muscles[muscleIndex].childIndexes.Length + 1)];
		int num = 0;
		for (int i = 0; i < muscles.Length; i++)
		{
			if (i != muscleIndex && !muscles[muscleIndex].childFlags[i])
			{
				array[num] = muscles[i];
				num++;
				continue;
			}
			if ((Object)(object)muscles[i].broadcaster != (Object)null)
			{
				((Behaviour)muscles[i].broadcaster).enabled = false;
				Object.Destroy((Object)(object)muscles[i].broadcaster);
			}
			if ((Object)(object)muscles[i].jointBreakBroadcaster != (Object)null)
			{
				((Behaviour)muscles[i].jointBreakBroadcaster).enabled = false;
				Object.Destroy((Object)(object)muscles[i].jointBreakBroadcaster);
			}
		}
		switch (removeMode)
		{
		case MuscleRemoveMode.Sever:
		{
			DisconnectJoint(muscles[muscleIndex].joint);
			for (int k = 0; k < muscles[muscleIndex].childIndexes.Length; k++)
			{
				KillJoint(muscles[muscles[muscleIndex].childIndexes[k]].joint);
			}
			break;
		}
		case MuscleRemoveMode.Explode:
		{
			DisconnectJoint(muscles[muscleIndex].joint);
			for (int l = 0; l < muscles[muscleIndex].childIndexes.Length; l++)
			{
				DisconnectJoint(muscles[muscles[muscleIndex].childIndexes[l]].joint);
			}
			break;
		}
		case MuscleRemoveMode.Numb:
		{
			KillJoint(muscles[muscleIndex].joint);
			for (int j = 0; j < muscles[muscleIndex].childIndexes.Length; j++)
			{
				KillJoint(muscles[muscles[muscleIndex].childIndexes[j]].joint);
			}
			break;
		}
		}
		muscles[muscleIndex].transform.parent = null;
		for (int m = 0; m < muscles[muscleIndex].childIndexes.Length; m++)
		{
			if (removeMode == MuscleRemoveMode.Explode || (Object)(object)muscles[muscles[muscleIndex].childIndexes[m]].transform.parent == (Object)(object)((Component)this).transform)
			{
				muscles[muscles[muscleIndex].childIndexes[m]].transform.parent = null;
			}
		}
		BehaviourBase[] array2 = behaviours;
		foreach (BehaviourBase behaviourBase in array2)
		{
			behaviourBase.OnMuscleRemoved(muscles[muscleIndex]);
			for (int num2 = 0; num2 < muscles[muscleIndex].childIndexes.Length; num2++)
			{
				Muscle m2 = muscles[muscles[muscleIndex].childIndexes[num2]];
				behaviourBase.OnMuscleRemoved(m2);
			}
		}
		if (attachTarget)
		{
			muscles[muscleIndex].target.parent = muscles[muscleIndex].transform;
			muscles[muscleIndex].target.position = muscles[muscleIndex].transform.position;
			muscles[muscleIndex].target.rotation = muscles[muscleIndex].transform.rotation * muscles[muscleIndex].targetRotationRelative;
			for (int num3 = 0; num3 < muscles[muscleIndex].childIndexes.Length; num3++)
			{
				Muscle muscle = muscles[muscles[muscleIndex].childIndexes[num3]];
				muscle.target.parent = muscle.transform;
				muscle.target.position = muscle.transform.position;
				muscle.target.rotation = muscle.transform.rotation;
			}
		}
		if (blockTargetAnimation)
		{
			AnimationBlocker component = ((Component)muscles[muscleIndex].target).gameObject.GetComponent<AnimationBlocker>();
			if ((Object)(object)component == (Object)null)
			{
				component = ((Component)muscles[muscleIndex].target).gameObject.AddComponent<AnimationBlocker>();
			}
			for (int num4 = 0; num4 < muscles[muscleIndex].childIndexes.Length; num4++)
			{
				Muscle muscle2 = muscles[muscles[muscleIndex].childIndexes[num4]];
				component = ((Component)muscle2.target).gameObject.GetComponent<AnimationBlocker>();
				if ((Object)(object)component == (Object)null)
				{
					component = ((Component)muscle2.target).gameObject.AddComponent<AnimationBlocker>();
				}
			}
		}
		if (OnMuscleRemoved != null)
		{
			OnMuscleRemoved(muscles[muscleIndex]);
		}
		for (int num5 = 0; num5 < muscles[muscleIndex].childIndexes.Length; num5++)
		{
			Muscle muscle3 = muscles[muscles[muscleIndex].childIndexes[num5]];
			if (OnMuscleRemoved != null)
			{
				OnMuscleRemoved(muscle3);
			}
		}
		if (!internalCollisionsEnabled)
		{
			Muscle[] array3 = array;
			foreach (Muscle muscle4 in array3)
			{
				muscle4.ResetInternalCollisions(muscles[muscleIndex], useInternalCollisionIgnores: false);
				for (int num7 = 0; num7 < muscles[muscleIndex].childIndexes.Length; num7++)
				{
					muscle4.ResetInternalCollisions(muscles[num7], useInternalCollisionIgnores: false);
				}
			}
		}
		muscles = array;
		UpdateHierarchies();
	}

	public void ReplaceMuscle(ConfigurableJoint oldJoint, ConfigurableJoint newJoint)
	{
		if (CheckIfInitiated())
		{
		}
	}

	public void SetMuscles(Muscle[] newMuscles)
	{
		if (CheckIfInitiated())
		{
		}
	}

	public void DisableMuscleRecursive(ConfigurableJoint joint)
	{
		if (CheckIfInitiated())
		{
		}
	}

	public void EnableMuscleRecursive(ConfigurableJoint joint)
	{
		if (CheckIfInitiated())
		{
		}
	}

	[ContextMenu("Flatten Muscle Hierarchy")]
	public void FlattenHierarchy()
	{
		Muscle[] array = muscles;
		foreach (Muscle muscle in array)
		{
			if ((Object)(object)muscle.joint != (Object)null)
			{
				((Component)muscle.joint).transform.parent = ((Component)this).transform;
			}
		}
		hierarchyIsFlat = true;
	}

	[ContextMenu("Tree Muscle Hierarchy")]
	public void TreeHierarchy()
	{
		Muscle[] array = muscles;
		foreach (Muscle muscle in array)
		{
			if ((Object)(object)muscle.joint != (Object)null)
			{
				((Component)muscle.joint).transform.parent = (((Object)(object)((Joint)muscle.joint).connectedBody != (Object)null) ? ((Component)((Joint)muscle.joint).connectedBody).transform : ((Component)this).transform);
			}
		}
		hierarchyIsFlat = false;
	}

	[ContextMenu("Fix Muscle Positions")]
	public void FixMusclePositions()
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		Muscle[] array = muscles;
		foreach (Muscle muscle in array)
		{
			if ((Object)(object)muscle.joint != (Object)null && (Object)(object)muscle.target != (Object)null)
			{
				((Component)muscle.joint).transform.position = muscle.target.position;
			}
		}
	}

	[ContextMenu("Fix Muscle Positions and Rotations")]
	public void FixMusclePositionsAndRotations()
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		Muscle[] array = muscles;
		foreach (Muscle muscle in array)
		{
			if ((Object)(object)muscle.joint != (Object)null && (Object)(object)muscle.target != (Object)null)
			{
				((Component)muscle.joint).transform.position = muscle.target.position;
				((Component)muscle.joint).transform.rotation = muscle.target.rotation;
			}
		}
	}

	public bool HierarchyIsFlat()
	{
		Muscle[] array = muscles;
		foreach (Muscle muscle in array)
		{
			if ((Object)(object)((Component)muscle.joint).transform.parent != (Object)(object)((Component)this).transform)
			{
				return false;
			}
		}
		return true;
	}

	private int GetHighestDisconnectedParentIndex(int index)
	{
		for (int num = muscles[index].parentIndexes.Length - 1; num > -1; num--)
		{
			int num2 = muscles[index].parentIndexes[num];
			if (muscles[num2].state.isDisconnected)
			{
				return num2;
			}
		}
		return index;
	}

	private void ProcessDisconnects()
	{
		for (int i = 0; i < disconnectMuscleFlags.Length; i++)
		{
			if (disconnectMuscleFlags[i])
			{
				OnDisconnectMuscleRecursive(i, muscleDisconnectModes[i], disconnectDeactivateFlags[i]);
			}
		}
		for (int j = 0; j < disconnectMuscleFlags.Length; j++)
		{
			disconnectMuscleFlags[j] = false;
			disconnectDeactivateFlags[j] = false;
		}
	}

	private void ProcessReconnects()
	{
		for (int i = 0; i < reconnectMuscleFlags.Length; i++)
		{
			if (reconnectMuscleFlags[i])
			{
				OnReconnectMuscleRecursive(i);
			}
		}
		for (int j = 0; j < reconnectMuscleFlags.Length; j++)
		{
			reconnectMuscleFlags[j] = false;
		}
	}

	private void OnDisconnectMuscleRecursive(int index, MuscleDisconnectMode disconnectMode = MuscleDisconnectMode.Sever, bool deactivate = false)
	{
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Invalid comparison between Unknown and I4
		if (!((Component)muscles[index].joint).gameObject.activeInHierarchy || deactivate)
		{
			muscles[index].state.resetFlag = true;
		}
		for (int i = 0; i < muscles[index].childIndexes.Length; i++)
		{
			int num = muscles[index].childIndexes[i];
			if (!((Component)muscles[num].joint).gameObject.activeInHierarchy || deactivate)
			{
				muscles[num].state.resetFlag = true;
			}
		}
		DisconnectMuscle(muscles[index], sever: true, deactivate);
		for (int j = 0; j < muscles[index].childIndexes.Length; j++)
		{
			int num2 = muscles[index].childIndexes[j];
			bool flag = disconnectMode == MuscleDisconnectMode.Sever && muscles[num2].state.isDisconnected;
			if (disconnectMode == MuscleDisconnectMode.Explode && (int)muscles[num2].joint.xMotion != 2)
			{
				flag = false;
			}
			if (!flag)
			{
				DisconnectMuscle(muscles[num2], disconnectMode == MuscleDisconnectMode.Explode, deactivate);
			}
		}
		if (muscles[0].state.isDisconnected)
		{
			return;
		}
		bool flag2 = true;
		for (int k = 1; k < muscles.Length; k++)
		{
			if (!muscles[k].state.isDisconnected)
			{
				flag2 = false;
				break;
			}
			if (flag2)
			{
				DisconnectMuscleRecursive(0);
			}
		}
	}

	private void DisconnectMuscle(Muscle m, bool sever, bool deactivate)
	{
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_027e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0290: Unknown result type (might be due to invalid IL or missing references)
		m.state.pinWeightMlp = 0f;
		m.state.muscleWeightMlp = 0f;
		m.state.muscleDamperAdd = 0f;
		m.state.muscleDamperMlp = 0f;
		m.state.mappingWeightMlp = 0f;
		m.state.maxForceMlp = 0f;
		m.state.immunity = 0f;
		m.state.impulseMlp = 1f;
		if (sever)
		{
			m.joint.xMotion = (ConfigurableJointMotion)2;
			m.joint.yMotion = (ConfigurableJointMotion)2;
			m.joint.zMotion = (ConfigurableJointMotion)2;
			m.IgnoreAngularLimits(ignore: true);
			if (!hierarchyIsFlat)
			{
				((Component)m.joint).transform.parent = ((Component)this).transform;
			}
		}
		else
		{
			m.IgnoreAngularLimits(ignore: false);
		}
		bool flag = !((Component)m.joint).gameObject.activeInHierarchy || m.rigidbody.isKinematic;
		if (activeState == State.Frozen)
		{
			flag = false;
		}
		if (!((Component)m.joint).gameObject.activeInHierarchy && !deactivate)
		{
			m.MoveToTarget();
			((Component)m.joint).gameObject.SetActive(true);
		}
		m.SetKinematic(to: false);
		JointDrive slerpDrive = default(JointDrive);
		((JointDrive)(ref slerpDrive)).positionSpring = 0f;
		((JointDrive)(ref slerpDrive)).maximumForce = 0f;
		((JointDrive)(ref slerpDrive)).positionDamper = 0f;
		m.joint.slerpDrive = slerpDrive;
		if (!deactivate)
		{
			for (int i = 0; i < muscles.Length; i++)
			{
				if (muscles[i] == m || muscles[i].state.isDisconnected)
				{
					continue;
				}
				Collider[] colliders = m.colliders;
				foreach (Collider val in colliders)
				{
					Collider[] colliders2 = muscles[i].colliders;
					foreach (Collider val2 in colliders2)
					{
						if (val.enabled && val2.enabled)
						{
							Physics.IgnoreCollision(val, val2, false);
						}
					}
				}
			}
			if (flag)
			{
				m.rigidbody.velocity = m.mappedVelocity;
				m.rigidbody.angularVelocity = m.mappedAngularVelocity;
			}
		}
		else
		{
			((Component)m.joint).gameObject.SetActive(false);
		}
		if (m.isPropMuscle)
		{
			PropMuscle component = ((Component)m.joint).GetComponent<PropMuscle>();
			if ((Object)(object)component.activeProp != (Object)null)
			{
				component.currentProp = null;
			}
		}
		m.state.isDisconnected = true;
		BehaviourBase[] array = behaviours;
		foreach (BehaviourBase behaviourBase in array)
		{
			behaviourBase.OnMuscleDisconnected(m);
		}
		if (OnMuscleDisconnected != null)
		{
			OnMuscleDisconnected(m);
		}
	}

	private void OnReconnectMuscleRecursive(int index)
	{
		if (index == 0)
		{
			state = State.Alive;
			Muscle[] array = muscles;
			foreach (Muscle muscle in array)
			{
				if (!muscle.isPropMuscle)
				{
					muscle.state.isDisconnected = false;
					muscle.FixTargetTransforms();
				}
			}
			Muscle[] array2 = muscles;
			foreach (Muscle muscle2 in array2)
			{
				if (!muscle2.isPropMuscle)
				{
					muscle2.Reset();
					muscle2.Read();
					muscle2.ClearVelocities();
				}
			}
		}
		ReconnectMuscle(muscles[index]);
		for (int k = 0; k < muscles[index].childIndexes.Length; k++)
		{
			int num = muscles[index].childIndexes[k];
			if (!muscles[num].isPropMuscle)
			{
				ReconnectMuscle(muscles[num]);
			}
		}
	}

	private void ReconnectMuscle(Muscle m)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		m.state.isDisconnected = false;
		if (activeState != State.Frozen && !m.isPropMuscle)
		{
			m.target.position = m.targetAnimatedPosition;
			m.target.rotation = m.targetAnimatedWorldRotation;
		}
		if (m != muscles[0])
		{
			m.joint.xMotion = (ConfigurableJointMotion)0;
			m.joint.yMotion = (ConfigurableJointMotion)0;
			m.joint.zMotion = (ConfigurableJointMotion)0;
			if (!hierarchyIsFlat && (Object)(object)((Joint)m.joint).connectedBody != (Object)null)
			{
				m.transform.parent = ((Component)((Joint)m.joint).connectedBody).transform;
			}
		}
		bool flag = false;
		if ((Object)(object)((Joint)m.joint).connectedBody != (Object)null && !((Component)((Joint)m.joint).connectedBody).gameObject.activeInHierarchy)
		{
			flag = true;
		}
		if ((Object)(object)((Joint)m.joint).connectedBody == (Object)null && (activeMode == Mode.Disabled || activeState == State.Frozen))
		{
			flag = true;
		}
		if (flag)
		{
			((Component)m.joint).gameObject.SetActive(false);
		}
		else if (!((Component)m.joint).gameObject.activeInHierarchy || m.state.resetFlag)
		{
			m.Reset();
			((Component)m.joint).gameObject.SetActive(true);
		}
		else if (activeState != State.Frozen)
		{
			m.MoveToTarget();
		}
		if (activeMode == Mode.Kinematic)
		{
			m.SetKinematic(to: true);
		}
		if (activeState == State.Dead)
		{
			m.ResetTargetLocalPosition();
			m.SetMuscleRotation(muscleWeight * stateSettings.deadMuscleWeight, muscleSpring, muscleDamper + stateSettings.deadMuscleDamper);
		}
		m.state.resetFlag = false;
		m.ClearVelocities();
		m.state.pinWeightMlp = 1f;
		m.state.muscleWeightMlp = 1f;
		m.state.muscleDamperMlp = 1f;
		m.state.maxForceMlp = 1f;
		m.state.mappingWeightMlp = 1f;
		UpdateInternalCollisions(m);
		m.IgnoreAngularLimits(!angularLimits);
		BehaviourBase[] array = behaviours;
		foreach (BehaviourBase behaviourBase in array)
		{
			behaviourBase.OnMuscleReconnected(m);
		}
		if (OnMuscleReconnected != null)
		{
			OnMuscleReconnected(m);
		}
	}

	private void AddIndexesRecursive(int index, ref int[] indexes)
	{
		int num = indexes.Length;
		Array.Resize(ref indexes, indexes.Length + 1 + muscles[index].childIndexes.Length);
		indexes[num] = index;
		if (muscles[index].childIndexes.Length != 0)
		{
			for (int i = 0; i < muscles[index].childIndexes.Length; i++)
			{
				AddIndexesRecursive(muscles[index].childIndexes[i], ref indexes);
			}
		}
	}

	private void DisconnectJoint(ConfigurableJoint joint)
	{
		if (mode == Mode.Disabled)
		{
			((Component)joint).gameObject.SetActive(true);
		}
		((Joint)joint).connectedBody = null;
		KillJoint(joint);
		joint.xMotion = (ConfigurableJointMotion)2;
		joint.yMotion = (ConfigurableJointMotion)2;
		joint.zMotion = (ConfigurableJointMotion)2;
		joint.angularXMotion = (ConfigurableJointMotion)2;
		joint.angularYMotion = (ConfigurableJointMotion)2;
		joint.angularZMotion = (ConfigurableJointMotion)2;
	}

	private void KillJoint(ConfigurableJoint joint)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		joint.targetRotation = Quaternion.identity;
		JointDrive slerpDrive = default(JointDrive);
		((JointDrive)(ref slerpDrive)).positionSpring = 0f;
		((JointDrive)(ref slerpDrive)).positionDamper = 0f;
		joint.slerpDrive = slerpDrive;
	}

	public void SwitchToActiveMode()
	{
		mode = Mode.Active;
	}

	public void SwitchToKinematicMode()
	{
		mode = Mode.Kinematic;
	}

	public void SwitchToDisabledMode()
	{
		mode = Mode.Disabled;
	}

	public void DisableImmediately()
	{
		mappingBlend = 0f;
		isSwitchingMode = false;
		mode = Mode.Disabled;
		activeMode = mode;
		lastMode = mode;
		Muscle[] array = muscles;
		foreach (Muscle muscle in array)
		{
			((Component)muscle.rigidbody).gameObject.SetActive(false);
		}
	}

	protected virtual void SwitchModes()
	{
		if (!initiated)
		{
			return;
		}
		if (isKilling)
		{
			mode = Mode.Active;
		}
		if (!isAlive)
		{
			mode = Mode.Active;
		}
		BehaviourBase[] array = behaviours;
		foreach (BehaviourBase behaviourBase in array)
		{
			if (behaviourBase.forceActive)
			{
				mode = Mode.Active;
				break;
			}
		}
		if (mode == lastMode || isSwitchingMode || (isKilling && mode != 0) || (state != 0 && mode != 0))
		{
			return;
		}
		isSwitchingMode = true;
		if (lastMode == Mode.Disabled)
		{
			if (mode == Mode.Kinematic)
			{
				DisabledToKinematic();
			}
			else if (mode == Mode.Active)
			{
				((MonoBehaviour)this).StartCoroutine(DisabledToActive());
			}
		}
		else if (lastMode == Mode.Kinematic)
		{
			if (mode == Mode.Disabled)
			{
				KinematicToDisabled();
			}
			else if (mode == Mode.Active)
			{
				((MonoBehaviour)this).StartCoroutine(KinematicToActive());
			}
		}
		else if (lastMode == Mode.Active)
		{
			if (mode == Mode.Disabled)
			{
				((MonoBehaviour)this).StartCoroutine(ActiveToDisabled());
			}
			else if (mode == Mode.Kinematic)
			{
				((MonoBehaviour)this).StartCoroutine(ActiveToKinematic());
			}
		}
		lastMode = mode;
	}

	private void DisabledToKinematic()
	{
		Muscle[] array = muscles;
		foreach (Muscle muscle in array)
		{
			if (!muscle.state.isDisconnected)
			{
				muscle.Reset();
			}
		}
		Muscle[] array2 = muscles;
		foreach (Muscle muscle2 in array2)
		{
			if (!muscle2.state.isDisconnected)
			{
				((Component)muscle2.rigidbody).gameObject.SetActive(true);
				muscle2.SetKinematic(to: true);
			}
		}
		FlagInternalCollisionsForUpdate();
		Muscle[] array3 = muscles;
		foreach (Muscle muscle3 in array3)
		{
			if (!muscle3.state.isDisconnected)
			{
				muscle3.MoveToTarget();
			}
		}
		activeMode = Mode.Kinematic;
		isSwitchingMode = false;
	}

	[IteratorStateMachine(typeof(_003CDisabledToActive_003Ed__171))]
	private IEnumerator DisabledToActive()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CDisabledToActive_003Ed__171(0)
		{
			_003C_003E4__this = this
		};
	}

	private void KinematicToDisabled()
	{
		Muscle[] array = muscles;
		foreach (Muscle muscle in array)
		{
			if (!muscle.state.isDisconnected)
			{
				((Component)muscle.rigidbody).gameObject.SetActive(false);
			}
		}
		activeMode = Mode.Disabled;
		isSwitchingMode = false;
	}

	[IteratorStateMachine(typeof(_003CKinematicToActive_003Ed__173))]
	private IEnumerator KinematicToActive()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CKinematicToActive_003Ed__173(0)
		{
			_003C_003E4__this = this
		};
	}

	[IteratorStateMachine(typeof(_003CActiveToDisabled_003Ed__174))]
	private IEnumerator ActiveToDisabled()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CActiveToDisabled_003Ed__174(0)
		{
			_003C_003E4__this = this
		};
	}

	[IteratorStateMachine(typeof(_003CActiveToKinematic_003Ed__175))]
	private IEnumerator ActiveToKinematic()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CActiveToKinematic_003Ed__175(0)
		{
			_003C_003E4__this = this
		};
	}

	public void SetMuscleWeights(Muscle.Group group, float muscleWeight, float pinWeight = 1f, float mappingWeight = 1f, float muscleDamper = 1f)
	{
		if (!CheckIfInitiated())
		{
			return;
		}
		Muscle[] array = muscles;
		foreach (Muscle muscle in array)
		{
			if (muscle.props.group == group)
			{
				muscle.props.muscleWeight = muscleWeight;
				muscle.props.pinWeight = pinWeight;
				muscle.props.mappingWeight = mappingWeight;
				muscle.props.muscleDamper = muscleDamper;
			}
		}
	}

	public void SetMuscleWeights(Transform target, float muscleWeight, float pinWeight = 1f, float mappingWeight = 1f, float muscleDamper = 1f)
	{
		if (CheckIfInitiated())
		{
			int muscleIndex = GetMuscleIndex(target);
			if (muscleIndex != -1)
			{
				SetMuscleWeights(muscleIndex, muscleWeight, pinWeight, mappingWeight, muscleDamper);
			}
		}
	}

	public void SetMuscleWeights(HumanBodyBones humanBodyBone, float muscleWeight, float pinWeight = 1f, float mappingWeight = 1f, float muscleDamper = 1f)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		if (CheckIfInitiated())
		{
			int muscleIndex = GetMuscleIndex(humanBodyBone);
			if (muscleIndex != -1)
			{
				SetMuscleWeights(muscleIndex, muscleWeight, pinWeight, mappingWeight, muscleDamper);
			}
		}
	}

	public void SetMuscleWeightsRecursive(Transform target, float muscleWeight, float pinWeight = 1f, float mappingWeight = 1f, float muscleDamper = 1f)
	{
		if (!CheckIfInitiated())
		{
			return;
		}
		for (int i = 0; i < muscles.Length; i++)
		{
			if ((Object)(object)muscles[i].target == (Object)(object)target)
			{
				SetMuscleWeightsRecursive(i, muscleWeight, pinWeight, mappingWeight, muscleDamper);
				break;
			}
		}
	}

	public void SetMuscleWeightsRecursive(int muscleIndex, float muscleWeight, float pinWeight = 1f, float mappingWeight = 1f, float muscleDamper = 1f)
	{
		if (CheckIfInitiated())
		{
			SetMuscleWeights(muscleIndex, muscleWeight, pinWeight, mappingWeight, muscleDamper);
			for (int i = 0; i < muscles[muscleIndex].childIndexes.Length; i++)
			{
				int muscleIndex2 = muscles[muscleIndex].childIndexes[i];
				SetMuscleWeights(muscleIndex2, muscleWeight, pinWeight, mappingWeight, muscleDamper);
			}
		}
	}

	public void SetMuscleWeightsRecursive(HumanBodyBones humanBodyBone, float muscleWeight, float pinWeight = 1f, float mappingWeight = 1f, float muscleDamper = 1f)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		if (CheckIfInitiated())
		{
			int muscleIndex = GetMuscleIndex(humanBodyBone);
			if (muscleIndex != -1)
			{
				SetMuscleWeightsRecursive(muscleIndex, muscleWeight, pinWeight, mappingWeight, muscleDamper);
			}
		}
	}

	public void SetMuscleWeights(int muscleIndex, float muscleWeight, float pinWeight, float mappingWeight, float muscleDamper)
	{
		if (CheckIfInitiated() && !((float)muscleIndex < 0f) && muscleIndex < muscles.Length)
		{
			muscles[muscleIndex].props.muscleWeight = muscleWeight;
			muscles[muscleIndex].props.pinWeight = pinWeight;
			muscles[muscleIndex].props.mappingWeight = mappingWeight;
			muscles[muscleIndex].props.muscleDamper = muscleDamper;
		}
	}

	public Muscle GetMuscle(Transform target)
	{
		int muscleIndex = GetMuscleIndex(target);
		if (muscleIndex == -1)
		{
			return null;
		}
		return muscles[muscleIndex];
	}

	public Muscle GetMuscle(Rigidbody rigidbody)
	{
		int muscleIndex = GetMuscleIndex(rigidbody);
		if (muscleIndex == -1)
		{
			return null;
		}
		return muscles[muscleIndex];
	}

	public Muscle GetMuscle(ConfigurableJoint joint)
	{
		int muscleIndex = GetMuscleIndex(joint);
		if (muscleIndex == -1)
		{
			return null;
		}
		return muscles[muscleIndex];
	}

	public bool ContainsJoint(ConfigurableJoint joint)
	{
		if (!CheckIfInitiated())
		{
			return false;
		}
		Muscle[] array = muscles;
		foreach (Muscle muscle in array)
		{
			if ((Object)(object)muscle.joint == (Object)(object)joint)
			{
				return true;
			}
		}
		return false;
	}

	public int GetMuscleIndex(HumanBodyBones humanBodyBone)
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		if (!CheckIfInitiated())
		{
			return -1;
		}
		if ((Object)(object)targetAnimator == (Object)null)
		{
			return -1;
		}
		if (!targetAnimator.isHuman)
		{
			return -1;
		}
		Transform boneTransform = targetAnimator.GetBoneTransform(humanBodyBone);
		if ((Object)(object)boneTransform == (Object)null)
		{
			return -1;
		}
		return GetMuscleIndex(boneTransform);
	}

	public int GetMuscleIndex(Transform target)
	{
		if (!CheckIfInitiated())
		{
			return -1;
		}
		if ((Object)(object)target == (Object)null)
		{
			return -1;
		}
		for (int i = 0; i < muscles.Length; i++)
		{
			if ((Object)(object)muscles[i].target == (Object)(object)target)
			{
				return i;
			}
		}
		return -1;
	}

	public int GetMuscleIndex(Rigidbody rigidbody)
	{
		if (!CheckIfInitiated())
		{
			return -1;
		}
		if ((Object)(object)rigidbody == (Object)null)
		{
			return -1;
		}
		for (int i = 0; i < muscles.Length; i++)
		{
			if ((Object)(object)muscles[i].rigidbody == (Object)(object)rigidbody)
			{
				return i;
			}
		}
		return -1;
	}

	public int GetMuscleIndex(ConfigurableJoint joint)
	{
		if ((Object)(object)joint == (Object)null)
		{
			return -1;
		}
		for (int i = 0; i < muscles.Length; i++)
		{
			if ((Object)(object)muscles[i].joint == (Object)(object)joint)
			{
				return i;
			}
		}
		return -1;
	}

	public static PuppetMaster SetUp(Transform target, Transform ragdoll, int characterControllerLayer, int ragdollLayer)
	{
		if ((Object)(object)ragdoll != (Object)(object)target)
		{
			PuppetMaster puppetMaster = ((Component)ragdoll).gameObject.AddComponent<PuppetMaster>();
			puppetMaster.SetUpTo(target, characterControllerLayer, ragdollLayer);
			return puppetMaster;
		}
		return SetUp(ragdoll, characterControllerLayer, ragdollLayer);
	}

	public static PuppetMaster SetUp(Transform target, int characterControllerLayer, int ragdollLayer)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		Transform transform = Object.Instantiate<GameObject>(((Component)target).gameObject, target.position, target.rotation).transform;
		PuppetMaster puppetMaster = ((Component)transform).gameObject.AddComponent<PuppetMaster>();
		puppetMaster.SetUpTo(target, characterControllerLayer, ragdollLayer);
		RemoveRagdollComponents(target, characterControllerLayer);
		return puppetMaster;
	}

	public void SetUpTo(Transform setUpTo, int characterControllerLayer, int ragdollLayer)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0268: Unknown result type (might be due to invalid IL or missing references)
		//IL_028c: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02df: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_030d: Unknown result type (might be due to invalid IL or missing references)
		//IL_031f: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)setUpTo == (Object)null)
		{
			return;
		}
		if ((Object)(object)setUpTo == (Object)(object)((Component)this).transform)
		{
			setUpTo = Object.Instantiate<GameObject>(((Component)setUpTo).gameObject, setUpTo.position, setUpTo.rotation).transform;
			((Object)setUpTo).name = ((Object)this).name;
			RemoveRagdollComponents(setUpTo, characterControllerLayer);
		}
		RemoveUnnecessaryBones();
		Component[] componentsInChildren = ((Component)this).GetComponentsInChildren<Component>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (!(componentsInChildren[i] is PuppetMaster) && !(componentsInChildren[i] is Transform) && !(componentsInChildren[i] is Rigidbody) && !(componentsInChildren[i] is BoxCollider) && !(componentsInChildren[i] is CapsuleCollider) && !(componentsInChildren[i] is SphereCollider) && !(componentsInChildren[i] is MeshCollider) && !(componentsInChildren[i] is Joint) && !(componentsInChildren[i] is Animator))
			{
				Object.DestroyImmediate((Object)(object)componentsInChildren[i]);
			}
		}
		Animator[] componentsInChildren2 = ((Component)this).GetComponentsInChildren<Animator>();
		for (int j = 0; j < componentsInChildren2.Length; j++)
		{
			Object.DestroyImmediate((Object)(object)componentsInChildren2[j]);
		}
		componentsInChildren = ((Component)((Component)this).transform).GetComponents<Component>();
		for (int k = 0; k < componentsInChildren.Length; k++)
		{
			if (!(componentsInChildren[k] is PuppetMaster) && !(componentsInChildren[k] is Transform))
			{
				Object.DestroyImmediate((Object)(object)componentsInChildren[k]);
			}
		}
		Rigidbody[] componentsInChildren3 = ((Component)((Component)this).transform).GetComponentsInChildren<Rigidbody>();
		Rigidbody[] array = componentsInChildren3;
		foreach (Rigidbody val in array)
		{
			if ((Object)(object)((Component)val).transform != (Object)(object)((Component)this).transform && (Object)(object)((Component)val).GetComponent<ConfigurableJoint>() == (Object)null)
			{
				((Component)val).gameObject.AddComponent<ConfigurableJoint>();
			}
		}
		targetRoot = setUpTo;
		SetUpMuscles(setUpTo);
		((Object)this).name = "PuppetMaster";
		Transform val2 = (((Object)(object)setUpTo.parent == (Object)null || (Object)(object)setUpTo.parent != (Object)(object)((Component)this).transform.parent || ((Object)setUpTo.parent).name != ((Object)setUpTo).name + " Root") ? new GameObject(((Object)setUpTo).name + " Root").transform : setUpTo.parent);
		val2.parent = ((Component)this).transform.parent;
		Transform transform = new GameObject("Behaviours").transform;
		Comments comments = ((Component)transform).gameObject.GetComponent<Comments>();
		if ((Object)(object)comments == (Object)null)
		{
			comments = ((Component)transform).gameObject.AddComponent<Comments>();
		}
		comments.text = "All Puppet Behaviours should be parented to this GameObject, the PuppetMaster will automatically find them from here. All Puppet Behaviours have been designed so that they could be simply copied from one character to another without changing any references. It is important because they contain a lot of parameters and would be otherwise tedious to set up and tweak.";
		val2.position = setUpTo.position;
		val2.rotation = setUpTo.rotation;
		transform.position = setUpTo.position;
		transform.rotation = setUpTo.rotation;
		((Component)this).transform.position = setUpTo.position;
		((Component)this).transform.rotation = setUpTo.rotation;
		transform.parent = val2;
		((Component)this).transform.parent = val2;
		setUpTo.parent = val2;
		((Component)targetRoot).gameObject.layer = characterControllerLayer;
		Transform[] componentsInChildren4 = ((Component)this).GetComponentsInChildren<Transform>();
		Transform[] array2 = componentsInChildren4;
		foreach (Transform val3 in array2)
		{
			((Component)val3).gameObject.layer = ragdollLayer;
		}
		Physics.IgnoreLayerCollision(characterControllerLayer, ragdollLayer);
	}

	public static void RemoveRagdollComponents(Transform target, int characterControllerLayer)
	{
		if ((Object)(object)target == (Object)null)
		{
			return;
		}
		Rigidbody[] componentsInChildren = ((Component)target).GetComponentsInChildren<Rigidbody>();
		Cloth[] componentsInChildren2 = ((Component)target).GetComponentsInChildren<Cloth>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (!((Object)(object)((Component)componentsInChildren[i]).gameObject != (Object)(object)((Component)target).gameObject))
			{
				continue;
			}
			Joint component = ((Component)componentsInChildren[i]).GetComponent<Joint>();
			Collider component2 = ((Component)componentsInChildren[i]).GetComponent<Collider>();
			if ((Object)(object)component != (Object)null)
			{
				Object.DestroyImmediate((Object)(object)component);
			}
			if ((Object)(object)component2 != (Object)null)
			{
				if (!IsClothCollider(component2, componentsInChildren2))
				{
					Object.DestroyImmediate((Object)(object)component2);
				}
				else
				{
					((Component)component2).gameObject.layer = characterControllerLayer;
				}
			}
			Object.DestroyImmediate((Object)(object)componentsInChildren[i]);
		}
		Collider[] componentsInChildren3 = ((Component)target).GetComponentsInChildren<Collider>();
		for (int j = 0; j < componentsInChildren3.Length; j++)
		{
			if ((Object)(object)((Component)componentsInChildren3[j]).transform != (Object)(object)target && !IsClothCollider(componentsInChildren3[j], componentsInChildren2))
			{
				Object.DestroyImmediate((Object)(object)componentsInChildren3[j]);
			}
		}
		PuppetMaster component3 = ((Component)target).GetComponent<PuppetMaster>();
		if ((Object)(object)component3 != (Object)null)
		{
			Object.DestroyImmediate((Object)(object)component3);
		}
	}

	public void SetUpMuscles(Transform setUpTo)
	{
		ConfigurableJoint[] componentsInChildren = ((Component)((Component)this).transform).GetComponentsInChildren<ConfigurableJoint>();
		if (componentsInChildren.Length == 0)
		{
			return;
		}
		Animator componentInChildren = ((Component)targetRoot).GetComponentInChildren<Animator>();
		Transform[] componentsInChildren2 = ((Component)setUpTo).GetComponentsInChildren<Transform>();
		muscles = new Muscle[componentsInChildren.Length];
		int num = -1;
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			muscles[i] = new Muscle();
			muscles[i].joint = componentsInChildren[i];
			muscles[i].name = ((Object)componentsInChildren[i]).name;
			muscles[i].props = new Muscle.Props(1f, 1f, 1f, 1f);
			if ((Object)(object)((Joint)muscles[i].joint).connectedBody == (Object)null && num == -1)
			{
				num = i;
			}
			Transform[] array = componentsInChildren2;
			foreach (Transform val in array)
			{
				if (((Object)val).name == ((Object)componentsInChildren[i]).name)
				{
					muscles[i].target = val;
					if ((Object)(object)componentInChildren != (Object)null)
					{
						muscles[i].props.group = FindGroup(componentInChildren, muscles[i].target);
					}
					break;
				}
			}
		}
		if (num != 0)
		{
			Muscle muscle = muscles[0];
			Muscle muscle2 = muscles[num];
			muscles[num] = muscle;
			muscles[0] = muscle2;
		}
		bool flag = true;
		Muscle[] array2 = muscles;
		foreach (Muscle muscle3 in array2)
		{
			if ((Object)(object)muscle3.target == (Object)null)
			{
			}
			if (muscle3.props.group != muscles[0].props.group)
			{
				flag = false;
			}
		}
		if (!flag)
		{
		}
	}

	private static Muscle.Group FindGroup(Animator animator, Transform t)
	{
		if (!animator.isHuman)
		{
			return Muscle.Group.Hips;
		}
		if ((Object)(object)t == (Object)(object)animator.GetBoneTransform((HumanBodyBones)8))
		{
			return Muscle.Group.Spine;
		}
		if ((Object)(object)t == (Object)(object)animator.GetBoneTransform((HumanBodyBones)10))
		{
			return Muscle.Group.Head;
		}
		if ((Object)(object)t == (Object)(object)animator.GetBoneTransform((HumanBodyBones)0))
		{
			return Muscle.Group.Hips;
		}
		if ((Object)(object)t == (Object)(object)animator.GetBoneTransform((HumanBodyBones)5))
		{
			return Muscle.Group.Foot;
		}
		if ((Object)(object)t == (Object)(object)animator.GetBoneTransform((HumanBodyBones)17))
		{
			return Muscle.Group.Hand;
		}
		if ((Object)(object)t == (Object)(object)animator.GetBoneTransform((HumanBodyBones)15))
		{
			return Muscle.Group.Arm;
		}
		if ((Object)(object)t == (Object)(object)animator.GetBoneTransform((HumanBodyBones)3))
		{
			return Muscle.Group.Leg;
		}
		if ((Object)(object)t == (Object)(object)animator.GetBoneTransform((HumanBodyBones)13))
		{
			return Muscle.Group.Arm;
		}
		if ((Object)(object)t == (Object)(object)animator.GetBoneTransform((HumanBodyBones)1))
		{
			return Muscle.Group.Leg;
		}
		if ((Object)(object)t == (Object)(object)animator.GetBoneTransform((HumanBodyBones)6))
		{
			return Muscle.Group.Foot;
		}
		if ((Object)(object)t == (Object)(object)animator.GetBoneTransform((HumanBodyBones)18))
		{
			return Muscle.Group.Hand;
		}
		if ((Object)(object)t == (Object)(object)animator.GetBoneTransform((HumanBodyBones)16))
		{
			return Muscle.Group.Arm;
		}
		if ((Object)(object)t == (Object)(object)animator.GetBoneTransform((HumanBodyBones)4))
		{
			return Muscle.Group.Leg;
		}
		if ((Object)(object)t == (Object)(object)animator.GetBoneTransform((HumanBodyBones)14))
		{
			return Muscle.Group.Arm;
		}
		if ((Object)(object)t == (Object)(object)animator.GetBoneTransform((HumanBodyBones)2))
		{
			return Muscle.Group.Leg;
		}
		return Muscle.Group.Spine;
	}

	private void RemoveUnnecessaryBones()
	{
		Transform[] componentsInChildren = ((Component)this).GetComponentsInChildren<Transform>();
		for (int i = 1; i < componentsInChildren.Length; i++)
		{
			bool flag = false;
			if ((Object)(object)((Component)componentsInChildren[i]).GetComponent<Rigidbody>() != (Object)null || (Object)(object)((Component)componentsInChildren[i]).GetComponent<ConfigurableJoint>() != (Object)null)
			{
				flag = true;
			}
			if ((Object)(object)((Component)componentsInChildren[i]).GetComponent<Collider>() != (Object)null && (Object)(object)((Component)componentsInChildren[i]).GetComponent<Rigidbody>() == (Object)null)
			{
				flag = true;
			}
			if ((Object)(object)((Component)componentsInChildren[i]).GetComponent<CharacterController>() != (Object)null)
			{
				flag = false;
			}
			if (!flag)
			{
				Transform[] array = (Transform[])(object)new Transform[componentsInChildren[i].childCount];
				for (int j = 0; j < array.Length; j++)
				{
					array[j] = componentsInChildren[i].GetChild(j);
				}
				for (int k = 0; k < array.Length; k++)
				{
					array[k].parent = componentsInChildren[i].parent;
				}
				Object.DestroyImmediate((Object)(object)((Component)componentsInChildren[i]).gameObject);
			}
		}
	}

	private static bool IsClothCollider(Collider collider, Cloth[] cloths)
	{
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		if (cloths == null)
		{
			return false;
		}
		foreach (Cloth val in cloths)
		{
			if ((Object)(object)val == (Object)null)
			{
				return false;
			}
			CapsuleCollider[] capsuleColliders = val.capsuleColliders;
			foreach (CapsuleCollider val2 in capsuleColliders)
			{
				if ((Object)(object)val2 != (Object)null && (Object)(object)((Component)val2).gameObject == (Object)(object)((Component)collider).gameObject)
				{
					return true;
				}
			}
			ClothSphereColliderPair[] sphereColliders = val.sphereColliders;
			for (int k = 0; k < sphereColliders.Length; k++)
			{
				ClothSphereColliderPair val3 = sphereColliders[k];
				if ((Object)(object)((ClothSphereColliderPair)(ref val3)).first != (Object)null && (Object)(object)((Component)((ClothSphereColliderPair)(ref val3)).first).gameObject == (Object)(object)((Component)collider).gameObject)
				{
					return true;
				}
				if ((Object)(object)((ClothSphereColliderPair)(ref val3)).second != (Object)null && (Object)(object)((Component)((ClothSphereColliderPair)(ref val3)).second).gameObject == (Object)(object)((Component)collider).gameObject)
				{
					return true;
				}
			}
		}
		return false;
	}

	public void Kill()
	{
		state = State.Dead;
	}

	public void Kill(StateSettings stateSettings)
	{
		this.stateSettings = stateSettings;
		state = State.Dead;
	}

	public void Freeze()
	{
		state = State.Frozen;
	}

	public void Freeze(StateSettings stateSettings)
	{
		this.stateSettings = stateSettings;
		state = State.Frozen;
	}

	public void Resurrect()
	{
		state = State.Alive;
	}

	protected virtual void SwitchStates()
	{
		if (state == lastState || isKilling)
		{
			return;
		}
		if (freezeFlag)
		{
			if (state == State.Alive)
			{
				activeState = State.Dead;
				lastState = State.Dead;
				freezeFlag = false;
			}
			else if (state == State.Dead)
			{
				lastState = State.Dead;
				freezeFlag = false;
				return;
			}
			if (freezeFlag)
			{
				return;
			}
		}
		if (lastState == State.Alive)
		{
			if (state == State.Dead)
			{
				((MonoBehaviour)this).StartCoroutine(AliveToDead(freeze: false));
			}
			else if (state == State.Frozen)
			{
				((MonoBehaviour)this).StartCoroutine(AliveToDead(freeze: true));
			}
		}
		else if (lastState == State.Dead)
		{
			if (state == State.Alive)
			{
				DeadToAlive();
			}
			else if (state == State.Frozen)
			{
				DeadToFrozen();
			}
		}
		else if (lastState == State.Frozen)
		{
			if (state == State.Alive)
			{
				FrozenToAlive();
			}
			else if (state == State.Dead)
			{
				FrozenToDead();
			}
		}
		lastState = state;
	}

	[IteratorStateMachine(typeof(_003CAliveToDead_003Ed__226))]
	private IEnumerator AliveToDead(bool freeze)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CAliveToDead_003Ed__226(0)
		{
			_003C_003E4__this = this,
			freeze = freeze
		};
	}

	private void OnFreezeFlag()
	{
		if (!CanFreeze())
		{
			return;
		}
		SetAnimationEnabled(to: false);
		Muscle[] array = muscles;
		foreach (Muscle muscle in array)
		{
			((Component)muscle.joint).gameObject.SetActive(false);
		}
		BehaviourBase[] array2 = behaviours;
		foreach (BehaviourBase behaviourBase in array2)
		{
			behaviourBase.Freeze();
			if (((Component)behaviourBase).gameObject.activeSelf)
			{
				behaviourBase.deactivated = true;
				((Component)behaviourBase).gameObject.SetActive(false);
			}
		}
		freezeFlag = false;
		activeState = State.Frozen;
		if (OnFreeze != null)
		{
			OnFreeze();
		}
		if (stateSettings.freezePermanently)
		{
			if (behaviours.Length != 0 && (Object)(object)behaviours[0] != (Object)null)
			{
				Object.Destroy((Object)(object)((Component)((Component)behaviours[0]).transform.parent).gameObject);
			}
			Object.Destroy((Object)(object)((Component)this).gameObject);
		}
	}

	private void DeadToAlive()
	{
		Muscle[] array = muscles;
		foreach (Muscle muscle in array)
		{
			muscle.state.pinWeightMlp = 1f;
			muscle.state.muscleWeightMlp = 1f;
			muscle.state.muscleDamperAdd = 0f;
		}
		if (angularLimitsEnabledOnKill)
		{
			angularLimits = false;
			angularLimitsEnabledOnKill = false;
		}
		if (internalCollisionsEnabledOnKill)
		{
			internalCollisions = false;
			internalCollisionsEnabledOnKill = false;
		}
		BehaviourBase[] array2 = behaviours;
		foreach (BehaviourBase behaviourBase in array2)
		{
			behaviourBase.Resurrect();
		}
		SetAnimationEnabled(to: true);
		activeState = State.Alive;
		if (OnResurrection != null)
		{
			OnResurrection();
		}
	}

	private void SetAnimationEnabled(bool to)
	{
		animatorDisabled = false;
		if ((Object)(object)targetAnimator != (Object)null)
		{
			((Behaviour)targetAnimator).enabled = to;
		}
		if ((Object)(object)targetAnimation != (Object)null)
		{
			((Behaviour)targetAnimation).enabled = to;
		}
	}

	private void DeadToFrozen()
	{
		freezeFlag = true;
	}

	private void FrozenToAlive()
	{
		freezeFlag = false;
		Muscle[] array = muscles;
		foreach (Muscle muscle in array)
		{
			muscle.state.pinWeightMlp = 1f;
			muscle.state.muscleWeightMlp = 1f;
			muscle.state.muscleDamperAdd = 0f;
		}
		if (angularLimitsEnabledOnKill)
		{
			angularLimits = false;
			angularLimitsEnabledOnKill = false;
		}
		if (internalCollisionsEnabledOnKill)
		{
			internalCollisions = false;
			internalCollisionsEnabledOnKill = false;
		}
		ActivateRagdoll();
		BehaviourBase[] array2 = behaviours;
		foreach (BehaviourBase behaviourBase in array2)
		{
			behaviourBase.Unfreeze();
			behaviourBase.Resurrect();
			if (behaviourBase.deactivated)
			{
				((Component)behaviourBase).gameObject.SetActive(true);
			}
		}
		if ((Object)(object)targetAnimator != (Object)null)
		{
			((Behaviour)targetAnimator).enabled = true;
		}
		if ((Object)(object)targetAnimation != (Object)null)
		{
			((Behaviour)targetAnimation).enabled = true;
		}
		activeState = State.Alive;
		if (OnUnfreeze != null)
		{
			OnUnfreeze();
		}
		if (OnResurrection != null)
		{
			OnResurrection();
		}
	}

	private void FrozenToDead()
	{
		freezeFlag = false;
		ActivateRagdoll();
		BehaviourBase[] array = behaviours;
		foreach (BehaviourBase behaviourBase in array)
		{
			behaviourBase.Unfreeze();
			if (behaviourBase.deactivated)
			{
				((Component)behaviourBase).gameObject.SetActive(true);
			}
		}
		activeState = State.Dead;
		if (OnUnfreeze != null)
		{
			OnUnfreeze();
		}
	}

	private void ActivateRagdoll(bool kinematic = false)
	{
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		Muscle[] array = muscles;
		foreach (Muscle muscle in array)
		{
			muscle.Reset();
		}
		Muscle[] array2 = muscles;
		foreach (Muscle muscle2 in array2)
		{
			((Component)muscle2.joint).gameObject.SetActive(true);
			if (kinematic)
			{
				muscle2.rigidbody.collisionDetectionMode = (CollisionDetectionMode)0;
			}
			muscle2.SetKinematic(kinematic);
			muscle2.rigidbody.velocity = Vector3.zero;
			muscle2.rigidbody.angularVelocity = Vector3.zero;
		}
		FlagInternalCollisionsForUpdate();
		Read();
		Muscle[] array3 = muscles;
		foreach (Muscle muscle3 in array3)
		{
			muscle3.MoveToTarget();
		}
	}

	private bool CanFreeze()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		Muscle[] array = muscles;
		foreach (Muscle muscle in array)
		{
			Vector3 velocity = muscle.rigidbody.velocity;
			if (((Vector3)(ref velocity)).sqrMagnitude > stateSettings.maxFreezeSqrVelocity)
			{
				return false;
			}
		}
		return true;
	}

	public void SampleTargetMappedState()
	{
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		if (!CheckIfInitiated())
		{
			return;
		}
		sampleTargetMappedState = true;
		if (!targetMappedStateStored)
		{
			sampleTargetMappedState = true;
			return;
		}
		for (int i = 0; i < muscles.Length; i++)
		{
			if (i == 0)
			{
				muscles[i].targetSampledPosition = muscles[i].targetMappedPosition;
			}
			muscles[i].targetSampledRotation = muscles[i].targetMappedRotation;
		}
		targetMappedStateSampled = true;
	}

	public void FixTargetToSampledState(float weight)
	{
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		if (!CheckIfInitiated() || weight <= 0f || !targetMappedStateSampled)
		{
			return;
		}
		for (int i = 0; i < muscles.Length; i++)
		{
			if (i == 0)
			{
				muscles[i].target.position = Vector3.Lerp(muscles[i].target.position, muscles[i].targetSampledPosition, weight);
			}
			muscles[i].target.rotation = Quaternion.Lerp(muscles[i].target.rotation, muscles[i].targetSampledRotation, weight);
		}
		Muscle[] array = muscles;
		foreach (Muscle muscle in array)
		{
			muscle.positionOffset = muscle.target.position - muscle.rigidbody.position;
		}
	}

	public void StoreTargetMappedState()
	{
		if (!CheckIfInitiated() || !storeTargetMappedState)
		{
			return;
		}
		for (int i = 0; i < muscles.Length; i++)
		{
			if (i == 0)
			{
				muscles[i].StoreTargetMappedPosition();
			}
			muscles[i].StoreTargetMappedRotation();
		}
		targetMappedStateStored = true;
		if (sampleTargetMappedState)
		{
			SampleTargetMappedState();
		}
		sampleTargetMappedState = false;
	}

	private void UpdateHierarchies()
	{
		for (int i = 0; i < muscles.Length; i++)
		{
			muscles[i].index = i;
			if ((Object)(object)muscles[i].broadcaster != (Object)null)
			{
				muscles[i].broadcaster.muscleIndex = i;
			}
			if ((Object)(object)muscles[i].jointBreakBroadcaster != (Object)null)
			{
				muscles[i].jointBreakBroadcaster.muscleIndex = i;
			}
		}
		targetMappedStateStored = false;
		targetMappedStateSampled = false;
		AssignParentAndChildIndexes();
		AssignKinshipDegrees();
		UpdateBroadcasterMuscleIndexes();
		if (disconnectMuscleFlags.Length != muscles.Length)
		{
			Array.Resize(ref disconnectMuscleFlags, muscles.Length);
			Array.Resize(ref muscleDisconnectModes, muscles.Length);
			Array.Resize(ref disconnectDeactivateFlags, muscles.Length);
			Array.Resize(ref reconnectMuscleFlags, muscles.Length);
		}
		propMuscles = ((Component)this).GetComponentsInChildren<PropMuscle>();
		hasProp = HasProp();
		if (OnHierarchyChanged != null)
		{
			OnHierarchyChanged();
		}
	}

	private bool HasProp()
	{
		Muscle[] array = muscles;
		foreach (Muscle muscle in array)
		{
			if (muscle.props.group == Muscle.Group.Prop)
			{
				return true;
			}
		}
		return false;
	}

	private void UpdateBroadcasterMuscleIndexes()
	{
		for (int i = 0; i < muscles.Length; i++)
		{
			if ((Object)(object)muscles[i].broadcaster != (Object)null)
			{
				muscles[i].broadcaster.muscleIndex = i;
			}
			if ((Object)(object)muscles[i].jointBreakBroadcaster != (Object)null)
			{
				muscles[i].jointBreakBroadcaster.muscleIndex = i;
			}
		}
	}

	private void AssignParentAndChildIndexes()
	{
		for (int i = 0; i < muscles.Length; i++)
		{
			muscles[i].parentIndexes = new int[0];
			if ((Object)(object)((Joint)muscles[i].joint).connectedBody != (Object)null)
			{
				AddToParentsRecursive(((Component)((Joint)muscles[i].joint).connectedBody).GetComponent<ConfigurableJoint>(), ref muscles[i].parentIndexes);
			}
			muscles[i].childIndexes = new int[0];
			muscles[i].childFlags = new bool[muscles.Length];
			for (int j = 0; j < muscles.Length; j++)
			{
				if (i != j && (Object)(object)((Joint)muscles[j].joint).connectedBody == (Object)(object)muscles[i].rigidbody)
				{
					AddToChildrenRecursive(muscles[j].joint, ref muscles[i].childIndexes, ref muscles[i].childFlags);
				}
			}
		}
	}

	private void AddToParentsRecursive(ConfigurableJoint joint, ref int[] indexes)
	{
		if ((Object)(object)joint == (Object)null)
		{
			return;
		}
		int muscleIndexLowLevel = GetMuscleIndexLowLevel(joint);
		if (muscleIndexLowLevel != -1)
		{
			Array.Resize(ref indexes, indexes.Length + 1);
			indexes[indexes.Length - 1] = muscleIndexLowLevel;
			if (!((Object)(object)((Joint)joint).connectedBody == (Object)null))
			{
				AddToParentsRecursive(((Component)((Joint)joint).connectedBody).GetComponent<ConfigurableJoint>(), ref indexes);
			}
		}
	}

	private void AddToChildrenRecursive(ConfigurableJoint joint, ref int[] indexes, ref bool[] childFlags)
	{
		if ((Object)(object)joint == (Object)null)
		{
			return;
		}
		int muscleIndexLowLevel = GetMuscleIndexLowLevel(joint);
		if (muscleIndexLowLevel == -1)
		{
			return;
		}
		Array.Resize(ref indexes, indexes.Length + 1);
		indexes[indexes.Length - 1] = muscleIndexLowLevel;
		childFlags[muscleIndexLowLevel] = true;
		for (int i = 0; i < muscles.Length; i++)
		{
			if (i != muscleIndexLowLevel && (Object)(object)((Joint)muscles[i].joint).connectedBody == (Object)(object)((Component)joint).GetComponent<Rigidbody>())
			{
				AddToChildrenRecursive(muscles[i].joint, ref indexes, ref childFlags);
			}
		}
	}

	private void AssignKinshipDegrees()
	{
		for (int i = 0; i < muscles.Length; i++)
		{
			muscles[i].kinshipDegrees = new int[muscles.Length];
			AssignKinshipsDownRecursive(ref muscles[i].kinshipDegrees, 1, i);
			AssignKinshipsUpRecursive(ref muscles[i].kinshipDegrees, 1, i);
		}
	}

	private void AssignKinshipsDownRecursive(ref int[] kinshipDegrees, int degree, int index)
	{
		for (int i = 0; i < muscles.Length; i++)
		{
			if (i != index && (Object)(object)((Joint)muscles[i].joint).connectedBody == (Object)(object)muscles[index].rigidbody)
			{
				kinshipDegrees[i] = degree;
				AssignKinshipsDownRecursive(ref kinshipDegrees, degree + 1, i);
			}
		}
	}

	private void AssignKinshipsUpRecursive(ref int[] kinshipDegrees, int degree, int index)
	{
		for (int i = 0; i < muscles.Length; i++)
		{
			if (i == index || !((Object)(object)muscles[i].rigidbody == (Object)(object)((Joint)muscles[index].joint).connectedBody))
			{
				continue;
			}
			kinshipDegrees[i] = degree;
			AssignKinshipsUpRecursive(ref kinshipDegrees, degree + 1, i);
			for (int j = 0; j < muscles.Length; j++)
			{
				if (j != i && j != index && (Object)(object)((Joint)muscles[j].joint).connectedBody == (Object)(object)muscles[i].rigidbody)
				{
					kinshipDegrees[j] = degree + 1;
					AssignKinshipsDownRecursive(ref kinshipDegrees, degree + 2, j);
				}
			}
		}
	}

	private int GetMuscleIndexLowLevel(ConfigurableJoint joint)
	{
		for (int i = 0; i < muscles.Length; i++)
		{
			if ((Object)(object)muscles[i].joint == (Object)(object)joint)
			{
				return i;
			}
		}
		return -1;
	}

	public bool IsValid(bool log)
	{
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		if (muscles == null && log)
		{
			return false;
		}
		if (muscles.Length == 0 && log)
		{
			return false;
		}
		for (int i = 0; i < muscles.Length; i++)
		{
			if (muscles[i] == null && log)
			{
				return false;
			}
			if (!muscles[i].IsValid(log))
			{
				return false;
			}
		}
		if ((Object)(object)targetRoot == (Object)null && log)
		{
			return false;
		}
		((Component)this).transform.position = targetRoot.position;
		Muscle[] array = muscles;
		foreach (Muscle muscle in array)
		{
			((Component)muscle.joint).transform.SetPositionAndRotation(muscle.target.position, muscle.target.rotation);
		}
		Physics.SyncTransforms();
		if ((Object)(object)((Joint)muscles[0].joint).connectedBody != (Object)null && muscles.Length > 1)
		{
			for (int k = 1; k < muscles.Length; k++)
			{
				if ((Object)(object)((Component)muscles[k].joint).GetComponent<Rigidbody>() == (Object)(object)((Joint)muscles[0].joint).connectedBody && log)
				{
					return false;
				}
			}
		}
		for (int l = 0; l < muscles.Length; l++)
		{
			if (Vector3.SqrMagnitude(((Component)muscles[l].joint).transform.position - muscles[l].target.position) > 0.001f && log)
			{
				return false;
			}
		}
		CheckMassVariation(100f, log: true);
		return true;
	}

	private bool CheckMassVariation(float threshold, bool log)
	{
		float num = float.PositiveInfinity;
		float num2 = 0f;
		for (int i = 0; i < muscles.Length; i++)
		{
			float mass = ((Component)muscles[i].joint).GetComponent<Rigidbody>().mass;
			if (mass < num)
			{
				num = mass;
			}
			if (mass > num2)
			{
				num2 = mass;
			}
		}
		if (num2 / num > threshold)
		{
			if (log)
			{
			}
			return false;
		}
		return true;
	}

	private bool CheckIfInitiated()
	{
		if (!initiated)
		{
		}
		return initiated;
	}
}
