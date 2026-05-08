using System;

public struct GStruct273
{
	[NonSerialized]
	public string RealConditionalId;

	public int ConditionalIdHash;

	public GStruct382<GStruct274> Conditions;

	public override string ToString()
	{
		return string.Format("{0} :{1}, {2}: {3}, {4}: {5}", "RealConditionalId", RealConditionalId, "ConditionalIdHash", ConditionalIdHash, "Conditions", Conditions.ToShortString());
	}
}
