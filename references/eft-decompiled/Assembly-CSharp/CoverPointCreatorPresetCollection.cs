using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CoverPointCreatorPresetCollection", menuName = "Scriptable objects/AI/CoverPointCreator Preset Collection")]
public class CoverPointCreatorPresetCollection : ScriptableObject
{
	[SerializeField]
	private List<CoverPointCreatorPreset> _presets;

	public List<CoverPointCreatorPreset> Presets => _presets;

	public static CoverPointCreatorPreset GetDefault()
	{
		return ((CoverPointCreatorPresetCollection)Resources.Load("CoverPointCreatorPresetCollection")).GetPresetByName("Default");
	}

	public CoverPointCreatorPreset GetPresetByName(string name)
	{
		int num = 0;
		while (true)
		{
			if (num < _presets.Count)
			{
				if (_presets[num].Name == name)
				{
					break;
				}
				num++;
				continue;
			}
			int num2 = 0;
			while (true)
			{
				if (num2 < _presets.Count)
				{
					if (_presets[num2].Name == "Default")
					{
						break;
					}
					num2++;
					continue;
				}
				return null;
			}
			return _presets[num2];
		}
		return _presets[num];
	}

	public void DrawPresetsInspector()
	{
	}
}
