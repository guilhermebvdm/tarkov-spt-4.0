using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

[Serializable]
public class BotZoneDebugPointsGroup
{
	[CompilerGenerated]
	public class Class125
	{
		public string label;

		public bool method_0(BotZoneDebugPoints p)
		{
			return p.Label == label;
		}
	}

	[SerializeField]
	public string _label;

	public bool IsDraw;

	public Color Color = Color.magenta;

	public Vector3 Offset = Vector3.zero;

	public float Radius = 0.1f;

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
	public List<BotZoneDebugPoints> _points = new List<BotZoneDebugPoints>();

	public int ActivePointsIndex;

	public string Label => _label;

	public List<BotZoneDebugPoints> Points => _points;

	public BotZoneDebugPoints List
	{
		get
		{
			if (ActivePointsIndex >= _points.Count)
			{
				return null;
			}
			return Points[ActivePointsIndex];
		}
	}

	public BotZoneDebugPointsGroup(string label)
	{
		_label = label;
		Color = NextColor();
	}

	public string[] GetElementsList()
	{
		List<string> list = new List<string>();
		foreach (BotZoneDebugPoints point in _points)
		{
			list.Add(point.Label + "(" + point.Count + " points)");
		}
		return list.ToArray();
	}

	public Color NextColor()
	{
		Color result = Colors[ColorIndex % Colors.Count];
		ColorIndex++;
		return result;
	}

	public void Reset()
	{
		ColorIndex = 0;
	}

	public void Add(string label, Vector3 point)
	{
		BotZoneDebugPoints botZoneDebugPoints = _points.FirstOrDefault((BotZoneDebugPoints p) => p.Label == label);
		if (botZoneDebugPoints == null)
		{
			botZoneDebugPoints = new BotZoneDebugPoints(label);
			_points.Add(botZoneDebugPoints);
		}
		botZoneDebugPoints.AddPoint(point);
	}

	public virtual void Draw()
	{
		if (_points != null && _points.Count > ActivePointsIndex && ActivePointsIndex >= 0)
		{
			_points[ActivePointsIndex].Draw(IsDraw, Color, Offset, Radius);
		}
	}
}
