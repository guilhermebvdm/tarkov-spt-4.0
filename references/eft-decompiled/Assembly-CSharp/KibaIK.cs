using RootMotion.FinalIK;
using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(LimbIK))]
public class KibaIK : MonoBehaviour
{
	private float float_0;

	private Animator animator_0;

	private LimbIK limbIK_0;

	private AnimationCurve animationCurve_0;

	[Header("Curves")]
	public AnimationCurve OnCurve = new AnimationCurve(new Keyframe(0f, 0f, 1f, 1f), new Keyframe(1f, 1f, 1f, 1f));

	public AnimationCurve OffCurve = new AnimationCurve(new Keyframe(0f, 1f, -1f, -1f), new Keyframe(1f, 0f, -1f, -1f));

	private float float_1 = 1f;

	[Header("Debug")]
	public float TimeScale = 1f;

	public int GripType = 1;

	private float float_2;

	public void Start()
	{
		animator_0 = GetComponent<Animator>();
		limbIK_0 = GetComponent<LimbIK>();
		animator_0.SetFloat("UnderMod", 2f);
	}

	public void Update()
	{
		animator_0.SetFloat("UnderMod", GripType);
		if (GripType == 0)
		{
			IKSolverLimb solver = limbIK_0.solver;
			IKSolverLimb solver2 = limbIK_0.solver;
			float iKRotationWeight = 0f;
			solver2.IKPositionWeight = 0f;
			solver.IKRotationWeight = iKRotationWeight;
			return;
		}
		Time.timeScale = TimeScale;
		if (animationCurve_0 != null)
		{
			float_0 = Mathf.Clamp01(animationCurve_0.Evaluate(float_2));
			animator_0.SetFloat("GripWeight", float_0);
			limbIK_0.solver.IKRotationWeight = (limbIK_0.solver.IKPositionWeight = float_0);
			float_2 += Time.deltaTime * float_1;
			if ((float_0 == 1f && animationCurve_0 == OnCurve) || (float_0 == 0f && animationCurve_0 == OffCurve))
			{
				animationCurve_0 = null;
			}
		}
	}

	public void GripOn(float t)
	{
		if (animationCurve_0 != OnCurve)
		{
			Debug.Log("Init Grip On");
			animationCurve_0 = OnCurve;
			float_2 = 0f;
		}
		else
		{
			Debug.LogError("ALREADY ON STATE");
		}
		float_1 = 1f / Mathf.Max(0.0001f, t);
		Debug.Log(float_1);
	}

	public void GripOff(float t)
	{
		if (animationCurve_0 != OffCurve)
		{
			Debug.Log("Init Grip Off");
			animationCurve_0 = OffCurve;
			float_2 = 0f;
		}
		else
		{
			Debug.LogError("ALREADY OFF STATE");
		}
		float_1 = 1f / Mathf.Max(0.0001f, t);
		Debug.Log(float_1);
	}
}
