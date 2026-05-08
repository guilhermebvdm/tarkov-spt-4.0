using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using EFT.Animations;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class WalkEffector : IEffector
{
	public enum EWalkPreset
	{
		normal,
		lame,
		sprint,
		duck
	}

	[Serializable]
	[CompilerGenerated]
	public class Class547
	{
		public static readonly Class547 class547_0 = new Class547();

		public static Func<AnimVal, GClass2793> func_0;

		public static Func<WalkPreset, GClass2793[]> func_1;

		public static Func<AnimVal, GClass2793> func_2;

		public static Func<WalkPreset, GClass2793[]> func_3;

		public static Func<GClass2793[], IEnumerable<GClass2793>> func_4;

		public static Func<GClass2793[], IEnumerable<GClass2793>> func_5;

		public GClass2793[] method_0(WalkPreset p)
		{
			return p.Curves.Select((AnimVal curve) => new GClass2793(curve)).ToArray();
		}

		public GClass2793 method_1(AnimVal curve)
		{
			return new GClass2793(curve);
		}

		public GClass2793[] method_2(WalkPreset p)
		{
			return p.Curves.Select((AnimVal curve) => new GClass2793(curve)).ToArray();
		}

		public GClass2793 method_3(AnimVal curve)
		{
			return new GClass2793(curve);
		}

		public IEnumerable<GClass2793> method_4(GClass2793[] presets)
		{
			return presets;
		}

		public IEnumerable<GClass2793> method_5(GClass2793[] presets)
		{
			return presets;
		}
	}

	public float StepFrequency = 1f;

	public float Intensity = 1f;

	public float SideSpeedMultyplyer = 1.6f;

	public float BackSpeedMultyplyer = 2f;

	public float Treshold = 0.01f;

	[NonSerialized]
	public Vector3 LastPosition;

	[NonSerialized]
	public bool IsWalking;

	public WalkPreset[] Presets;

	[FormerlySerializedAs("CameraPresets")]
	public WalkPreset[] OverweightPresets;

	public GClass2793[][] PresetProcessors;

	public GClass2793[][] OverweightPresetProcessors;

	public Vector2[] IntensityMinMax = new Vector2[2]
	{
		new Vector2(0.5f, 1.33f),
		new Vector2(0.5f, 0.7f)
	};

	public float Overweight;

	public EWalkPreset CurrentWalkPreset;

	[NonSerialized]
	public float Speed_1;

	[field: NonSerialized]
	public Transform Transform { get; set; }

	public float Speed
	{
		get
		{
			return Speed_1;
		}
		set
		{
			Speed_1 = value;
			Vector2 vector = ((CurrentWalkPreset == EWalkPreset.duck) ? IntensityMinMax[1] : IntensityMinMax[0]);
			Intensity = Mathf.Lerp(vector.x, vector.y, Speed_1);
		}
	}

	public void Initialize(PlayerSpring playerSpring)
	{
		Transform = playerSpring.TrackingTransform;
		PresetProcessors = Presets.Select((WalkPreset p) => p.Curves.Select((AnimVal curve) => new GClass2793(curve)).ToArray()).ToArray();
		OverweightPresetProcessors = OverweightPresets.Select((WalkPreset p) => p.Curves.Select((AnimVal curve) => new GClass2793(curve)).ToArray()).ToArray();
		foreach (GClass2793 item in PresetProcessors.SelectMany((GClass2793[] presets) => presets))
		{
			item.Initialize(playerSpring.CameraRotation, playerSpring.HandsPosition, playerSpring.HandsRotation, Intensity, StepFrequency, isHeadbobbing: true);
		}
		foreach (GClass2793 item2 in OverweightPresetProcessors.SelectMany((GClass2793[] presets) => presets))
		{
			item2.Initialize(playerSpring.CameraRotation, playerSpring.HandsPosition, playerSpring.HandsRotation, Intensity, StepFrequency, isHeadbobbing: true);
		}
	}

	public void OnStop()
	{
		if (PresetProcessors != null)
		{
			GClass2793[] array = PresetProcessors[(int)CurrentWalkPreset];
			foreach (GClass2793 obj in array)
			{
				obj.SetupParentValues(Intensity, StepFrequency);
				obj.method_0();
			}
			array = OverweightPresetProcessors[(int)CurrentWalkPreset];
			foreach (GClass2793 obj2 in array)
			{
				obj2.SetupParentValues(Intensity, StepFrequency);
				obj2.method_0();
			}
		}
	}

	public void Process(float deltaTime)
	{
		GClass2793[] array = PresetProcessors[(int)CurrentWalkPreset];
		for (int i = 0; i < array.Length; i++)
		{
			array[i].ProcessRaw(deltaTime * StepFrequency, Intensity);
		}
		if (Overweight > 0f)
		{
			array = OverweightPresetProcessors[(int)CurrentWalkPreset];
			for (int i = 0; i < array.Length; i++)
			{
				array[i].ProcessRaw(deltaTime * StepFrequency, Overweight);
			}
		}
	}

	public string DebugOutput()
	{
		throw new NotImplementedException();
	}

	public void AdjustPose()
	{
		Speed = Speed_1;
	}
}
