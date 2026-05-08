public struct FirearmScopeStateStruct
{
	public string Id;

	public int ScopeMode;

	public int ScopeIndexInsideSight;

	public int ScopeCalibrationIndex;

	public static bool IsScopeStatesDifferent(FirearmScopeStateStruct stateA, FirearmScopeStateStruct stateB)
	{
		if (stateA.ScopeMode == stateB.ScopeMode && stateA.ScopeIndexInsideSight == stateB.ScopeIndexInsideSight)
		{
			return stateA.ScopeCalibrationIndex != stateB.ScopeCalibrationIndex;
		}
		return true;
	}
}
