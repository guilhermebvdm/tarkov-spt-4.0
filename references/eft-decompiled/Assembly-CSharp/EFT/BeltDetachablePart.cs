using System.Collections;
using UnityEngine;

namespace EFT;

public class BeltDetachablePart : MonoBehaviour
{
	[SerializeField]
	private Rigidbody _rigidbody;

	[SerializeField]
	private EAmmoBeltSpawnDirection _spawnDirection;

	[SerializeField]
	private float _beltLifetime = 3f;

	[SerializeField]
	private float _forceInSpawn = 2f;

	private IEnumerator ienumerator_0;

	private bool bool_0;

	public void Spawn()
	{
		EFTPhysicsClass.GClass745.SupportRigidbody(_rigidbody);
		_rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
		_rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
		Vector3 vector = method_0(_spawnDirection);
		_rigidbody.AddForceAtPosition(vector * _forceInSpawn, base.transform.position);
	}

	public void OnCollisionEnter(Collision other)
	{
		if (!bool_0)
		{
			bool_0 = true;
			ienumerator_0 = method_1();
			StartCoroutine(ienumerator_0);
		}
	}

	public Vector3 method_0(EAmmoBeltSpawnDirection spawnDirection)
	{
		return spawnDirection switch
		{
			EAmmoBeltSpawnDirection.Right => base.transform.right, 
			EAmmoBeltSpawnDirection.Left => -base.transform.right, 
			EAmmoBeltSpawnDirection.Forward => base.transform.forward, 
			EAmmoBeltSpawnDirection.Backward => -base.transform.forward, 
			EAmmoBeltSpawnDirection.Up => base.transform.up, 
			EAmmoBeltSpawnDirection.Down => -base.transform.up, 
			_ => base.transform.right, 
		};
	}

	public IEnumerator method_1()
	{
		float num = _beltLifetime;
		while (num > 0f)
		{
			num -= Time.deltaTime;
			yield return null;
		}
		ienumerator_0 = null;
		Object.DestroyImmediate(base.gameObject);
	}

	public void OnDestroy()
	{
		if (ienumerator_0 != null)
		{
			StopCoroutine(ienumerator_0);
		}
	}
}
