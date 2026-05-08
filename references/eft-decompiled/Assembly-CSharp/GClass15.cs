using System;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

public abstract class GClass15
{
	[Serializable]
	[CompilerGenerated]
	public class Class96
	{
		public static readonly Class96 class96_0 = new Class96();

		public static Func<GClass14.EPauseReason, GClass14.EPauseReason, GClass14.EPauseReason> func_0;

		public GClass14.EPauseReason method_0(GClass14.EPauseReason current, GClass14.EPauseReason reason)
		{
			return current | reason;
		}
	}

	public static GClass14.EPauseReason ToBitMask([CanBeNull] this GClass14.EPauseReason[] reasons, GClass14.EPauseReason defaultPauseReason = GClass14.EPauseReason.All)
	{
		if (reasons != null && reasons.Length != 0)
		{
			return reasons.Aggregate(GClass14.EPauseReason.None, (GClass14.EPauseReason current, GClass14.EPauseReason reason) => current | reason);
		}
		return defaultPauseReason;
	}
}
