using UnityEngine;

public class Class591
{
	public static string GetComponentPath(Component component)
	{
		string text = null;
		text = " .. " + component.name;
		return GetGameObjectPath(component.gameObject) + text;
	}

	public static string GetGameObjectPath(GameObject gameObject)
	{
		string text = "";
		Transform transform = gameObject.transform;
		while (transform != null)
		{
			text = transform.gameObject.name + "." + text;
			transform = transform.parent;
		}
		return text;
	}
}
