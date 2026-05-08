namespace RootMotion.FinalIK;

public abstract class IK : SolverManager
{
	public abstract IKSolver GetIKSolver();

	public override void UpdateSolver()
	{
		if (!GetIKSolver().initiated)
		{
			InitiateSolver();
		}
		if (GetIKSolver().initiated)
		{
			GetIKSolver().Update();
		}
	}

	public override void InitiateSolver()
	{
		if (!GetIKSolver().initiated)
		{
			GetIKSolver().Initiate(base.transform);
		}
	}

	public override void FixTransforms()
	{
		if (GetIKSolver().initiated)
		{
			GetIKSolver().FixTransforms();
		}
	}

	public abstract void OpenUserManual();

	public abstract void OpenScriptReference();

	public IK()
	{
	}
}
