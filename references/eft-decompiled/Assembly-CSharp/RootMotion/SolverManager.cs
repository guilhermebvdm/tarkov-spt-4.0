using UnityEngine;

namespace RootMotion;

public class SolverManager : MonoBehaviour
{
	[Tooltip("If zero, will update the solver in every LateUpdate(). Use this for chains that are animated. If > 0, will be used as updating frequency so that the solver will reach its target in the same time on all machines.")]
	public float timeStep;

	[Tooltip("If true, will fix all the Transforms used by the solver to their initial state in each Update. This prevents potential problems with unanimated bones and animator culling with a small cost of performance. Not recommended for CCD and FABRIK solvers.")]
	public bool fixTransforms = true;

	private float float_0;

	private Animator animator_0;

	private Animation animation_0;

	private bool bool_0;

	private bool bool_1;

	private bool bool_2;

	public bool Boolean_0
	{
		get
		{
			if (animator_0 != null)
			{
				return animator_0.updateMode == AnimatorUpdateMode.AnimatePhysics;
			}
			if (animation_0 != null)
			{
				return animation_0.animatePhysics;
			}
			return false;
		}
	}

	public bool Boolean_1
	{
		get
		{
			if (!(animator_0 != null))
			{
				return animation_0 != null;
			}
			return true;
		}
	}

	public void Disable()
	{
		Debug.Log("IK.Disable() is deprecated. Use enabled = false instead", base.transform);
		base.enabled = false;
	}

	public virtual void InitiateSolver()
	{
	}

	public virtual void UpdateSolver()
	{
	}

	public virtual void FixTransforms()
	{
	}

	public void OnDisable()
	{
		if (Application.isPlaying)
		{
			method_0();
		}
	}

	public void Start()
	{
		method_0();
	}

	public void method_0()
	{
		if (!bool_1)
		{
			method_1(base.transform, findInChildren: true);
			InitiateSolver();
			bool_1 = true;
		}
	}

	public void Update()
	{
		if (!bool_2 && !Boolean_0 && fixTransforms)
		{
			FixTransforms();
		}
	}

	public void method_1(Transform t, bool findInChildren)
	{
		if (Boolean_1)
		{
			return;
		}
		animator_0 = t.GetComponent<Animator>();
		animation_0 = t.GetComponent<Animation>();
		if (!Boolean_1)
		{
			if (animator_0 == null && findInChildren)
			{
				animator_0 = t.GetComponentInChildren<Animator>();
			}
			if (animation_0 == null && findInChildren)
			{
				animation_0 = t.GetComponentInChildren<Animation>();
			}
			if (!Boolean_1 && t.parent != null)
			{
				method_1(t.parent, findInChildren: false);
			}
		}
	}

	public void FixedUpdate()
	{
		if (bool_2)
		{
			bool_2 = false;
		}
		bool_0 = true;
		if (Boolean_0 && fixTransforms)
		{
			FixTransforms();
		}
	}

	public void LateUpdate()
	{
		if (bool_2)
		{
			return;
		}
		if (!Boolean_0)
		{
			bool_0 = true;
		}
		if (bool_0)
		{
			bool_0 = false;
			if (timeStep == 0f)
			{
				UpdateSolver();
			}
			else if (Time.time >= float_0 + timeStep)
			{
				UpdateSolver();
				float_0 = Time.time;
			}
		}
	}

	public void UpdateSolverExternal()
	{
		if (base.enabled)
		{
			bool_2 = true;
			if (timeStep == 0f)
			{
				UpdateSolver();
			}
			else if (Time.time >= float_0 + timeStep)
			{
				UpdateSolver();
				float_0 = Time.time;
			}
		}
	}
}
