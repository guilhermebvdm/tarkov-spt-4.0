using UnityEngine;

[DisallowMultipleComponent]
public abstract class JointSpawner : MonoBehaviour
{
	public RigidbodySpawner connectedBody;

	public Vector3 anchor;

	public Vector3 axis;

	public bool autoConfigureConnectedAnchor;

	public Vector3 connectedAnchor;

	public bool enableCollision;

	public bool enablePreprocessing;

	protected Joint _joint;

	public virtual Joint Create()
	{
		if (_joint == null)
		{
			GetComponent<RigidbodySpawner>().Create();
			_joint = CreateComponent();
			if (connectedBody != null)
			{
				Rigidbody rigidbody = connectedBody.Create();
				_joint.connectedBody = rigidbody;
			}
			_joint.anchor = anchor;
			_joint.axis = axis;
			_joint.autoConfigureConnectedAnchor = autoConfigureConnectedAnchor;
			_joint.connectedAnchor = connectedAnchor;
			_joint.enableCollision = enableCollision;
			_joint.enablePreprocessing = enablePreprocessing;
		}
		return _joint;
	}

	public void Remove()
	{
		if (_joint != null)
		{
			Object.Destroy(_joint);
			_joint = null;
			GetComponent<RigidbodySpawner>().Remove();
		}
	}

	public virtual Joint CreateComponent()
	{
		return base.gameObject.AddComponent<Joint>();
	}

	public JointSpawner()
	{
	}
}
