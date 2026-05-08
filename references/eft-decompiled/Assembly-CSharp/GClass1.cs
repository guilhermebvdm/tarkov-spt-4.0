using UnityEngine;

public class GClass1
{
	public bool success;

	public RaycastHit hit;

	public GClass1(bool success, RaycastHit hit)
	{
		this.success = success;
		this.hit = hit;
	}
}
