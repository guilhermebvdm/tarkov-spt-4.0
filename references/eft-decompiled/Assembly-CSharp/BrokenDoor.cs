using System.Collections;
using UnityEngine;

public class BrokenDoor : MonoBehaviour
{
	public Rigidbody[] Splinters;

	public Vector3 ExplosionCenter;

	public ParticleSystem VFX;

	[Header("Turned ON when breaking door")]
	public GameObject[] On;

	[Header("Turned OFF when breaking door")]
	public GameObject[] Off;

	public Collider[] IgnoreColliders;

	public void OnDrawGizmos()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawCube(base.transform.parent.InverseTransformPoint(ExplosionCenter), Vector3.one * 0.2f);
	}

	public void Init()
	{
		Rigidbody[] splinters = Splinters;
		foreach (Rigidbody rigidbody in splinters)
		{
			Collider[] ignoreColliders = IgnoreColliders;
			foreach (Collider collider in ignoreColliders)
			{
				if (collider != null)
				{
					Physics.IgnoreCollision(rigidbody.GetComponentInChildren<Collider>(), collider);
				}
			}
		}
	}

	public void BreachQuick()
	{
		GameObject[] off = Off;
		for (int i = 0; i < off.Length; i++)
		{
			off[i].SetActive(value: false);
		}
		off = On;
		for (int i = 0; i < off.Length; i++)
		{
			off[i].SetActive(value: true);
		}
	}

	public void Breach(Vector3 playerPosition)
	{
		GameObject[] off = Off;
		for (int i = 0; i < off.Length; i++)
		{
			off[i].SetActive(value: false);
		}
		off = On;
		for (int i = 0; i < off.Length; i++)
		{
			off[i].SetActive(value: true);
		}
		base.gameObject.SetActive(value: true);
		Vector3 vector = base.transform.parent.InverseTransformPoint(ExplosionCenter);
		Vector3 vector2 = base.transform.parent.InverseTransformPoint(new Vector3(0f - ExplosionCenter.x, ExplosionCenter.y, ExplosionCenter.z));
		Vector3 explosionPosition = ((Vector3.Distance(vector2, playerPosition) > Vector3.Distance(vector, playerPosition)) ? vector : vector2);
		VFX.Play(withChildren: true);
		Rigidbody[] splinters = Splinters;
		foreach (Rigidbody rigidbody in splinters)
		{
			if (!(rigidbody == null))
			{
				rigidbody.isKinematic = false;
				rigidbody.AddExplosionForce(0.66f, explosionPosition, 1f, 0.3f, ForceMode.Impulse);
				EFTPhysicsClass.GClass745.SupportRigidbody(rigidbody, 0f);
			}
		}
		StartCoroutine(method_0());
	}

	public IEnumerator method_0()
	{
		float num = 0f;
		int num2;
		do
		{
			yield return new WaitForSeconds(1f);
			num += 1f;
			bool flag = num > 5f;
			num2 = 0;
			Rigidbody[] splinters = Splinters;
			foreach (Rigidbody rigidbody in splinters)
			{
				if (!(rigidbody == null))
				{
					if (rigidbody.IsSleeping() || flag)
					{
						Object.Destroy(rigidbody);
					}
					else
					{
						num2++;
					}
				}
			}
		}
		while (num2 > 0);
	}
}
