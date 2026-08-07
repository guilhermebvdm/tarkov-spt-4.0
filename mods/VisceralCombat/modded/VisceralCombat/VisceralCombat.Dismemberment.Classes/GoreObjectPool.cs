using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace VisceralCombat.Dismemberment.Classes;

public class GoreObjectPool : MonoBehaviour
{
	public static GoreObjectPool Instance { get; private set; }

	private readonly Dictionary<string, Queue<GameObject>> _pool = new Dictionary<string, Queue<GameObject>>();
	private readonly Dictionary<GameObject, string> _activeObjects = new Dictionary<GameObject, string>();

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);
			return;
		}
		Instance = this;
		DontDestroyOnLoad(gameObject);
	}

	public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null)
	{
		if (prefab == null) return null;

		string prefabName = prefab.name;
		GameObject instance = null;

		if (_pool.TryGetValue(prefabName, out Queue<GameObject> queue) && queue.Count > 0)
		{
			instance = queue.Dequeue();
		}

		if (instance == null)
		{
			instance = Instantiate(prefab);
			instance.name = prefabName;
		}

		instance.transform.SetParent(parent, false);
		instance.transform.position = position;
		instance.transform.rotation = rotation;
		instance.SetActive(true);

		_activeObjects[instance] = prefabName;
		return instance;
	}

	public void Recycle(GameObject instance, float delaySeconds = 0f)
	{
		if (instance == null) return;

		if (delaySeconds > 0f)
		{
			StartCoroutine(RecycleCoroutine(instance, delaySeconds));
		}
		else
		{
			DoRecycle(instance);
		}
	}

	private System.Collections.IEnumerator RecycleCoroutine(GameObject instance, float delay)
	{
		yield return new WaitForSeconds(delay);
		DoRecycle(instance);
	}

	private void DoRecycle(GameObject instance)
	{
		if (instance == null) return;

		if (_activeObjects.TryGetValue(instance, out string prefabName))
		{
			_activeObjects.Remove(instance);
			instance.SetActive(false);
			instance.transform.SetParent(transform, false);

			if (!_pool.TryGetValue(prefabName, out Queue<GameObject> queue))
			{
				queue = new Queue<GameObject>();
				_pool[prefabName] = queue;
			}
			queue.Enqueue(instance);
		}
		else
		{
			Destroy(instance);
		}
	}

	public void ClearPool()
	{
		foreach (var kvp in _pool)
		{
			foreach (var go in kvp.Value)
			{
				if (go != null) Destroy(go);
			}
		}
		_pool.Clear();
		_activeObjects.Clear();
	}
}
