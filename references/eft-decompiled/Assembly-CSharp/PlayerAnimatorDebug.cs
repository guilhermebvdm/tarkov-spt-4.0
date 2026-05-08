using UnityEngine;

public class PlayerAnimatorDebug : MonoBehaviour
{
	public KeyCodeAnimatorParameter[] Keys;

	public Animator _a;

	public void Start()
	{
		_a = GetComponent<Animator>();
	}

	public void Update()
	{
		if (_a == null)
		{
			_a = GetComponent<Animator>();
			return;
		}
		KeyCodeAnimatorParameter[] keys = Keys;
		for (int i = 0; i < keys.Length; i++)
		{
			keys[i].ProcessInput(_a);
		}
	}
}
