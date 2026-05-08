public abstract class GClass1326
{
	public readonly float Duration;

	public readonly string Name;

	public abstract bool IsLooped { get; }

	public GClass1326(float duration, string name)
	{
		Duration = duration;
		Name = name;
	}

	public override string ToString()
	{
		return $"Name: {Name}, Duration: {Duration}";
	}
}
