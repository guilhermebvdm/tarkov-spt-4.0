using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace MultiFlare;

public class ProFlareAtlas : ScriptableObject
{
	[Serializable]
	public struct Container
	{
		[SerializeField]
		[FormerlySerializedAs("name")]
		public string _name;

		[SerializeField]
		[FormerlySerializedAs("UV")]
		public Rect _rect;

		public string Name => _name;

		public Rect UvRect => _rect;
	}

	[SerializeField]
	[FormerlySerializedAs("elementsList")]
	private Container[] _containers;

	public IEnumerable<Container> GetAll => _containers;

	public Rect GetUvRectByIndex(int index)
	{
		return _containers[index].UvRect;
	}
}
