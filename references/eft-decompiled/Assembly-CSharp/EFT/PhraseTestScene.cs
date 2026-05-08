using System.Threading.Tasks;
using Comfort.Common;
using UnityEngine;

namespace EFT;

public class PhraseTestScene : MonoBehaviour
{
	private const string string_0 = "assets/content/audio/prefabs/phrases/bear/bearsashaphrasesounds.bundle";

	[SerializeField]
	private Camera _camera;

	public void Awake()
	{
		method_0();
	}

	public async void method_0()
	{
		CameraClass.Instance.SetCamera(_camera);
		IOperation<IAssetsManager> operation = await GClass1805.CreateAssetsManager();
		if (operation.Failed)
		{
			Debug.Log("<color=red>OperationFailed</color>");
			return;
		}
		AssetsManagerSingletonClass.Manager = operation.Result;
		await method_1();
	}

	public async Task method_1()
	{
		await AssetsManagerSingletonClass.Manager.LoadBundlesAsync(new string[1] { "assets/content/audio/prefabs/phrases/bear/bearsashaphrasesounds.bundle" });
		if (Singleton<BetterAudio>.Instantiated)
		{
			Singleton<BetterAudio>.Release(Singleton<BetterAudio>.Instance);
		}
		Singleton<BetterAudio>.Create(new GameObject("Audio").AddComponent<BetterAudio>());
		await Singleton<BetterAudio>.Instance.PreloadCoroutine();
	}
}
