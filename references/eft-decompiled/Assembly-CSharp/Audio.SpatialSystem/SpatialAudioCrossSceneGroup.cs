using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Audio.SpatialSystem;

[RequireComponent(typeof(GuidComponent))]
public class SpatialAudioCrossSceneGroup : MonoBehaviour
{
	public static readonly List<SpatialAudioCrossSceneGroup> AllCrossGroups = new List<SpatialAudioCrossSceneGroup>();

	[HideInInspector]
	public GuidReference crossSceneGroup;

	public List<SpatialAudioRoom> AudioRooms;

	public List<BaseSpatialAudioPortal> AudioPortals;

	public void Awake()
	{
		method_0();
		AllCrossGroups.Add(this);
	}

	public void OnDestroy()
	{
		AllCrossGroups.Clear();
	}

	public void OnValidate()
	{
		method_0();
	}

	public void method_0()
	{
		AudioRooms = GetComponentsInChildren<SpatialAudioRoom>().ToList();
		AudioPortals = GetComponentsInChildren<BaseSpatialAudioPortal>().ToList();
	}
}
