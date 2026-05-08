using System;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

[Serializable]
public class KeyCodeAnimatorParameter
{
	[CompilerGenerated]
	public class Class502
	{
		public Animator animator;

		public KeyCodeAnimatorParameter keyCodeAnimatorParameter_0;

		public void method_0()
		{
			animator.SetBool(keyCodeAnimatorParameter_0.Parameter, value: true);
		}

		public void method_1()
		{
			animator.SetBool(keyCodeAnimatorParameter_0.Parameter, value: false);
		}
	}

	public KeyCode PositiveKeyCode;

	public KeyCode NegativeKeyCode;

	public string Parameter;

	[NonSerialized]
	public Action PositiveAction;

	[NonSerialized]
	public Action NegativeAction;

	[SerializeField]
	public bool _isSetted;

	public void ProcessInput(Animator animator)
	{
		if (!_isSetted)
		{
			method_0(animator);
		}
		if (Input.GetKeyUp(PositiveKeyCode))
		{
			PositiveAction();
		}
		else if (Input.GetKeyUp(NegativeKeyCode))
		{
			NegativeAction();
		}
	}

	public void method_0(Animator animator)
	{
		AnimatorControllerParameter animatorControllerParameter = animator.parameters.FirstOrDefault((AnimatorControllerParameter parameter) => parameter.name == Parameter);
		if (animatorControllerParameter != null && animatorControllerParameter.type == AnimatorControllerParameterType.Bool)
		{
			method_1(animator);
		}
	}

	public void method_1(Animator animator)
	{
		PositiveAction = delegate
		{
			animator.SetBool(Parameter, value: true);
		};
		NegativeAction = delegate
		{
			animator.SetBool(Parameter, value: false);
		};
		_isSetted = true;
	}

	[CompilerGenerated]
	public bool method_2(AnimatorControllerParameter parameter)
	{
		return parameter.name == Parameter;
	}
}
