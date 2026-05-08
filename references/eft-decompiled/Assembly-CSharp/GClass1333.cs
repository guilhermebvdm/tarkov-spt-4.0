public class GClass1333 : GClass1332<AnimatorStateDebugger, GClass1329>
{
	public override GClass1329 InternalCreate(AnimatorStateDebugger behaviour, GClass1312 state)
	{
		GClass1329 gClass = base.InternalCreate(behaviour, state);
		gClass.Name = behaviour.Name;
		return gClass;
	}
}
