using System;
using UnityEngine;

[ExecuteInEditMode]
[DisallowMultipleComponent]
public class GuidComponent : MonoBehaviour, ISerializationCallbackReceiver
{
	private Guid guid_0 = Guid.Empty;

	[SerializeField]
	private byte[] serializedGuid;

	public byte[] AsByteGuid => serializedGuid;

	public bool IsGuidAssigned()
	{
		return guid_0 != Guid.Empty;
	}

	public void method_0()
	{
		if (serializedGuid != null && serializedGuid.Length == 16)
		{
			if (guid_0 == Guid.Empty)
			{
				guid_0 = new Guid(serializedGuid);
			}
		}
		else
		{
			guid_0 = Guid.NewGuid();
			serializedGuid = guid_0.ToByteArray();
		}
		if (guid_0 != Guid.Empty && !GClass875.Add(this))
		{
			serializedGuid = null;
			guid_0 = Guid.Empty;
			method_0();
		}
	}

	public void OnBeforeSerialize()
	{
		if (guid_0 != Guid.Empty)
		{
			serializedGuid = guid_0.ToByteArray();
		}
	}

	public void OnAfterDeserialize()
	{
		if (serializedGuid != null && serializedGuid.Length == 16)
		{
			guid_0 = new Guid(serializedGuid);
		}
	}

	public void Awake()
	{
		method_0();
	}

	public void OnValidate()
	{
		method_0();
	}

	public Guid GetGuid()
	{
		if (guid_0 == Guid.Empty && serializedGuid != null && serializedGuid.Length == 16)
		{
			guid_0 = new Guid(serializedGuid);
		}
		return guid_0;
	}

	public void OnDestroy()
	{
		GClass875.Remove(guid_0);
	}
}
