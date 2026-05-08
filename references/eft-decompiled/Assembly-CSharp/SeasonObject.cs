using UnityEngine;

public class SeasonObject : MonoBehaviour
{
	public void Awake()
	{
		MonoBehaviourSingleton<SeasonObjectsRegistry>.Instance.Add(base.gameObject);
	}

	public void OnDisable()
	{
		MonoBehaviourSingleton<SeasonObjectsRegistry>.Instance?.Remove(base.gameObject);
	}
}
