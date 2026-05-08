using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class GUILabelDebugger : MonoBehaviour
{
	public class Class330
	{
		[NonSerialized]
		public string String_0;

		[NonSerialized]
		public string String_1;

		[NonSerialized]
		public GUIStyle Guistyle_0;

		[NonSerialized]
		public Color Color_0;

		[NonSerialized]
		public string String_2;

		public Class330(string name, string text, Color color, bool withWhiteBack)
		{
			String_1 = name;
			String_2 = text;
			Color_0 = color;
			if (withWhiteBack)
			{
				Guistyle_0 = new GUIStyle
				{
					normal = 
					{
						background = Texture2D.whiteTexture,
						textColor = Color_0
					},
					fixedHeight = 22f
				};
			}
			else
			{
				Guistyle_0 = GUIStyle.none;
			}
			String_0 = "{0} {1}";
		}

		public void Show()
		{
			GUI.color = Color_0;
			GUILayout.BeginHorizontal(Guistyle_0);
			GUILayout.Label(string.Format(String_0, String_1, String_2));
			GUILayout.EndHorizontal();
		}

		public void Set(object msg, Color color)
		{
			Color_0 = color;
			String_2 = msg.ToString();
		}
	}

	private static GUILabelDebugger guilabelDebugger_0;

	private readonly List<Class330> list_0 = new List<Class330>(30);

	private readonly Dictionary<string, Class330> dictionary_0 = new Dictionary<string, Class330>();

	public static GUILabelDebugger GUILabelDebugger_0
	{
		get
		{
			if (guilabelDebugger_0 == null)
			{
				guilabelDebugger_0 = GClass870.FindUnityObjectOfType<GUILabelDebugger>();
				if (guilabelDebugger_0 == null)
				{
					guilabelDebugger_0 = new GameObject(typeof(GUILabelDebugger).ToString()).AddComponent<GUILabelDebugger>();
					UnityEngine.Object.DontDestroyOnLoad(guilabelDebugger_0);
				}
			}
			return guilabelDebugger_0;
		}
	}

	[Conditional("DEBUG")]
	public static void ShowLabel(string labelName, object msg)
	{
	}

	[Conditional("DEBUG")]
	public static void ShowLabelContrasted(string labelName, object msg)
	{
	}

	[Conditional("DEBUG")]
	public static void ShowLabel(string labelName, object msg, Color color, bool withWhiteBack = false)
	{
		Dictionary<string, Class330> dictionary = GUILabelDebugger_0.dictionary_0;
		List<Class330> list = GUILabelDebugger_0.list_0;
		if (dictionary.TryGetValue(labelName, out var value))
		{
			if (msg != null)
			{
				value.Set(msg, color);
				return;
			}
			list.Remove(value);
			dictionary.Remove(labelName);
		}
		else if (msg != null)
		{
			Class330 item = (dictionary[labelName] = new Class330(labelName, msg.ToString(), color, withWhiteBack));
			list.Add(item);
		}
	}

	public void Start()
	{
		if (guilabelDebugger_0 == null)
		{
			guilabelDebugger_0 = this;
			UnityEngine.Object.DontDestroyOnLoad(this);
		}
		else if (this != guilabelDebugger_0)
		{
			UnityEngine.Object.DestroyImmediate(this);
		}
	}

	public void OnGUI()
	{
		for (int i = 0; i < list_0.Count; i++)
		{
			list_0[i].Show();
		}
	}
}
