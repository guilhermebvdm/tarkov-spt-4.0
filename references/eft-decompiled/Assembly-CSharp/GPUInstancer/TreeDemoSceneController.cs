using UnityEngine;
using UnityEngine.UI;

namespace GPUInstancer;

public class TreeDemoSceneController : MonoBehaviour
{
	public GPUInstancerTreeManager manager;

	public Text GPUIStateText;

	public Text FPSCountTextText;

	private FPS fps_0;

	public void Awake()
	{
		fps_0 = GetComponent<FPS>();
	}

	public void Start()
	{
		QualitySettings.shadowDistance = 450f;
	}

	public void Update()
	{
		FPSCountTextText.text = "FPS: " + fps_0.FPSCount;
	}

	public void ToggleManager()
	{
		manager.gameObject.SetActive(!manager.gameObject.activeSelf);
		GPUIStateText.text = (manager.gameObject.activeSelf ? "GPU Instancer ON" : "GPU Instancer OFF");
		GPUIStateText.color = (manager.gameObject.activeSelf ? Color.green : Color.red);
	}
}
