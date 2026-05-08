using System.Collections.Generic;
using RootMotion.FinalIK;
using UnityEngine;

public class PlayerAnimationTest : MonoBehaviour
{
	[SerializeField]
	private GameObject _playerPrefab;

	[SerializeField]
	private int _count = 20;

	private List<Animator> list_0 = new List<Animator>();

	private bool bool_0;

	private bool bool_1;

	public void Awake()
	{
		for (int i = 0; i < _count; i++)
		{
			GameObject gameObject = Object.Instantiate(_playerPrefab);
			method_0(gameObject);
			gameObject.SetActive(value: true);
			Animator componentInChildren = gameObject.GetComponentInChildren<Animator>();
			list_0.Add(componentInChildren);
			gameObject.transform.position = Vector3.Lerp(Vector3.zero, new Vector3(0f, 0f, _count), (float)(i + 1) / (float)_count);
		}
	}

	public void method_0(GameObject gameObject)
	{
		Object.DestroyImmediate(gameObject.GetComponent<CharacterJoint>());
		Object.DestroyImmediate(gameObject.GetComponent<Collider>());
		Object.DestroyImmediate(gameObject.GetComponent<Rigidbody>());
		Object.DestroyImmediate(gameObject.GetComponent<FullBodyBipedIK>());
		Object.DestroyImmediate(gameObject.GetComponent<SolverManager>());
		Object.DestroyImmediate(gameObject.GetComponent<GrounderFBBIK>());
		for (int i = 0; i < gameObject.transform.childCount; i++)
		{
			method_0(gameObject.transform.GetChild(i).gameObject);
		}
	}

	public void Update()
	{
		if (Input.GetKeyDown(KeyCode.Space))
		{
			if (bool_0)
			{
				method_2();
			}
			else
			{
				method_1();
			}
			bool_0 = !bool_0;
		}
		if (!Input.GetKeyDown(KeyCode.Backspace) && !bool_1)
		{
			return;
		}
		foreach (Animator item in list_0)
		{
			item.SetFloat("Direct_X", Random.value);
		}
		bool_1 = true;
	}

	public void method_1()
	{
		foreach (Animator item in list_0)
		{
			item.SetBool("Inert", value: true);
			item.SetBool("Sprint", value: false);
			item.SetFloat("Speed", 1f);
		}
	}

	public void method_2()
	{
		foreach (Animator item in list_0)
		{
			item.SetBool("Inert", value: false);
		}
	}
}
