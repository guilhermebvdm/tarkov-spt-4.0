using UnityEngine;

public abstract class GClass941
{
	public static string GetFullPath(this Component component)
	{
		if (component == null)
		{
			return null;
		}
		string text = $" ({component.gameObject.GetInstanceID()})";
		Transform transform = component.transform;
		while (transform != null)
		{
			text = transform.gameObject.name + "." + text;
			transform = transform.parent;
		}
		return text;
	}
}
