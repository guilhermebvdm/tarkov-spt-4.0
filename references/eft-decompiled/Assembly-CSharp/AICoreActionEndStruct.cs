public struct AICoreActionEndStruct
{
	public bool Value;

	public string Reason;

	public AICoreActionEndStruct(bool val = false)
	{
		Value = val;
		Reason = null;
	}

	public AICoreActionEndStruct(string reason, bool val = true)
	{
		Value = val;
		Reason = reason;
	}
}
