using UnityEngine;

public class FloatingObject : UpdateInEditorSystemComponent<FloatingObject>
{
	[SerializeField]
	private float _angle = 10f;

	[SerializeField]
	private Vector3 _buoyancy = new Vector3(0.1f, 0.1f, 0.1f);

	[SerializeField]
	private float _angleAmplitude = 1f;

	[SerializeField]
	private float _buoyancyAmplitude = 1f;

	private Vector3 vector3_0;

	private Quaternion quaternion_0;

	private float float_0;

	private float float_1;

	private float float_2;

	public void Start()
	{
		float_2 = Random.Range(0.1f, 10f);
		vector3_0 = base.gameObject.transform.position;
		quaternion_0 = base.gameObject.transform.rotation;
	}

	public override void ManualUpdate(float dt)
	{
		float num = float_2 + Time.time;
		float_0 = num * _angleAmplitude;
		float_1 = num * _buoyancyAmplitude;
		method_0(float_1, float_0);
	}

	public void method_0(float positionFloatingTime, float rotationFloatingTime)
	{
		float num = Mathf.Sin(positionFloatingTime);
		float num2 = Mathf.Cos(positionFloatingTime);
		float x = vector3_0.x + num * num2 * _buoyancy.x;
		float y = vector3_0.y + num * _buoyancy.y;
		float z = vector3_0.z + num2 * _buoyancy.z;
		float num3 = Mathf.Sin(rotationFloatingTime);
		float num4 = Mathf.Cos(rotationFloatingTime);
		float x2 = num3 * num4 * _angle;
		float z2 = num4 * _angle;
		base.transform.SetPositionAndRotation(new Vector3(x, y, z), Quaternion.Euler(x2, 0f, z2) * quaternion_0);
	}
}
