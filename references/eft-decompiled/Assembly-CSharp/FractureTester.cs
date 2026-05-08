using UnityEngine;

public class FractureTester : MonoBehaviour
{
	public void Update()
	{
		if (Input.GetMouseButtonDown(0) && Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out var hitInfo, 1000f, -1))
		{
			Vector3[] array = new Vector3[75];
			FracturePointsPlacer.PlacePoints(hitInfo.transform, hitInfo.point, array);
			for (int i = 0; i < array.Length; i++)
			{
				Debug.DrawRay(array[i], hitInfo.normal * 0.2f * ((float)i / 75f), (i < 50) ? Color.red : Color.green, 25f);
			}
		}
	}
}
