using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Streamer : MonoBehaviour
{
	[Serializable]
	public class Scene
	{
		public string Name;

		public string Path;

		public Chunk[] Chunks;

		public const float DefaultDistance = 150f;

		public float Distance = 150f;

		public GameObject[] LODs;

		[HideInInspector]
		public Bounds Bounds;

		[HideInInspector]
		public LoadingState State = LoadingState.NotLoaded;

		[HideInInspector]
		public GameObject[] LODsInstances;

		[field: NonSerialized]
		public Vector2 Distances { get; set; }

		public Scene()
		{
		}

		public Scene(UnityEngine.SceneManagement.Scene scene, Bounds bounds, float distance)
		{
			Name = scene.name;
			Path = scene.path;
			Bounds = bounds;
			Distance = distance;
		}
	}

	[Serializable]
	public class Chunk
	{
		public string Name;

		public string Path;

		[HideInInspector]
		public LoadingState State;

		public Chunk()
		{
		}

		public Chunk(UnityEngine.SceneManagement.Scene scene)
		{
			Name = scene.name;
			Path = scene.path;
		}
	}

	public enum LoadingState
	{
		Loading,
		Loaded,
		NotLoaded
	}

	[Serializable]
	[CompilerGenerated]
	public class Class696
	{
		public static readonly Class696 class696_0 = new Class696();

		public static Func<Chunk, AsyncOperation> func_0;

		public static Func<AsyncOperation, bool> func_1;

		public AsyncOperation method_0(Chunk chunk)
		{
			return SceneManager.UnloadSceneAsync(chunk.Name);
		}

		public bool method_1(AsyncOperation operation)
		{
			return !operation.isDone;
		}
	}

	public Transform Player;

	[Range(0f, 0.5f)]
	public float Backlash = 0.3f;

	public int MaxPerformanceRaiting = 8000;

	public Scene[] Scenes;

	private readonly Queue<Scene> queue_0 = new Queue<Scene>();

	private readonly HashSet<Scene> hashSet_0 = new HashSet<Scene>();

	private bool bool_0;

	public void Awake()
	{
		Scene[] scenes = Scenes;
		foreach (Scene scene in scenes)
		{
			scene.Distances = new Vector2(1f - Backlash, 1f + Backlash) * scene.Distance;
			scene.State = LoadingState.NotLoaded;
			scene.LODsInstances = new GameObject[scene.LODs.Length];
			for (int j = 0; j < scene.LODs.Length; j++)
			{
				scene.LODsInstances[j] = UnityEngine.Object.Instantiate(scene.LODs[j]);
				scene.LODsInstances[j].transform.SetParent(base.transform, worldPositionStays: true);
				scene.LODsInstances[j].SetActive(value: true);
			}
		}
	}

	public void Update()
	{
		CameraClass.Instance.SetCamera(Camera.main);
		if (CameraClass.Instance.Camera != null)
		{
			Vector3 position = CameraClass.Instance.Camera.transform.position;
			Scene[] scenes = Scenes;
			foreach (Scene scene in scenes)
			{
				if (scene.State == LoadingState.Loading)
				{
					continue;
				}
				Bounds bounds = scene.Bounds;
				float val = Math.Abs(bounds.center.x - position.x) - bounds.extents.x;
				float val2 = Math.Abs(bounds.center.z - position.z) - bounds.extents.z;
				float num = Math.Max(val, 0f) + Math.Max(val2, 0f);
				if (scene.State == LoadingState.Loaded)
				{
					if (num > scene.Distances.y)
					{
						StartCoroutine(method_0(scene));
					}
				}
				else if (num < scene.Distances.x)
				{
					method_1(scene);
				}
			}
		}
		if (queue_0.Count > 0 && !bool_0)
		{
			Scene scene2 = queue_0.Dequeue();
			StartCoroutine(method_2(scene2));
		}
	}

	public IEnumerator method_0(Scene scene)
	{
		Debug.Log("Unload: " + scene.Name);
		if (scene.State == LoadingState.NotLoaded)
		{
			yield break;
		}
		if (scene.State == LoadingState.Loading)
		{
			hashSet_0.Add(scene);
			yield break;
		}
		AsyncOperation[] source = scene.Chunks.Select((Chunk chunk) => SceneManager.UnloadSceneAsync(chunk.Name)).ToArray();
		while (source.Any((AsyncOperation operation) => !operation.isDone))
		{
			yield return null;
		}
		scene.State = LoadingState.NotLoaded;
		GameObject[] lODsInstances = scene.LODsInstances;
		for (int num = 0; num < lODsInstances.Length; num++)
		{
			lODsInstances[num].SetActive(value: true);
		}
	}

	public void method_1(Scene scene)
	{
		Debug.Log("Start Load Scene:\n" + scene.Name);
		if (scene.State == LoadingState.NotLoaded)
		{
			queue_0.Enqueue(scene);
			scene.State = LoadingState.Loading;
		}
	}

	public IEnumerator method_2(Scene scene)
	{
		bool_0 = true;
		Application.backgroundLoadingPriority = ThreadPriority.Low;
		Chunk[] chunks = scene.Chunks;
		int num = 0;
		while (true)
		{
			if (num >= chunks.Length)
			{
				GameObject[] lODsInstances = scene.LODsInstances;
				for (int i = 0; i < lODsInstances.Length; i++)
				{
					lODsInstances[i].SetActive(value: false);
				}
				if (hashSet_0.Contains(scene))
				{
					chunks = scene.Chunks;
					foreach (Chunk chunk in chunks)
					{
						yield return SceneManager.UnloadSceneAsync(chunk.Name);
					}
					hashSet_0.Remove(scene);
					scene.State = LoadingState.NotLoaded;
					lODsInstances = scene.LODsInstances;
					for (int i = 0; i < lODsInstances.Length; i++)
					{
						lODsInstances[i].SetActive(value: true);
					}
				}
				else
				{
					scene.State = LoadingState.Loaded;
				}
				break;
			}
			Chunk chunk2 = chunks[num];
			if (hashSet_0.Contains(scene))
			{
				Chunk[] chunks2 = scene.Chunks;
				foreach (Chunk chunk3 in chunks2)
				{
					yield return SceneManager.UnloadSceneAsync(chunk3.Name);
				}
				hashSet_0.Remove(scene);
				bool_0 = false;
				scene.State = LoadingState.NotLoaded;
				GameObject[] lODsInstances = scene.LODsInstances;
				for (int i = 0; i < lODsInstances.Length; i++)
				{
					lODsInstances[i].SetActive(value: true);
				}
				yield break;
			}
			yield return StartCoroutine(method_3(chunk2));
			num++;
		}
		bool_0 = false;
	}

	public IEnumerator method_3(Chunk chunk)
	{
		AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(chunk.Name, LoadSceneMode.Additive);
		asyncOperation.priority = 0;
		asyncOperation.allowSceneActivation = false;
		while (asyncOperation.progress < 0.9f)
		{
			yield return null;
		}
		yield return null;
		asyncOperation.allowSceneActivation = true;
		yield return asyncOperation;
		Debug.Log("Chunk Loaded:\n" + chunk.Name);
	}
}
