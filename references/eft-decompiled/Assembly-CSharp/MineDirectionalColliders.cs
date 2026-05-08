using UnityEngine;

public class MineDirectionalColliders : MonoBehaviour, IPhysicsTrigger
{
	private MineDirectional mineDirectional_0;

	public string Description => "MineDirectionalColliders";

	bool IPhysicsTrigger.enabled
	{
		get
		{
			return base.enabled;
		}
		set
		{
			base.enabled = value;
		}
	}

	public void Awake()
	{
		mineDirectional_0 = GetComponentInParent<MineDirectional>();
		if (!(mineDirectional_0 != null))
		{
			Debug.LogError(base.gameObject.name + " mine parent not found");
			Object.Destroy(this);
		}
	}

	public void OnTriggerEnter(Collider collider)
	{
		mineDirectional_0.OnTriggerEnter(collider);
	}

	public void OnTriggerExit(Collider collider)
	{
		mineDirectional_0.OnTriggerExit(collider);
	}
}
