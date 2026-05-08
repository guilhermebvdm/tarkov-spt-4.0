using System;

[Serializable]
public struct NavGraphFindDebugPointInfo
{
	public ENavGraphFindRemoveCause Cause;

	public int Id;

	public string GetInfo()
	{
		return $" {Id} {Cause.ToString()}";
	}
}
