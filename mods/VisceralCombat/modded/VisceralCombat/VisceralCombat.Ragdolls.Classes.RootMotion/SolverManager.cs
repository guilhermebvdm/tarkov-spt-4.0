using UnityEngine;
using Object = UnityEngine.Object;

namespace VisceralCombat.Ragdolls.Classes.RootMotion;

public class SolverManager : MonoBehaviour
{
	[Tooltip("If true, will fix all the Transforms used by the solver to their initial state in each Update. This prevents potential problems with unanimated bones and animator culling with a small cost of performance. Not recommended for CCD and FABRIK solvers.")]
	public bool fixTransforms = true;

	private Animator animator;

	private Animation legacy;

	private bool updateFrame;

	private bool componentInitiated;

	private bool skipSolverUpdate;

	private bool animatePhysics
	{
		get
		{
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Invalid comparison between Unknown and I4
			if ((Object)(object)animator != (Object)null)
			{
				return (int)animator.updateMode == 1;
			}
			if ((Object)(object)legacy != (Object)null)
			{
				return legacy.animatePhysics;
			}
			return false;
		}
	}

	private bool isAnimated => (Object)(object)animator != (Object)null || (Object)(object)legacy != (Object)null;

	public void Disable()
	{
		((Behaviour)this).enabled = false;
	}

	protected virtual void InitiateSolver()
	{
	}

	protected virtual void UpdateSolver()
	{
	}

	protected virtual void FixTransforms()
	{
	}

	private void OnDisable()
	{
		if (Application.isPlaying)
		{
			Initiate();
		}
	}

	private void Start()
	{
		Initiate();
	}

	private void Initiate()
	{
		if (!componentInitiated)
		{
			FindAnimatorRecursive(((Component)this).transform, findInChildren: true);
			InitiateSolver();
			componentInitiated = true;
		}
	}

	private void Update()
	{
		if (!skipSolverUpdate && !animatePhysics && fixTransforms)
		{
			FixTransforms();
		}
	}

	private void FindAnimatorRecursive(Transform t, bool findInChildren)
	{
		if (isAnimated)
		{
			return;
		}
		animator = ((Component)t).GetComponent<Animator>();
		legacy = ((Component)t).GetComponent<Animation>();
		if (!isAnimated)
		{
			if ((Object)(object)animator == (Object)null && findInChildren)
			{
				animator = ((Component)t).GetComponentInChildren<Animator>();
			}
			if ((Object)(object)legacy == (Object)null && findInChildren)
			{
				legacy = ((Component)t).GetComponentInChildren<Animation>();
			}
			if (!isAnimated && (Object)(object)t.parent != (Object)null)
			{
				FindAnimatorRecursive(t.parent, findInChildren: false);
			}
		}
	}

	private void FixedUpdate()
	{
		if (skipSolverUpdate)
		{
			skipSolverUpdate = false;
		}
		updateFrame = true;
		if (animatePhysics && fixTransforms)
		{
			FixTransforms();
		}
	}

	private void LateUpdate()
	{
		if (!skipSolverUpdate)
		{
			if (!animatePhysics)
			{
				updateFrame = true;
			}
			if (updateFrame)
			{
				updateFrame = false;
				UpdateSolver();
			}
		}
	}

	public void UpdateSolverExternal()
	{
		if (((Behaviour)this).enabled)
		{
			skipSolverUpdate = true;
			UpdateSolver();
		}
	}
}
