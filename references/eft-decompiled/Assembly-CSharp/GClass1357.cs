using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Comfort.Common;
using CustomPlayerLoopSystem;
using EFT;
using NLog;
using UnityEngine;

public class GClass1357 : IDisposable
{
	public class GClass714 : LoggerClass
	{
		public class GClass1358
		{
			public float AvgStatisticsLogInterval;

			public float NetworkQualityLogInterval;

			public float BadFrame;

			public float BadFixedUpdate;

			public float BadFrameWithoutFixedUpdates;
		}

		[NonSerialized]
		public GClass735<double> Gclass735_0;

		[NonSerialized]
		public GClass735<double> Gclass735_1;

		[NonSerialized]
		public GClass735<double> Gclass735_2;

		[NonSerialized]
		public GClass1358 Gclass1358_0;

		[NonSerialized]
		public float Float_0;

		[NonSerialized]
		public float Float_1;

		public GClass714(LoggerMode loggerMode, GClass1358 config)
			: base("fps", loggerMode)
		{
			Gclass1358_0 = config;
			Gclass735_0 = new GClass735<double>(this, "TooHigh", "Frame", Gclass1358_0.BadFrame);
			Gclass735_1 = new GClass735<double>(this, "TooHigh", "FixedUpdate", Gclass1358_0.BadFixedUpdate);
			Gclass735_2 = new GClass735<double>(this, "TooHigh", "FrameWithoutFixedUpdates", Gclass1358_0.BadFrameWithoutFixedUpdates);
		}

		public void TrackFixedUpdate(GClass1357 frameMeasurer)
		{
			Gclass735_1.Check(frameMeasurer.SingleFixedUpdatesMeasurer.MeasureStatistics.LastValue);
		}

		public void TrackUpdate(GClass1357 frameMeasurer)
		{
			Gclass735_0.Check(frameMeasurer.GetCurrentFrameTime());
			Gclass735_2.Check(frameMeasurer.CurrentFrameWithoutFixedUpdates);
		}

		public override void Dispose()
		{
			Gclass735_0.Dispose();
			Gclass735_1.Dispose();
			Gclass735_2.Dispose();
			base.Dispose();
		}

		public void TryToLogAvgStatistics(GClass1357 frameMeasurer)
		{
			if (!(Gclass1358_0.AvgStatisticsLogInterval < 0f) && IsEnabled(LogLevel.Info) && Float_0 + Gclass1358_0.AvgStatisticsLogInterval <= Time.time)
			{
				Float_0 = Time.time;
				LogInfoFields("FrameMeasurer", "AvgStatistics", "Frame", frameMeasurer.GameFrameMeasurer.MeasureStatistics.Average.ToString("0.00"), "Fps", frameMeasurer.UpdateCountMeasurer.MeasureStatistics.Average.ToString("00.0"), "SingeFixedUpdate", frameMeasurer.SingleFixedUpdatesMeasurer.MeasureStatistics.Average.ToString("0.00"), "OverallFixedUpdates", frameMeasurer.FixedUpdatesBetweenUpdateMeasurer.MeasureStatistics.Average.ToString("0.00"), "FixedUpdatesPerFrame", frameMeasurer.FixedUpdateCountMeasurer.MeasureStatistics.Average.ToString("00.0"), "Update", frameMeasurer.UpdateMeasurer.MeasureStatistics.Average.ToString("0.00"));
			}
		}

		public void TryToLogAvgNetworkQuality(GClass1356 networkQuality)
		{
			if (!(Gclass1358_0.NetworkQualityLogInterval < 0f) && IsEnabled(LogLevel.Info) && Float_1 + Gclass1358_0.NetworkQualityLogInterval <= Time.time)
			{
				Float_1 = Time.time;
				LogInfoFields("AvgNetworkQuality", networkQuality);
			}
		}
	}

	public static readonly string FIXED_UPDATE = "FrameMeasurer_FixedUpdate";

	public static readonly string SINGLE_FIXED_UPDATE = "FrameMeasurer_SingleFixedUpdate";

	public static readonly string UPDATE = "FrameMeasurer_Update";

	public static readonly string GAME_UPDATE = "FrameMeasurer_GameUpdate";

	public static readonly string RENDER = "FrameMeasurer_Render";

	public static readonly string FRAME = "FrameMeasurer_Frame";

	public static readonly string NET_OUTGOINIG = "FrameMeasurer_NetOutgoing";

	public static readonly string PACKETS_OUTGOINIG = "FrameMeasurer_PacketsOutgoing";

	public static readonly string PACKETS_INGOINIG = "FrameMeasurer_PacketsIngoing";

	[CompilerGenerated]
	private Action<double> action_0;

	[NonSerialized]
	public double Double_0;

	[NonSerialized]
	public float Float_0;

	[NonSerialized]
	public float Float_1;

	[NonSerialized]
	public float Float_2;

	[NonSerialized]
	public GInterface170 Ginterface170_0;

	[NonSerialized]
	public GClass714 Gclass714_0;

	[NonSerialized]
	[CompilerGenerated]
	public int Int_0;

	[NonSerialized]
	[CompilerGenerated]
	public int Int_1;

	[NonSerialized]
	[CompilerGenerated]
	public int Int_2;

	[NonSerialized]
	[CompilerGenerated]
	public GClass1361 Gclass1361_0;

	[NonSerialized]
	[CompilerGenerated]
	public GClass1361 Gclass1361_1;

	[NonSerialized]
	[CompilerGenerated]
	public GClass1361 Gclass1361_2;

	[NonSerialized]
	[CompilerGenerated]
	public GClass1361 Gclass1361_3;

	[NonSerialized]
	[CompilerGenerated]
	public GClass1361 Gclass1361_4;

	[NonSerialized]
	[CompilerGenerated]
	public GClass1361 Gclass1361_5;

	[NonSerialized]
	[CompilerGenerated]
	public GClass1362 Gclass1362_0;

	[NonSerialized]
	[CompilerGenerated]
	public GClass1362 Gclass1362_1;

	[NonSerialized]
	[CompilerGenerated]
	public GClass1362 Gclass1362_2;

	[NonSerialized]
	[CompilerGenerated]
	public GClass1359 Gclass1359_0;

	[NonSerialized]
	[CompilerGenerated]
	public GClass1359 Gclass1359_1;

	public readonly GClass1356 NetworkQuality = new GClass1356();

	public int PlayerRTT
	{
		[CompilerGenerated]
		get
		{
			return Int_0;
		}
		[CompilerGenerated]
		set
		{
			Int_0 = value;
		}
	}

	public int ServerFixedUpdateTime
	{
		[CompilerGenerated]
		get
		{
			return Int_1;
		}
		[CompilerGenerated]
		set
		{
			Int_1 = value;
		}
	}

	public int ServerTime
	{
		[CompilerGenerated]
		get
		{
			return Int_2;
		}
		[CompilerGenerated]
		set
		{
			Int_2 = value;
		}
	}

	public GClass1361 FixedUpdatesBetweenUpdateMeasurer
	{
		[CompilerGenerated]
		get
		{
			return Gclass1361_0;
		}
		[CompilerGenerated]
		set
		{
			Gclass1361_0 = value;
		}
	}

	public GClass1361 SingleFixedUpdatesMeasurer
	{
		[CompilerGenerated]
		get
		{
			return Gclass1361_1;
		}
		[CompilerGenerated]
		set
		{
			Gclass1361_1 = value;
		}
	}

	public GClass1361 UpdateMeasurer
	{
		[CompilerGenerated]
		get
		{
			return Gclass1361_2;
		}
		[CompilerGenerated]
		set
		{
			Gclass1361_2 = value;
		}
	}

	public GClass1361 GameUpdateMeasurer
	{
		[CompilerGenerated]
		get
		{
			return Gclass1361_3;
		}
		[CompilerGenerated]
		set
		{
			Gclass1361_3 = value;
		}
	}

	public GClass1361 RenderMeasurer
	{
		[CompilerGenerated]
		get
		{
			return Gclass1361_4;
		}
		[CompilerGenerated]
		set
		{
			Gclass1361_4 = value;
		}
	}

	public GClass1361 GameFrameMeasurer
	{
		[CompilerGenerated]
		get
		{
			return Gclass1361_5;
		}
		[CompilerGenerated]
		set
		{
			Gclass1361_5 = value;
		}
	}

	public GClass1362 NetOutgoingMeasurer
	{
		[CompilerGenerated]
		get
		{
			return Gclass1362_0;
		}
		[CompilerGenerated]
		set
		{
			Gclass1362_0 = value;
		}
	}

	public GClass1362 PacketsOutgoingMeasurer
	{
		[CompilerGenerated]
		get
		{
			return Gclass1362_1;
		}
		[CompilerGenerated]
		set
		{
			Gclass1362_1 = value;
		}
	}

	public GClass1362 PacketsIngoingMeasurer
	{
		[CompilerGenerated]
		get
		{
			return Gclass1362_2;
		}
		[CompilerGenerated]
		set
		{
			Gclass1362_2 = value;
		}
	}

	public GClass1359 UpdateCountMeasurer
	{
		[CompilerGenerated]
		get
		{
			return Gclass1359_0;
		}
		[CompilerGenerated]
		set
		{
			Gclass1359_0 = value;
		}
	}

	public GClass1359 FixedUpdateCountMeasurer
	{
		[CompilerGenerated]
		get
		{
			return Gclass1359_1;
		}
		[CompilerGenerated]
		set
		{
			Gclass1359_1 = value;
		}
	}

	public double CurrentFrameWithoutFixedUpdates => GameFrameMeasurer.MeasureStatistics.LastValue - FixedUpdatesBetweenUpdateMeasurer.MeasureStatistics.LastValue;

	public event Action<double> OnFrameLengthReceived
	{
		[CompilerGenerated]
		add
		{
			Action<double> action = action_0;
			Action<double> action2;
			do
			{
				action2 = action;
				Action<double> value2 = (Action<double>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<double> action = action_0;
			Action<double> action2;
			do
			{
				action2 = action;
				Action<double> value2 = (Action<double>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public static GClass1357 Create(LoggerMode loggerMode)
	{
		GClass1357 gClass = new GClass1357();
		gClass.method_7();
		StartOfFrame.OnUpdate += gClass.method_0;
		EndOfFrame.OnUpdate += gClass.method_1;
		StartOfFixedUpdate.OnUpdate += gClass.method_2;
		EndOfFixedUpdate.OnUpdate += gClass.method_3;
		StartOfUpdate.OnUpdate += gClass.method_4;
		EndOfUpdate.OnUpdate += gClass.method_5;
		StartOfPostLateUpdate.OnUpdate += gClass.method_6;
		gClass.Float_1 = BackendConfigAbstractClass.Config.Log.UpdateMetricsInterval;
		gClass.Gclass714_0 = new GClass714(loggerMode, BackendConfigAbstractClass.Config.Log.FrameMeasurerLoggerConfig);
		return gClass;
	}

	public void Dispose()
	{
		if (Gclass714_0 != null)
		{
			Gclass714_0.Dispose();
		}
		Ginterface170_0 = null;
		StartOfFrame.OnUpdate -= method_0;
		EndOfFrame.OnUpdate -= method_1;
		StartOfFixedUpdate.OnUpdate -= method_2;
		EndOfFixedUpdate.OnUpdate -= method_3;
		StartOfUpdate.OnUpdate -= method_4;
		EndOfUpdate.OnUpdate -= method_5;
		StartOfPostLateUpdate.OnUpdate -= method_6;
	}

	public void method_0()
	{
		GameFrameMeasurer.StopMeasure();
		double currentFrameTime = GetCurrentFrameTime();
		GameUpdateMeasurer.MeasureStatistics.AddMeasurement(new GStruct132
		{
			Value = currentFrameTime - RenderMeasurer.MeasureStatistics.LastValue
		});
		action_0?.Invoke(currentFrameTime);
		FixedUpdateCountMeasurer.StartCounting();
		Gclass714_0.TrackUpdate(this);
		GameFrameMeasurer.StartMeasure();
	}

	public void method_1()
	{
		FixedUpdatesBetweenUpdateMeasurer.MeasureStatistics.AddMeasurement(new GStruct132
		{
			Value = Double_0
		});
		Double_0 = 0.0;
		FixedUpdateCountMeasurer.StopCounting();
		RenderMeasurer.StopMeasure();
		Gclass714_0.TryToLogAvgStatistics(this);
		Gclass714_0.TryToLogAvgNetworkQuality(NetworkQuality);
		method_8();
	}

	public void method_2()
	{
		SingleFixedUpdatesMeasurer.StartMeasure();
		if (Singleton<GameWorld>.Instantiated)
		{
			ulong totalOutgoingBytes = Singleton<GameWorld>.Instance.TotalOutgoingBytes;
			PacketsOutgoingMeasurer.StopMeasure(totalOutgoingBytes);
			PacketsOutgoingMeasurer.StartMeasure(totalOutgoingBytes);
			ulong totalIngoingBytes = Singleton<GameWorld>.Instance.TotalIngoingBytes;
			PacketsIngoingMeasurer.StopMeasure(totalIngoingBytes);
			PacketsIngoingMeasurer.StartMeasure(totalIngoingBytes);
		}
	}

	public void method_3()
	{
		SingleFixedUpdatesMeasurer.StopMeasure();
		double lastValue = SingleFixedUpdatesMeasurer.MeasureStatistics.LastValue;
		Double_0 += lastValue;
		FixedUpdateCountMeasurer.Increment();
		NetworkQuality.Commit();
		Gclass714_0.TrackFixedUpdate(this);
	}

	public void method_4()
	{
		UpdateMeasurer.StartMeasure();
		if (Singleton<GameWorld>.Instantiated)
		{
			PacketsOutgoingMeasurer.StopMeasure(Singleton<GameWorld>.Instance.TotalOutgoingBytes);
			PacketsIngoingMeasurer.StopMeasure(Singleton<GameWorld>.Instance.TotalIngoingBytes);
		}
	}

	public void method_5()
	{
		UpdateMeasurer.StopMeasure();
	}

	public void method_6()
	{
		if (Time.realtimeSinceStartup - Float_2 > 1f)
		{
			UpdateCountMeasurer.StopCounting();
			UpdateCountMeasurer.StartCounting();
			Float_2 = Time.realtimeSinceStartup;
		}
		UpdateCountMeasurer.Increment();
		RenderMeasurer.StartMeasure();
	}

	public void method_7()
	{
		int framesCountForAvgForTimeMeasurers = BackendConfigAbstractClass.Config.Log.FramesCountForAvgForTimeMeasurers;
		int framesCountForAvgForIncrementMeasurers = BackendConfigAbstractClass.Config.Log.FramesCountForAvgForIncrementMeasurers;
		FixedUpdatesBetweenUpdateMeasurer = GClass1355.CreateMeasureTimer(FIXED_UPDATE, framesCountForAvgForTimeMeasurers);
		SingleFixedUpdatesMeasurer = GClass1355.CreateMeasureTimer(SINGLE_FIXED_UPDATE, framesCountForAvgForTimeMeasurers);
		UpdateMeasurer = GClass1355.CreateMeasureTimer(UPDATE, framesCountForAvgForTimeMeasurers);
		GameUpdateMeasurer = GClass1355.CreateMeasureTimer(GAME_UPDATE, framesCountForAvgForTimeMeasurers);
		RenderMeasurer = GClass1355.CreateMeasureTimer(RENDER, framesCountForAvgForTimeMeasurers);
		GameFrameMeasurer = GClass1355.CreateMeasureTimer(FRAME, framesCountForAvgForTimeMeasurers);
		UpdateCountMeasurer = GClass1355.CreateDiagnosticsCounter(UPDATE, framesCountForAvgForIncrementMeasurers);
		FixedUpdateCountMeasurer = GClass1355.CreateDiagnosticsCounter(FIXED_UPDATE, framesCountForAvgForIncrementMeasurers);
		NetOutgoingMeasurer = GClass1355.CreateValueChangeMeasurer(NET_OUTGOINIG, framesCountForAvgForTimeMeasurers);
		PacketsOutgoingMeasurer = GClass1355.CreateValueChangeMeasurer(PACKETS_OUTGOINIG, framesCountForAvgForTimeMeasurers);
		PacketsIngoingMeasurer = GClass1355.CreateValueChangeMeasurer(PACKETS_INGOINIG, framesCountForAvgForTimeMeasurers);
		NetworkQuality.CreateMeasurers();
	}

	public double GetCurrentFrameTime()
	{
		return GameFrameMeasurer.MeasureStatistics.LastValue;
	}

	public void method_8()
	{
		if (Ginterface170_0 != null && !(Float_1 < 0f) && Float_0 + Float_1 <= Time.time)
		{
			Float_0 = Time.time;
			int num = (int)NetOutgoingMeasurer.MeasureStatistics.Average;
			int num2 = (int)NetworkQuality.PacketsQueueCount.MeasureStatistics.Average;
			Ginterface170_0.UpdateMetric("NetOut", num);
			Ginterface170_0.UpdateMetric("PacketsQueue", num2);
		}
	}

	public void SetMetricsHandler(GInterface170 metrics)
	{
		Ginterface170_0 = metrics;
	}
}
