using UnityEngine;

public abstract class GClass852
{
	public static T Add<T>(bool dontDestroyOnLoad = false) where T : MonoBehaviour
	{
		T val = GClass870.FindUnityObjectOfType<T>();
		if (val == null)
		{
			GameObject gameObject = new GameObject(typeof(T).ToString());
			if (dontDestroyOnLoad)
			{
				Object.DontDestroyOnLoad(gameObject);
			}
			val = gameObject.AddComponent<T>();
		}
		return val;
	}

	public static void Remove<T>() where T : MonoBehaviour
	{
		T val = GClass870.FindUnityObjectOfType<T>();
		if (val != null)
		{
			Object.Destroy(val.gameObject);
		}
	}
}
