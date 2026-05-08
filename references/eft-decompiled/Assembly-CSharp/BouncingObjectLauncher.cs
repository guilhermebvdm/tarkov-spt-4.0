using UnityEngine;

public class BouncingObjectLauncher : MonoBehaviour
{
	[SerializeField]
	private LayerMask _castMask;

	[SerializeField]
	private int _maxCastCount;

	[SerializeField]
	private float _deltaTimeStep;

	[SerializeField]
	private float _jumpSpeed;

	[SerializeField]
	private float _playMult;

	[SerializeField]
	private float _randomReboundSpread;

	[SerializeField]
	private float _bounceSpeedMult;

	[SerializeField]
	private float _radius;

	private GameObject gameObject_0;

	public void Launch()
	{
		method_0(base.transform);
	}

	public void method_0(Transform from)
	{
		if (gameObject_0 != null)
		{
			Object.DestroyImmediate(gameObject_0);
		}
		gameObject_0 = new GameObject("BouncingObject");
		gameObject_0.AddComponent<BouncingObject>().Init(from.position, from.forward * _jumpSpeed, _radius, _playMult, _castMask, _maxCastCount, _deltaTimeStep, _randomReboundSpread, _bounceSpeedMult, showDebug: true);
	}
}
