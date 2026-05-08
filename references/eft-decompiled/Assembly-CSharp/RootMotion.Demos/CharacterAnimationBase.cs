using UnityEngine;

namespace RootMotion.Demos;

public abstract class CharacterAnimationBase : MonoBehaviour
{
	public bool smoothFollow = true;

	public float smoothFollowSpeed = 20f;

	private Vector3 vector3_0;

	private Vector3 vector3_1;

	private Quaternion quaternion_0;

	private Quaternion quaternion_1;

	public virtual bool animationGrounded => true;

	public virtual Vector3 GetPivotPoint()
	{
		return base.transform.position;
	}

	public float GetAngleFromForward(Vector3 worldDirection)
	{
		Vector3 vector = base.transform.InverseTransformDirection(worldDirection);
		return Mathf.Atan2(vector.x, vector.z) * 57.29578f;
	}

	public virtual void Start()
	{
		if (base.transform.parent.GetComponent<CharacterBase>() == null)
		{
			Debug.LogWarning("Animation controllers should be parented to character controllers!", base.transform);
		}
		vector3_0 = base.transform.position;
		vector3_1 = base.transform.parent.InverseTransformPoint(base.transform.position);
		quaternion_1 = base.transform.rotation;
		quaternion_0 = Quaternion.Inverse(base.transform.parent.rotation) * base.transform.rotation;
	}

	public virtual void LateUpdate()
	{
		if (smoothFollow)
		{
			base.transform.position = Vector3.Lerp(vector3_0, base.transform.parent.TransformPoint(vector3_1), Time.deltaTime * smoothFollowSpeed);
			base.transform.rotation = Quaternion.Lerp(quaternion_1, base.transform.parent.rotation * quaternion_0, Time.deltaTime * smoothFollowSpeed);
		}
		vector3_0 = base.transform.position;
		quaternion_1 = base.transform.rotation;
	}

	public CharacterAnimationBase()
	{
	}
}
