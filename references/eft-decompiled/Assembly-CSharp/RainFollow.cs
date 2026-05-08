using UnityEngine;

[ExecuteInEditMode]
public class RainFollow : MonoBehaviour
{
	[SerializeField]
	private Transform _target;

	[SerializeField]
	private float _offset = 10f;

	private Transform transform_0;

	public void Start()
	{
		transform_0 = GetComponent<Transform>();
	}

	public void Update()
	{
		transform_0.position = _target.position + new Vector3(0f, _offset, 0f);
	}
}
