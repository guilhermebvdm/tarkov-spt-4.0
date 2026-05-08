using System;
using System.Collections;
using UnityEngine;

namespace RootMotion.FinalIK;

[RequireComponent(typeof(Animator))]
public class RagdollUtility : MonoBehaviour
{
	public class GClass1467
	{
		public Rigidbody r;

		public Transform t;

		public Collider collider;

		public Joint joint;

		public Rigidbody c;

		public bool updateAnchor;

		public Vector3 deltaPosition;

		public Quaternion deltaRotation;

		public float deltaTime;

		public Vector3 lastPosition;

		public Quaternion lastRotation;

		public GClass1467(Rigidbody r)
		{
			this.r = r;
			t = r.transform;
			joint = t.GetComponent<Joint>();
			collider = t.GetComponent<Collider>();
			if (joint != null)
			{
				c = joint.connectedBody;
				updateAnchor = c != null;
			}
			lastPosition = t.position;
			lastRotation = t.rotation;
		}

		public void RecordVelocity()
		{
			deltaPosition = t.position - lastPosition;
			lastPosition = t.position;
			deltaRotation = GClass1463.FromToRotation(lastRotation, t.rotation);
			lastRotation = t.rotation;
			deltaTime = Time.deltaTime;
		}

		public void WakeUp(float velocityWeight, float angularVelocityWeight)
		{
			if (updateAnchor)
			{
				joint.connectedAnchor = t.InverseTransformPoint(c.position);
			}
			r.isKinematic = false;
			if (velocityWeight != 0f)
			{
				r.velocity = deltaPosition / deltaTime * velocityWeight;
			}
			if (angularVelocityWeight != 0f)
			{
				float angle = 0f;
				Vector3 axis = Vector3.zero;
				deltaRotation.ToAngleAxis(out angle, out axis);
				angle *= MathF.PI / 180f;
				angle /= deltaTime;
				axis *= angle * angularVelocityWeight;
				r.angularVelocity = Vector3.ClampMagnitude(axis, r.maxAngularVelocity);
			}
			r.WakeUp();
		}
	}

	public class GClass1468
	{
		public Transform t;

		public Vector3 localPosition;

		public Quaternion localRotation;

		public GClass1468(Transform transform)
		{
			t = transform;
			localPosition = t.localPosition;
			localRotation = t.localRotation;
		}

		public void FixTransform(float weight)
		{
			if (!(weight <= 0f))
			{
				if (weight >= 1f)
				{
					t.localPosition = localPosition;
					t.localRotation = localRotation;
				}
				else
				{
					t.localPosition = Vector3.Lerp(t.localPosition, localPosition, weight);
					t.localRotation = Quaternion.Lerp(t.localRotation, localRotation, weight);
				}
			}
		}

		public void StoreLocalState()
		{
			localPosition = t.localPosition;
			localRotation = t.localRotation;
		}
	}

	[Tooltip("If you have multiple IK components, then this should be the one that solves last each frame.")]
	public IK ik;

	[Tooltip("How long does it take to blend from ragdoll to animation?")]
	public float ragdollToAnimationTime = 0.2f;

	[Tooltip("If true, IK can be used on top of physical ragdoll simulation.")]
	public bool applyIkOnRagdoll;

	[Tooltip("How much velocity transfer from animation to ragdoll?")]
	public float applyVelocity = 1f;

	[Tooltip("How much angular velocity to transfer from animation to ragdoll?")]
	public float applyAngularVelocity = 1f;

	private Animator animator_0;

	private GClass1467[] gclass1467_0 = new GClass1467[0];

	private GClass1468[] gclass1468_0 = new GClass1468[0];

	private bool bool_0;

	private AnimatorUpdateMode animatorUpdateMode_0;

	private IK[] ik_0 = new IK[0];

	private bool[] bool_1 = new bool[0];

	private float float_0;

	private float float_1;

	private bool bool_2;

	private bool[] bool_3 = new bool[0];

	public bool Boolean_0
	{
		get
		{
			if (!gclass1467_0[0].r.isKinematic)
			{
				return !animator_0.enabled;
			}
			return false;
		}
	}

	public bool Boolean_1
	{
		get
		{
			if (ik == null)
			{
				return false;
			}
			if (ik.enabled && ik.GetIKSolver().IKPositionWeight > 0f)
			{
				return true;
			}
			IK[] array = ik_0;
			int num = 0;
			while (true)
			{
				if (num < array.Length)
				{
					IK iK = array[num];
					if (iK.enabled && !(iK.GetIKSolver().IKPositionWeight <= 0f))
					{
						break;
					}
					num++;
					continue;
				}
				return false;
			}
			return true;
		}
	}

	public void EnableRagdoll()
	{
		if (!Boolean_0)
		{
			StopAllCoroutines();
			bool_0 = true;
		}
	}

	public void DisableRagdoll()
	{
		if (Boolean_0)
		{
			method_6();
			StopAllCoroutines();
			StartCoroutine(method_0());
		}
	}

	public void Start()
	{
		animator_0 = GetComponent<Animator>();
		ik_0 = GetComponentsInChildren<IK>();
		bool_3 = new bool[ik_0.Length];
		bool_1 = new bool[ik_0.Length];
		if (ik != null)
		{
			IKSolver iKSolver = ik.GetIKSolver();
			iKSolver.OnPostUpdate = (IKSolver.GDelegate47)Delegate.Combine(iKSolver.OnPostUpdate, new IKSolver.GDelegate47(method_1));
		}
		Rigidbody[] componentsInChildren = GetComponentsInChildren<Rigidbody>();
		int num = ((componentsInChildren[0].gameObject == base.gameObject) ? 1 : 0);
		gclass1467_0 = new GClass1467[(num == 0) ? componentsInChildren.Length : (componentsInChildren.Length - 1)];
		for (int i = 0; i < gclass1467_0.Length; i++)
		{
			gclass1467_0[i] = new GClass1467(componentsInChildren[i + num]);
		}
		Transform[] componentsInChildren2 = GetComponentsInChildren<Transform>();
		gclass1468_0 = new GClass1468[componentsInChildren2.Length - 1];
		for (int j = 0; j < gclass1468_0.Length; j++)
		{
			gclass1468_0[j] = new GClass1468(componentsInChildren2[j + 1]);
		}
	}

	public IEnumerator method_0()
	{
		for (int i = 0; i < gclass1467_0.Length; i++)
		{
			gclass1467_0[i].r.isKinematic = true;
		}
		for (int j = 0; j < ik_0.Length; j++)
		{
			ik_0[j].fixTransforms = bool_1[j];
			if (bool_3[j])
			{
				ik_0[j].enabled = true;
			}
		}
		animator_0.updateMode = animatorUpdateMode_0;
		animator_0.enabled = true;
		while (float_0 > 0f)
		{
			float_0 = Mathf.SmoothDamp(float_0, 0f, ref float_1, ragdollToAnimationTime);
			if (float_0 < 0.001f)
			{
				float_0 = 0f;
			}
			yield return null;
		}
		yield return null;
	}

	public void Update()
	{
		if (!Boolean_0)
		{
			return;
		}
		if (!applyIkOnRagdoll)
		{
			bool flag = false;
			for (int i = 0; i < ik_0.Length; i++)
			{
				if (ik_0[i].enabled)
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				for (int j = 0; j < ik_0.Length; j++)
				{
					bool_3[j] = false;
				}
			}
			for (int k = 0; k < ik_0.Length; k++)
			{
				if (ik_0[k].enabled)
				{
					ik_0[k].enabled = false;
					bool_3[k] = true;
				}
			}
			return;
		}
		bool flag2 = false;
		for (int l = 0; l < ik_0.Length; l++)
		{
			if (bool_3[l])
			{
				flag2 = true;
				break;
			}
		}
		if (!flag2)
		{
			return;
		}
		for (int m = 0; m < ik_0.Length; m++)
		{
			if (bool_3[m])
			{
				ik_0[m].enabled = true;
			}
		}
		for (int n = 0; n < ik_0.Length; n++)
		{
			bool_3[n] = false;
		}
	}

	public void FixedUpdate()
	{
		if (Boolean_0 && applyIkOnRagdoll)
		{
			method_7(1f);
		}
		bool_2 = true;
	}

	public void LateUpdate()
	{
		if (animator_0.updateMode != AnimatorUpdateMode.AnimatePhysics || (animator_0.updateMode == AnimatorUpdateMode.AnimatePhysics && bool_2))
		{
			method_2();
		}
		bool_2 = false;
		if (!Boolean_1)
		{
			method_3();
		}
	}

	public void method_1()
	{
		if (Boolean_1)
		{
			method_3();
		}
	}

	public void method_2()
	{
		if (Boolean_0)
		{
			method_6();
		}
		else
		{
			method_7(float_0);
		}
	}

	public void method_3()
	{
		if (!Boolean_0)
		{
			method_5();
		}
		if (bool_0)
		{
			method_4();
		}
	}

	public void method_4()
	{
		method_6();
		for (int i = 0; i < ik_0.Length; i++)
		{
			bool_3[i] = false;
		}
		if (!applyIkOnRagdoll)
		{
			for (int j = 0; j < ik_0.Length; j++)
			{
				if (ik_0[j].enabled)
				{
					ik_0[j].enabled = false;
					bool_3[j] = true;
				}
			}
		}
		animatorUpdateMode_0 = animator_0.updateMode;
		animator_0.updateMode = AnimatorUpdateMode.AnimatePhysics;
		animator_0.enabled = false;
		for (int k = 0; k < gclass1467_0.Length; k++)
		{
			gclass1467_0[k].WakeUp(applyVelocity, applyAngularVelocity);
		}
		for (int l = 0; l < bool_1.Length; l++)
		{
			bool_1[l] = ik_0[l].fixTransforms;
			ik_0[l].fixTransforms = false;
		}
		float_0 = 1f;
		float_1 = 0f;
		bool_0 = false;
	}

	public void method_5()
	{
		GClass1467[] array = gclass1467_0;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].RecordVelocity();
		}
	}

	public void method_6()
	{
		GClass1468[] array = gclass1468_0;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].StoreLocalState();
		}
	}

	public void method_7(float weight)
	{
		GClass1468[] array = gclass1468_0;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].FixTransform(weight);
		}
	}

	public void OnDestroy()
	{
		if (ik != null)
		{
			IKSolver iKSolver = ik.GetIKSolver();
			iKSolver.OnPostUpdate = (IKSolver.GDelegate47)Delegate.Remove(iKSolver.OnPostUpdate, new IKSolver.GDelegate47(method_1));
		}
	}
}
