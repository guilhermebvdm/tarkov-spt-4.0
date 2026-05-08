using UnityEngine;

namespace MultiFlare;

public class FlareScheduler : MonoBehaviour
{
	public void Awake()
	{
		GClass1023.Instance.IsRenderingEnabled = true;
		GClass1023.Instance.Initialize();
		Object.DontDestroyOnLoad(base.gameObject);
	}

	public void OnDestroy()
	{
		GClass1023.Instance.Dispose();
	}

	public void Update()
	{
		GClass1023.Instance.StartRendering();
	}

	public void LateUpdate()
	{
		GClass1023.Instance.CompleteRendering();
		GClass1023.Instance.RenderInstantBatches();
	}
}
