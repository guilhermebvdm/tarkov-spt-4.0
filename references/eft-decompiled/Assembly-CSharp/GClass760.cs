public abstract class GClass760
{
	public static void Apply(this GStruct38[] animatorParams, IAnimator animator)
	{
		for (int i = 0; i < animatorParams.Length; i++)
		{
			animatorParams[i].Apply(animator);
		}
	}
}
