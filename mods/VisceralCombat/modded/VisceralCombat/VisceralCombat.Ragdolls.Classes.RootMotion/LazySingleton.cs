using UnityEngine;
using Object = UnityEngine.Object;

namespace VisceralCombat.Ragdolls.Classes.RootMotion;

public abstract class LazySingleton<T> : MonoBehaviour where T : LazySingleton<T>
{
	private static T sInstance;

	public static bool hasInstance => (Object)(object)sInstance != (Object)null;

	public static T instance
	{
		get
		{
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)sInstance == (Object)null)
			{
				string text = typeof(T).ToString();
				sInstance = new GameObject(text).AddComponent<T>();
			}
			return sInstance;
		}
	}

	protected virtual void Awake()
	{
		sInstance = (T)this;
	}
}
