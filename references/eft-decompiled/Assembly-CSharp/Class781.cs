using UnityEngine;

public class Class781
{
	public readonly GUIStyle Style;

	public readonly SourceContainerClass Container;

	public bool Show = true;

	public bool DrawPropagationPath;

	public string Info;

	public Class781(SourceContainerClass container, GUIStyle style, bool drawPropagationPath = false, string info = "")
	{
		Container = container;
		Style = style;
		DrawPropagationPath = drawPropagationPath;
		Info = info;
	}
}
