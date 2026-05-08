using System.Collections.Generic;
using UnityEngine;

namespace Audio.AmbientSubsystem;

[RequireComponent(typeof(BaseAmbientSoundPlayer))]
public class SoundPlayerRandomPointComponent : MonoBehaviour
{
	[SerializeField]
	[HideInInspector]
	private List<SoundPoint> _worldSoundPoints = new List<SoundPoint>();

	private ISoundPlayer isoundPlayer_0;

	private readonly Dictionary<int, SoundPoint> dictionary_0 = new Dictionary<int, SoundPoint>();

	private readonly List<int> list_0 = new List<int>();

	private SoundPoint soundPoint_0;

	public void Awake()
	{
		method_0();
		foreach (SoundPoint worldSoundPoint in _worldSoundPoints)
		{
			int instanceID = worldSoundPoint.GetInstanceID();
			dictionary_0[instanceID] = worldSoundPoint;
			worldSoundPoint.Occupied += method_3;
			worldSoundPoint.Released += method_4;
			list_0.Add(instanceID);
		}
		isoundPlayer_0 = GetComponent<ISoundPlayer>();
		isoundPlayer_0.Played += method_1;
	}

	public void method_0()
	{
		for (int num = _worldSoundPoints.Count - 1; num >= 0; num--)
		{
			if (_worldSoundPoints[num] == null)
			{
				_worldSoundPoints.RemoveAt(num);
			}
		}
	}

	public void method_1()
	{
		if (method_2(out var point))
		{
			if (soundPoint_0 != null)
			{
				soundPoint_0.Release();
			}
			soundPoint_0 = point;
			soundPoint_0.Occupy();
			isoundPlayer_0.Position = point.transform.position;
		}
	}

	public bool method_2(out SoundPoint point)
	{
		point = null;
		if (list_0.Count == 0)
		{
			return false;
		}
		int index = Random.Range(0, list_0.Count);
		int key = list_0[index];
		if (dictionary_0.TryGetValue(key, out var value) && !value.IsOccupied)
		{
			point = value;
			return true;
		}
		return false;
	}

	public void method_3(int pointInstanceID)
	{
		list_0.Remove(pointInstanceID);
	}

	public void method_4(int pointInstanceID)
	{
		list_0.Add(pointInstanceID);
	}

	public void OnDestroy()
	{
		foreach (SoundPoint worldSoundPoint in _worldSoundPoints)
		{
			if (!(worldSoundPoint == null))
			{
				worldSoundPoint.Occupied -= method_3;
				worldSoundPoint.Released -= method_4;
			}
		}
		if (isoundPlayer_0 != null)
		{
			isoundPlayer_0.Played -= method_1;
		}
	}
}
