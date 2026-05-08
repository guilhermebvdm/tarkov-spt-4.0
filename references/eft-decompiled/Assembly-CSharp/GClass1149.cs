using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Audio.AmbientSubsystem;
using Audio.SpatialSystem;
using UnityEngine;

public class GClass1149 : ISpatialAudioRoom, ISpatialRoom
{
	[NonSerialized]
	public static List<ISpatialPortal> List_0 = new List<ISpatialPortal>();

	[NonSerialized]
	[CompilerGenerated]
	public BoxCollider[] BoxCollider_0 = Array.Empty<BoxCollider>();

	[NonSerialized]
	[CompilerGenerated]
	public RoomAmbientData RoomAmbientData_0 = new RoomAmbientData();

	[NonSerialized]
	[CompilerGenerated]
	public Bounds Bounds_0;

	public EAudioRoomTypeMask Type => EAudioRoomTypeMask.Outdoor;

	public string Name => "Empty Audio Room";

	public short ID => -1;

	public BoxCollider[] Colliders
	{
		[CompilerGenerated]
		get
		{
			return BoxCollider_0;
		}
	}

	public bool IsOutdoor => true;

	public bool IsIsolated => false;

	public float RoomSize => 0f;

	public RoomAmbientData RoomAmbientData
	{
		[CompilerGenerated]
		get
		{
			return RoomAmbientData_0;
		}
	}

	public Bounds Bounds
	{
		[CompilerGenerated]
		get
		{
			return Bounds_0;
		}
	}

	public bool IsValid => false;

	public bool CheckIsolationRelation(ISpatialAudioRoom roomToCheck)
	{
		return false;
	}

	public void PlayRoomToneSound()
	{
	}

	public void StopRoomToneSound()
	{
	}

	public List<ISpatialPortal> GetPortals()
	{
		return List_0;
	}
}
