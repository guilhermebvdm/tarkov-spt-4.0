using UnityEngine;

public class CompassArrow : MonoBehaviour
{
	[SerializeField]
	private BetterSpring _spring = new BetterSpring();

	public Vector3 NorthDirection = Vector3.forward;

	private Transform transform_0;

	private float float_0;

	private float float_1;

	public void Start()
	{
		transform_0 = base.transform;
		_spring.Cache();
		_spring.EquilibriumPos = Vector3.zero;
		_spring.ApplyPosition(_spring.EquilibriumPos);
	}

	public void Update()
	{
		Quaternion quaternion = Quaternion.LookRotation(transform_0.parent.forward, -NorthDirection);
		Quaternion quaternion2 = Quaternion.Inverse(base.transform.parent.rotation) * quaternion;
		Vector3 eulerAngles = quaternion2.eulerAngles;
		float_1 = quaternion2.eulerAngles.z;
		float num = float_0 - quaternion2.eulerAngles.z;
		num = ((num > 180f) ? (num - 360f) : ((num < -180f) ? (num + 360f) : num));
		_spring.ApplyPosition(new Vector3(0f, 0f, num));
		_spring.Process(Time.deltaTime);
		float_0 = (eulerAngles + _spring.Value).z;
		base.transform.localRotation = Quaternion.Euler(eulerAngles + _spring.Value);
	}

	public int PanelValue()
	{
		int num = 180 - (int)float_1;
		if (num >= 0)
		{
			return num;
		}
		return num + 360;
	}
}
