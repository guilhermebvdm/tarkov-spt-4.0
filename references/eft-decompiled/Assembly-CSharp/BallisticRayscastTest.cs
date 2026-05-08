using System;
using System.Runtime.CompilerServices;
using UnityEngine;

[ExecuteInEditMode]
public class BallisticRayscastTest : MonoBehaviour
{
	[Serializable]
	[CompilerGenerated]
	public class Class416
	{
		public static readonly Class416 class416_0 = new Class416();

		public static Func<RaycastHit, bool> func_0;

		public bool method_0(RaycastHit hit)
		{
			return false;
		}
	}

	[SerializeField]
	private Transform _point1;

	[SerializeField]
	private Transform _point2;

	[SerializeField]
	private float _forwardRadius = 0.005f;

	[SerializeField]
	private float _backwardRadius = 0.005f;

	[SerializeField]
	private int _forwardCount;

	private RaycastHit[] raycastHit_0 = new RaycastHit[32];

	[SerializeField]
	private int _backwardCount;

	private RaycastHit[] raycastHit_1 = new RaycastHit[32];

	[SerializeField]
	private bool _hitFound;

	private RaycastHit raycastHit_2;

	public void Update()
	{
		if (!(_point1 == null) && !(_point2 == null))
		{
			method_2();
		}
	}

	public void method_0()
	{
		_forwardCount = method_3(_point1.position, _point2.position, raycastHit_0);
		_backwardCount = method_3(_point2.position, _point1.position, raycastHit_1);
	}

	public void method_1()
	{
		Physics.queriesHitBackfaces = true;
		_forwardCount = method_3(_point1.position, _point2.position, raycastHit_0);
		Physics.queriesHitBackfaces = false;
	}

	public void method_2()
	{
		_hitFound = EFTPhysicsClass.LinecastPrecise(_point2.position, _point1.position, out raycastHit_2, LayerMasksDataAbstractClass.HitMask, reverseCheck: true, raycastHit_1, (RaycastHit hit) => false);
	}

	public int method_3(Vector3 point1, Vector3 point2, RaycastHit[] hits)
	{
		Vector3 vector = point2 - point1;
		return Physics.RaycastNonAlloc(new Ray(point1, vector.normalized), hits, vector.magnitude, LayerMasksDataAbstractClass.HitMask);
	}

	public void OnDrawGizmos()
	{
		Gizmos.color = Color.blue;
		Gizmos.DrawSphere(_point1.position, 0.05f);
		method_4(raycastHit_0, _forwardCount, _forwardRadius, Color.blue);
		method_4(raycastHit_1, _backwardCount, _backwardRadius, Color.red);
		Gizmos.color = Color.red;
		Gizmos.DrawSphere(_point2.position, 0.05f);
		Gizmos.color = Color.white;
		Gizmos.DrawLine(_point1.position, _point2.position);
		if (_hitFound)
		{
			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere(raycastHit_2.point, _backwardRadius * 1.25f);
		}
	}

	public void method_4(RaycastHit[] hits, int count, float radius, Color color)
	{
		for (int i = 0; i < count; i++)
		{
			RaycastHit raycastHit = hits[i];
			Gizmos.color = color;
			Gizmos.DrawSphere(raycastHit.point, radius);
		}
	}
}
