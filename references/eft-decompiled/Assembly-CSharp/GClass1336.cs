using System.Reflection;
using UnityEngine.Animations;

public abstract class GClass1336
{
	public static void SwapHandles(AnimationClipPlayable original, AnimationClipPlayable handleSource)
	{
		FieldInfo field = typeof(AnimationClipPlayable).GetField("m_Handle", BindingFlags.Instance | BindingFlags.NonPublic);
		field.SetValue(original, field.GetValue(handleSource));
	}
}
