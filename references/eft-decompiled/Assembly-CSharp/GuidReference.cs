using System;
using System.Runtime.CompilerServices;
using UnityEngine;

[Serializable]
public class GuidReference : ISerializationCallbackReceiver
{
	[Serializable]
	[CompilerGenerated]
	public class Class499
	{
		public static readonly Class499 class499_0 = new Class499();

		public static Action<GameObject> action_0;

		public static Action action_1;

		public static Action<GameObject> action_2;

		public static Action action_3;

		public static Action<GameObject> action_4;

		public static Action action_5;

		public void method_0(GameObject go)
		{
		}

		public void method_1()
		{
		}

		public void method_2(GameObject go)
		{
		}

		public void method_3()
		{
		}

		public void method_4(GameObject go)
		{
		}

		public void method_5()
		{
		}
	}

	[NonSerialized]
	public GameObject CachedReference;

	[NonSerialized]
	public bool IsCacheSet;

	[SerializeField]
	public byte[] serializedGuid;

	[NonSerialized]
	public Guid Guid;

	[NonSerialized]
	public Action<GameObject> AddDelegate;

	[NonSerialized]
	public Action RemoveDelegate;

	public byte[] AsBytes => serializedGuid;

	public Guid ObjectGuid => Guid;

	public string ObjectGuidString => Guid.ToString("N");

	public GameObject gameObject
	{
		get
		{
			if (IsCacheSet)
			{
				return CachedReference;
			}
			CachedReference = GClass875.ResolveGuid(Guid, AddDelegate, RemoveDelegate);
			IsCacheSet = true;
			return CachedReference;
		}
		set
		{
		}
	}

	public event Action<GameObject> OnGuidAdded = delegate
	{
	};

	public event Action OnGuidRemoved = delegate
	{
	};

	public static bool operator ==(GuidReference a, GuidReference b)
	{
		return a.Guid == b.Guid;
	}

	public static bool operator !=(GuidReference a, GuidReference b)
	{
		return !(a == b);
	}

	public GuidReference()
	{
	}

	public GuidReference(byte[] guidBytes)
	{
		serializedGuid = guidBytes;
		OnAfterDeserialize();
	}

	public GuidReference(GuidComponent target)
	{
		Guid = target.GetGuid();
	}

	public void method_0(GameObject go)
	{
		CachedReference = go;
		this.OnGuidAdded(go);
	}

	public void method_1()
	{
		CachedReference = null;
		IsCacheSet = false;
		this.OnGuidRemoved();
	}

	public void OnBeforeSerialize()
	{
		serializedGuid = Guid.ToByteArray();
	}

	public void OnAfterDeserialize()
	{
		CachedReference = null;
		IsCacheSet = false;
		if (serializedGuid == null || serializedGuid.Length != 16)
		{
			serializedGuid = new byte[16];
		}
		Guid = new Guid(serializedGuid);
		AddDelegate = method_0;
		RemoveDelegate = method_1;
	}

	public override string ToString()
	{
		return new Guid(serializedGuid).ToString();
	}

	public T GetComponent<T>()
	{
		if (gameObject != null)
		{
			return gameObject.GetComponent<T>();
		}
		return default(T);
	}
}
