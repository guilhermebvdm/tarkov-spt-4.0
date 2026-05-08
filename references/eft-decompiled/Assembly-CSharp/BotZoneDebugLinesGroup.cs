using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

[Serializable]
public class BotZoneDebugLinesGroup
{
	[CompilerGenerated]
	public class Class124
	{
		public string label;

		public bool method_0(BotZoneDebugLines p)
		{
			return p.Label == label;
		}
	}

	[SerializeField]
	public string _label;

	public bool IsDraw;

	public Color Color = Color.magenta;

	public Vector3 Offset = Vector3.zero;

	[NonSerialized]
	public List<Color> Colors = new List<Color>
	{
		Color.white,
		Color.red,
		Color.green,
		Color.blue,
		Color.cyan,
		Color.yellow,
		Color.magenta,
		Color.gray,
		Color.black
	};

	[NonSerialized]
	public static int ColorIndex;

	[SerializeField]
	public List<BotZoneDebugLines> _lines = new List<BotZoneDebugLines>();

	public int ActiveLinesIndex;

	public string Label => _label;

	public List<BotZoneDebugLines> Lines => _lines;

	public BotZoneDebugLines List
	{
		get
		{
			if (ActiveLinesIndex >= Lines.Count)
			{
				return null;
			}
			return Lines[ActiveLinesIndex];
		}
	}

	public BotZoneDebugLinesGroup(string label)
	{
		_label = label;
		Color = method_0();
	}

	public string[] GetElementsList()
	{
		List<string> list = new List<string>();
		foreach (BotZoneDebugLines line in _lines)
		{
			list.Add(line.Label + " (" + line.Count + " points)");
		}
		return list.ToArray();
	}

	public Color method_0()
	{
		Color result = Colors[ColorIndex % Colors.Count];
		ColorIndex++;
		return result;
	}

	public void Reset()
	{
		ColorIndex = 0;
	}

	public void AddLine(string label, Vector3 pointA, Vector3 pointB)
	{
		BotZoneDebugLines botZoneDebugLines = _lines.FirstOrDefault((BotZoneDebugLines p) => p.Label == label);
		if (botZoneDebugLines == null)
		{
			botZoneDebugLines = new BotZoneDebugLines(label);
			_lines.Add(botZoneDebugLines);
		}
		botZoneDebugLines.AddLine(pointA, pointB);
	}

	public void Draw()
	{
		if (_lines != null && _lines.Count > ActiveLinesIndex && ActiveLinesIndex >= 0)
		{
			_lines[ActiveLinesIndex].Draw(IsDraw, Color, Offset);
		}
	}
}
