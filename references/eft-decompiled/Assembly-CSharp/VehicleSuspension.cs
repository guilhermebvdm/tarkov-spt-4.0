using System.Collections;
using Comfort.Common;
using EFT;
using EFT.Vehicle.Vehicles;
using UnityEngine;

public class VehicleSuspension : MonoBehaviour
{
	public Rigidbody parentBody;

	public ConfigurableJoint joint;

	public SuspensionSpring springFLeft;

	public SuspensionSpring springFRight;

	public SuspensionSpring springBLeft;

	public SuspensionSpring springBRight;

	public bool suspensionEnabled;

	public Collider SuspensionCollider;

	public GameObject ViewRoot;

	private Coroutine coroutine_0;

	private Coroutine coroutine_1;

	private float float_0;

	public void OnDestroy()
	{
		if (coroutine_0 != null)
		{
			StopCoroutine(coroutine_0);
			coroutine_0 = null;
		}
		if (coroutine_1 != null)
		{
			StopCoroutine(coroutine_1);
			coroutine_1 = null;
		}
	}

	public void SetSuspensionParams(float restLength, float travel, float springStiffness, float damperStiffness, float wheelRadius)
	{
		method_0(springFLeft.spring, restLength, travel, springStiffness, damperStiffness, wheelRadius);
		method_0(springFRight.spring, restLength, travel, springStiffness, damperStiffness, wheelRadius);
		method_0(springBLeft.spring, restLength, travel, springStiffness, damperStiffness, wheelRadius);
		method_0(springBRight.spring, restLength, travel, springStiffness, damperStiffness, wheelRadius);
	}

	public void method_0(VehicleSuspensionSpring spring, float restLength, float travel, float springStiffness, float damperStiffness, float wheelRadius)
	{
		spring.restLenght = restLength;
		spring.springTravel = travel;
		spring.springStiffness = springStiffness;
		spring.damperStiffness = damperStiffness;
		spring.wheelRadius = wheelRadius;
		spring.CalculateBaseParams();
	}

	public void SetSuspensionInertiaOffset(float offset)
	{
		springFLeft.spring.SetOffset(offset);
		springFRight.spring.SetOffset(offset);
		springBLeft.spring.SetOffset(offset * -1f);
		springBRight.spring.SetOffset(offset * -1f);
		float_0 = offset;
	}

	public void SuspensionOn()
	{
		springFLeft.spring.gameObject.SetActive(value: true);
		springFRight.spring.gameObject.SetActive(value: true);
		springBLeft.spring.gameObject.SetActive(value: true);
		springBRight.spring.gameObject.SetActive(value: true);
		AddViewCollidersToIgnore();
		parentBody.isKinematic = false;
		parentBody.useGravity = true;
		EFTPhysicsClass.GClass745.SupportRigidbody(parentBody);
		joint.yMotion = ConfigurableJointMotion.Free;
	}

	public void SuspensionOff()
	{
		parentBody.isKinematic = true;
		parentBody.useGravity = false;
		springFLeft.spring.gameObject.SetActive(value: false);
		springFRight.spring.gameObject.SetActive(value: false);
		springBLeft.spring.gameObject.SetActive(value: false);
		springBRight.spring.gameObject.SetActive(value: false);
		suspensionEnabled = false;
	}

	public void EnableSuspensionCoroutineStart(float time)
	{
		suspensionEnabled = true;
		coroutine_0 = StartCoroutine(method_1(time));
	}

	public void DisableSuspensionCoroutineStart(float time)
	{
		suspensionEnabled = false;
		coroutine_1 = StartCoroutine(method_2(time));
	}

	public IEnumerator method_1(float time)
	{
		if (!(Singleton<GameWorld>.Instance.LocationId == "TarkovStreets"))
		{
			yield return new WaitForSeconds(time);
			SuspensionOn();
		}
	}

	public IEnumerator method_2(float time)
	{
		if (!(Singleton<GameWorld>.Instance.LocationId == "TarkovStreets"))
		{
			yield return new WaitForSeconds(time);
			SuspensionOff();
		}
	}

	public void AddViewCollidersToIgnore()
	{
		Collider[] componentsInChildren = ViewRoot.GetComponentsInChildren<Collider>();
		foreach (Collider collider in componentsInChildren)
		{
			Physics.IgnoreCollision(SuspensionCollider, collider);
		}
	}
}
