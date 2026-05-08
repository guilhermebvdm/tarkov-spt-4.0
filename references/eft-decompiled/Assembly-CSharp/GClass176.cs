public abstract class GClass176<T> : BotNodeAbstractClass where T : GClass26
{
	public GClass176()
	{
	}

	public virtual void Awake()
	{
	}

	public abstract void UpdateNodeByBrain(T data);

	public override void UpdateNodeByMain(GClass26 lastResultData)
	{
		UpdateNodeByBrain(lastResultData as T);
	}
}
