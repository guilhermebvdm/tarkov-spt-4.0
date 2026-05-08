using UnityEngine;

public class GClass1327 : GClass1326
{
	public readonly bool IsLooping;

	public readonly int ClipInstanceId;

	public override bool IsLooped => IsLooping;

	public GClass1327(float duration, string name, bool isLooping, int clipInstanceId)
		: base(duration, name)
	{
		IsLooping = isLooping;
		ClipInstanceId = clipInstanceId;
	}

	public static GClass1327 Create(AnimationClip animClip)
	{
		if (animClip != null)
		{
			return Create(animClip.length, animClip.name, animClip.isLooping, animClip.GetInstanceID());
		}
		return Create(1E-06f, string.Empty, isLooping: false, 0);
	}

	public static GClass1327 Create(float duration, string name, bool isLooping, int instanceId)
	{
		return new GClass1327(duration, name, isLooping, instanceId);
	}

	public override string ToString()
	{
		return $"{base.ToString()}, IsLooping: {IsLooping}";
	}
}
