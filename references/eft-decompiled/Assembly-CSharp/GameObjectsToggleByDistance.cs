using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameObjectsToggleByDistance : MonoBehaviour
{
	[Tooltip("Default state of game objects enable or disable")]
	[SerializeField]
	private bool defaultObjectsState = true;

	[SerializeField]
	private List<GameObject> objectsForToggle = new List<GameObject>();

	[SerializeField]
	private Transform pointForCalculation;

	[SerializeField]
	private float distanceForToggle = 0.01f;

	[SerializeField]
	private bool autoStart;

	private Coroutine coroutine_0;

	private float float_0;

	private List<Transform> list_0 = new List<Transform>();

	private WaitForEndOfFrame waitForEndOfFrame_0 = new WaitForEndOfFrame();

	public void Awake()
	{
		float_0 = distanceForToggle * distanceForToggle;
		foreach (GameObject item in objectsForToggle)
		{
			list_0.Add(item.transform);
		}
		if (autoStart)
		{
			method_0(toggle: true);
		}
	}

	public void OnDisable()
	{
		method_0(toggle: false);
	}

	public void OnDestroy()
	{
		method_0(toggle: false);
	}

	public void ToggleCheck(bool toggle)
	{
		method_0(toggle);
	}

	public void method_0(bool toggle)
	{
		if (toggle)
		{
			if (coroutine_0 == null)
			{
				coroutine_0 = StartCoroutine(method_1());
			}
		}
		else
		{
			if (coroutine_0 == null)
			{
				return;
			}
			StopCoroutine(coroutine_0);
			coroutine_0 = null;
			foreach (GameObject item in objectsForToggle)
			{
				item.SetActive(defaultObjectsState);
			}
		}
	}

	public IEnumerator method_1()
	{
		while (true)
		{
			for (int i = 0; i < list_0.Count; i++)
			{
				if ((list_0[i].position - pointForCalculation.position).sqrMagnitude <= float_0)
				{
					objectsForToggle[i].SetActive(!defaultObjectsState);
				}
				else
				{
					objectsForToggle[i].SetActive(defaultObjectsState);
				}
			}
			yield return waitForEndOfFrame_0;
		}
	}
}
