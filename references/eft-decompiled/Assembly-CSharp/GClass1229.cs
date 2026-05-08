using UnityEngine;

public abstract class GClass1229
{
	public static bool HasNullMesh(this Renderer rend)
	{
		if (rend is MeshRenderer && (rend as MeshRenderer).GetComponent<MeshFilter>().sharedMesh == null)
		{
			return true;
		}
		return false;
	}
}
