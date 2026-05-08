using System;
using System.Collections;
using UnityEngine;

namespace Audio.Data;

[Serializable]
public class AudioMixerParamsContainer : IEnumerable
{
	[SerializeField]
	public string[] _mixerParams = Array.Empty<string>();

	public string this[int i] => _mixerParams[i];

	public int Count => _mixerParams.Length;

	public IEnumerator GetEnumerator()
	{
		return _mixerParams.GetEnumerator();
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
