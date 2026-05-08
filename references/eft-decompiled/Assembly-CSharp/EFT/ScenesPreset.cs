using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Serialization;

namespace EFT;

public class ScenesPreset : ScriptableObject
{
	[Serializable]
	public class Guid : ISceneResource
	{
		public string guid;

		[SerializeField]
		public bool _onlyOffline;

		public bool onlyOffline
		{
			get
			{
				return _onlyOffline;
			}
			set
			{
				_onlyOffline = value;
			}
		}

		public Guid(string guid, bool onlyOffline)
		{
			this.guid = guid;
			this.onlyOffline = onlyOffline;
		}
	}

	[Serializable]
	[CompilerGenerated]
	public class Class1460
	{
		public static readonly Class1460 class1460_0 = new Class1460();

		public static Func<ScenesPreset, int> func_0;

		public int method_0(ScenesPreset x)
		{
			return x.GetTotalSceneCount();
		}
	}

	public Guid ActiveSceneGuid;

	public string ServerName;

	public List<Guid> ScenesGuids;

	private bool bool_0;

	public ScenesPreset[] ChildPresets;

	[SerializeField]
	[FormerlySerializedAs("ActiveSceneName")]
	private string _activeSceneName;

	[SerializeField]
	[FormerlySerializedAs("ScenesResourceKeys")]
	private List<SceneResourceKey> _scenesResourceKeys;

	public string ActiveSceneName => _activeSceneName;

	public string PresetName => base.name.Replace(".ScenesPreset", string.Empty);

	public SceneResourceKey[] ScenesResourceKeys => _scenesResourceKeys.Where(method_0).ToArray();

	public void DisableServerScenes(bool val)
	{
		bool_0 = val;
		for (int i = 0; i < ChildPresets.Length; i++)
		{
			ChildPresets[i].bool_0 = val;
		}
	}

	public bool method_0(ISceneResource scene)
	{
		if (!bool_0)
		{
			return true;
		}
		return !scene.onlyOffline;
	}

	public int GetTotalSceneCount()
	{
		return ScenesGuids.Where(method_0).Count() + ChildPresets.Sum((ScenesPreset x) => x.GetTotalSceneCount());
	}
}
