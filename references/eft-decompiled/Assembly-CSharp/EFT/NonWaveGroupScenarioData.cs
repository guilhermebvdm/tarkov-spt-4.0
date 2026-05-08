using System;

namespace EFT;

[Serializable]
public class NonWaveGroupScenarioData
{
	public bool Enabled;

	public int MinToBeGroup;

	public int MaxToBeGroup;

	public float Chance;
}
