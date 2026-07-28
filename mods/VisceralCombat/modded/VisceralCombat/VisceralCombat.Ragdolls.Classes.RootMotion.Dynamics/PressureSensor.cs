using UnityEngine;

namespace VisceralCombat.Ragdolls.Classes.RootMotion.Dynamics;

public class PressureSensor : MonoBehaviour
{
	public bool visualize;

	public LayerMask layers;

	private bool fixedFrame;

	private Vector3 P;

	private int count;

	public Vector3 center { get; private set; }

	public bool inContact { get; private set; }

	public Vector3 bottom { get; private set; }

	public Rigidbody r { get; private set; }

	private void Awake()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		r = ((Component)this).GetComponent<Rigidbody>();
		center = ((Component)this).transform.position;
	}

	private void OnCollisionEnter(Collision c)
	{
		ProcessCollision(c);
	}

	private void OnCollisionStay(Collision c)
	{
		ProcessCollision(c);
	}

	private void OnCollisionExit(Collision c)
	{
		inContact = false;
	}

	private void FixedUpdate()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		fixedFrame = true;
		if (!r.IsSleeping())
		{
			P = Vector3.zero;
			count = 0;
		}
	}

	private void LateUpdate()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		if (fixedFrame)
		{
			if (count > 0)
			{
				center = P / (float)count;
			}
			fixedFrame = false;
		}
	}

	private void ProcessCollision(Collision c)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		if (LayerMaskExtensions.Contains(layers, c.gameObject.layer))
		{
			Vector3 val = Vector3.zero;
			for (int i = 0; i < c.contacts.Length; i++)
			{
				val += ((ContactPoint)(ref c.contacts[i])).point;
			}
			val /= (float)c.contacts.Length;
			P += val;
			count++;
			inContact = true;
		}
	}

	private void OnDrawGizmos()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		if (visualize)
		{
			Gizmos.DrawSphere(center, 0.1f);
		}
	}
}
