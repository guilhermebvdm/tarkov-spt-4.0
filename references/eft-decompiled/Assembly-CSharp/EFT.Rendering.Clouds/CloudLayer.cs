using System;
using UnityEngine;

namespace EFT.Rendering.Clouds;

[CreateAssetMenu(menuName = "Settings/Clouds/Create CloudLayer Settings", fileName = "CloudLayerSettings", order = 0)]
public class CloudLayer : CloudSettings
{
	[Serializable]
	public class CloudMap
	{
		[NonSerialized]
		public static Texture2D DefaultTexture;

		[Tooltip("Specify the texture HDRP uses to render the clouds ( in LatLong layout).")]
		public Texture2D CloudTexture = DefaultTexture;

		[NonSerialized]
		[Tooltip("Opacity multiplier for the red channel.")]
		[Range(0f, 1f)]
		public float OpacityR = 1f;

		[NonSerialized]
		[Tooltip("Opacity multiplier for the green channel .")]
		[Range(0f, 1f)]
		public float OpacityG;

		[NonSerialized]
		[Tooltip("Opacity multiplier for the  blue channel.")]
		[Range(0f, 1f)]
		public float OpacityB;

		[NonSerialized]
		[Tooltip("Opacity multiplier for the alpha channel.")]
		[Range(0f, 1f)]
		public float OpacityA;

		[Tooltip("Altitude of the bottom of the cloud layer in meters.")]
		[Min(0f)]
		public float Altitude = 2000f;

		[Tooltip("Sets the rotation of the clouds (in degrees).")]
		[Range(0f, 360f)]
		public float Rotation;

		[Tooltip("Specifies the color HDRP uses to tint the clouds.")]
		public Color Tint = Color.white;

		[InspectorName("Exposure Compensation")]
		[Tooltip("Sets the exposure compensation of the clouds in EV.")]
		public float Exposure;

		[InspectorName("Orientation")]
		[Tooltip("Controls the orientation of the wind relative  to the X world vector.\nThis value can be relative to the Global Wind Orientation defined in the Visual Environment.")]
		[Range(0f, 360f)]
		public float ScrollOrientation = 180f;

		[InspectorName("Min Speed")]
		[Tooltip("Sets the minimal wind speed in kilometers per hour.")]
		public float MinScrollSpeed = 40f;

		[InspectorName("Max Speed")]
		[Tooltip("Sets the maximal wind speed in kilometers per hour.")]
		public float MaxScrollSpeed = 200f;

		[NonSerialized]
		[HideInInspector]
		public float ScrollSpeed;

		[InspectorName("Raymarching")]
		[Tooltip("Simulates cloud self-shadowing using raymarching.")]
		public bool Lighting = true;

		[NonSerialized]
		[Tooltip("Number of raymarching steps.")]
		[Range(2f, 32f)]
		public int Steps = 6;

		[InspectorName("Density")]
		[Tooltip("Density of the cloud layer.")]
		[Range(0f, 1f)]
		public float Thickness = 0.5f;

		[Tooltip("Controls the influence of the ambient probe on the cloud layer volume. A lower value will suppress the ambient light  and produce darker clouds overall.")]
		[Range(0f, 1f)]
		public float AmbientProbeDimmer = 1f;

		[NonSerialized]
		[Tooltip("Projects a portion of the clouds around the sun  light to simulate cloud shadows. This will override the cookie of your directional light.")]
		public bool CastShadows = true;

		[NonSerialized]
		public float ScrollFactor;

		public int Int32_0
		{
			get
			{
				if (!Lighting)
				{
					return 0;
				}
				return Steps;
			}
		}

		public Vector4 Vector4_0 => new Vector4(OpacityR, OpacityG, OpacityB, OpacityA);

		public Color Color_0 => Tint * GClass2385.ConvertEv100ToExposure(0f - Exposure);

		public Vector4 method_0()
		{
			float f = MathF.PI / 180f * ScrollOrientation;
			return new Vector3(0f - Mathf.Cos(f), 0f - Mathf.Sin(f), ScrollFactor);
		}

		public (Vector4, Vector4) method_1()
		{
			return new ValueTuple<Vector4, Vector4>(item2: new Vector4(Rotation / 360f, Int32_0, Thickness * 0.095f + 0.005f, Altitude), item1: Vector4_0);
		}

		public int method_2()
		{
			int num = 17;
			num = 391 + CloudTexture.GetHashCode();
			num = num * 23 + OpacityR.GetHashCode();
			num = num * 23 + OpacityG.GetHashCode();
			num = num * 23 + OpacityB.GetHashCode();
			num = num * 23 + OpacityA.GetHashCode();
			num = num * 23 + Rotation.GetHashCode();
			num = num * 23 + CastShadows.GetHashCode();
			if (Lighting)
			{
				num = num * 23 + Lighting.GetHashCode();
				num = num * 23 + Steps.GetHashCode();
				num = num * 23 + Altitude.GetHashCode();
				num = num * 23 + Thickness.GetHashCode();
			}
			return num;
		}

		public override int GetHashCode()
		{
			return (((((((((((((((391 + CloudTexture.GetHashCode()) * 23 + OpacityR.GetHashCode()) * 23 + OpacityG.GetHashCode()) * 23 + OpacityB.GetHashCode()) * 23 + OpacityA.GetHashCode()) * 23 + Altitude.GetHashCode()) * 23 + Rotation.GetHashCode()) * 23 + Tint.GetHashCode()) * 23 + Exposure.GetHashCode()) * 23 + ScrollOrientation.GetHashCode()) * 23 + ScrollSpeed.GetHashCode()) * 23 + Lighting.GetHashCode()) * 23 + Steps.GetHashCode()) * 23 + Thickness.GetHashCode()) * 23 + AmbientProbeDimmer.GetHashCode()) * 23 + CastShadows.GetHashCode();
		}
	}

	private const int int_0 = 3;

	public const float DefaultPlanetRadius = 6378100f;

	[Tooltip("Controls the global opacity of the cloud layer.")]
	[Range(0f, 1f)]
	public float Opacity = 1f;

	[NonSerialized]
	[Tooltip("Specifies the resolution of the texture HDRP  uses to represent the clouds.")]
	public CloudResolution Resolution = CloudResolution.CloudResolution2048;

	[Header("Cloud Shadows")]
	[Tooltip("Controls the opacity of the cloud shadows.")]
	public float ShadowMultiplier = 1f;

	[Tooltip("Controls the tint of the cloud shadows.")]
	public Color ShadowTint = Color.black;

	[NonSerialized]
	[Tooltip("Specifies the resolution of the texture HDRP uses to represent the cloud shadows.")]
	public CloudShadowsResolution ShadowResolution = CloudShadowsResolution.Medium;

	[Tooltip("Specifies the size of the projected shadows.")]
	public float ShadowSize = 500f;

	[Tooltip("Specifies the position of the planet and its radius")]
	public Vector4 PlanetCenterRadius = new Vector4(0f, -6378100f, 0f, 6378100f);

	public CloudMap LayerA = new CloudMap();

	public bool Boolean_0 => LayerA.CastShadows;

	public static Vector3Int smethod_0(Vector3 vec, int tolerance)
	{
		int x = smethod_1((int)vec.x, tolerance);
		int y = smethod_1((int)vec.y, tolerance);
		int z = smethod_1((int)vec.z, tolerance);
		return new Vector3Int(x, y, z);
	}

	public static int smethod_1(int value, int tolerance)
	{
		return value / tolerance * tolerance;
	}

	public int method_0(Light sunLight)
	{
		int num = 17;
		bool lighting = LayerA.Lighting;
		bool flag = sunLight != null && LayerA.CastShadows;
		num = num * 23 + Resolution.GetHashCode();
		num = num * 23 + LayerA.method_2();
		if (lighting && sunLight != null)
		{
			num = num * 23 + smethod_0(sunLight.transform.rotation.eulerAngles, 3).GetHashCode();
		}
		if (flag)
		{
			num = num * 23 + ShadowResolution.GetHashCode();
		}
		return num;
	}

	public override int GetHashCode()
	{
		return ((391 + Opacity.GetHashCode()) * 23 + Resolution.GetHashCode()) * 23 + LayerA.GetHashCode();
	}

	[RuntimeInitializeOnLoadMethod]
	public static void smethod_2()
	{
		CloudMap.DefaultTexture = CloudDataDefaultResources.Instance.DefaultCloudMap;
	}
}
