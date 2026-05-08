using UnityEngine;

public class DragTarget : MonoBehaviour
{
	private Transform transform_0;

	private Camera camera_0;

	private Vector3 vector3_0;

	public void Start()
	{
		transform_0 = base.transform;
		camera_0 = Camera.main;
	}

	public void OnMouseDown()
	{
		Vector2 vector = Input.mousePosition;
		float z = camera_0.WorldToScreenPoint(transform_0.position).z;
		Vector3 vector2 = camera_0.ScreenToWorldPoint(new Vector3(vector.x, vector.y, z));
		vector3_0 = transform_0.position - vector2;
	}

	public void OnMouseDrag()
	{
		Vector2 vector = Input.mousePosition;
		float z = camera_0.WorldToScreenPoint(transform_0.position).z;
		transform_0.position = camera_0.ScreenToWorldPoint(new Vector3(vector.x, vector.y, z)) + vector3_0;
	}
}
