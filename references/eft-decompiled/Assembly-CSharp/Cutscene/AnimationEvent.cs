using System;

namespace Cutscene;

[Serializable]
public class AnimationEvent
{
	public int stateHash;

	public float time;

	public AnimationTrack fire;

	public float duration => fire?.duration ?? 0f;

	public string hint
	{
		get
		{
			AnimationTrack animationTrack = fire;
			object obj;
			if (animationTrack == null)
			{
				obj = null;
			}
			else
			{
				obj = animationTrack.hint;
				if (obj != null)
				{
					goto IL_001b;
				}
			}
			obj = "(null)";
			goto IL_001b;
			IL_001b:
			return (string)obj;
		}
	}
}
