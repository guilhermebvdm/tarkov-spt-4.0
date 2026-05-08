public struct AICoreActionResultStruct<T, U>(T action, string reason, U paramsData = null) where U : GClass26
{
	public T Action = action;

	public string Reason = reason;

	public U Data = paramsData;
}
