using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonAssets.Scripts.Audio.RadioSystem;

[Serializable]
[CreateAssetMenu(menuName = "Scriptable objects/Audio/BroadcastGridContainer", fileName = "BroadcastGridContainer")]
public class BroadcastItemsContainer : ScriptableObject
{
	[Serializable]
	public class BroadcastItemsData : SerializableEnumDictionary<EBroadcastItemType, BroadcastItemsDataContainer>
	{
	}

	[Serializable]
	public class BroadcastItemsDataContainer : IEnumerable<BroadcastItemData>, IEnumerable
	{
		[SerializeField]
		public List<BroadcastItemData> _data = new List<BroadcastItemData>();

		[NonSerialized]
		public GClass1503<BroadcastItemData> ExclusiveRandomizer = new GClass1503<BroadcastItemData>();

		public BroadcastItemData this[int i] => _data[i];

		public int Count => _data.Count;

		public void Add(BroadcastItemData data)
		{
			_data.Add(data);
		}

		public bool Remove(BroadcastItemData data)
		{
			return _data.Remove(data);
		}

		public bool TryGetRandomData(out BroadcastItemData data)
		{
			return ExclusiveRandomizer.TryGetRandomObject(_data, out data);
		}

		public IEnumerator<BroadcastItemData> GetEnumerator()
		{
			return _data.GetEnumerator();
		}

		public IEnumerator GetEnumerator_1()
		{
			return GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			//ILSpy generated this explicit interface implementation from .override directive in GetEnumerator_1
			return this.GetEnumerator_1();
		}
	}

	[SerializeField]
	private BroadcastItemsData _broadcastItemsData;

	public void OnEnable()
	{
		method_1();
	}

	public void OnValidate()
	{
		method_0();
	}

	public void OnDisable()
	{
		method_1();
	}

	public void method_0()
	{
		foreach (KeyValuePair<EBroadcastItemType, BroadcastItemsDataContainer> broadcastItemsDatum in _broadcastItemsData)
		{
			broadcastItemsDatum.Deconstruct(out var key, out var value);
			EBroadcastItemType itemType = key;
			BroadcastItemsDataContainer broadcastItemsDataContainer = value;
			for (int num = broadcastItemsDataContainer.Count - 1; num >= 0; num--)
			{
				BroadcastItemData broadcastItemData = broadcastItemsDataContainer[num];
				AudioClip audioClip = broadcastItemData?.clip;
				if (!(audioClip == null))
				{
					broadcastItemData.clipIndex = num;
					broadcastItemData.clipLength = audioClip.length;
					broadcastItemData.itemType = itemType;
				}
			}
		}
	}

	public void method_1()
	{
		foreach (KeyValuePair<EBroadcastItemType, BroadcastItemsDataContainer> broadcastItemsDatum in _broadcastItemsData)
		{
			broadcastItemsDatum.Deconstruct(out var _, out var value);
			BroadcastItemsDataContainer broadcastItemsDataContainer = value;
			for (int num = broadcastItemsDataContainer.Count - 1; num >= 0; num--)
			{
				BroadcastItemData broadcastItemData = broadcastItemsDataContainer[num];
				if (broadcastItemData?.clip == null)
				{
					broadcastItemsDataContainer.Remove(broadcastItemData);
				}
			}
		}
	}

	public bool TryGetContent(EBroadcastItemType broadcastItemType, int index, out AudioClip clip)
	{
		clip = null;
		if (!_broadcastItemsData.TryGetValue(broadcastItemType, out var value))
		{
			return false;
		}
		if (value != null && value.Count > index)
		{
			clip = value[index]?.clip;
			return clip != null;
		}
		return false;
	}

	public bool TryGetRandomBroadcastData(EBroadcastItemType broadcastItemType, out BroadcastItemData data)
	{
		data = null;
		if (_broadcastItemsData.TryGetValue(broadcastItemType, out var value))
		{
			return value.TryGetRandomData(out data);
		}
		return false;
	}
}
