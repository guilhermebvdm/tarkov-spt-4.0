using UnityEngine;

public class ActionTrigger : MonoBehaviour
{
	public bool State;

	public Animator Target;

	public string StateFalseAnim = "DoorClose";

	public string StateTrueAnim = "DoorOpenDelegate";

	public int Layer;

	public void Use()
	{
		State = !State;
		float normalizedTime = 1f - Mathf.Clamp01(Target.GetCurrentAnimatorStateInfo(Layer).normalizedTime);
		if (State)
		{
			Target.Play(StateTrueAnim, Layer, normalizedTime);
		}
		else
		{
			Target.Play(StateFalseAnim, Layer, normalizedTime);
		}
	}
}
