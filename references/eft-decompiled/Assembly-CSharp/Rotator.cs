using UnityEngine;

public class Rotator : MonoBehaviour
{
	[SerializeField]
	private Vector3 _axis;

	[SerializeField]
	private float _speed;

	public void LateUpdate()
	{
		base.transform.Rotate(_axis, _speed * Time.unscaledDeltaTime);
	}
}
