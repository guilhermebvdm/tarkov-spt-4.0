using System;
using System.Diagnostics;
using System.Threading;
using Comfort.Common;
using UnityEngine;

namespace UnityDiagnostics;

public class CPULagSimulator : MonoBehaviour
{
	public enum ELoadType
	{
		Calculation,
		ThreadSleep
	}

	[Serializable]
	public class LagSimulator
	{
		public const int MIN_LAG = 0;

		public const int MAX_LAG = 5000;

		[NonSerialized]
		public Stopwatch Stopwatch = new Stopwatch();

		public bool Enabled;

		public GClass1361 TimeMesurer;

		public KeyCode ToggleGameKey;

		[Range(0f, 5000f)]
		public int FramesBetweenSpikes;

		[Range(0f, 5000f)]
		public int LagDispersion;

		[Range(0f, 5000f)]
		public int ConstantLagMS;

		public ELoadType LoadType;

		[NonSerialized]
		public int LastLagFrame;

		[field: NonSerialized]
		public int LastLag { get; set; }

		public void MakeLag()
		{
			if (method_1())
			{
				int num = UnityEngine.Random.Range(-LagDispersion, LagDispersion);
				LastLag = Mathf.Clamp(ConstantLagMS + num, 0, 5000);
				LastLag = Math.Max(0, LastLag - (int)TimeMesurer.CurrentMilliseconds);
				if (LoadType == ELoadType.Calculation)
				{
					method_0(LastLag);
				}
				else
				{
					Thread.Sleep(LastLag);
				}
				LastLagFrame = Time.frameCount;
			}
		}

		public void method_0(long ms)
		{
			Quaternion rotation = UnityEngine.Random.rotationUniform;
			Stopwatch.Restart();
			while (Stopwatch.ElapsedMilliseconds < ms)
			{
				rotation = Quaternion.Inverse(rotation);
			}
			Stopwatch.Stop();
		}

		public bool method_1()
		{
			if (!Enabled)
			{
				return false;
			}
			return Time.frameCount > LastLagFrame + FramesBetweenSpikes;
		}

		public void CheckToggleInput()
		{
			if (Input.GetKeyUp(ToggleGameKey))
			{
				Enabled = !Enabled;
			}
		}
	}

	private GUIStyle guistyle_0;

	public LagSimulator FixedUpdateLagSimulator = new LagSimulator
	{
		ToggleGameKey = KeyCode.Keypad2
	};

	public LagSimulator UpdateLagSimulator = new LagSimulator
	{
		ToggleGameKey = KeyCode.Keypad1
	};

	public void Awake()
	{
		guistyle_0 = new GUIStyle
		{
			fontStyle = FontStyle.Bold,
			fontSize = 14,
			normal = new GUIStyleState
			{
				textColor = Color.yellow
			}
		};
	}

	public void Start()
	{
		if (!Singleton<GClass1357>.Instantiated)
		{
			BackendConfigAbstractClass.LoadApplicationConfig(new ApplicationConfigClass());
			Singleton<GClass1357>.Create(GClass1357.Create(LoggerMode.Add));
		}
		FixedUpdateLagSimulator.TimeMesurer = Singleton<GClass1357>.Instance.SingleFixedUpdatesMeasurer;
		UpdateLagSimulator.TimeMesurer = Singleton<GClass1357>.Instance.UpdateMeasurer;
	}

	public void Update()
	{
		UpdateLagSimulator.CheckToggleInput();
		FixedUpdateLagSimulator.CheckToggleInput();
		UpdateLagSimulator.MakeLag();
	}

	public void FixedUpdate()
	{
		FixedUpdateLagSimulator.MakeLag();
	}

	public void OnGUI()
	{
		Rect rect = new Rect(Screen.width - 150, 300f, 140f, 32f);
		Rect position = rect;
		position.height += 4f;
		Color color = GUI.color;
		GUI.color = new Color(0.5f, 0.5f, 0.5f, 0.7f);
		GUI.Box(position, "");
		GUI.color = Color.white;
		int num = 16;
		rect.yMin += 2f;
		rect.xMin += 2f;
		method_1($"Update lag: {(UpdateLagSimulator.Enabled ? UpdateLagSimulator.LastLag.ToString() : UpdateLagSimulator.ToggleGameKey.ToString())}", Color.white, rect);
		rect.yMin += num;
		rect.yMax += num;
		method_1($"FixedUpd lag: {(FixedUpdateLagSimulator.Enabled ? FixedUpdateLagSimulator.LastLag.ToString() : FixedUpdateLagSimulator.ToggleGameKey.ToString())}", Color.white, rect);
		rect.yMin += num;
		rect.yMax += num;
		GUI.color = color;
	}

	public bool method_0()
	{
		if (!UpdateLagSimulator.Enabled)
		{
			return FixedUpdateLagSimulator.Enabled;
		}
		return true;
	}

	public void method_1(string info, Color color, Rect rect)
	{
		guistyle_0.normal.textColor = color;
		GUI.Label(rect, info, guistyle_0);
	}

	public static CPULagSimulator Add()
	{
		return GClass852.Add<CPULagSimulator>(dontDestroyOnLoad: true);
	}

	public static void Remove()
	{
		GClass852.Remove<CPULagSimulator>();
	}
}
