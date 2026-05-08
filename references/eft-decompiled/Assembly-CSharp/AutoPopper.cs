using EFT.Ballistics;
using UnityEngine;

public class AutoPopper : MonoBehaviour
{
	public Transform MovingPart;

	public BallisticCollider Target;

	public Vector3 DefaultRotation;

	public Vector3 HitRotation = new Vector3(90f, 0f, 0f);

	public float HitTime = 3f;

	private float float_0;

	public void Start()
	{
		Target.OnHitAction += method_0;
	}

	public void method_0(DamageInfoStruct obj)
	{
		MovingPart.localRotation = Quaternion.Euler(HitRotation);
		float_0 = HitTime;
	}

	public void Update()
	{
		if (float_0 > 0f)
		{
			float_0 -= Time.deltaTime;
			if (float_0 <= 0f)
			{
				MovingPart.localRotation = Quaternion.Euler(DefaultRotation);
			}
		}
	}
}
