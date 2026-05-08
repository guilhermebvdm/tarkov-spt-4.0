using UnityEngine;

public class PlayerOverlapManager : MonoBehaviour
{
	public enum EExtrusionCollider
	{
		Default,
		Prone
	}

	[SerializeField]
	private ColliderExtrusion _headColliderExtrusion;

	[SerializeField]
	private ColliderExtrusion _cameraColliderExtrusion;

	[SerializeField]
	private Collider[] _headColliders;

	[SerializeField]
	private Transform _proneColliderPivot;

	private Vector3 vector3_0;

	private EExtrusionCollider eextrusionCollider_0;

	public Collider Collider => _headColliderExtrusion.Collider;

	public Collider CameraCollider => _cameraColliderExtrusion.Collider;

	public void Init(Collider playerCollider)
	{
		_headColliderExtrusion.Init(LayerMaskClass.PlayerCollisionTestMask);
		_headColliderExtrusion.AddIgnoredCollider(playerCollider);
		_cameraColliderExtrusion.Init(LayerMaskClass.PlayerCollisionTestMask);
		_cameraColliderExtrusion.AddIgnoredCollider(playerCollider);
		Off();
	}

	public void IgnoreCollider(Collider collider, bool ignore = true)
	{
		if (ignore)
		{
			_headColliderExtrusion.AddIgnoredCollider(collider);
		}
		else
		{
			_headColliderExtrusion.RemoveIgnoredCollider(collider);
		}
		if (ignore)
		{
			_cameraColliderExtrusion.AddIgnoredCollider(collider);
		}
		else
		{
			_cameraColliderExtrusion.RemoveIgnoredCollider(collider);
		}
	}

	public bool IsHeadCollider(Collider colliderForCheck)
	{
		int num = 0;
		while (true)
		{
			if (num < _headColliders.Length)
			{
				if (colliderForCheck == _headColliders[num])
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

	public void SetCollider(EExtrusionCollider colliderIndex)
	{
		_headColliderExtrusion.SetCollider(_headColliders[(int)colliderIndex]);
		eextrusionCollider_0 = colliderIndex;
	}

	public void On()
	{
		Collider[] headColliders = _headColliders;
		for (int i = 0; i < headColliders.Length; i++)
		{
			headColliders[i].enabled = true;
		}
		_cameraColliderExtrusion.Collider.enabled = true;
		TransformHelperClass.SetLayersRecursively(_cameraColliderExtrusion.gameObject, LayerMaskClass.PlayerCollisionTestLayer);
		headColliders = _headColliders;
		for (int i = 0; i < headColliders.Length; i++)
		{
			TransformHelperClass.SetLayersRecursively(headColliders[i].gameObject, LayerMaskClass.PlayerCollisionTestLayer);
		}
		_headColliderExtrusion.gameObject.SetActive(value: true);
	}

	public Vector3 GetOffsetXZ()
	{
		return new Vector3(vector3_0.x, 0f, vector3_0.z);
	}

	public Vector3 GetOffset()
	{
		return vector3_0;
	}

	public void Off()
	{
		Collider[] headColliders = _headColliders;
		for (int i = 0; i < headColliders.Length; i++)
		{
			headColliders[i].enabled = false;
		}
		_cameraColliderExtrusion.Collider.enabled = false;
	}

	public void Calculate(Vector3 motion)
	{
		if (eextrusionCollider_0 == EExtrusionCollider.Prone)
		{
			method_0();
		}
		_headColliderExtrusion.CalculateThroughMotion(motion);
		vector3_0 = _headColliderExtrusion.GetDepenetration();
	}

	public void method_0()
	{
		Vector3 localPosition = _headColliderExtrusion.Collider.transform.parent.InverseTransformPoint(_proneColliderPivot.position);
		localPosition.y = Mathf.Min(localPosition.y, 0.2f);
		_headColliderExtrusion.Collider.transform.localPosition = localPosition;
	}

	public void Calculate(Vector3 motion, Quaternion rotation)
	{
		if (eextrusionCollider_0 == EExtrusionCollider.Prone)
		{
			method_0();
		}
		Transform transform = Collider.transform;
		_headColliderExtrusion.Calculate(transform.position + motion, rotation);
		vector3_0 = _headColliderExtrusion.GetDepenetration();
	}

	public void ExtrudeCamera()
	{
		_cameraColliderExtrusion.Calculate();
		Vector3 depenetration = _cameraColliderExtrusion.GetDepenetration();
		_cameraColliderExtrusion.transform.position += depenetration;
	}
}
