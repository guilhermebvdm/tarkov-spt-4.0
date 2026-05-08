using System;
using UnityEngine;

public class UBHelper : MonoBehaviour
{
	public class Class501
	{
		public GUIStyle header = "ShurikenModuleTitle";

		public GUIStyle headerArrow = "AC RightArrow";

		[NonSerialized]
		public RectOffset RectOffset_0;

		[NonSerialized]
		public float Float_0;

		[NonSerialized]
		public Vector2 Vector2_0;

		[NonSerialized]
		public TextAnchor TextAnchor_0;

		[NonSerialized]
		public FontStyle FontStyle_0;

		[NonSerialized]
		public int Int_0;

		public Class501()
		{
			header.font = new GUIStyle("Label").font;
		}

		public void Backup()
		{
			RectOffset_0 = class501_0.header.border;
			Float_0 = class501_0.header.fixedHeight;
			Vector2_0 = class501_0.header.contentOffset;
			TextAnchor_0 = class501_0.header.alignment;
			FontStyle_0 = class501_0.header.fontStyle;
			Int_0 = class501_0.header.fontSize;
		}

		public void Apply()
		{
			class501_0.header.border = new RectOffset(7, 7, 4, 4);
			class501_0.header.fixedHeight = 22f;
			class501_0.header.contentOffset = new Vector2(20f, -2f);
			class501_0.header.alignment = TextAnchor.MiddleLeft;
			class501_0.header.fontStyle = FontStyle.Bold;
			class501_0.header.fontSize = 12;
		}

		public void Revert()
		{
			class501_0.header.border = RectOffset_0;
			class501_0.header.fixedHeight = Float_0;
			class501_0.header.contentOffset = Vector2_0;
			class501_0.header.alignment = TextAnchor_0;
			class501_0.header.fontStyle = FontStyle_0;
			class501_0.header.fontSize = Int_0;
		}
	}

	private static Class501 class501_0;

	static UBHelper()
	{
		class501_0 = new Class501();
	}

	public static bool GroupHeader(string text, bool isExpanded)
	{
		Rect rect = GUILayoutUtility.GetRect(16f, 22f, class501_0.header);
		class501_0.Backup();
		class501_0.Apply();
		if (Event.current.type == EventType.Repaint)
		{
			class501_0.header.Draw(rect, text, isExpanded, isExpanded, isExpanded, isExpanded);
		}
		Event current = Event.current;
		if (current.type == EventType.MouseDown && rect.Contains(current.mousePosition))
		{
			isExpanded = !isExpanded;
			current.Use();
		}
		class501_0.Revert();
		return isExpanded;
	}
}
