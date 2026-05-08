using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CutsceneTriggerStartInfoSO", menuName = "ScriptableObjects/CutsceneTriggerStartInfoSO")]
public class CutsceneTriggerStartInfoSO : ScriptableObject
{
	[Serializable]
	public class SceneCutscenesInfoRelation
	{
		public string sceneName;

		public List<CutsceneIDWithStartValues> cutsceneTriggersStartInfo = new List<CutsceneIDWithStartValues>();
	}

	[Serializable]
	public class CutsceneIDWithStartValues
	{
		public int cutsceneID;

		public StartPlayerValues startValues;
	}

	[Serializable]
	public class StartPlayerValues
	{
		public Vector3 startPosition;

		public Vector3 startViewDirection;

		public float startPlayerPosLevel;

		public bool needToProneAtStart;

		public Vector3 cutsceneEndPlayerPos;
	}

	private static CutsceneTriggerStartInfoSO cutsceneTriggerStartInfoSO_0;

	[SerializeField]
	private List<SceneCutscenesInfoRelation> infoByScenes = new List<SceneCutscenesInfoRelation>();

	private Dictionary<string, Dictionary<int, StartPlayerValues>> dictionary_0 = new Dictionary<string, Dictionary<int, StartPlayerValues>>();

	private Dictionary<int, Dictionary<int, StartPlayerValues>> dictionary_1 = new Dictionary<int, Dictionary<int, StartPlayerValues>>();

	public static CutsceneTriggerStartInfoSO Instance
	{
		get
		{
			if (cutsceneTriggerStartInfoSO_0 == null)
			{
				cutsceneTriggerStartInfoSO_0 = Resources.Load<CutsceneTriggerStartInfoSO>("CutsceneTriggerStartInfoSO");
				cutsceneTriggerStartInfoSO_0.method_0();
			}
			return cutsceneTriggerStartInfoSO_0;
		}
	}

	public void method_0()
	{
		dictionary_0.Clear();
		foreach (SceneCutscenesInfoRelation infoByScene in infoByScenes)
		{
			if (!dictionary_0.ContainsKey(infoByScene.sceneName))
			{
				dictionary_0[infoByScene.sceneName] = new Dictionary<int, StartPlayerValues>();
			}
			foreach (CutsceneIDWithStartValues item in infoByScene.cutsceneTriggersStartInfo)
			{
				dictionary_0[infoByScene.sceneName].Add(item.cutsceneID, item.startValues);
			}
		}
		dictionary_1.Clear();
		foreach (SceneCutscenesInfoRelation infoByScene2 in infoByScenes)
		{
			int hashCode = infoByScene2.sceneName.GetHashCode();
			if (!dictionary_1.ContainsKey(hashCode))
			{
				dictionary_1[hashCode] = new Dictionary<int, StartPlayerValues>();
			}
			foreach (CutsceneIDWithStartValues item2 in infoByScene2.cutsceneTriggersStartInfo)
			{
				dictionary_1[hashCode].Add(item2.cutsceneID, item2.startValues);
			}
		}
	}

	public StartPlayerValues GetPlayerStartInfo(string sceneName, int cutsceneID)
	{
		if (dictionary_0.TryGetValue(sceneName, out var value) && value.TryGetValue(cutsceneID, out var value2))
		{
			return value2;
		}
		return null;
	}

	public StartPlayerValues GetPlayerStartInfo(int sceneNameHash, int cutsceneID)
	{
		if (dictionary_1.TryGetValue(sceneNameHash, out var value) && value.TryGetValue(cutsceneID, out var value2))
		{
			return value2;
		}
		return null;
	}
}
