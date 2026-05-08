using UnityEngine;

public class GClass0
{
	public Vector3 position;

	public Vector3 upVector;

	public Vector3 dirVector;

	public Quaternion rotation;

	public GClass0()
	{
		UpdatePoint(Vector3.zero, Vector3.zero, Vector3.zero);
	}

	public GClass0(Vector3 incomingPos, Vector3 incomingUpVector, Vector3 incomingDirVector)
	{
		UpdatePoint(incomingPos, incomingUpVector, incomingDirVector);
	}

	public void UpdatePoint(Vector3 incomingPos, Vector3 incomingUpVector, Vector3 incomingDirVector)
	{
		position = incomingPos;
		upVector = incomingUpVector;
		dirVector = incomingDirVector;
	}
}
