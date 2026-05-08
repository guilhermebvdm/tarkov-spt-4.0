using UnityEngine;

namespace RootMotion.FinalIK;

[HelpURL("http://www.root-motion.com/finalikdox/html/page4.html")]
[AddComponentMenu("Scripts/RootMotion.FinalIK/IK/FABRIK")]
public class FABRIK : IK
{
	public IKSolverFABRIK solver = new IKSolverFABRIK();

	[ContextMenu("User Manual")]
	public override void OpenUserManual()
	{
		Application.OpenURL("http://www.root-motion.com/finalikdox/html/page4.html");
	}

	[ContextMenu("Scrpt Reference")]
	public override void OpenScriptReference()
	{
		Application.OpenURL("http://www.root-motion.com/finalikdox/html/class_root_motion_1_1_final_i_k_1_1_f_a_b_r_i_k.html");
	}

	[ContextMenu("Support Group")]
	public void method_3()
	{
		Application.OpenURL("https://groups.google.com/forum/#!forum/final-ik");
	}

	[ContextMenu("Asset Store Thread")]
	public void method_4()
	{
		Application.OpenURL("http://forum.unity3d.com/threads/final-ik-full-body-ik-aim-look-at-fabrik-ccd-ik-1-0-released.222685/");
	}

	public override IKSolver GetIKSolver()
	{
		return solver;
	}
}
