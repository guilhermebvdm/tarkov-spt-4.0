using UnityEngine;
using UnityEngine.UI;

namespace GPUInstancer;

public class TerrainGenerator : MonoBehaviour
{
	public Texture2D groundTexture;

	public Texture2D detailTexture;

	public GameObject FpsController;

	public GameObject FixedCamera;

	private int int_0 = 32;

	private int int_1;

	private Vector3 vector3_0;

	private Vector3 vector3_1;

	private Terrain[] terrain_0;

	private bool bool_0 = true;

	private Color color_0 = new Color(0.263f, 0.976f, 0.165f, 1f);

	private Color color_1 = new Color(0.804f, 0.737f, 0.102f, 1f);

	private float float_0 = 0.2f;

	private float float_1 = 0.5f;

	private float float_2 = 0.5f;

	private float float_3 = 0.6f;

	private bool bool_1;

	private float float_4 = 0.5f;

	private float float_5 = 0.5f;

	private float float_6 = 0.5f;

	private Color color_2 = new Color(32f / 51f, 0.32156864f, 0.1764706f, 1f);

	private bool bool_2 = true;

	private bool bool_3;

	private int int_2 = 2;

	private float float_7 = 50f;

	private Vector4 vector4_0 = new Vector4(0.5f, 3f, 0.5f, 3f);

	private bool bool_4;

	private bool bool_5 = true;

	private float float_8 = 0.2f;

	private float float_9 = 250f;

	private Image image_0;

	private Image image_1;

	private Slider slider_0;

	private Slider slider_1;

	private Slider slider_2;

	private Slider slider_3;

	private Toggle toggle_0;

	private Slider slider_4;

	private Slider slider_5;

	private Slider slider_6;

	private Image image_2;

	private Toggle toggle_1;

	private Toggle toggle_2;

	private Slider slider_7;

	private Slider slider_8;

	private InputField inputField_0;

	private InputField inputField_1;

	private InputField inputField_2;

	private InputField inputField_3;

	private Toggle toggle_3;

	private Toggle toggle_4;

	private Slider slider_9;

	private Slider slider_10;

	private Text text_0;

	private Text text_1;

	private Selectable selectable_0;

	private Selectable selectable_1;

	private Canvas canvas_0;

	public static readonly string HELPTEXT_detailHealthyColor = "\"Healthy\" color of the the Healthy / Dry noise variation for the prototypes that use the \"GPUInstancer\\Foliage\" shader. This corresponds to the \"Healhy Color\" property of the detail prototypes in the Unity terrain. The Healthy / Dry noise texture can be changed globally (terrain specific) for all detail prototypes that use the \"GPUInstancer\\Foliage\" shader from the \"Healthy/Dry Noise Texture\" property of the detail manager.";

	public static readonly string HELPTEXT_detailDryColor = "\"Dry\" color of the the Healthy / Dry noise variation for the prototypes that use the \"GPUInstancer\\Foliage\" shader. This corresponds to the \"Dry Color\" property of the detail prototypes in the Unity terrain. The Healthy / Dry noise texture can be changed globally (terrain specific) for all detail prototypes that use the \"GPUInstancer\\Foliage\" shader from the \"Healthy/Dry Noise Texture\" property of the detail manager.";

	public static readonly string HELPTEXT_noiseSpread = "The \"Noise Spread\" property specifies the size of the \"Healthy\" and \"Dry\" patches for the Detail Prototypes that use the \"GPUInstancer\\Foliage\" shader. This corresponds to the \"Noise Spread\" propety of the detail prototypes in the Unity terrain. A higher number results in smaller patches. The Healthy / Dry noise texture can be changed globally (terrain specific) for all detail prototypes that use the \"GPUInstancer\\Foliage\" shader from the \"Healthy/Dry Noise Texture\" property of the detail manager.";

	public static readonly string HELPTEXT_ambientOcclusion = "The amount of \"Ambient Occlusion\" to apply to the objects that use the \"GPUInstancer\\Foliage\" shader.";

	public static readonly string HELPTEXT_gradientPower = "\"GPUInstancer\\Foliage\" shader provides an option to darken the lower regions of the mesh that uses it. This results in a more realistic look for foliage. You can set the amount of this darkening effect with the \"Gradient Power\" property, or turn it off completly by setting the slider to zero.";

	public static readonly string HELPTEXT_windIdleSway = "\"Wind Idle Sway\" specifies the amount of idle wind animation for the detail prototypes that use the \"GPUInstancer\\Foliage\" shader. This is the wind animation that occurs where wind waves are not present. Turning the slider down to zero disables this idle wind animation.";

	public static readonly string HELPTEXT_windWavesOn = "The \"Wind Waves\" toggle specifies whether there will be wind waves for the detail prototypes that use the \"GPUInstancer\\Foliage\" shader. The normals texture that is used to calculate winds can be changed globally (terrain specific) for all detail prototypes that use the \"GPUInstancer\\Foliage\" shader from the \"Wind Wave Normal Texture\" property of the detail manager. ";

	public static readonly string HELPTEXT_windWaveTintColor = "\"Wind Wave Tint Color\" is a shader property that acts similar to the \"Grass Tint\" property of the Unity terrain, except it applies on a per-prototype basis. This color applies to the \"Wind Wave Tint\" properties of all the objects that use the \"GPUInstancer\\Foliage\" shader.";

	public static readonly string HELPTEXT_windWaveSize = "\"Wind Wave Size\" specifies the size of the wind waves for the detail prototypes that use the \"GPUInstancer\\Foliage\" shader.";

	public static readonly string HELPTEXT_windWaveSway = "\"Wind Wave Sway\" specifies the amount of wind animation that is applied by the wind waves for the detail prototypes that use the \"GPUInstancer\\Foliage\" shader. This is the wind animation that occurs in addition to the idle wind animation. Turning the slider down to zero disables this extra wave animation.";

	public static readonly string HELPTEXT_windWaveTint = "\"Wind Wave Tint\" specifies how much the \"Wind Wave Tint\" color applies to the wind wave effect for the detail prototypes that use the \"GPUInstancer\\Foliage\" shader. Turning the slider down to zero disables wind wave coloring.";

	public static readonly string HELPTEXT_isBillboard = "If \"Billboard\" is enabled, the generated mesh for this prototype will be billboarded. Note that billboarding will turn off automatically if cross quads are enabled.";

	public static readonly string HELPTEXT_crossQuads = "If \"Cross Quads\" is enabled, a mesh with multiple quads will be generated for this detail texture (instead of a single quad or billboard). The generated quads will be equiangular to each other. Cross quadding means more polygons for a given prototype, so turning this off will increase performance if there will be a huge number of instances of this detail prototype.";

	public static readonly string HELPTEXT_quadCount = "\"Cross Quad Count\" defines the number of generated equiangular quads for this detail texture. Using less quads will increase performance (especially where there will be a huge number of instances of this prototype).";

	public static readonly string HELPTEXT_billboardDistance = "When Cross Quads is enabled, \"Cross Quad Billboard Distance\" specifies the distance from the selected camera where the objects will be drawn as billboards to increase performance further. This is useful beacause at a certain distance, the difference between a multiple quad mesh and a billboard is barely visible. The value used here is similar to the screenRelativeTransitionHeight property of Unity LOD groups.";

	public static readonly string HELPTEXT_detailScale = "\"Detail Scale\" can be used to set a range for the instance sizes for detail prototypes. For the texture type detail prototypes, this range applies to the \"Healthy / Dry Noise Texture\" property of the detail manager. The values here correspond to the width and height values for the detail prototypes in the Unity terrain. \nX: Minimum Width - Y: Maximum Width\nZ: Minimum Height - W: Maximum Height";

	public static readonly string HELPTEXT_isShadowCasting = "The \"Shadow Casting\" toggle specifies whether the object will cast shadows or not. Shadow casting requires extra shadow passes in the shader resulting in additional rendering operations. GPU Instancer uses various techniques that boost the performance of these operations, but turning shadow casting off completely will increase performance.";

	public static readonly string HELPTEXT_isFrustumCulling = "The \"Frustum Culling\" toggle specifies whether the objects that are not in the selected camera's view frustum will be rendered or not. If enabled, GPU Instancer will not render the objects that are outside the selected camera's view frustum. This will increase performance. It is recommended to turn frustum culling on unless there are multiple cameras rendering the scene at the same time.";

	public static readonly string HELPTEXT_frustumOffset = "\"Frustum Offset\" defines the size of the area around the camera frustum planes within which objects will be rendered while frustum culling is enabled. GPU Instancer does frustum culling on the GPU which provides a performance boost. However, if there is a performance hit (usually while rendering an extreme amount of objects), and if the camera is moving very fast, rendering can lag behind camera movement. This could result in some objects not being rendered around the frustum edges. This offset expands the calculated frustum area so that the renderer can keep up.";

	public static readonly string HELPTEXT_maxDetailDistance = "\"Max Detail Distance\" defines the maximum distance from the camera within which the terrain details will be rendered. Details that are farther than the specified distance will not be visible. Note that this setting can also be limited globally (terrain specific) by the \"Max distance\" property of the detail manager itself.";

	public static readonly string HELPTEXT_windVector = "The \"Wind Vector\" specifies the [X, Z] vector (world axis) of the wind for all the prototypes (terrain specific) that use the \"GPUInstancer\\Foliage\" shader. This vector supplies both direction and magnitude information for wind.";

	public void Start()
	{
		method_2();
		if ((bool)FixedCamera)
		{
			FixedCamera.SetActive(value: true);
		}
		if ((bool)FpsController)
		{
			FpsController.SetActive(value: false);
		}
		int_1 = 0;
		vector3_0 = new Vector3(int_0, 0f, 0f);
		vector3_1 = new Vector3(0f, 0f, -int_0);
		terrain_0 = new Terrain[9];
		AddTerrain();
	}

	public void Update()
	{
		if (Input.GetKeyUp(KeyCode.Q))
		{
			AddTerrain();
		}
		if (Input.GetKeyUp(KeyCode.E))
		{
			RemoveTerrain();
		}
		if (Input.GetKeyUp(KeyCode.C))
		{
			method_4();
			bool_0 = !bool_0;
		}
		if (Input.GetKeyUp(KeyCode.U))
		{
			canvas_0.gameObject.SetActive(!canvas_0.gameObject.activeSelf);
		}
	}

	public void AddTerrain()
	{
		if (int_1 != 9)
		{
			method_5();
			method_0(terrain_0[int_1]);
			int_1++;
			method_3();
		}
	}

	public void RemoveTerrain()
	{
		if (int_1 != 0)
		{
			Object.Destroy(terrain_0[int_1 - 1].gameObject);
			int_1--;
			method_3();
		}
	}

	public void method_0(Terrain terrain)
	{
		GPUInstancerDetailManager gPUInstancerDetailManager = terrain.gameObject.AddComponent<GPUInstancerDetailManager>();
		GClass1257.SetupManagerWithTerrain(gPUInstancerDetailManager, terrain);
		if (gPUInstancerDetailManager.prototypeList.Count > 0)
		{
			GPUInstancerDetailPrototype obj = (GPUInstancerDetailPrototype)gPUInstancerDetailManager.prototypeList[0];
			obj.detailHealthyColor = color_0;
			obj.detailDryColor = color_1;
			obj.noiseSpread = float_0;
			obj.ambientOcclusion = float_1;
			obj.gradientPower = float_2;
			obj.windIdleSway = float_3;
			obj.windWavesOn = bool_1;
			obj.windWaveTint = float_4;
			obj.windWaveSway = float_6;
			obj.windWaveTintColor = color_2;
			obj.isBillboard = bool_2;
			obj.useCrossQuads = bool_3;
			obj.quadCount = int_2;
			obj.billboardDistance = float_7;
			obj.detailScale = vector4_0;
			obj.isShadowCasting = bool_4;
			obj.isFrustumCulling = bool_5;
			obj.maxDistance = float_9;
		}
		GClass1257.InitializeGPUInstancer(gPUInstancerDetailManager);
	}

	public void method_1()
	{
		for (int i = 0; i < int_1; i++)
		{
			GPUInstancerDetailManager component = terrain_0[i].GetComponent<GPUInstancerDetailManager>();
			for (int j = 0; j < component.prototypeList.Count; j++)
			{
				GPUInstancerDetailPrototype obj = (GPUInstancerDetailPrototype)component.prototypeList[j];
				obj.detailHealthyColor = color_0;
				obj.detailDryColor = color_1;
				obj.noiseSpread = float_0;
				obj.ambientOcclusion = float_1;
				obj.gradientPower = float_2;
				obj.windIdleSway = float_3;
				obj.windWavesOn = bool_1;
				obj.windWaveTint = float_4;
				obj.windWaveSway = float_6;
				obj.windWaveTintColor = color_2;
				obj.isBillboard = !bool_3 && bool_2;
				obj.useCrossQuads = bool_3;
				obj.quadCount = int_2;
				obj.billboardDistance = float_7;
				obj.detailScale = vector4_0;
				obj.isShadowCasting = bool_4;
				obj.isFrustumCulling = bool_5;
				obj.maxDistance = float_9;
			}
			GClass1257.UpdateDetailInstances(component, updateMeshes: true);
		}
	}

	public void ReInitializeManagers()
	{
		for (int i = 0; i < int_1; i++)
		{
			GClass1257.InitializeGPUInstancer(terrain_0[i].GetComponent<GPUInstancerDetailManager>());
		}
	}

	public void method_2()
	{
		canvas_0 = GameObject.Find("Canvas").GetComponent<Canvas>();
		selectable_0 = GameObject.Find("AddTerrainButton").GetComponent<Selectable>();
		selectable_1 = GameObject.Find("RemoveTerrainButton").GetComponent<Selectable>();
		text_0 = GameObject.Find("HelpDescriptionText").GetComponent<Text>();
		text_1 = GameObject.Find("HelpDescriptionTitleText").GetComponent<Text>();
		image_0 = GameObject.Find("HealthyColorPicker").transform.GetChild(0).GetComponent<Image>();
		GameObject.Find("HealthyColorPicker").GetComponent<ColorPicker>().Color = color_0;
		GameObject.Find("HealthyColorPicker").GetComponent<ColorPicker>().SetOnValueChangeCallback(UpdateDetailSettings);
		image_1 = GameObject.Find("DryColorPicker").transform.GetChild(0).GetComponent<Image>();
		GameObject.Find("DryColorPicker").GetComponent<ColorPicker>().Color = color_1;
		GameObject.Find("DryColorPicker").GetComponent<ColorPicker>().SetOnValueChangeCallback(UpdateDetailSettings);
		slider_0 = GameObject.Find("NoiseSpreadSlider").GetComponent<Slider>();
		slider_1 = GameObject.Find("AmbientOcclusionSlider").GetComponent<Slider>();
		slider_2 = GameObject.Find("GradientPowerSlider").GetComponent<Slider>();
		slider_3 = GameObject.Find("WindIdleSwaySlider").GetComponent<Slider>();
		toggle_0 = GameObject.Find("WindWavesToggle").GetComponent<Toggle>();
		slider_4 = GameObject.Find("WindWavesTintSlider").GetComponent<Slider>();
		slider_5 = GameObject.Find("WindWavesSizeSlider").GetComponent<Slider>();
		slider_6 = GameObject.Find("WindWavesSwaySlider").GetComponent<Slider>();
		image_2 = GameObject.Find("WindWavesTintColorPicker").transform.GetChild(0).GetComponent<Image>();
		GameObject.Find("WindWavesTintColorPicker").GetComponent<ColorPicker>().Color = color_2;
		GameObject.Find("WindWavesTintColorPicker").GetComponent<ColorPicker>().SetOnValueChangeCallback(UpdateDetailSettings);
		toggle_1 = GameObject.Find("BillboardToggle").GetComponent<Toggle>();
		toggle_2 = GameObject.Find("CrossQuadsToggle").GetComponent<Toggle>();
		slider_7 = GameObject.Find("CrossQuadsCountSlider").GetComponent<Slider>();
		slider_8 = GameObject.Find("CrossQuadsBillboardDistanceSlider").GetComponent<Slider>();
		inputField_0 = GameObject.Find("ScaleMinimumWidthInput").GetComponent<InputField>();
		inputField_1 = GameObject.Find("ScaleMaximumWidthInput").GetComponent<InputField>();
		inputField_2 = GameObject.Find("ScaleMinimumHeightInput").GetComponent<InputField>();
		inputField_3 = GameObject.Find("ScaleMaximumHeightInput").GetComponent<InputField>();
		toggle_3 = GameObject.Find("ShadowCastingToggle").GetComponent<Toggle>();
		toggle_4 = GameObject.Find("FrustumCullingToggle").GetComponent<Toggle>();
		slider_9 = GameObject.Find("FrustumOffsetSlider").GetComponent<Slider>();
		slider_10 = GameObject.Find("MaxDistanceSlider").GetComponent<Slider>();
	}

	public void UpdateDetailSettings()
	{
		color_0 = image_0.color;
		color_1 = image_1.color;
		float_0 = slider_0.value;
		float_1 = slider_1.value;
		float_2 = slider_2.value;
		float_3 = slider_3.value;
		bool_1 = toggle_0.isOn;
		float_4 = slider_4.value;
		float_5 = slider_5.value;
		float_6 = slider_6.value;
		color_2 = image_2.color;
		bool_2 = !toggle_2.isOn && toggle_1.isOn;
		toggle_1.isOn = bool_2;
		bool_3 = toggle_2.isOn;
		int_2 = (int)slider_7.value;
		float_7 = slider_8.value;
		if (!float.TryParse(inputField_0.text, out vector4_0.x))
		{
			vector4_0.x = 1f;
			inputField_0.text = "1.0";
		}
		if (!float.TryParse(inputField_1.text, out vector4_0.y))
		{
			vector4_0.y = 1f;
			inputField_1.text = "1.0";
		}
		if (!float.TryParse(inputField_2.text, out vector4_0.z))
		{
			vector4_0.z = 1f;
			inputField_2.text = "1.0";
		}
		if (!float.TryParse(inputField_3.text, out vector4_0.w))
		{
			vector4_0.w = 1f;
			inputField_3.text = "1.0";
		}
		bool_4 = toggle_3.isOn;
		bool_5 = toggle_4.isOn;
		float_8 = slider_9.value;
		float_9 = slider_10.value;
		method_1();
	}

	public void ShowHelpDescription(Text itemTitle)
	{
		text_1.text = itemTitle.text;
		string text = itemTitle.text;
		switch (Class3645.smethod_0(text))
		{
		case 154768752u:
			if (text == "Gradient Power")
			{
				text_0.text = HELPTEXT_gradientPower;
			}
			break;
		case 5515769u:
			if (text == "Wind Waves Tint Color")
			{
				text_0.text = HELPTEXT_windWaveTintColor;
			}
			break;
		case 219126802u:
			if (text == "Wind Waves Tint")
			{
				text_0.text = HELPTEXT_windWaveTint;
			}
			break;
		case 178565150u:
			if (text == "Ambient Occlusion")
			{
				text_0.text = HELPTEXT_ambientOcclusion;
			}
			break;
		case 159681588u:
			if (text == "Noise Spread")
			{
				text_0.text = HELPTEXT_noiseSpread;
			}
			break;
		case 1195358637u:
			if (text == "Dry Color")
			{
				text_0.text = HELPTEXT_detailDryColor;
			}
			break;
		case 676498961u:
			if (text == "Scale")
			{
				text_0.text = HELPTEXT_detailScale;
			}
			break;
		case 1743262317u:
			if (text == "Cross Quad Count")
			{
				text_0.text = HELPTEXT_quadCount;
			}
			break;
		case 1555381858u:
			if (text == "Wind Vector")
			{
				text_0.text = HELPTEXT_windVector;
			}
			break;
		case 1239456949u:
			if (text == "Wind Idle Sway")
			{
				text_0.text = HELPTEXT_windIdleSway;
			}
			break;
		case 1799054852u:
			if (text == "Frustum Offset")
			{
				text_0.text = HELPTEXT_frustumOffset;
			}
			break;
		case 1789134789u:
			if (text == "Wind Waves")
			{
				text_0.text = HELPTEXT_windWavesOn;
			}
			break;
		case 2802141700u:
			if (text == "Billboard")
			{
				text_0.text = HELPTEXT_isBillboard;
			}
			break;
		case 2090033114u:
			if (text == "Max Render Distance")
			{
				text_0.text = HELPTEXT_maxDetailDistance;
			}
			break;
		case 1997187254u:
			if (text == "Wind Waves Size")
			{
				text_0.text = HELPTEXT_windWaveSize;
			}
			break;
		case 3157479340u:
			if (text == "Shadow Casting")
			{
				text_0.text = HELPTEXT_isShadowCasting;
			}
			break;
		case 3045790633u:
			if (text == "Healthy Color")
			{
				text_0.text = HELPTEXT_detailHealthyColor;
			}
			break;
		case 2820061190u:
			if (text == "(GPU) Furstum Culling")
			{
				text_0.text = HELPTEXT_isFrustumCulling;
			}
			break;
		case 4108190592u:
			if (text == "CQ Billboard Dist.")
			{
				text_1.text = "Cross Quad Billboard Distance";
				text_0.text = HELPTEXT_billboardDistance;
			}
			break;
		case 4102596883u:
			if (text == "Wind Waves Sway")
			{
				text_0.text = HELPTEXT_windWaveSway;
			}
			break;
		case 3876160651u:
			if (text == "Cross Quads")
			{
				text_0.text = HELPTEXT_crossQuads;
			}
			break;
		}
	}

	public void HideHelpDescription()
	{
		text_1.text = "Description";
		text_0.text = "This scene is intended to act as a tutorial and to showcase how to use GPU Instancer's terrain detail system to add, remove and modify instanced detail prototypes at runtime.\n\nCheck the \"TerrainGenerator.cs\" in this scene for details.\n\nHover over a setting's title to see it's description here.";
	}

	public void method_3()
	{
		selectable_0.interactable = int_1 < 9;
		selectable_1.interactable = int_1 > 0;
	}

	public void method_4()
	{
		if ((bool)FpsController && (bool)FixedCamera)
		{
			FpsController.SetActive(bool_0);
			FixedCamera.SetActive(!bool_0);
			GPUInstancerDetailManager[] array = Object.FindObjectsOfType<GPUInstancerDetailManager>();
			for (int i = 0; i < array.Length; i++)
			{
				GClass1257.SetCamera(bool_0 ? FpsController.GetComponentInChildren<Camera>() : FixedCamera.GetComponentInChildren<Camera>());
			}
			if (!bool_0)
			{
				Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;
			}
		}
	}

	public void method_5()
	{
		SplatPrototype[] array = new SplatPrototype[1]
		{
			new SplatPrototype()
		};
		array[0].texture = groundTexture;
		DetailPrototype[] array2 = new DetailPrototype[1]
		{
			new DetailPrototype()
		};
		array2[0].prototypeTexture = detailTexture;
		array2[0].renderMode = DetailRenderMode.GrassBillboard;
		Vector3 position = Vector3.zero + vector3_0 * (int_1 % 3) + vector3_1 * Mathf.FloorToInt((float)int_1 / 3f);
		terrain_0[int_1] = method_6(position, int_0, (float)int_0 / 2f, 16, 16, array, array2);
		terrain_0[int_1].transform.SetParent(base.transform);
		method_9(terrain_0[int_1]);
	}

	public Terrain method_6(Vector3 position, int terrainSize, float terrainHeight, int baseTextureResolution = 16, int detailResolutionPerPatch = 16, SplatPrototype[] splatPrototypes = null, DetailPrototype[] detailPrototypes = null)
	{
		GameObject obj = new GameObject("GenratedTerrain");
		obj.transform.position = position;
		Terrain terrain = obj.AddComponent<Terrain>();
		TerrainData terrainData = (obj.AddComponent<TerrainCollider>().terrainData = method_7(terrainSize, terrainHeight, baseTextureResolution, detailResolutionPerPatch, splatPrototypes, detailPrototypes));
		terrain.terrainData = terrainData;
		return terrain;
	}

	public TerrainData method_7(int terrainSize, float terrainHeight, int baseTextureResolution = 16, int detailResolutionPerPatch = 16, SplatPrototype[] splatPrototypes = null, DetailPrototype[] detailPrototypes = null)
	{
		TerrainData terrainData = new TerrainData();
		terrainData.heightmapResolution = terrainSize + 1;
		terrainData.baseMapResolution = baseTextureResolution;
		terrainData.alphamapResolution = terrainSize;
		terrainData.SetDetailResolution(terrainSize, detailResolutionPerPatch);
		terrainData.terrainLayers = method_8(splatPrototypes);
		terrainData.detailPrototypes = detailPrototypes;
		terrainData.size = new Vector3(terrainSize, terrainHeight, terrainSize);
		return terrainData;
	}

	public TerrainLayer[] method_8(SplatPrototype[] splatPrototypes)
	{
		if (splatPrototypes == null)
		{
			return null;
		}
		TerrainLayer[] array = new TerrainLayer[splatPrototypes.Length];
		for (int i = 0; i < splatPrototypes.Length; i++)
		{
			array[i] = new TerrainLayer
			{
				diffuseTexture = splatPrototypes[i].texture,
				normalMapTexture = splatPrototypes[i].normalMap
			};
		}
		return array;
	}

	public void method_9(Terrain terrain)
	{
		int[,] array = new int[terrain.terrainData.detailResolution, terrain.terrainData.detailResolution];
		for (int i = 0; i < terrain.terrainData.detailPrototypes.Length; i++)
		{
			for (int j = 0; j < terrain.terrainData.detailResolution; j++)
			{
				for (int k = 0; k < terrain.terrainData.detailResolution; k++)
				{
					array[j, k] = Random.Range(4, 8);
				}
			}
			terrain.terrainData.SetDetailLayer(0, 0, i, array);
		}
		terrain.detailObjectDistance = 250f;
	}
}
