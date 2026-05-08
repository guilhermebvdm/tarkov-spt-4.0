using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GPUInstancer;

public class DetailDemoSceneController : MonoBehaviour
{
	public enum CameraModes
	{
		FPMode,
		SpaceshipMode,
		MowerMode
	}

	public enum QualityMode
	{
		Low,
		Mid,
		High
	}

	public GameObject fpController;

	public GameObject spaceshipCamera;

	public GameObject grassMowerCamera;

	public GPUInstancerDetailManager detailManager;

	public bool persistRemoval;

	private GameObject gameObject_0;

	private GameObject gameObject_1;

	private GameObject gameObject_2;

	private GameObject gameObject_3;

	private Text text_0;

	private Transform transform_0;

	private Transform transform_1;

	private GameObject gameObject_4;

	private CameraModes cameraModes_0;

	private ParticleSystem particleSystem_0;

	private QualityMode qualityMode_0 = QualityMode.High;

	private List<int[,]> list_0;

	public void Awake()
	{
		gameObject_0 = GameObject.Find("Canvas");
		gameObject_1 = GameObject.Find("SpaceShipControlsPanel");
		gameObject_2 = GameObject.Find("GrassMowerControlsPanel");
		gameObject_3 = GameObject.Find("LoadingInfoPanel");
		text_0 = GameObject.Find("CurrentQualityModeInfoText").GetComponent<Text>();
		text_0.text = "Current Quality Mode: " + qualityMode_0.ToString() + " Quality";
		transform_0 = Object.FindObjectOfType<SpaceshipController>().transform;
		particleSystem_0 = transform_0.GetChild(0).GetChild(0).GetComponent<ParticleSystem>();
		transform_1 = Object.FindObjectOfType<GrassMowerController>().transform;
		method_1(CameraModes.FPMode);
		method_4(qualityMode_0);
		GClass1257.StartListeningGPUIEvent(GPUInstancerEventType.DetailInitializationFinished, method_2);
	}

	public void Update()
	{
		if (Input.GetKeyUp(KeyCode.C))
		{
			method_0();
		}
		if (Input.GetKeyUp(KeyCode.U))
		{
			gameObject_0.gameObject.SetActive(!gameObject_0.gameObject.activeSelf);
		}
		if (Input.GetKeyUp(KeyCode.F1))
		{
			if (detailManager.gameObject.activeSelf && persistRemoval)
			{
				list_0 = GClass1257.GetDetailMapData(detailManager);
			}
			detailManager.gameObject.SetActive(!detailManager.gameObject.activeSelf);
			if (detailManager.gameObject.activeSelf)
			{
				if (persistRemoval && list_0 != null)
				{
					GClass1257.SetDetailMapData(detailManager, list_0);
				}
				gameObject_3.SetActive(value: true);
				GClass1257.SetCamera(gameObject_4.GetComponentInChildren<Camera>());
				GClass1257.StartListeningGPUIEvent(GPUInstancerEventType.DetailInitializationFinished, method_2);
			}
			method_3(qualityMode_0);
		}
		if (Input.GetKeyUp(KeyCode.F2))
		{
			method_3(QualityMode.Low);
		}
		if (Input.GetKeyUp(KeyCode.F3))
		{
			method_3(QualityMode.Mid);
		}
		if (Input.GetKeyUp(KeyCode.F4))
		{
			method_3(QualityMode.High);
		}
	}

	public void method_0()
	{
		method_1((CameraModes)((int)(cameraModes_0 + 1) % 3));
	}

	public void method_1(CameraModes cameraMode)
	{
		if ((bool)fpController && (bool)spaceshipCamera && (bool)grassMowerCamera)
		{
			fpController.SetActive(value: false);
			spaceshipCamera.SetActive(value: false);
			grassMowerCamera.SetActive(value: false);
			transform_0.GetComponent<SpaceshipController>().enabled = false;
			particleSystem_0.gameObject.SetActive(value: false);
			gameObject_1.gameObject.SetActive(value: false);
			transform_1.GetComponent<GrassMowerController>().enabled = false;
			transform_1.GetComponent<GPUInstancerInstanceRemover>().enabled = false;
			gameObject_2.gameObject.SetActive(value: false);
			switch (cameraMode)
			{
			case CameraModes.FPMode:
				fpController.SetActive(value: true);
				gameObject_4 = fpController;
				break;
			case CameraModes.SpaceshipMode:
				spaceshipCamera.SetActive(value: true);
				transform_0.GetComponent<SpaceshipController>().enabled = true;
				particleSystem_0.gameObject.SetActive(value: true);
				gameObject_1.gameObject.SetActive(value: true);
				gameObject_4 = spaceshipCamera;
				break;
			case CameraModes.MowerMode:
				grassMowerCamera.SetActive(value: true);
				transform_1.GetComponent<GrassMowerController>().enabled = true;
				transform_1.GetComponent<GPUInstancerInstanceRemover>().enabled = true;
				gameObject_2.gameObject.SetActive(value: true);
				gameObject_4 = grassMowerCamera;
				break;
			}
			cameraModes_0 = cameraMode;
			GClass1257.SetCamera(gameObject_4.GetComponentInChildren<Camera>());
		}
		else
		{
			Debug.Log("Not all cameras are set. Please assign the relevant cameras from the inspector");
		}
	}

	public void method_2()
	{
		gameObject_3.SetActive(value: false);
		GClass1257.StopListeningGPUIEvent(GPUInstancerEventType.DetailInitializationFinished, method_2);
	}

	public void method_3(QualityMode qualityMode)
	{
		if (!detailManager.gameObject.activeSelf)
		{
			text_0.text = "Current Quality Mode: GPU Instancer disabled (Unity terrain)";
			return;
		}
		text_0.text = "Current Quality Mode: " + qualityMode.ToString() + " Quality";
		if (qualityMode_0 != qualityMode)
		{
			qualityMode_0 = qualityMode;
			method_4(qualityMode);
			GClass1257.UpdateDetailInstances(detailManager, updateMeshes: true);
		}
	}

	public void method_4(QualityMode qualityMode)
	{
		for (int i = 0; i < detailManager.prototypeList.Count; i++)
		{
			GPUInstancerDetailPrototype gPUInstancerDetailPrototype = (GPUInstancerDetailPrototype)detailManager.prototypeList[i];
			switch (qualityMode)
			{
			case QualityMode.Low:
				gPUInstancerDetailPrototype.isBillboard = !gPUInstancerDetailPrototype.usePrototypeMesh;
				gPUInstancerDetailPrototype.useCrossQuads = false;
				gPUInstancerDetailPrototype.isShadowCasting = false;
				gPUInstancerDetailPrototype.maxDistance = 150f;
				break;
			case QualityMode.Mid:
				gPUInstancerDetailPrototype.isBillboard = false;
				gPUInstancerDetailPrototype.useCrossQuads = !gPUInstancerDetailPrototype.usePrototypeMesh;
				gPUInstancerDetailPrototype.quadCount = 2;
				gPUInstancerDetailPrototype.isShadowCasting = gPUInstancerDetailPrototype.usePrototypeMesh;
				gPUInstancerDetailPrototype.maxDistance = 250f;
				break;
			case QualityMode.High:
				gPUInstancerDetailPrototype.isBillboard = false;
				gPUInstancerDetailPrototype.useCrossQuads = !gPUInstancerDetailPrototype.usePrototypeMesh;
				gPUInstancerDetailPrototype.quadCount = 4;
				gPUInstancerDetailPrototype.isShadowCasting = true;
				gPUInstancerDetailPrototype.maxDistance = 500f;
				break;
			}
		}
	}
}
