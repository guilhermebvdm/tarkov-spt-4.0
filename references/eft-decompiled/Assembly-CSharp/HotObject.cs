using System;
using System.Collections.Generic;
using Systems.Effects;
using Comfort.Common;
using UnityEngine;

[DefaultExecutionOrder(100)]
[RequireComponent(typeof(Renderer))]
public class HotObject : MonoBehaviour
{
	[Header("Выставляет температуру конкретному объекту")]
	[Tooltip("Применить ко всем материалам на Renderer")]
	[SerializeField]
	public bool IsApplyAllMaterials;

	[Tooltip("(min, max, factor)")]
	[SerializeField]
	public Vector3 Temperature = new Vector3(0.1f, 1f, 3.5f);

	[Tooltip("Множитель Temperature.z для управления температуры из скрипта")]
	[SerializeField]
	public float TemperatureCelsio = 29f;

	[Tooltip("Порядковый номер материала в Renderer к которым необходимо применить температуру")]
	[SerializeField]
	private List<int> materialsId = new List<int>();

	[SerializeField]
	public Bounds HeatBounds = new Bounds(new Vector3(0f, 0f, 0f), new Vector3(0.03f, 0.08f, 0.03f));

	[SerializeField]
	public float VisibleHeatAlpha = 1f;

	[Space]
	[Header("Эффект HeatHaze")]
	[SerializeField]
	public bool UseHeatHaze;

	[SerializeField]
	public Bounds HeatHazeBounds = new Bounds(new Vector3(0f, 0f, 0f), new Vector3(0.018f, 0.06f, 0.018f));

	[SerializeField]
	private Vector2 HeatParticleLifetimeDelta = new Vector2(0.3f, 0.5f);

	[SerializeField]
	private AnimationCurve HeatParticleCountByTemp = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 20f));

	[SerializeField]
	private AnimationCurve HeatParticleLifeTimeByTemp = new AnimationCurve(new Keyframe(0f, 0.15f), new Keyframe(1f, 0.3f));

	[SerializeField]
	private AnimationCurve HeatParticleSizeByTemp = new AnimationCurve(new Keyframe(0f, 0.01f), new Keyframe(1f, 0.14f));

	private const float float_0 = 0.5f;

	private const float float_1 = 5f;

	private const float float_2 = 4f;

	private static readonly int int_0 = Shader.PropertyToID("_HeatSize");

	private static readonly int int_1 = Shader.PropertyToID("_HeatTemp");

	private static readonly int int_2 = Shader.PropertyToID("_HeatCenter");

	private static readonly int int_3 = Shader.PropertyToID("_Temperature");

	private static readonly int int_4 = Shader.PropertyToID("_Temperature2");

	private static readonly int int_5 = Shader.PropertyToID("_HeatVisibleFactor");

	private static readonly int int_6 = Shader.PropertyToID("_HeatThermalFactor");

	private static readonly int int_7 = Shader.PropertyToID("_HeatVisible");

	private static readonly string string_0 = "USEHEAT";

	private MaterialPropertyBlock materialPropertyBlock_0;

	private Renderer renderer_0;

	private Vector3 vector3_0;

	private int int_8;

	private float float_3;

	private int int_9;

	private Transform transform_0;

	private float float_4;

	private float float_5;

	private float float_6;

	private float float_7;

	private bool bool_0;

	public float Single_0 => Singleton<BackendConfigSettingsClass>.Instance?.Overheat.HeatVisibleFactor ?? 1f;

	public float Single_1 => Singleton<BackendConfigSettingsClass>.Instance?.Overheat.HeatThermalFactor ?? 1f;

	public void Awake()
	{
		bool_0 = false;
		method_0();
		vector3_0 = transform_0.position;
		float_3 = Time.time;
		if (IsApplyAllMaterials)
		{
			int_9 = (renderer_0 ? renderer_0.sharedMaterials.Length : 0);
		}
		else
		{
			int_9 = materialsId.Count;
		}
		SetTemperatureToRenderer(TemperatureCelsio, force: true);
	}

	public void method_0()
	{
		if (!bool_0)
		{
			if (renderer_0 == null)
			{
				renderer_0 = GetComponent<Renderer>();
			}
			if (materialPropertyBlock_0 == null)
			{
				materialPropertyBlock_0 = new MaterialPropertyBlock();
			}
			if (transform_0 == null)
			{
				transform_0 = base.transform;
			}
			bool_0 = true;
		}
	}

	public static float ConvertHeat2Celsio(float heatTemp)
	{
		return 10f * heatTemp / 220f + 30f;
	}

	public float method_1(float celcio)
	{
		return 0.6f + (celcio - 30.54f) / 6.5999985f;
	}

	public float method_2(float celcio)
	{
		return Mathf.Max(0f, celcio - 30f) / 5.5999985f;
	}

	[ContextMenu("SetTemperatureToRenderer")]
	public void DebugSetTemperatureToRenderer()
	{
		SetTemperatureToRenderer();
	}

	public void SetTemperatureToRenderer()
	{
		SetTemperatureToRenderer(TemperatureCelsio, force: true);
	}

	public void SetTemperatureToRenderer(float temperatureCelsio, bool force = false)
	{
		if ((!force && Math.Abs(temperatureCelsio - TemperatureCelsio) < Mathf.Epsilon) || int_9 == 0 || !renderer_0)
		{
			return;
		}
		method_0();
		float num = method_1(temperatureCelsio);
		Vector3 temperature = Temperature;
		temperature.z *= num;
		temperature.x *= num;
		materialPropertyBlock_0.SetVector(int_3, temperature);
		materialPropertyBlock_0.SetVector(int_4, temperature);
		materialPropertyBlock_0.SetVector(int_2, HeatBounds.center);
		materialPropertyBlock_0.SetVector(int_0, HeatBounds.size);
		materialPropertyBlock_0.SetFloat(int_1, temperatureCelsio);
		materialPropertyBlock_0.SetFloat(int_5, Single_0);
		materialPropertyBlock_0.SetFloat(int_6, Single_1);
		materialPropertyBlock_0.SetFloat(int_7, VisibleHeatAlpha);
		if (IsApplyAllMaterials)
		{
			for (int i = 0; i < int_9; i++)
			{
				renderer_0.SetPropertyBlock(materialPropertyBlock_0, i);
			}
		}
		else
		{
			for (int j = 0; j < int_9; j++)
			{
				renderer_0.SetPropertyBlock(materialPropertyBlock_0, materialsId[j]);
			}
		}
		TemperatureCelsio = temperatureCelsio;
		float_4 = method_2(temperatureCelsio);
		float_5 = HeatParticleCountByTemp?.Evaluate(float_4) ?? 0f;
		float_6 = HeatParticleLifeTimeByTemp?.Evaluate(float_4) ?? 0f;
		float_7 = HeatParticleSizeByTemp?.Evaluate(float_4) ?? 0f;
	}

	public static bool NeedProcessEffects(Vector3 camPos, Vector3 pos)
	{
		return !(Vector3.SqrMagnitude(camPos - pos) > 25f);
	}

	public void ManualSyncEffects()
	{
		if (!(float_5 < 4f))
		{
			Vector3 velocity = (transform_0.position - vector3_0) * 0.5f / Time.deltaTime;
			float num = 1f / float_5;
			int num2 = Mathf.FloorToInt((Time.time - float_3) / num);
			for (int i = 0; i < (int)float_5 * num2; i++)
			{
				float x = UnityEngine.Random.Range(HeatHazeBounds.min.x, HeatHazeBounds.max.x);
				float y = UnityEngine.Random.Range(HeatHazeBounds.min.y, HeatHazeBounds.max.y);
				float z = UnityEngine.Random.Range(HeatHazeBounds.min.z, HeatHazeBounds.max.z);
				Vector3 position = transform_0.TransformPoint(new Vector3(x, y, z));
				Singleton<Effects>.Instance.MuzzleEffect.Heat.Emit(new ParticleSystem.EmitParams
				{
					position = position,
					velocity = velocity,
					startSize = float_7,
					startLifetime = float_6 + UnityEngine.Random.Range(HeatParticleLifetimeDelta.x, HeatParticleLifetimeDelta.y)
				}, 1);
			}
			float_3 += (float)num2 * num;
		}
	}

	public void SyncPosition()
	{
		if ((bool)transform_0)
		{
			vector3_0 = transform_0.position;
			int_8 = Time.frameCount;
		}
	}

	public void PrepareForEffects()
	{
		if (Time.frameCount - int_8 > 30)
		{
			SyncPosition();
		}
	}
}
