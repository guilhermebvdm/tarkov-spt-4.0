using System.Threading.Tasks;
using Systems.Effects;
using Comfort.Common;
using UnityEngine;

public class TestEffects : MonoBehaviour
{
	public int ID;

	public Camera Camera;

	public Effects Effects;

	private LightAllocationPoolClass lightAllocationPoolClass;

	public async Task Start()
	{
		if (Singleton<Effects>.Instantiated)
		{
			Singleton<Effects>.Release(Singleton<Effects>.Instance);
		}
		Singleton<Effects>.Create(Effects);
		CameraClass.Instance.SetCamera(Camera);
		AssetsManagerSingletonClass.Manager = (await GClass1805.CreateAssetsManager()).Result;
		await AssetsManagerSingletonClass.Manager.LoadAssetAsync("assets/content/audio/audioshot.bundle", "audioshot");
		BetterAudio obj = GClass870.FindUnityObjectOfType<BetterAudio>() ?? new GameObject("Audio").AddComponent<BetterAudio>();
		if (Singleton<BetterAudio>.Instantiated)
		{
			Singleton<BetterAudio>.Release(Singleton<BetterAudio>.Instance);
		}
		Singleton<BetterAudio>.Create(obj);
		obj.Preload();
		lightAllocationPoolClass = new LightAllocationPoolClass();
		lightAllocationPoolClass.Init();
	}

	public void Update()
	{
		if (Input.GetMouseButtonDown(0) && Physics.Raycast(CameraClass.Instance.Camera.ScreenPointToRay(Input.mousePosition), out var hitInfo))
		{
			Singleton<Effects>.Instance.TestEmit(ID, hitInfo.point, hitInfo.normal);
		}
	}
}
