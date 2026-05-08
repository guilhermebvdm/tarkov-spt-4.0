using System;

public class GClass1317 : GClass1315
{
	[NonSerialized]
	public GClass1315 Gclass1315_0;

	public override GClass1313 ParentStateMachine => Gclass1315_0;

	public GClass1317(string name, int hash, int defaultStateHash)
		: base(name, hash, defaultStateHash)
	{
	}

	public override void Dispose(bool disposing)
	{
		if (disposing)
		{
			Gclass1315_0 = null;
		}
		base.Dispose(disposing);
	}
}
