using System;
using System.Collections.Generic;
using AmplifyMotion;
using UnityEngine;
using UnityEngine.Rendering;

[AddComponentMenu("")]
public class AmplifyMotionObjectBase : MonoBehaviour
{
	public enum MinMaxCurveState
	{
		Scalar,
		Curve,
		TwoCurves,
		TwoScalars
	}

	internal static bool bool_0 = true;

	[SerializeField]
	private bool m_applyToChildren = bool_0;

	private ObjectType objectType_0;

	private Dictionary<Camera, MotionState> dictionary_0 = new Dictionary<Camera, MotionState>();

	private bool bool_1;

	private int int_0;

	private Vector3 vector3_0 = Vector3.zero;

	private int int_1 = -1;

	public bool Boolean_0 => bool_1;

	public int Int32_0 => int_0;

	public ObjectType Type => objectType_0;

	public void method_0(AmplifyMotionCamera camera)
	{
		Camera component = camera.GetComponent<Camera>();
		if ((component.cullingMask & (1 << base.gameObject.layer)) != 0 && !dictionary_0.ContainsKey(component))
		{
			MotionState motionState = null;
			motionState = objectType_0 switch
			{
				ObjectType.Solid => new Class3640(camera, this), 
				ObjectType.Skinned => new Class3639(camera, this), 
				ObjectType.Cloth => new Class3637(camera, this), 
				ObjectType.Particle => new Class3638(camera, this), 
				_ => throw new Exception("[AmplifyMotion] Invalid object type."), 
			};
			camera.RegisterObject(this);
			dictionary_0.Add(component, motionState);
		}
	}

	public void method_1(AmplifyMotionCamera camera)
	{
		Camera component = camera.GetComponent<Camera>();
		if (dictionary_0.TryGetValue(component, out var value))
		{
			camera.UnregisterObject(this);
			if (dictionary_0.TryGetValue(component, out value))
			{
				value.vmethod_1();
			}
			dictionary_0.Remove(component);
		}
	}

	public bool method_2()
	{
		Renderer component = GetComponent<Renderer>();
		if (AmplifyMotionEffectBase.smethod_5(base.gameObject, autoReg: false))
		{
			if (GetComponent<ParticleSystem>() != null)
			{
				objectType_0 = ObjectType.Particle;
				AmplifyMotionEffectBase.smethod_2(this);
			}
			else if (component != null)
			{
				if (component.GetType() == typeof(MeshRenderer))
				{
					objectType_0 = ObjectType.Solid;
				}
				else if (component.GetType() == typeof(SkinnedMeshRenderer))
				{
					if (GetComponent<Cloth>() != null)
					{
						objectType_0 = ObjectType.Cloth;
					}
					else
					{
						objectType_0 = ObjectType.Skinned;
					}
				}
				AmplifyMotionEffectBase.smethod_2(this);
			}
		}
		return component != null;
	}

	public void OnEnable()
	{
		bool flag;
		if (flag = method_2())
		{
			if (objectType_0 == ObjectType.Cloth)
			{
				bool_1 = false;
			}
			else if (objectType_0 == ObjectType.Solid)
			{
				Rigidbody component = GetComponent<Rigidbody>();
				if (component != null && component.interpolation == RigidbodyInterpolation.None && !component.isKinematic)
				{
					bool_1 = true;
				}
			}
		}
		if (m_applyToChildren)
		{
			foreach (Transform item in base.gameObject.transform)
			{
				AmplifyMotionEffectBase.RegisterRecursivelyS(item.gameObject);
			}
		}
		if (!flag)
		{
			base.enabled = false;
		}
	}

	public void OnDisable()
	{
		AmplifyMotionEffectBase.smethod_3(this);
	}

	public void method_3()
	{
		Dictionary<Camera, MotionState>.Enumerator enumerator = dictionary_0.GetEnumerator();
		while (enumerator.MoveNext())
		{
			MotionState value = enumerator.Current.Value;
			if (value.Owner.Initialized && !value.Error && !value.Initialized)
			{
				value.vmethod_0();
			}
		}
	}

	public void Start()
	{
		if (AmplifyMotionEffectBase.Instance != null)
		{
			method_3();
		}
		vector3_0 = base.transform.position;
	}

	public void Update()
	{
		if (AmplifyMotionEffectBase.Instance != null)
		{
			method_3();
		}
	}

	public static void smethod_0(Transform transform, AmplifyMotionObjectBase obj, int frame)
	{
		if (obj != null)
		{
			obj.int_1 = frame;
		}
		foreach (Transform item in transform)
		{
			smethod_0(item, item.GetComponent<AmplifyMotionObjectBase>(), frame);
		}
	}

	public void ResetMotionNow()
	{
		smethod_0(base.transform, this, Time.frameCount);
	}

	public void ResetMotionAtFrame(int frame)
	{
		smethod_0(base.transform, this, frame);
	}

	public void method_4(AmplifyMotionEffectBase inst)
	{
		if (Vector3.SqrMagnitude(base.transform.position - vector3_0) > inst.MinResetDeltaDistSqr)
		{
			smethod_0(base.transform, this, Time.frameCount + inst.ResetFrameDelay);
		}
	}

	public void method_5(AmplifyMotionEffectBase inst, Camera camera, CommandBuffer updateCB, bool starting)
	{
		if (dictionary_0.TryGetValue(camera, out var value) && !value.Error)
		{
			method_4(inst);
			bool flag = int_1 > 0 && Time.frameCount >= int_1;
			value.vmethod_3(updateCB, starting || flag);
		}
		vector3_0 = base.transform.position;
	}

	public void method_6(Camera camera, CommandBuffer renderCB, float scale, Quality quality)
	{
		if (dictionary_0.TryGetValue(camera, out var value) && !value.Error)
		{
			value.vmethod_4(camera, renderCB, scale, quality);
			if (int_1 > 0 && Time.frameCount >= int_1)
			{
				int_1 = -1;
			}
		}
	}
}
