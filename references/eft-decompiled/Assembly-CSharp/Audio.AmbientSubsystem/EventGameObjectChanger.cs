using System;
using System.Collections.Generic;
using EFT;
using UnityEngine;

namespace Audio.AmbientSubsystem;

public class EventGameObjectChanger : MonoBehaviour, GInterface104
{
	[Serializable]
	public class EventGameObjects : SerializableEnumDictionary<EEventType, GameObject>
	{
	}

	[SerializeField]
	private GameObject _originalContent;

	[SerializeField]
	private bool _isOriginalGoEnabledByDefault = true;

	[SerializeField]
	private EventGameObjects _eventGameObjects;

	public void Awake()
	{
		_originalContent.SetActive(_isOriginalGoEnabledByDefault);
		foreach (KeyValuePair<EEventType, GameObject> eventGameObject in _eventGameObjects)
		{
			eventGameObject.Deconstruct(out var _, out var value);
			value.SetActive(value: false);
		}
	}

	public void ChangeSoundContent(EEventType eventType)
	{
		if (_eventGameObjects.TryGetValue(eventType, out var value))
		{
			value.SetActive(value: true);
			_originalContent.SetActive(value: false);
		}
		else
		{
			_originalContent.SetActive(_isOriginalGoEnabledByDefault);
		}
	}
}
