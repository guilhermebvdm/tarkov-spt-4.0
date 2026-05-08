using RootMotion.FinalIK;
using UnityEngine;

public class SimpleTIK : TrigonometricIK
{
	public new GClass1466 solver = new GClass1466();

	[ContextMenu("User Manual")]
	public override void OpenUserManual()
	{
		Application.OpenURL("http://www.root-motion.com/finalikdox/html/page9.html");
	}

	[ContextMenu("Scrpt Reference")]
	public override void OpenScriptReference()
	{
		Application.OpenURL("http://www.root-motion.com/finalikdox/html/class_root_motion_1_1_final_i_k_1_1_trigonometric_i_k.html");
	}

	[ContextMenu("Support Group")]
	public void method_5()
	{
		Application.OpenURL("https://groups.google.com/forum/#!forum/final-ik");
	}

	[ContextMenu("Asset Store Thread")]
	public void method_6()
	{
		Application.OpenURL("http://forum.unity3d.com/threads/final-ik-full-body-ik-aim-look-at-fabrik-ccd-ik-1-0-released.222685/");
	}

	public override IKSolver GetIKSolver()
	{
		return solver;
	}
}
