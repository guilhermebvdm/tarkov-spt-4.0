using System;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

namespace ChartAndGraph;

[Serializable]
public class ChartDivisionInfo : IInternalSettings
{
	public enum DivisionMessure
	{
		TotalDivisions,
		DataUnits
	}

	[SerializeField]
	[GAttribute21]
	[GAttribute22]
	[Tooltip("messure used to create divisions")]
	public DivisionMessure messure;

	[SerializeField]
	[GAttribute21]
	[GAttribute22]
	[Tooltip("data units per division")]
	public float unitsPerDivision;

	[SerializeField]
	[GAttribute21]
	[GAttribute22]
	[Tooltip("total division lines")]
	public int total = 3;

	[SerializeField]
	[GAttribute21]
	[GAttribute22]
	[Tooltip("Material for the line of the division")]
	public Material material;

	[SerializeField]
	[GAttribute21]
	[GAttribute22]
	[Tooltip("Material tiling for the division lines. Use this to strech or tile the material along the line")]
	public MaterialTiling materialTiling = new MaterialTiling(enable: false, 100f);

	[SerializeField]
	[GAttribute23]
	[GAttribute22]
	[Tooltip("The length of the far side of the division lines. This is used only by 3d chart when MarkDepth >0")]
	public AutoFloat markBackLength = new AutoFloat(automatic: true, 0.5f);

	[SerializeField]
	[GAttribute23]
	[GAttribute21]
	[GAttribute22]
	[Tooltip("The length of the the division lines.")]
	public AutoFloat markLength = new AutoFloat(automatic: true, 0.5f);

	[SerializeField]
	[GAttribute23]
	[GAttribute22]
	public AutoFloat markDepth = new AutoFloat(automatic: true, 0.5f);

	[SerializeField]
	[GAttribute23]
	[GAttribute21]
	[GAttribute22]
	[Tooltip("the thickness of the division lines")]
	public float markThickness = 0.1f;

	[SerializeField]
	[GAttribute23]
	[GAttribute21]
	[GAttribute22]
	[Tooltip("A prefab for the division labels")]
	public Text textPrefab;

	[SerializeField]
	[GAttribute23]
	[GAttribute21]
	[GAttribute22]
	[Tooltip("prefix for the axis labels")]
	public string textPrefix;

	[SerializeField]
	[GAttribute23]
	[GAttribute21]
	[GAttribute22]
	[Tooltip("suffix for the axis labels")]
	public string textSuffix;

	[Range(0f, 7f)]
	[GAttribute23]
	[GAttribute21]
	[GAttribute22]
	[SerializeField]
	[Tooltip("the number of fraction digits in text labels")]
	public int fractionDigits = 2;

	[GAttribute23]
	[GAttribute21]
	[GAttribute22]
	[SerializeField]
	[Tooltip("Label font size")]
	public int fontSize = 12;

	[Range(1f, 3f)]
	[GAttribute23]
	[GAttribute21]
	[GAttribute22]
	[Tooltip("makes the labels sharper if they are blurry")]
	[SerializeField]
	public float fontSharpness = 1f;

	[GAttribute23]
	[GAttribute22]
	[SerializeField]
	[Tooltip("depth seperation the division lables")]
	public float textDepth;

	[GAttribute23]
	[GAttribute21]
	[GAttribute22]
	[SerializeField]
	[Tooltip("breadth seperation for the division labels")]
	public float textSeperation;

	[GAttribute23]
	[GAttribute21]
	[GAttribute22]
	[SerializeField]
	[Tooltip("Alignment of the division lables")]
	public ChartDivisionAligment alignment = ChartDivisionAligment.Standard;

	[CompilerGenerated]
	private EventHandler OnDataUpdate;

	[CompilerGenerated]
	private EventHandler OnDataChanged;

	public int Total
	{
		get
		{
			return total;
		}
		set
		{
			RaiseOnChanged();
		}
	}

	public Material Material
	{
		get
		{
			return material;
		}
		set
		{
			material = value;
			RaiseOnChanged();
		}
	}

	public MaterialTiling MaterialTiling
	{
		get
		{
			return materialTiling;
		}
		set
		{
			materialTiling = value;
			RaiseOnChanged();
		}
	}

	public AutoFloat MarkBackLength
	{
		get
		{
			return markBackLength;
		}
		set
		{
			markBackLength = value;
			RaiseOnChanged();
		}
	}

	public AutoFloat MarkLength
	{
		get
		{
			return markLength;
		}
		set
		{
			markLength = value;
			RaiseOnChanged();
		}
	}

	public AutoFloat MarkDepth
	{
		get
		{
			return markDepth;
		}
		set
		{
			markDepth = value;
			RaiseOnChanged();
		}
	}

	public float MarkThickness
	{
		get
		{
			return markThickness;
		}
		set
		{
			markThickness = value;
			RaiseOnChanged();
		}
	}

	public Text TextPrefab
	{
		get
		{
			return textPrefab;
		}
		set
		{
			textPrefab = value;
			RaiseOnChanged();
		}
	}

	public string TextPrefix
	{
		get
		{
			return textPrefix;
		}
		set
		{
			textPrefix = value;
			RaiseOnChanged();
		}
	}

	public string TextSuffix
	{
		get
		{
			return textSuffix;
		}
		set
		{
			textSuffix = value;
			RaiseOnChanged();
		}
	}

	public int FractionDigits
	{
		get
		{
			return fractionDigits;
		}
		set
		{
			fractionDigits = value;
			RaiseOnChanged();
		}
	}

	public int FontSize
	{
		get
		{
			return fontSize;
		}
		set
		{
			fontSize = value;
			RaiseOnChanged();
		}
	}

	public float FontSharpness
	{
		get
		{
			return fontSharpness;
		}
		set
		{
			fontSharpness = value;
			RaiseOnChanged();
		}
	}

	public float TextDepth
	{
		get
		{
			return textDepth;
		}
		set
		{
			textDepth = value;
			RaiseOnChanged();
		}
	}

	public float TextSeperation
	{
		get
		{
			return textSeperation;
		}
		set
		{
			textSeperation = value;
			RaiseOnChanged();
		}
	}

	public ChartDivisionAligment Alignment
	{
		get
		{
			return alignment;
		}
		set
		{
			alignment = value;
			RaiseOnChanged();
		}
	}

	public event EventHandler Event_0
	{
		[CompilerGenerated]
		add
		{
			EventHandler eventHandler = OnDataUpdate;
			EventHandler eventHandler2;
			do
			{
				eventHandler2 = eventHandler;
				EventHandler value2 = (EventHandler)Delegate.Combine(eventHandler2, value);
				eventHandler = Interlocked.CompareExchange(ref OnDataUpdate, value2, eventHandler2);
			}
			while ((object)eventHandler != eventHandler2);
		}
		[CompilerGenerated]
		remove
		{
			EventHandler eventHandler = OnDataUpdate;
			EventHandler eventHandler2;
			do
			{
				eventHandler2 = eventHandler;
				EventHandler value2 = (EventHandler)Delegate.Remove(eventHandler2, value);
				eventHandler = Interlocked.CompareExchange(ref OnDataUpdate, value2, eventHandler2);
			}
			while ((object)eventHandler != eventHandler2);
		}
	}

	public event EventHandler Event_1
	{
		[CompilerGenerated]
		add
		{
			EventHandler eventHandler = OnDataChanged;
			EventHandler eventHandler2;
			do
			{
				eventHandler2 = eventHandler;
				EventHandler value2 = (EventHandler)Delegate.Combine(eventHandler2, value);
				eventHandler = Interlocked.CompareExchange(ref OnDataChanged, value2, eventHandler2);
			}
			while ((object)eventHandler != eventHandler2);
		}
		[CompilerGenerated]
		remove
		{
			EventHandler eventHandler = OnDataChanged;
			EventHandler eventHandler2;
			do
			{
				eventHandler2 = eventHandler;
				EventHandler value2 = (EventHandler)Delegate.Remove(eventHandler2, value);
				eventHandler = Interlocked.CompareExchange(ref OnDataChanged, value2, eventHandler2);
			}
			while ((object)eventHandler != eventHandler2);
		}
	}

	public event EventHandler ChartAndGraph_002EIInternalSettings_002EInternalOnDataUpdate
	{
		add
		{
			Event_0 += value;
		}
		remove
		{
			Event_0 -= value;
		}
	}

	public event EventHandler ChartAndGraph_002EIInternalSettings_002EInternalOnDataChanged
	{
		add
		{
			Event_1 += value;
		}
		remove
		{
			Event_1 -= value;
		}
	}

	public void ValidateProperites()
	{
		if (total < 0)
		{
			total = 0;
		}
		fontSharpness = Mathf.Clamp(fontSharpness, 1f, 3f);
		fontSize = Mathf.Max(fontSize, 0);
		fractionDigits = Mathf.Clamp(fractionDigits, 0, 7);
		markThickness = Mathf.Max(markThickness, 0f);
		materialTiling.TileFactor = Mathf.Max(materialTiling.TileFactor, 0f);
		textPrefix = ((textPrefix == null) ? "" : textPrefix);
		textSuffix = ((textSuffix == null) ? "" : textSuffix);
	}

	public virtual float ValidateTotal(float total)
	{
		return total;
	}

	public virtual void RaiseOnChanged()
	{
		if (OnDataChanged != null)
		{
			OnDataChanged(this, EventArgs.Empty);
		}
	}
}
