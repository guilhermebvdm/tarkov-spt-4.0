using Audio.SpatialSystem;
using Audio.SpatialSystem.Data;

public class GClass1135 : GClass1128
{
	public GClass1135(uint propagationDepth, SpatialAudioSystem audioSystem)
		: base(audioSystem)
	{
	}

	public GClass1135(AudioOcclusionSettings occlusionSettings, SpatialAudioSystem system)
		: base(system)
	{
	}

	public override float GetScore()
	{
		return 0f;
	}
}
