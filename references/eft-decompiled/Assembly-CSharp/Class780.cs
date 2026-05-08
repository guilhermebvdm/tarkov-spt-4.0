using System.Collections.Generic;
using UnityEngine;

public abstract class Class780
{
	public static GUIStyle CreateListenerPlayerStyle()
	{
		return GClass1158.CreateStyle(Color.gray, Color.green, GUI.skin.box, TextAnchor.MiddleCenter, FontStyle.Bold);
	}

	public static GUIStyle CreateFastSourcesStyle(Color backgroundColor)
	{
		return GClass1158.CreateStyle(backgroundColor, Color.white, GUI.skin.box, TextAnchor.MiddleLeft, FontStyle.Bold);
	}

	public static GUIStyle CreatePropagationToggleStyle()
	{
		return GClass1158.CreateStyle(Color.gray, Color.white, GUI.skin.toggle, TextAnchor.MiddleLeft);
	}

	public static List<GUIStyle> CreateRandomSourceStyles(int count)
	{
		List<GUIStyle> list = new List<GUIStyle>(count);
		GUIStyle box = GUI.skin.box;
		for (int i = 0; i < count; i++)
		{
			GUIStyle item = GClass1158.CreateStyle(GClass1158.GetRandomColor(), Color.white, box, TextAnchor.MiddleLeft, FontStyle.Bold);
			list.Add(item);
		}
		return list;
	}
}
