using System;
using System.Collections.Generic;
using EFT.Interactive;
using JetBrains.Annotations;
using MultiFlare;
using UnityEngine;

public class ControlledLampGroup : ISyncAble
{
	public enum EMaterialEmissionType
	{
		EmissionVisibility,
		EmissionColor
	}

	public struct Struct72
	{
		public EMaterialEmissionType EmissionType;

		public Color InitialEmissionColor;

		public float InitialEmission;

		public MaterialPropertyBlock MaterialPropertyBlock;

		[CanBeNull]
		public LampController ParentLampController;
	}

	[SerializeField]
	private LampController[] _lampControllers;

	[SerializeField]
	private CullingAdvancedLightObject[] _cullingAdvancedLightObjects;

	[SerializeField]
	private CullingLightObject[] _cullingLightObjects;

	[SerializeField]
	private Light[] _lights;

	[SerializeField]
	private MeshRenderer[] _meshRenderers;

	[SerializeField]
	private VolumetricLight[] _volumetricLights;

	[SerializeField]
	private BaseLight[] _baseLights;

	[Tooltip("Drag root light object here")]
	[SerializeField]
	private GameObject _fillLightFrom;

	[SerializeField]
	private AudioClip _turnOnSFX;

	[SerializeField]
	private AudioClip _turnOffSFX;

	[SerializeField]
	private Transform _sfxRootPosition;

	[SerializeField]
	private int _soundRolloff = 20;

	[SerializeField]
	private EOcclusionTest _occlusionTest;

	[SerializeField]
	private FlareLight[] _multiFlareLights;

	[SerializeField]
	private bool _initialLightState;

	private bool bool_0;

	private bool bool_1;

	private static readonly int int_1 = Shader.PropertyToID("_EmissionVisibility");

	private static readonly int int_2 = Shader.PropertyToID("_EmissionColor");

	private readonly Dictionary<int, List<Struct72>> dictionary_0 = new Dictionary<int, List<Struct72>>();

	private readonly Dictionary<int, float> dictionary_1 = new Dictionary<int, float>();

	private Color color_0 = new Color(0f, 0f, 0f, 0f);

	public ulong ID => GetSceneId();

	public void Start()
	{
		method_2();
		if (!bool_1)
		{
			LampController[] lampControllers = _lampControllers;
			foreach (LampController obj in lampControllers)
			{
				obj.LampState = Turnable.EState.On;
				obj.TurnLights(on: true, force: true);
				obj.OnLampDestroy += method_1;
			}
			method_8(_initialLightState);
		}
	}

	public void method_1()
	{
		method_8(bool_0);
	}

	public void method_2()
	{
		dictionary_0.Clear();
		dictionary_1.Clear();
		MeshRenderer[] meshRenderers = _meshRenderers;
		foreach (MeshRenderer meshRenderer in meshRenderers)
		{
			int instanceID = meshRenderer.GetInstanceID();
			if (!dictionary_0.ContainsKey(instanceID))
			{
				LampController componentInParent = meshRenderer.transform.GetComponentInParent<LampController>();
				List<Struct72> list = new List<Struct72>();
				Material[] sharedMaterials = meshRenderer.sharedMaterials;
				foreach (Material sharedMaterial in sharedMaterials)
				{
					method_3(list, sharedMaterial, componentInParent);
				}
				dictionary_0.Add(instanceID, list);
			}
		}
		Light[] lights = _lights;
		foreach (Light light in lights)
		{
			dictionary_1[light.GetInstanceID()] = light.intensity;
		}
		BaseLight[] baseLights = _baseLights;
		foreach (BaseLight baseLight in baseLights)
		{
			dictionary_1[baseLight.GetInstanceID()] = baseLight.m_Intensity;
		}
		if (_cullingAdvancedLightObjects == null)
		{
			_cullingAdvancedLightObjects = Array.Empty<CullingAdvancedLightObject>();
		}
		if (_cullingLightObjects == null)
		{
			_cullingLightObjects = Array.Empty<CullingLightObject>();
		}
		if (_lights == null)
		{
			_lights = Array.Empty<Light>();
		}
		if (_baseLights == null)
		{
			_baseLights = Array.Empty<BaseLight>();
		}
		if (_volumetricLights == null)
		{
			_volumetricLights = Array.Empty<VolumetricLight>();
		}
		if (_meshRenderers == null)
		{
			_meshRenderers = Array.Empty<MeshRenderer>();
		}
		if (_lampControllers == null)
		{
			_lampControllers = Array.Empty<LampController>();
		}
		if (_multiFlareLights == null)
		{
			_multiFlareLights = Array.Empty<FlareLight>();
		}
	}

	public void method_3(List<Struct72> materialEmissionInfos, Material sharedMaterial, LampController lampController)
	{
		bool flag = sharedMaterial.HasProperty(int_1);
		if (sharedMaterial.HasProperty(int_2) || flag)
		{
			if (flag)
			{
				method_4(materialEmissionInfos, sharedMaterial, lampController);
			}
			else
			{
				method_5(materialEmissionInfos, sharedMaterial, lampController);
			}
		}
	}

	public void method_4(List<Struct72> materialEmissionInfos, Material sharedMaterial, LampController lampController)
	{
		materialEmissionInfos.Add(new Struct72
		{
			InitialEmission = sharedMaterial.GetFloat(int_1),
			MaterialPropertyBlock = new MaterialPropertyBlock(),
			EmissionType = EMaterialEmissionType.EmissionVisibility,
			ParentLampController = lampController
		});
	}

	public void method_5(List<Struct72> materialEmissionInfos, Material sharedMaterial, LampController lampController)
	{
		materialEmissionInfos.Add(new Struct72
		{
			InitialEmissionColor = sharedMaterial.GetColor(int_2),
			MaterialPropertyBlock = new MaterialPropertyBlock(),
			EmissionType = EMaterialEmissionType.EmissionColor,
			ParentLampController = lampController
		});
	}

	public void ToggleState()
	{
		method_8(!bool_0);
	}

	public void SetLightStateExternal(bool state)
	{
		method_8(state);
	}

	public void method_6(bool nextState)
	{
		method_7(nextState ? _turnOnSFX : _turnOffSFX);
	}

	public void method_7(AudioClip clip)
	{
		if (!(clip == null))
		{
			Vector3 position = base.transform.position;
			if (_sfxRootPosition != null)
			{
				position = _sfxRootPosition.position;
			}
			float distance = CameraClass.Instance.Distance(position);
			BetterSource betterSource = MonoBehaviourSingleton<BetterAudio>.Instance.PlayAtPoint(position, clip, distance, BetterAudio.AudioSourceGroupType.PropagatedExclusive, _soundRolloff, 1f, _occlusionTest, null, forceStereo: true);
			if (!(betterSource == null))
			{
				betterSource.SetBaseVolume(1f);
			}
		}
	}

	public void method_8(bool state)
	{
		if (bool_0 != state)
		{
			method_6(state);
		}
		bool_0 = state;
		float num = (state ? 1f : 0f);
		CullingAdvancedLightObject[] cullingAdvancedLightObjects = _cullingAdvancedLightObjects;
		foreach (CullingAdvancedLightObject cullingAdvancedLightObject in cullingAdvancedLightObjects)
		{
			if (!(cullingAdvancedLightObject == null))
			{
				cullingAdvancedLightObject.IntensityMultiplier = num;
			}
		}
		FlareLight[] multiFlareLights = _multiFlareLights;
		foreach (FlareLight flareLight in multiFlareLights)
		{
			if (!(flareLight == null))
			{
				flareLight.SetAlpha(num);
			}
		}
		CullingLightObject[] cullingLightObjects = _cullingLightObjects;
		foreach (CullingLightObject cullingLightObject in cullingLightObjects)
		{
			if (!(cullingLightObject == null))
			{
				cullingLightObject.IntensityMultiplier = num;
			}
		}
		Light[] lights = _lights;
		foreach (Light light in lights)
		{
			if (!(light == null))
			{
				float value = 0f;
				if (state)
				{
					dictionary_1.TryGetValue(light.GetInstanceID(), out value);
				}
				light.enabled = state;
				light.intensity = value;
			}
		}
		BaseLight[] baseLights = _baseLights;
		foreach (BaseLight baseLight in baseLights)
		{
			if (!(baseLight == null))
			{
				float value2 = 0f;
				if (state)
				{
					dictionary_1.TryGetValue(baseLight.GetInstanceID(), out value2);
				}
				baseLight.m_Intensity = value2;
			}
		}
		VolumetricLight[] volumetricLights = _volumetricLights;
		foreach (VolumetricLight volumetricLight in volumetricLights)
		{
			if (!(volumetricLight == null))
			{
				volumetricLight.enabled = state;
			}
		}
		MeshRenderer[] meshRenderers = _meshRenderers;
		foreach (MeshRenderer meshRenderer in meshRenderers)
		{
			int infoIndex = 0;
			if (!dictionary_0.TryGetValue(meshRenderer.GetInstanceID(), out var value3))
			{
				continue;
			}
			for (int j = 0; j < meshRenderer.sharedMaterials.Length; j++)
			{
				Material sharedMaterial = meshRenderer.sharedMaterials[j];
				if (value3.Count <= infoIndex)
				{
					break;
				}
				Struct72 materialInfo = value3[infoIndex];
				bool state2 = state;
				if (materialInfo.ParentLampController != null && materialInfo.ParentLampController.LampState == Turnable.EState.Destroyed)
				{
					state2 = false;
				}
				if (materialInfo.EmissionType == EMaterialEmissionType.EmissionVisibility)
				{
					method_9(meshRenderer, sharedMaterial, j, materialInfo, state2, ref infoIndex);
				}
				else if (materialInfo.EmissionType == EMaterialEmissionType.EmissionColor)
				{
					method_10(meshRenderer, sharedMaterial, j, materialInfo, state2, ref infoIndex);
				}
			}
		}
	}

	public void method_9(MeshRenderer meshRenderer, Material sharedMaterial, int index, Struct72 materialInfo, bool state, ref int infoIndex)
	{
		if (sharedMaterial.HasProperty(int_1))
		{
			MaterialPropertyBlock materialPropertyBlock = materialInfo.MaterialPropertyBlock;
			float value = (state ? materialInfo.InitialEmission : 0f);
			materialPropertyBlock.SetFloat(int_1, value);
			meshRenderer.SetPropertyBlock(materialPropertyBlock, index);
			infoIndex++;
		}
	}

	public void method_10(MeshRenderer meshRenderer, Material sharedMaterial, int index, Struct72 materialInfo, bool state, ref int infoIndex)
	{
		if (sharedMaterial.HasProperty(int_2))
		{
			MaterialPropertyBlock materialPropertyBlock = materialInfo.MaterialPropertyBlock;
			Color value = (state ? materialInfo.InitialEmissionColor : color_0);
			materialPropertyBlock.SetColor(int_2, value);
			meshRenderer.SetPropertyBlock(materialPropertyBlock, index);
			infoIndex++;
		}
	}

	public override void Serialize(GInterface131 writerStream)
	{
		writerStream.Write(bool_0);
	}

	public override void Deserialize(IDataReader readerStream)
	{
		bool_0 = readerStream.ReadBool();
		bool_1 = true;
		method_8(bool_0);
	}
}
